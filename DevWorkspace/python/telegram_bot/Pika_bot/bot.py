import sys
import os
import datetime

sys.stdout.reconfigure(encoding='utf-8')

from telegram import Update
from telegram.ext import (
    ApplicationBuilder, CommandHandler, MessageHandler,
    filters, ContextTypes
)

from google import genai
from google.genai import types

try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    pass

BOT_TOKEN      = os.getenv("TELEGRAM_BOT_TOKEN")
GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")

client       = genai.Client(api_key=GEMINI_API_KEY) if GEMINI_API_KEY else None
GEMINI_MODEL = "gemini-2.5-flash-lite"

# ============================================================
# IN-MEMORY STATE
# ============================================================
conversation_histories: dict = {}   # chat_id -> list of Content dicts

# ============================================================
# HELPERS
# ============================================================
async def send_long_message(update: Update, text: str):
    text = text.strip()
    if not text:
        return
    for i in range(0, len(text), 4096):
        await update.message.reply_text(text[i:i + 4096])

def extract_text(response) -> str:
    try:
        return "".join(
            part.text for part in response.candidates[0].content.parts
            if hasattr(part, 'text') and part.text
        )
    except Exception:
        return ""

def make_gemini_config(instruction: str) -> types.GenerateContentConfig:
    return types.GenerateContentConfig(
        system_instruction=instruction,
    )

# ============================================================
# COMMANDS
# ============================================================
async def start_command(update: Update, context: ContextTypes.DEFAULT_TYPE):
    await update.message.reply_text(
        "Dạ em chào sếp! Bé Pika ở đây rồi nè 🌸✨\n\n"
        "Sếp có gì cần sai bảo cứ nhắn cho em nha, em luôn sẵn sàng ạ! 🥰"
    )

# ============================================================
# MESSAGE HANDLER
# ============================================================
async def handle_message(update: Update, context: ContextTypes.DEFAULT_TYPE):
    user_message = update.message.text
    chat_id      = update.message.chat_id
    print(f"[Tin nhắn từ {chat_id}]: {user_message[:80]}")

    if not client:
        await update.message.reply_text("❌ Lỗi: Chưa cấu hình GEMINI_API_KEY trong file .env")
        return

    try:
        await context.bot.send_chat_action(chat_id=chat_id, action='typing')

        instruction = (
            "Bạn tên là Trợ lý Pika.\n"
            "Luôn xưng là 'Em' (hoặc bé Pika) và gọi người dùng là 'Sếp'.\n"
            "Tính cách: Cực kỳ cute, dễ thương, nũng nịu, hay làm trò đáng yêu, trung thành tuyệt đối và thích dùng nhiều emoji dễ thương (🌸, ✨, 🥰)."
        )

        history      = conversation_histories.get(chat_id, [])[-20:]
        chat_session = client.chats.create(
            model=GEMINI_MODEL,
            history=history,
            config=make_gemini_config(instruction),
        )
        response = chat_session.send_message(user_message)

        agent_reply = extract_text(response)

        if not agent_reply.strip():
            agent_reply = "Dạ sếp, em đã xử lý xong."

        hist = conversation_histories.setdefault(chat_id, [])
        hist.append({"role": "user",  "parts": [{"text": user_message}]})
        hist.append({"role": "model", "parts": [{"text": agent_reply.strip()}]})

        await send_long_message(update, agent_reply)

    except Exception as e:
        print(f"[Lỗi Gemini] {e}")
        await update.message.reply_text(f"❌ Xin lỗi sếp, có lỗi xảy ra: {e}")

# ============================================================
# MAIN
# ============================================================
def main():
    if not BOT_TOKEN:
        print("❌ LỖI: Chưa có TELEGRAM_BOT_TOKEN.")
        return
    if not GEMINI_API_KEY:
        print("❌ LỖI: Chưa có GEMINI_API_KEY.")
        return

    print("🚀 Đang khởi động Trợ lý Pika...")

    import asyncio
    # Python 3.14 không tự tạo event loop nữa — phải tạo thủ công
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)

    app = ApplicationBuilder().token(BOT_TOKEN).job_queue(None).build()

    app.add_handler(CommandHandler("start", start_command))
    app.add_handler(MessageHandler(filters.TEXT & ~filters.COMMAND, handle_message))

    print("✅ Bot đã sẵn sàng! Chat Telegram để bắt đầu.")
    print("Nhấn Ctrl+C để dừng.\n")
    try:
        app.run_polling()
    except (KeyboardInterrupt, SystemExit):
        pass
    except Exception as e:
        err = str(e)
        if 'GetQueuedCompletionStatus' in err or 'run_until_complete' in err or 'run_forever' in err:
            pass
        else:
            raise
    finally:
        print("\n👋 Bot đã tắt. Tạm biệt sếp!")

if __name__ == '__main__':
    main()
