#!/usr/bin/env python3
"""Generates MultiBranchSwitcher.ico - a branch-fork glyph on a dark badge.

Drawn at 1024px and downsampled per icon size, with the small sizes rendered from a
tighter master so the fork still reads at 16px.

Only needed to change the design; the .ico ships alongside the source. Double-click
make_icon.bat (which installs Pillow if missing), or double-click this file directly if
Python and Pillow are already set up. Output always lands next to this script.
"""
import os
import sys
import traceback

# Everything is written beside this script, not into whatever the current directory is
# when Explorer launches it.
HERE = os.path.dirname(os.path.abspath(__file__))

S = 1024
BG = (38, 51, 64, 255)        # dark slate badge
EDGE = (58, 74, 92, 255)      # subtle rim
TRUNK = (232, 238, 242, 255)  # master line, near-white
BRANCH = (240, 160, 48, 255)  # the new branch, amber


def build():
    from PIL import Image, ImageDraw

    def render(pad, radius, k, rim):
        """Draws the badge and glyph. k scales the glyph about the centre so small icon
        sizes can use a tighter badge and a proportionally larger fork."""
        img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.rounded_rectangle([pad, pad, S - pad, S - pad], radius=radius,
                            fill=BG, outline=EDGE, width=rim)

        c = S / 2.0

        def sx(v):                      # scale a coordinate about the centre
            return c + (v - c) * k

        stroke = int(84 * k)
        node_r = 104 * k
        trunk_x = sx(380)

        d.line([(trunk_x, sx(250)), (trunk_x, sx(790))], fill=TRUNK, width=stroke)
        r = 320 * k
        ay = sx(340)
        d.arc([trunk_x - r, ay - r, trunk_x + r, ay + r], start=0, end=90,
              fill=BRANCH, width=stroke)

        def dot(cx, cy, colour):
            d.ellipse([cx - node_r, cy - node_r, cx + node_r, cy + node_r], fill=colour)

        dot(trunk_x, sx(250), TRUNK)        # master, top
        dot(trunk_x, sx(790), TRUNK)        # master, bottom
        dot(sx(700), sx(340), BRANCH)       # branch tip
        return img

    large = render(pad=24, radius=190, k=1.0, rim=14)   # 48px and up
    small = render(pad=4, radius=150, k=1.22, rim=0)    # 16, 24, 32: fills the tile

    sizes = [256, 128, 64, 48, 32, 24, 16]
    frames = [(large if n >= 48 else small).resize((n, n), Image.LANCZOS) for n in sizes]

    ico = os.path.join(HERE, "MultiBranchSwitcher.ico")
    frames[0].save(ico, format="ICO", sizes=[(n, n) for n in sizes],
                   append_images=frames[1:])
    print("wrote " + ico)

    # Preview sheet: every size on a light and a dark strip, as it would appear in
    # Explorer and on the taskbar.
    gap = 16
    scale = 3
    width = sum(min(n * scale, 128) + gap for n in sizes) + gap
    sheet = Image.new("RGBA", (width, 320), (245, 245, 245, 255))
    sd = ImageDraw.Draw(sheet)
    sd.rectangle([0, 160, width, 320], fill=(32, 32, 32, 255))
    x = gap
    for n, f in zip(sizes, frames):
        view = f.resize((min(n * scale, 128), min(n * scale, 128)), Image.NEAREST)
        sheet.alpha_composite(view, (x, 80 - view.height // 2))
        sheet.alpha_composite(view, (x, 240 - view.height // 2))
        sd.text((x, 148), "%dpx" % n, fill=(90, 90, 90, 255))
        x += view.width + gap

    png = os.path.join(HERE, "icon_preview.png")
    sheet.save(png)
    print("wrote " + png)


def main():
    try:
        build()
    except ImportError:
        print("")
        print("Pillow is not installed. Install it with:")
        print("    py -m pip install --user pillow")
        print("or just double-click make_icon.bat, which does it for you.")
        return 1
    except Exception:
        print("")
        traceback.print_exc()
        return 1
    print("")
    print("Done. Rebuild with build.bat to embed the new icon.")
    return 0


if __name__ == "__main__":
    code = main()
    # Keep the console open when launched from Explorer, where the window would
    # otherwise close before anything above could be read.
    if os.name == "nt" and sys.stdin is not None and sys.stdin.isatty():
        try:
            input("Press Enter to close...")
        except EOFError:
            pass
    sys.exit(code)
