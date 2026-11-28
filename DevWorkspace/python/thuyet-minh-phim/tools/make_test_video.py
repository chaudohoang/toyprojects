"""Tạo video test có phụ đề in cứng (hardsub) + file .srt đáp án, để thử chức năng quét/thuyết minh.

    python tools/make_test_video.py <thư_mục_ra>
"""
import subprocess
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
from longtieng.srt import Cue, write_srt  # noqa: E402
from longtieng.video import ffmpeg_exe  # noqa: E402

W, H, FPS, DUR = 1920, 1080, 24, 40
FONT = "C:/Windows/Fonts/segoeuib.ttf"

CUES = [  # (bắt đầu, kết thúc, nội dung) — có câu 2 dòng, câu ngắn, câu nối liền nhau
    (1.0, 3.2, "Sao? Định dạy tôi làm việc à?"),
    (3.4, 5.4, "Không dám, tôi chỉ nhắc anh thôi."),
    (5.4, 7.0, "Hôm qua anh đã đi đâu?"),
    (8.5, 11.5, "Tôi ở nhà cả ngày,\nkhông đi đâu cả."),
    (11.7, 13.0, "Có ai làm chứng không?"),
    (13.2, 15.6, "Vợ tôi, cô ấy có thể xác nhận."),
    (17.0, 19.0, "Được, chúng tôi sẽ kiểm tra lại."),
    (19.2, 22.4, "Còn mấy tháng nữa\nmới chính thức nhận việc ạ."),
    (24.0, 25.3, "Vậy thì cố gắng lên nhé."),
    (25.5, 26.7, "Vâng, em cảm ơn anh."),
    (28.0, 31.5, "Chuyện này không đơn giản như cậu nghĩ đâu."),
    (32.0, 34.0, "Ừ."),
    (34.2, 37.5, "Ngày mai tám giờ sáng,\nđến đồn cảnh sát gặp tôi."),
]


def render_text(draw, text, font, y_bottom, fill, stroke=0):
    lines = text.split("\n")
    lh = font.size + 14
    y = y_bottom - lh * len(lines)
    for line in lines:
        w = draw.textlength(line, font=font)
        draw.text(((W - w) / 2, y), line, font=font, fill=fill, stroke_width=stroke, stroke_fill=(0, 0, 0))
        y += lh


def background(t):
    # Cảnh thay đổi mỗi ~4,5 s (cắt cảnh khi phụ đề vẫn hiện), có mảng trắng lớn để thử bộ lọc nền sáng
    scene = int(t / 4.5)
    rng = np.random.default_rng(scene)
    base = rng.integers(30, 140, size=3)
    yy, xx = np.mgrid[0:H:4, 0:W:4]
    g = (np.sin(xx / 90 + t * 0.8 + scene) * 40 + np.cos(yy / 70 - t * 0.5) * 30)[..., None]
    img = np.clip(base + g, 0, 255).astype(np.uint8)
    img = np.repeat(np.repeat(img, 4, axis=0), 4, axis=1)
    if scene % 2 == 1:  # áo/tường trắng ở nửa dưới
        img[700:1000, 200:700] = 235
    return Image.fromarray(img[:H, :W], "RGB")


def main():
    out_dir = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
    out_dir.mkdir(parents=True, exist_ok=True)
    video = out_dir / "test_video.mp4"
    font = ImageFont.truetype(FONT, 54)
    wm_font = ImageFont.truetype(FONT, 30)
    logo_font = ImageFont.truetype("C:/Windows/Fonts/comicbd.ttf", 44)

    cmd = [ffmpeg_exe(), "-y", "-hide_banner", "-loglevel", "error",
           "-f", "rawvideo", "-pix_fmt", "rgb24", "-s", f"{W}x{H}", "-r", str(FPS), "-i", "-",
           "-f", "lavfi", "-i", f"sine=frequency=220:sample_rate=48000:duration={DUR}",
           "-c:v", "libx264", "-preset", "veryfast", "-crf", "23", "-pix_fmt", "yuv420p",
           "-c:a", "aac", "-shortest", str(video)]
    proc = subprocess.Popen(cmd, stdin=subprocess.PIPE)
    for i in range(DUR * FPS):
        t = i / FPS
        img = background(t)
        d = ImageDraw.Draw(img)
        d.text((80, 90), "Motchillv.so", font=logo_font, fill=(255, 255, 255), stroke_width=2, stroke_fill=(0, 0, 0))
        render_text(d, "PHIM ĐƯỢC CẬP NHẬT NHANH NHẤT TẠI MOTCHILLV.SO", wm_font, 925, (150, 150, 150))
        for s, e, text in CUES:
            if s <= t < e:
                render_text(d, text, font, 880, (255, 255, 255), stroke=3)
        proc.stdin.write(img.tobytes())
    proc.stdin.close()
    proc.wait()
    write_srt(out_dir / "test_video.truth.srt", [Cue(s, e, txt.replace("\n", " ")) for s, e, txt in CUES])
    print(video)


if __name__ == "__main__":
    main()
