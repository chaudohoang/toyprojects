import sys
import os
import json
import datetime
import pytz
import uuid as _uuid

sys.stdout.reconfigure(encoding='utf-8')

from telegram import Update, InlineKeyboardButton, InlineKeyboardMarkup
from telegram.ext import (
    ApplicationBuilder, CommandHandler, MessageHandler,
    CallbackQueryHandler, filters, ContextTypes
)

from google import genai
from google.genai import types

from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.date import DateTrigger
from apscheduler.triggers.cron import CronTrigger

try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    pass

BOT_TOKEN      = os.getenv("TELEGRAM_BOT_TOKEN")
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")

client       = genai.Client(api_key=GEMINI_API_KEY) if GEMINI_API_KEY else None
GEMINI_MODEL = "gemini-2.0-flash-lite"

# APScheduler dùng trực tiếp, không qua PTB job_queue
scheduler    = AsyncIOScheduler()
app_instance = None   # set in main() sau khi build app

# ============================================================
# STORAGE
# ============================================================
STORAGE_FILE = "user_settings.json"

def load_storage() -> dict:
    if os.path.exists(STORAGE_FILE):
        try:
            with open(STORAGE_FILE, 'r', encoding='utf-8') as f:
                return json.load(f)
        except Exception:
            return {}
    return {}

def save_storage(data: dict):
    with open(STORAGE_FILE, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

def get_user_tz_name(chat_id: int) -> str:
    return load_storage().get(str(chat_id), {}).get("timezone", "Asia/Ho_Chi_Minh")

def get_user_tz(chat_id: int) -> pytz.BaseTzInfo:
    try:
        return pytz.timezone(get_user_tz_name(chat_id))
    except Exception:
        return pytz.timezone("Asia/Ho_Chi_Minh")

# ============================================================
# IN-MEMORY STATE
# ============================================================
conversation_histories: dict = {}   # chat_id -> list of Content dicts
waiting_for_schedule:   dict = {}   # chat_id -> datetime
pending_name_batch:     dict = {}   # chat_id -> batch_id (chờ nhập tên cặp)
pending_update:         dict = {}   # chat_id -> {reminders, batch_id} (chờ confirm update)

# ============================================================
# CONSTANTS
# ============================================================
TIMEZONES = [
    ("🇻🇳 Việt Nam (GMT+7)",     "Asia/Ho_Chi_Minh"),
    ("🇸🇬 Singapore (GMT+8)",     "Asia/Singapore"),
    ("🇯🇵 Nhật Bản (GMT+9)",      "Asia/Tokyo"),
    ("🇰🇷 Hàn Quốc (GMT+9)",      "Asia/Seoul"),
    ("🇨🇳 Trung Quốc (GMT+8)",    "Asia/Shanghai"),
    ("🇦🇺 Sydney (GMT+10/+11)",   "Australia/Sydney"),
    ("🇬🇧 London (GMT+0/+1)",     "Europe/London"),
    ("🇺🇸 New York (EST/EDT)",    "America/New_York"),
    ("🇺🇸 Los Angeles (PST/PDT)", "America/Los_Angeles"),
]

# (slot_name, fire_hour, fire_minute, is_ipo_only)
# Fire trước 5 phút để nhắc kịp
TINHIEUCODINH_SLOTS = [
    ("🕐 1:30 PM",  13, 25, False),
    ("🕖 7:30 PM",  19, 25, False),
    ("🕥 10:30 PM", 22, 25, True),
]

# ============================================================
# GEMINI TOOL
# ============================================================
def add_reminders(reminders: list[dict]):
    """
    Trích xuất các mốc thời gian từ lịch trình và cài đặt nhắc nhở.
    Args:
        reminders: Danh sách sự kiện. Mỗi item là dict với:
                   'time' (YYYY-MM-DD HH:MM:SS) và 'description' (mô tả).
    """
    pass

def make_gemini_config(instruction: str) -> types.GenerateContentConfig:
    return types.GenerateContentConfig(
        system_instruction=instruction,
        tools=[add_reminders],
    )

# ============================================================
# SCHEDULED JOB FUNCTIONS (gọi trực tiếp bởi APScheduler)
# ============================================================
async def reminder_job(chat_id: int, target_time: str, slots: list, job_id: str = ""):
    """slots = [{"desc": "...", "batch_id": "..."}, ...]"""
    moi_names = []
    gt_names  = []

    for slot in slots:
        bid  = slot.get("batch_id", "")
        desc = slot.get("desc", "")
        ni   = _get_batch_name(chat_id, bid) if bid else None
        if "người mới & người giới thiệu" in desc:
            if ni:
                if ni["moi"] not in moi_names: moi_names.append(ni["moi"])
                if ni["gt"]  not in gt_names:  gt_names.append(ni["gt"])
        elif "người giới thiệu" in desc:
            if ni and ni["gt"] not in gt_names: gt_names.append(ni["gt"])
        elif "người mới" in desc:
            if ni and ni["moi"] not in moi_names: moi_names.append(ni["moi"])

    if moi_names and gt_names:
        desc_display = f"người mới ({', '.join(moi_names)}) & người giới thiệu ({', '.join(gt_names)})"
    elif gt_names:
        desc_display = f"người giới thiệu ({', '.join(gt_names)})"
    elif moi_names:
        desc_display = f"người mới ({', '.join(moi_names)})"
    else:
        # Không có tên — dùng desc gốc
        descs = list(dict.fromkeys(s["desc"] for s in slots))
        desc_display = " & ".join(descs)

    # Format giờ từ target_time thành "3:00 PM"
    try:
        _tdt = datetime.datetime.strptime(target_time, "%Y-%m-%d %H:%M:%S")
        time_label = _tdt.strftime("%I:%M %p").lstrip("0")
    except Exception:
        time_label = target_time

    await app_instance.bot.send_message(
        chat_id,
        text=f"🚨 Sếp ơi! 5 phút nữa là tới 🕐 {time_label} — Đến giờ kiểm tra tín hiệu BG thưởng {desc_display} rồi ạ! Nhớ vào xem nhé sếp~ 🫡"
    )
    if job_id:
        _remove_tinhieuthuong(chat_id, job_id)

async def daily_tinhieucodinh_job(chat_id: int, slot_name: str, is_ipo: bool = False):
    if is_ipo:
        text = (
            f"🚨 Sếp ơi! 5 phút nữa là tới {slot_name} — Đến giờ kiểm tra tín hiệu BG rồi ạ!\n"
            f"⚠️ Lưu ý: Tín hiệu này chỉ dành cho những ai có tín hiệu BG IPO nhé sếp~ 🫡"
        )
    else:
        text = f"🚨 Sếp ơi! 5 phút nữa là tới {slot_name} — Đến giờ kiểm tra tín hiệu BG cố định rồi ạ! Nhớ vào xem nhé sếp~ 🫡"
    await app_instance.bot.send_message(chat_id, text=text)

# ============================================================
# HELPERS
# ============================================================
def extract_function_call_args(response):
    try:
        for part in response.candidates[0].content.parts:
            if part.function_call:
                return part.function_call.name, dict(part.function_call.args)
    except Exception:
        pass
    return None, None

def extract_text(response) -> str:
    try:
        return "".join(
            part.text for part in response.candidates[0].content.parts
            if hasattr(part, 'text') and part.text
        )
    except Exception:
        return ""

async def schedule_reminders_from_list(chat_id: int, reminders: list, job_prefix: str = "reminder", batch_id: str = "") -> int:
    """Nhóm reminders theo target_time, merge vào 1 job mỗi timeslot."""
    tz      = get_user_tz(chat_id)
    now_utc = datetime.datetime.now(pytz.utc)
    count   = 0

    # Group by target_time
    from collections import defaultdict as _dd
    by_time = _dd(list)
    for r in reminders:
        if r.get("time"):
            by_time[r["time"]].append(r.get("description", "Sự kiện"))

    for target_time, descs in by_time.items():
        try:
            naive     = datetime.datetime.strptime(target_time, "%Y-%m-%d %H:%M:%S")
            aware     = tz.localize(naive)
            remind_at = aware - datetime.timedelta(minutes=5)
            if remind_at <= now_utc:
                continue

            new_slots = [{"desc": d, "batch_id": batch_id} for d in descs]
            job_id    = _get_timeslot_job_id(chat_id, target_time)

            existing = scheduler.get_job(job_id)
            if existing:
                # Merge vào job đã có
                merged = existing.kwargs.get("slots", []) + new_slots
                scheduler.modify_job(job_id, kwargs={
                    "chat_id":     chat_id,
                    "target_time": target_time,
                    "slots":       merged,
                    "job_id":      job_id,
                })
                _update_tinhieuthuong_slots(chat_id, job_id, merged)
            else:
                scheduler.add_job(
                    reminder_job,
                    trigger=DateTrigger(run_date=remind_at),
                    kwargs={"chat_id": chat_id, "target_time": target_time, "slots": new_slots, "job_id": job_id},
                    id=job_id,
                    name=f"thuong|{chat_id}|{target_time}",
                )
                _save_tinhieuthuong(chat_id, {
                    "job_id":      job_id,
                    "job_prefix":  "thuong",
                    "target_time": target_time,
                    "remind_at":   remind_at.astimezone(pytz.utc).isoformat(),
                    "slots":       new_slots,
                })
            count += len(new_slots)
        except Exception as e:
            print(f"[schedule] Lỗi parse '{target_time}': {e}")
    return count

def _save_batch_name(chat_id: int, batch_id: str, moi: str, gt: str):
    storage = load_storage()
    user    = storage.setdefault(str(chat_id), {})
    user.setdefault("batch_names", {})[batch_id] = {"moi": moi, "gt": gt}
    save_storage(storage)

def _get_batch_name(chat_id: int, batch_id: str) -> dict | None:
    storage = load_storage()
    return storage.get(str(chat_id), {}).get("batch_names", {}).get(batch_id)

def _delete_batch_name(chat_id: int, batch_id: str):
    storage = load_storage()
    user    = storage.get(str(chat_id), {})
    if "batch_names" in user and batch_id in user["batch_names"]:
        del user["batch_names"][batch_id]
        save_storage(storage)


def _count_overlap(chat_id: int, reminders: list) -> tuple[int, int]:
    """Trả về (số timeslot bị trùng, tổng timeslot mới)."""
    from collections import defaultdict as _dd
    times = set()
    for r in reminders:
        if r.get("time"):
            times.add(r["time"])
    overlap = sum(1 for t in times if scheduler.get_job(_get_timeslot_job_id(chat_id, t)))
    return overlap, len(times)


def _get_timeslot_job_id(chat_id: int, target_time: str) -> str:
    """Job ID duy nhất theo (chat_id, target_time) để merge nhiều batch."""
    t = target_time.replace(" ", "").replace(":", "").replace("-", "")
    return f"thuong_{chat_id}_{t}"

def _update_tinhieuthuong_slots(chat_id: int, job_id: str, slots: list):
    """Cập nhật slots của một pending reminder trong storage."""
    storage = load_storage()
    pending = storage.get(str(chat_id), {}).get("pending_tinhieuthuong", [])
    for r in pending:
        if r.get("job_id") == job_id:
            r["slots"] = slots
            break
    save_storage(storage)


def _save_tinhieuthuong(chat_id: int, reminder: dict):
    """Lưu một pending reminder vào storage."""
    storage = load_storage()
    user    = storage.setdefault(str(chat_id), {})
    pending = user.setdefault("pending_tinhieuthuong", [])
    pending.append(reminder)
    save_storage(storage)

def _remove_tinhieuthuong(chat_id: int, job_id: str):
    """Xóa một pending reminder khỏi storage sau khi đã fire."""
    storage = load_storage()
    user    = storage.get(str(chat_id), {})
    pending = user.get("pending_tinhieuthuong", [])
    user["pending_tinhieuthuong"] = [r for r in pending if r.get("job_id") != job_id]
    save_storage(storage)

def _clear_tinhieuthuong(chat_id: int):
    """Xóa toàn bộ pending reminders của một chat_id."""
    storage = load_storage()
    if str(chat_id) in storage:
        storage[str(chat_id)].pop("pending_tinhieuthuong", None)
        save_storage(storage)


def add_tinhieucodinh_jobs_for(chat_id: int, tz: pytz.BaseTzInfo):
    """Thêm 3 daily tín hiệu cố định jobs cho một chat_id."""
    for slot_name, hour, minute, is_ipo in TINHIEUCODINH_SLOTS:
        job_id = f"tinhieucodinh_{chat_id}_{hour}{minute:02d}"
        scheduler.add_job(
            daily_tinhieucodinh_job,
            trigger=CronTrigger(hour=hour, minute=minute, timezone=tz),
            kwargs={"chat_id": chat_id, "slot_name": slot_name, "is_ipo": is_ipo},
            id=job_id,
            name=f"tinhieucodinh|{chat_id}|{slot_name}",
            replace_existing=True,
        )

def remove_tinhieucodinh_jobs_for(chat_id: int):
    """Xóa tất cả jobs của một chat_id."""
    prefix = str(chat_id)
    removed = 0
    for job in scheduler.get_jobs():
        # job.id format: "type_chatid_..." hoặc "type_chatid_..."
        parts = job.id.split("_")
        if len(parts) >= 2 and parts[1] == prefix:
            scheduler.remove_job(job.id)
            removed += 1
    return removed

async def send_long_message(update: Update, text: str):
    text = text.strip()
    if not text:
        return
    for i in range(0, len(text), 4096):
        await update.message.reply_text(text[i:i + 4096])

# ============================================================
# COMMANDS
# ============================================================
async def start_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    await update.message.reply_text(
        "Xin chào sếp! Em là Trợ lý Pika 🌸\n\n"
        "/timezone — Cài múi giờ để nhắc lịch đúng giờ\n"
        "/kiemtrahengiobg — Kiểm tra hẹn giờ BG\n"
        "/hengiobgcodinh — Bật nhắc tín hiệu BG cố định\n"
        "/hengiobgthuong — Bật nhắc tín hiệu BG thưởng\n"
        "/tathengiobgcodinh — Tắt nhắc tín hiệu BG cố định\n"
        "/tathengiobgthuong — Tắt nhắc tín hiệu BG thưởng\n"
        "/tathengiobgtatca — Tắt tất cả nhắc tín hiệu BG\n\n"
        "Hoặc sếp cứ chat tự nhiên với em nhé! 😊"
    )

async def timezone_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    keyboard = [
        [InlineKeyboardButton(label, callback_data=f"tz:{tz_name}")]
        for label, tz_name in TIMEZONES
    ]
    await update.message.reply_text(
        "🌏 Sếp đang ở timezone nào ạ?\nEm cần biết để cài lịch nhắc đúng giờ:",
        reply_markup=InlineKeyboardMarkup(keyboard)
    )

async def timezone_callback(update: Update, context: ContextTypes.DEFAULT_TYPE):
    query   = update.callback_query
    await query.answer()
    tz_name = query.data[len("tz:"):]
    chat_id = query.message.chat_id
    storage = load_storage()
    storage.setdefault(str(chat_id), {})["timezone"] = tz_name
    save_storage(storage)
    label = next((lbl for lbl, tz in TIMEZONES if tz == tz_name), tz_name)
    await query.edit_message_text(
        f"✅ Đã lưu timezone: {label}\nEm sẽ tính giờ nhắc theo múi giờ này cho sếp rồi ạ! 🫡"
    )

# --- /hengiobgcodinh ---
async def hengiobgcodinh_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    chat_id = update.message.chat_id
    tz      = get_user_tz(chat_id)
    add_tinhieucodinh_jobs_for(chat_id, tz)
    storage = load_storage()
    storage.setdefault(str(chat_id), {})["ipo_daily"] = True
    save_storage(storage)
    await update.message.reply_text(
        f"✅ Đã bật nhắc tín hiệu BG cố định cho sếp!\n"
        f"⏰ 1:30 PM, 7:30 PM và 10:30 PM mỗi ngày\n"
        f"🌏 Timezone: {get_user_tz_name(chat_id)}\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca"
    )

async def hengiobgcodinh_callback(update: Update, context: ContextTypes.DEFAULT_TYPE):
    query   = update.callback_query
    await query.answer()
    choice  = query.data[len("tinhieucodinh:"):]
    chat_id = query.message.chat_id

    if choice == "no":
        await query.edit_message_text("Dạ, em đã hủy. Sếp cần gì cứ nhắn em nhé! 🌸")
        return

    tz = get_user_tz(chat_id)
    add_tinhieucodinh_jobs_for(chat_id, tz)

    storage = load_storage()
    storage.setdefault(str(chat_id), {})["ipo_daily"] = True
    save_storage(storage)

    await query.edit_message_text(
        f"✅ Đã bật nhắc tín hiệu BG cố định cho sếp!\n"
        f"⏰ 1:30 PM, 7:30 PM và 10:30 PM mỗi ngày\n"
        f"🌏 Timezone: {get_user_tz_name(chat_id)}\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca"
    )

# --- /hengiobgthuong ---
async def hengiobgthuong_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    waiting_for_schedule[update.message.chat_id] = datetime.datetime.now()
    await update.message.reply_text(
        "📋 Dạ sếp! Sếp paste lịch trình vào đây trong vòng 30 giây nhé,\nem sẽ đọc và cài nhắc ngay! ⏳"
    )

# --- /kiemtrahengiobg ---
def job_status_label(target_dt: datetime.datetime) -> str:
    """Trả về ghi chú thời gian còn lại đến target (không phải lúc nhắc)."""
    if not target_dt:
        return "(đã qua)"
    now  = datetime.datetime.now(pytz.utc)
    diff = (target_dt - now).total_seconds()
    if diff < 0:
        return "(đã qua)"
    total_min = int(diff // 60)
    days      = total_min // (60 * 24)
    rem_min   = total_min % (60 * 24)
    hours     = rem_min // 60
    minutes   = rem_min % 60
    parts = []
    if days:    parts.append(f"{days} ngày")
    if hours:   parts.append(f"{hours} tiếng")
    if minutes or not parts: parts.append(f"{minutes} phút")
    return f"(còn {' '.join(parts)})"


async def show_schedule_list(chat_id: int, message):
    """Hiển thị toàn bộ lịch hẹn — dùng chung cho /kiemtrahengiobg và auto-list."""
    prefix    = str(chat_id)
    tz        = get_user_tz(chat_id)
    tz_name   = get_user_tz_name(chat_id)
    now_local = datetime.datetime.now(tz).strftime("%H:%M:%S ngày %d/%m/%Y")

    tinhieucodinh_jobs = []
    thuong_jobs        = []
    reminder_jobs      = []

    for job in scheduler.get_jobs():
        parts = job.id.split("_")
        if len(parts) >= 2 and parts[1] == prefix:
            if job.id.startswith("tinhieucodinh_"):
                tinhieucodinh_jobs.append(job)
            elif job.id.startswith("thuong_"):
                thuong_jobs.append(job)
            elif job.id.startswith("reminder_"):
                reminder_jobs.append(job)

    lines = [
        "🔍 Kiểm tra lịch hẹn giờ của sếp:\n",
        f"🌏 Timezone: {tz_name}",
        f"🕐 Giờ hiện tại: {now_local}\n",
    ]

    # --- Tín hiệu BG cố định ---
    if tinhieucodinh_jobs:
        lines.append("📊 Tín hiệu BG cố định: ✅ Đang bật")
        lines.append("   └ 1:30 PM | 7:30 PM | 10:30 PM")
    else:
        lines.append("📊 Tín hiệu BG cố định: ❌ Chưa bật")

    # --- Tín hiệu thưởng ---
    lines.append("")
    if thuong_jobs:
        from collections import defaultdict
        by_date = defaultdict(list)
        for job in thuong_jobs:
            target_str = (job.name or "").split("|")[2] if len((job.name or "").split("|")) > 2 else ""
            slots_j    = job.kwargs.get("slots", []) if hasattr(job, "kwargs") else []
            try:
                target_naive = datetime.datetime.strptime(target_str, "%Y-%m-%d %H:%M:%S")
                target_dt    = tz.localize(target_naive)
            except Exception:
                target_dt = job.next_run_time
            if not target_dt: continue
            desc_display = _format_slots_display(chat_id, slots_j)
            label        = job_status_label(target_dt)
            by_date[target_dt.strftime("%d/%m")].append((target_dt, desc_display, label))

        lines.append(f"🎁 Tín hiệu BG thưởng: {len(thuong_jobs)} tín hiệu đang chờ")
        for dk in sorted(by_date.keys(), key=lambda d: datetime.datetime.strptime(d + f"/{datetime.datetime.now().year}", "%d/%m/%Y")):
            lines.append(f"  📅 Ngày {dk}:")
            for tdt, ddisplay, lbl in sorted(by_date[dk]):
                time_str = tdt.strftime("%I:%M %p").lstrip("0")
                lines.append(f"     • {time_str} → {ddisplay} {lbl}")
    else:
        lines.append("🎁 Tín hiệu BG thưởng: ❌ Chưa bật")


    # --- Lịch nhắc thường ---
    if reminder_jobs:
        lines.append("")
        lines.append(f"⏰ Lịch nhắc khác: {len(reminder_jobs)} lịch đang chờ")
        for job in reminder_jobs:
            name_parts = (job.name or "").split("|")
            desc   = name_parts[2] if len(name_parts) > 2 else "?"
            target = name_parts[3] if len(name_parts) > 3 else "?"
            next_run = job.next_run_time
            if next_run:
                next_local = next_run.astimezone(tz).strftime("%H:%M ngày %d/%m")
                lines.append(f"   • {desc} → nhắc lúc {next_local}")
            else:
                lines.append(f"   • {desc}")

    lines.append("\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca")
    await message.reply_text("\n".join(lines))


async def kiemtrahengiobg_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    await show_schedule_list(update.message.chat_id, update.message)

# --- /list ---
async def list_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    chat_id  = update.message.chat_id
    prefix   = str(chat_id)
    tinhieucodinh_jobs = []
    rem_jobs = []

    for job in scheduler.get_jobs():
        parts = job.id.split("_")
        if len(parts) >= 2 and parts[1] == prefix:
            if job.id.startswith("tinhieucodinh_"):
                tinhieucodinh_jobs.append(job)
            elif job.id.startswith("reminder_"):
                rem_jobs.append(job)

    if not tinhieucodinh_jobs and not rem_jobs:
        await update.message.reply_text("📋 Sếp chưa có lịch nhắc nào đang chờ ạ!")
        return

    lines = ["📋 Danh sách lịch nhắc của sếp:\n"]
    if tinhieucodinh_jobs:
        lines.append("📊 Nhắc IPO hàng ngày: 1:30 PM | 7:30 PM | 10:30 PM")
    if rem_jobs:
        lines.append("\n⏰ Lịch nhắc sự kiện:")
        for job in rem_jobs:
            # name format: "reminder|chat_id|description|target_time"
            name_parts = (job.name or "").split("|")
            desc   = name_parts[2] if len(name_parts) > 2 else "?"
            target = name_parts[3] if len(name_parts) > 3 else "?"
            lines.append(f"  • {desc} → {target}")

    await update.message.reply_text("\n".join(lines))

# --- dupcheck callback (xử lý khi phát hiện lịch trùng) ---
async def dupcheck_callback(update: Update, context: ContextTypes.DEFAULT_TYPE):
    query    = update.callback_query
    await query.answer()
    parts    = query.data.split(":")  # dupcheck:yes/no:batch_id
    choice   = parts[1]
    batch_id = parts[2] if len(parts) > 2 else ""
    chat_id  = query.message.chat_id

    if choice == "no":
        pending_update.pop(chat_id, None)
        await query.edit_message_text("✅ Đã giữ nguyên lịch cũ sếp nhé!")
        return

    # Xóa các timeslot jobs bị overlap và schedule lại
    info      = pending_update.pop(chat_id, {})
    reminders = info.get("reminders", [])
    if not reminders:
        await query.edit_message_text("❌ Không tìm thấy dữ liệu lịch. Sếp paste lại nhé!")
        return

    # Xóa jobs bị trùng
    from collections import defaultdict as _dd
    times = set(r["time"] for r in reminders if r.get("time"))
    removed = 0
    for t in times:
        jid = _get_timeslot_job_id(chat_id, t)
        if scheduler.get_job(jid):
            scheduler.remove_job(jid)
            _remove_tinhieuthuong(chat_id, jid)
            removed += 1

    # Schedule lại
    count = await schedule_reminders_from_list(chat_id, reminders, job_prefix="thuong", batch_id=batch_id)
    if count > 0:
        pending_name_batch[chat_id] = {"batch_id": batch_id, "count": count}
        keyboard = [[
            InlineKeyboardButton("✏️ Nhập tên", callback_data=f"namebatch:yes:{batch_id}"),
            InlineKeyboardButton("⏭ Bỏ qua",   callback_data=f"namebatch:no:{batch_id}"),
        ]]
        await query.edit_message_text(
            f"✅ Đã cập nhật! Xóa {removed} lịch cũ, cài {count} lịch mới.\n\n"
            "👤 Sếp có muốn đặt tên cho cặp tín hiệu này không?\n"
            "Format: Tên người mới / Tên người giới thiệu",
            reply_markup=InlineKeyboardMarkup(keyboard)
        )
    else:
        await query.edit_message_text("⚠️ Không có lịch nào trong tương lai để cài ạ!")


# --- namebatch callback (nhập tên cặp sau khi hẹn lịch thưởng) ---
async def namebatch_callback(update: Update, context: ContextTypes.DEFAULT_TYPE):
    query   = update.callback_query
    await query.answer()
    parts   = query.data.split(":")  # namebatch:yes/no:batch_id
    choice  = parts[1]
    batch_id = parts[2] if len(parts) > 2 else ""
    chat_id = query.message.chat_id

    if choice == "no":
        info = pending_name_batch.pop(chat_id, {})
        bid  = info.get("batch_id", batch_id) if isinstance(info, dict) else info
        cnt  = info.get("count", 0) if isinstance(info, dict) else 0
        await query.edit_message_text("⏭ Đã bỏ qua tên. Đây là danh sách lịch thưởng:")
        await show_thuong_result(chat_id, cnt, query.message, batch_id=bid)
        return

    # Chờ user nhập tên — lưu dict với batch_id + count
    info = pending_name_batch.get(chat_id, {})
    cnt  = info.get("count", 0) if isinstance(info, dict) else 0
    pending_name_batch[chat_id] = {"batch_id": batch_id, "count": cnt, "awaiting_name": True}
    await query.edit_message_text(
        "✏️ Sếp nhập tên theo format:\n"
        "Tên người mới / Tên người giới thiệu\n\n"
        "Ví dụ: Nguyễn A / Trần B"
    )


# --- /tathengiobgcodinh ---
async def tathengiobgcodinh_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    chat_id = update.message.chat_id
    prefix  = str(chat_id)
    count   = 0
    for job in scheduler.get_jobs():
        parts = job.id.split("_")
        if len(parts) >= 2 and parts[1] == prefix and job.id.startswith("tinhieucodinh_"):
            scheduler.remove_job(job.id)
            count += 1
    storage = load_storage()
    if str(chat_id) in storage:
        storage[str(chat_id)].pop("ipo_daily", None)
        save_storage(storage)
    if count > 0:
        await update.message.reply_text("✅ Đã tắt nhắc tín hiệu BG cố định rồi ạ!\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca")
    else:
        await update.message.reply_text("ℹ️ Sếp chưa bật nhắc tín hiệu BG cố định nào cả ạ.\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca")

# --- /tathengiobgthuong ---
async def tathengiobgthuong_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    chat_id = update.message.chat_id
    prefix  = str(chat_id)
    count   = 0
    for job in scheduler.get_jobs():
        parts = job.id.split("_")
        if len(parts) >= 2 and parts[1] == prefix and (
            job.id.startswith("thuong_") or job.id.startswith("reminder_")
        ):
            scheduler.remove_job(job.id)
            count += 1
    _clear_tinhieuthuong(chat_id)
    if count > 0:
        await update.message.reply_text(
            f"✅ Đã tắt {count} lịch thưởng đang chờ rồi ạ!\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca"
        )
    else:
        await update.message.reply_text("ℹ️ Sếp chưa bật nhắc tín hiệu BG thưởng nào cả ạ.\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca")

# --- /tathengiobgtatca ---
async def tathengiobgtatca_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    chat_id = update.message.chat_id
    prefix  = str(chat_id)
    count   = 0
    for job in scheduler.get_jobs():
        parts = job.id.split("_")
        if len(parts) >= 2 and parts[1] == prefix:
            scheduler.remove_job(job.id)
            count += 1
    storage = load_storage()
    if str(chat_id) in storage:
        storage[str(chat_id)].pop("ipo_daily", None)
        storage[str(chat_id)].pop("pending_tinhieuthuong", None)
        save_storage(storage)
    waiting_for_schedule.pop(chat_id, None)
    if count > 0:
        await update.message.reply_text(f"✅ Dạ, em đã hủy tất cả {count} tín hiệu BG của sếp rồi ạ!\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca")
    else:
        await update.message.reply_text("ℹ️ Sếp chưa bật nhắc tín hiệu BG cố định hay thưởng nào cả ạ.\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca")

# ============================================================
# MESSAGE HANDLER
# ============================================================
async def handle_message(update: Update, context: ContextTypes.DEFAULT_TYPE):
    user_message = update.message.text
    chat_id      = update.message.chat_id
    print(f"[Tin nhắn từ {chat_id}]: {user_message[:80]}")

    # Đang chờ nhập tên cặp?
    if chat_id in pending_name_batch:
        info     = pending_name_batch.pop(chat_id)
        batch_id = info.get("batch_id", "") if isinstance(info, dict) else info
        count    = info.get("count", 0)     if isinstance(info, dict) else 0
        if "/" in user_message:
            parts_name = user_message.split("/", 1)
            moi = parts_name[0].strip()  # có thể rỗng
            gt  = parts_name[1].strip()  # có thể rỗng
            _save_batch_name(chat_id, batch_id, moi, gt)
            label_moi = f"👤 Người mới: {moi}" if moi else "👤 Người mới: (không tên)"
            label_gt  = f"👥 Người giới thiệu: {gt}" if gt else "👥 Người giới thiệu: (không tên)"
            await update.message.reply_text(
                f"✅ Đã lưu!\n{label_moi}\n{label_gt}\nĐây là danh sách lịch thưởng:"
            )
            await show_thuong_result(chat_id, count, update.message, batch_id=batch_id)
        else:
            # Format 4: không có "/" → coi như bỏ qua tên
            await update.message.reply_text("⏭ Không có tên, đây là danh sách lịch thưởng:")
            await show_thuong_result(chat_id, count, update.message, batch_id=batch_id)
        return

    # Đang chờ paste lịch trình?
    if chat_id in waiting_for_schedule:
        elapsed = (datetime.datetime.now() - waiting_for_schedule.pop(chat_id)).total_seconds()
        if elapsed <= 30:
            await process_schedule_paste(update, user_message)
            return
        else:
            await update.message.reply_text("⏰ Đã quá 30 giây rồi sếp ơi! Dùng /hengiobgthuong lại rồi paste nhé.")

    if not client:
        await update.message.reply_text("❌ Lỗi: Chưa cấu hình GEMINI_API_KEY trong file .env")
        return

    try:
        await context.bot.send_chat_action(chat_id=chat_id, action='typing')

        tz       = get_user_tz(chat_id)
        tz_name  = get_user_tz_name(chat_id)
        time_str = datetime.datetime.now(tz).strftime(f"%H:%M:%S ngày %d/%m/%Y ({tz_name})")

        instruction = (
            "Bạn tên là Trợ lý Pika.\n"
            "Luôn xưng là 'Em' và gọi người dùng là 'Sếp'.\n"
            "Tính cách: Ngoan ngoãn, vâng lời, nhiệt tình, dùng emoji dễ thương.\n"
            f"[Hệ thống: Thời gian hiện tại theo timezone của sếp là {time_str}]\n"
            "Nếu tin nhắn chứa lịch trình hoặc sự kiện tương lai, hãy DÙNG TOOL add_reminders.\n"
            "Nếu lịch kéo dài NHIỀU NGÀY, BẮT BUỘC tách ra từng ngày riêng biệt."
        )

        history      = conversation_histories.get(chat_id, [])[-20:]
        chat_session = client.chats.create(
            model=GEMINI_MODEL,
            history=history,
            config=make_gemini_config(instruction),
        )
        response = chat_session.send_message(user_message)

        fn_name, fn_args = extract_function_call_args(response)
        agent_reply      = extract_text(response)
        has_fn_call      = fn_name is not None

        if has_fn_call and fn_name == "add_reminders" and fn_args:
            count = await schedule_reminders_from_list(chat_id, fn_args.get("reminders", []))
            agent_reply += f"\nDạ, em đã cài {count} lịch nhắc nhở (trước 5 phút) cho sếp rồi ạ! 🫡"

        if not agent_reply.strip():
            agent_reply = "Dạ sếp, em đã xử lý xong."

        if not has_fn_call:
            hist = conversation_histories.setdefault(chat_id, [])
            hist.append({"role": "user",  "parts": [{"text": user_message}]})
            hist.append({"role": "model", "parts": [{"text": agent_reply.strip()}]})

        await send_long_message(update, agent_reply)

    except Exception as e:
        print(f"[Lỗi Gemini] {e}")
        await update.message.reply_text(f"❌ Xin lỗi sếp, có lỗi xảy ra: {e}")

def _format_slots_display(chat_id: int, slots: list) -> str:
    """Format danh sách slots thành chuỗi hiển thị với tên nếu có."""
    moi_names = []
    gt_names  = []
    for slot in slots:
        bid  = slot.get("batch_id", "")
        desc = slot.get("desc", "")
        ni   = _get_batch_name(chat_id, bid) if bid else None
        if "người mới & người giới thiệu" in desc:
            if ni:
                if ni["moi"] not in moi_names: moi_names.append(ni["moi"])
                if ni["gt"]  not in gt_names:  gt_names.append(ni["gt"])
        elif "người giới thiệu" in desc:
            if ni and ni["gt"] not in gt_names: gt_names.append(ni["gt"])
        elif "người mới" in desc:
            if ni and ni["moi"] not in moi_names: moi_names.append(ni["moi"])
    # Filter bỏ tên rỗng
    moi_names = [n for n in moi_names if n]
    gt_names  = [n for n in gt_names  if n]

    has_moi = any("người mới" in s["desc"] for s in slots)
    has_gt  = any("người giới thiệu" in s["desc"] for s in slots)

    if has_moi and has_gt:
        part_moi = f"người mới ({', '.join(moi_names)})" if moi_names else "người mới"
        part_gt  = f"người giới thiệu ({', '.join(gt_names)})" if gt_names else "người giới thiệu"
        return f"{part_moi} & {part_gt}"
    elif has_gt:
        return f"người giới thiệu ({', '.join(gt_names)})" if gt_names else "người giới thiệu"
    elif has_moi:
        return f"người mới ({', '.join(moi_names)})" if moi_names else "người mới"
    else:
        descs = list(dict.fromkeys(s["desc"] for s in slots))
        return " & ".join(descs)


async def show_thuong_result(chat_id: int, count: int, message, batch_id: str = ""):
    """Hiện kết quả cài lịch thưởng — format theo timeslot với tên cặp."""
    tz      = get_user_tz(chat_id)
    tz_name = get_user_tz_name(chat_id)
    prefix  = str(chat_id)
    from collections import defaultdict

    thuong_jobs = [
        j for j in scheduler.get_jobs()
        if j.id.startswith("thuong_") and j.id.split("_")[1] == prefix
    ]

    name_info = _get_batch_name(chat_id, batch_id) if batch_id else None
    lines = [
        "✅ Đã bật nhắc tín hiệu BG thưởng cho sếp!",
        f"🌏 Timezone: {tz_name}",
    ]
    if name_info:
        lines.append(f"👤 {name_info[chr(109)+chr(111)+chr(105)]} / {name_info[chr(103)+chr(116)]}")
    lines.append("")

    # Nhóm jobs theo ngày, hiện timeslot + combined names
    by_date = defaultdict(list)  # date_key -> [(target_dt, desc_display)]
    for job in thuong_jobs:
        target_str = (job.name or "").split("|")[2] if len((job.name or "").split("|")) > 2 else ""
        slots_j    = job.kwargs.get("slots", []) if hasattr(job, "kwargs") else []
        try:
            target_naive = datetime.datetime.strptime(target_str, "%Y-%m-%d %H:%M:%S")
            target_dt    = tz.localize(target_naive)
        except Exception:
            target_dt = job.next_run_time
        if not target_dt: continue
        desc_display = _format_slots_display(chat_id, slots_j)
        label        = job_status_label(target_dt)
        by_date[target_dt.strftime("%d/%m")].append((target_dt, desc_display, label))

    for dk in sorted(by_date.keys(), key=lambda d: datetime.datetime.strptime(d + f"/{datetime.datetime.now().year}", "%d/%m/%Y")):
        lines.append(f"  📅 Ngày {dk}:")
        for tdt, ddisplay, lbl in sorted(by_date[dk]):
            time_str = tdt.strftime("%I:%M %p").lstrip("0")
            lines.append(f"     • {time_str} → {ddisplay} {lbl}")

    text = "\n".join(lines) + "\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca"
    for i in range(0, len(text), 4096):
        await message.reply_text(text[i:i+4096])


async def process_schedule_paste(update: Update, schedule_text: str):
    """Parse cả 2 phần lịch trình và schedule luôn."""
    chat_id = update.message.chat_id
    await update.get_bot().send_chat_action(chat_id=chat_id, action='typing')

    try:
        reminders = parse_combined_schedules(schedule_text)
        if not reminders:
            await update.message.reply_text(
                "🤔 Em không tìm thấy lịch trình nào ạ.\n"
                "Sếp thử paste lại rõ hơn rồi dùng /hengiobgthuong nhé!"
            )
            return
        batch_id = _uuid.uuid4().hex[:8]
        count = await schedule_reminders_from_list(chat_id, reminders, job_prefix="thuong", batch_id=batch_id)
        if count > 0:
            # Lưu batch_id + count để show list SAU KHI nhập tên
            pending_name_batch[chat_id] = {"batch_id": batch_id, "count": count}
            keyboard = [[
                InlineKeyboardButton("✏️ Nhập tên", callback_data=f"namebatch:yes:{batch_id}"),
                InlineKeyboardButton("⏭ Bỏ qua",   callback_data=f"namebatch:no:{batch_id}"),
            ]]
            await update.message.reply_text(
                f"✅ Em đã cài {count} lịch nhắc thưởng rồi ạ!\n\n"
                "👤 Sếp có muốn đặt tên cho cặp tín hiệu này không?\n"
                "Format: Tên người mới / Tên người giới thiệu",
                reply_markup=InlineKeyboardMarkup(keyboard)
            )
        else:
            total = len(reminders)
            await update.message.reply_text(
                f"⚠️ Em đọc được {total} mốc thời gian trong lịch trình nhưng tất cả đã qua rồi ạ.\nSếp kiểm tra lại ngày tháng trong lịch trình nhé!\n\n🔍 Kiểm tra hẹn giờ BG: /kiemtrahengiobg\n✅ Bật hẹn giờ BG cố định: /hengiobgcodinh\n✅ Bật hẹn giờ BG thưởng: /hengiobgthuong\n❌ Tắt hẹn giờ BG cố định: /tathengiobgcodinh\n❌ Tắt hẹn giờ BG thường: /tathengiobgthuong\n🚫 Tắt tất cả hẹn giờ BG: /tathengiobgtatca"
            )
    except Exception as e:
        print(f"[Lỗi parse lịch] {e}")
        await update.message.reply_text(f"❌ Lỗi khi phân tích lịch trình: {e}")


def parse_combined_schedules(text: str) -> list[dict]:
    """
    Parse cả 2 phần lịch trình cùng lúc, gộp thông minh:
    - Giờ trùng cả 2 phần → 'người mới & người giới thiệu'
    - Giờ chỉ có phần người giới thiệu → 'người giới thiệu'
    - Giờ chỉ có phần người mới (edge case) → 'người mới'
    """
    gt_marker = "người giới thiệu của bạn"
    idx = text.find(gt_marker)
    section1 = text[:idx]  if idx != -1 else text
    section2 = text[idx:]  if idx != -1 else ""

    rem1 = parse_schedule_text(section1)
    rem2 = parse_schedule_text(section2)

    set1 = {r["time"] for r in rem1}
    set2 = {r["time"] for r in rem2}

    slots = {}  # time_str -> description

    for r in rem1:
        t = r["time"]
        if t in set2:
            slots[t] = "Tín hiệu thưởng người mới & người giới thiệu"
        else:
            slots[t] = "Tín hiệu thưởng người mới"

    for r in rem2:
        t = r["time"]
        if t not in slots:
            slots[t] = "Tín hiệu thưởng người giới thiệu"

    return [{"time": t, "description": desc} for t, desc in sorted(slots.items())]


def extract_schedule_section(text: str, is_referrer: bool) -> str:
    """Cắt đúng phần lịch trình cần parse (bản thân hoặc người giới thiệu)."""
    # Marker phân tách 2 phần
    gt_marker = "người giới thiệu của bạn"

    if is_referrer:
        idx = text.find(gt_marker)
        return text[idx:] if idx != -1 else ""
    else:
        # Phần bản thân: từ đầu đến trước marker người giới thiệu
        idx = text.find(gt_marker)
        return text[:idx] if idx != -1 else text


def parse_schedule_text(text: str) -> list[dict]:
    """
    Parse lịch trình từ text, trả về list {time, description}.
    Hỗ trợ format:
      - Ngày: "4.7-4.9", "4.7-4.10"
      - Giờ: "🕒 3:00 PM", "🕒 9:30 PM"
    """
    import re as _re

    # Parse date range: ví dụ "4.7-4.9" hoặc "4.7 - 4.10"
    date_pattern = _re.compile(r'(\d{1,2})\.(\d{1,2})\s*[-–]\s*(\d{1,2})\.(\d{1,2})')
    date_match = date_pattern.search(text)
    if not date_match:
        return []

    s_month, s_day, e_month, e_day = map(int, date_match.groups())
    year = datetime.datetime.now().year

    # Sinh danh sách ngày
    try:
        start = datetime.datetime(year, s_month, s_day)
        end   = datetime.datetime(year, e_month, e_day)
    except ValueError:
        return []

    dates = []
    cur = start
    while cur <= end:
        dates.append(cur)
        cur += datetime.timedelta(days=1)

    # Parse giờ: hỗ trợ "🕒 3:00 PM", "🕒 9:30 PM", "3:00 PM", "15:00"
    time_pattern = _re.compile(
        r'(?:🕒\s*)?(\d{1,2}):(\d{2})\s*(AM|PM)',
        _re.IGNORECASE
    )
    times_raw = time_pattern.findall(text)

    if not times_raw:
        return []

    reminders = []
    for date in dates:
        for hour_str, minute_str, ampm in times_raw:
            hour   = int(hour_str)
            minute = int(minute_str)
            ampm   = ampm.upper()
            if ampm == 'PM' and hour != 12:
                hour += 12
            elif ampm == 'AM' and hour == 12:
                hour = 0
            dt = date.replace(hour=hour, minute=minute, second=0)
            reminders.append({
                'time':        dt.strftime('%Y-%m-%d %H:%M:%S'),
                'description': f"Tín hiệu thưởng {date.strftime('%d/%m')} lúc {hour_str}:{minute_str} {ampm}",
            })

    return reminders


async def parse_schedule(chat_id: int, schedule_text: str, is_referrer: bool) -> tuple[int, str]:
    """Parse lịch trình bằng regex (không dùng AI), trả về (số lịch đã cài, lỗi)."""
    job_prefix = "thuonggt" if is_referrer else "thuong"
    try:
        section   = extract_schedule_section(schedule_text, is_referrer)
        reminders = parse_schedule_text(section)
        if not reminders:
            return 0, ""
        count = await schedule_reminders_from_list(chat_id, reminders, job_prefix=job_prefix)
        return count, ""
    except Exception as e:
        return 0, str(e)


# ============================================================
# RESTORE DAILY TINHIEUCODINH JOBS SAU RESTART
# ============================================================
def restore_tinhieucodinh_jobs():
    storage  = load_storage()
    restored = 0
    for chat_id_str, settings in storage.items():
        if not settings.get("ipo_daily"):
            continue
        chat_id = int(chat_id_str)
        try:
            tz = pytz.timezone(settings.get("timezone", "Asia/Ho_Chi_Minh"))
        except Exception:
            tz = pytz.timezone("Asia/Ho_Chi_Minh")
        add_tinhieucodinh_jobs_for(chat_id, tz)
        restored += 1
        print(f"  ✅ Restored tinhieucodinh jobs cho chat_id {chat_id}")
    if restored:
        print(f"[Startup] Restored tinhieucodinh jobs cho {restored} người dùng.")

def restore_tinhieuthuong_jobs():
    """Restore one-time reminder jobs từ storage sau khi restart."""
    storage  = load_storage()
    now_utc  = datetime.datetime.now(pytz.utc)
    restored = 0
    skipped  = 0
    for chat_id_str, settings in storage.items():
        chat_id = int(chat_id_str)
        pending = settings.get("pending_tinhieuthuong", [])
        kept    = []
        for r in pending:
            try:
                remind_at = datetime.datetime.fromisoformat(r["remind_at"])
                if remind_at <= now_utc:
                    skipped += 1
                    continue   # đã qua, bỏ qua
                job_id    = r["job_id"]
                job_prefix = r.get("job_prefix", "reminder")
                desc      = r.get("description", "")  # fallback cho format cũ
                target    = r["target_time"]
                slots_r = r.get("slots", [{"desc": desc, "batch_id": r.get("batch_id", "")}])
                scheduler.add_job(
                    reminder_job,
                    trigger=DateTrigger(run_date=remind_at),
                    kwargs={"chat_id": chat_id, "target_time": target, "slots": slots_r, "job_id": job_id},
                    id=job_id,
                    name=f"thuong|{chat_id}|{target}",
                )
                kept.append(r)
                restored += 1
            except Exception as e:
                print(f"  ⚠️ Lỗi restore reminder: {e}")
        # Cập nhật lại storage — chỉ giữ những cái chưa qua
        settings["pending_tinhieuthuong"] = kept
    if restored or skipped:
        save_storage(storage)
        print(f"[Startup] Restored {restored} tinhieuthuong jobs ({skipped} đã qua, bỏ qua).")


# ============================================================
# MAIN
# ============================================================
async def post_init(application):
    """Chạy khi event loop đã sẵn sàng — đây mới là chỗ đúng để start scheduler."""
    global app_instance
    app_instance = application
    restore_tinhieucodinh_jobs()
    restore_tinhieuthuong_jobs()
    scheduler.start()
    print("[Startup] Scheduler đã khởi động.")


def main():
    if not BOT_TOKEN:
        print("❌ LỖI: Chưa có TELEGRAM_BOT_TOKEN.")
        return
    if not GEMINI_API_KEY:
        print("❌ LỖI: Chưa có GEMINI_API_KEY.")
        return

    print("🚀 Đang khởi động Trợ lý Pika...")

    # Python 3.14 không tự tạo event loop nữa — phải tạo thủ công
    import asyncio
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)

    # Tắt job_queue của PTB, dùng APScheduler trực tiếp
    # post_init start scheduler sau khi event loop đã chạy
    app = ApplicationBuilder().token(BOT_TOKEN).job_queue(None).post_init(post_init).build()

    app.add_handler(CommandHandler("start",           start_command))
    app.add_handler(CommandHandler("timezone",        timezone_command))
    app.add_handler(CommandHandler("hengiobgcodinh",  hengiobgcodinh_command))
    app.add_handler(CommandHandler("hengiobgthuong",  hengiobgthuong_command))
    app.add_handler(CommandHandler("kiemtrahengiobg",   kiemtrahengiobg_command))
    app.add_handler(CommandHandler("list",              list_command))
    app.add_handler(CommandHandler("tathengiobgcodinh", tathengiobgcodinh_command))
    app.add_handler(CommandHandler("tathengiobgthuong", tathengiobgthuong_command))
    app.add_handler(CommandHandler("tathengiobgtatca",   tathengiobgtatca_command))
    app.add_handler(CallbackQueryHandler(timezone_callback, pattern=r"^tz:"))
    app.add_handler(MessageHandler(filters.TEXT & ~filters.COMMAND, handle_message))

    print("✅ Bot đã sẵn sàng! Chat Telegram để bắt đầu.")
    print("Nhấn Ctrl+C để dừng.\n")
    try:
        app.run_polling()
    except (KeyboardInterrupt, SystemExit):
        pass
    except Exception as e:
        # Bắt lỗi cleanup của Python 3.14 trên Windows khi Ctrl+C
        err = str(e)
        if 'GetQueuedCompletionStatus' in err or 'run_until_complete' in err or 'run_forever' in err:
            pass
        else:
            raise
    finally:
        print("\n👋 Bot đã tắt. Tạm biệt sếp!")

if __name__ == '__main__':
    main()
