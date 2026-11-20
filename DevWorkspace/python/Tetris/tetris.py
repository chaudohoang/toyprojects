"""
Tetris
By Vivian 💕
"""

import pygame
import random
import sys
import os
import json

# --- Constants ---
# 1600x900 window sharing the Snake build's chrome: same HUD band, footer,
# menu / settings / game-over coordinates and font sizes.
WINDOW_WIDTH = 1600
WINDOW_HEIGHT = 900

COLS = 10
ROWS = 20
CELL = 36
CELL_INSET = 2
RADIUS_CELL = 6
BORDER_W = 3
RADIUS_BOARD = 6

BOARD_W = COLS * CELL                      # 360
BOARD_H = ROWS * CELL                      # 720
BOARD_X = (WINDOW_WIDTH - BOARD_W) // 2    # 620
BOARD_Y = 100                              # well runs 100..820

# Side panels
MINI = 24                                  # cell size in hold / next previews
HOLD_LABEL_POS = (380, 100)
HOLD_BOX = (380, 140, 200, 180)
NEXT_LABEL_POS = (1020, 100)
NEXT_BOX = (1020, 140, 200, 420)
NEXT_SLOT_H = 140
NEXT_COUNT = 3
STAT_LABEL_X = 380
STAT_VALUE_RIGHT = 580
STAT_ROW_Y = (400, 460)

# Colors
BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
RED = (220, 50, 50)
GREEN = (50, 200, 50)
YELLOW = (255, 215, 0)
DARK_BG = (15, 15, 25)
WELL_BG = (20, 20, 35)
WELL_LINE = (30, 30, 45)
PANEL_BORDER = (60, 60, 80)
GRAY_TEXT = (150, 150, 150)
FOOTER_TEXT = (120, 120, 120)

PIECE_COLORS = {
    "I": (45, 210, 225),
    "O": (240, 200, 50),
    "T": (165, 80, 220),
    "S": (80, 200, 80),
    "Z": (220, 60, 60),
    "J": (70, 110, 230),
    "L": (235, 145, 50),
}

# Spawn cells inside a BOX[name]-squared grid. Rotation is (x,y) -> (n-1-y, x),
# which reproduces the SRS states for exactly these spawn layouts. O never rotates.
SHAPES = {
    "I": [(0, 1), (1, 1), (2, 1), (3, 1)],
    "O": [(1, 0), (2, 0), (1, 1), (2, 1)],
    "T": [(1, 0), (0, 1), (1, 1), (2, 1)],
    "S": [(1, 0), (2, 0), (0, 1), (1, 1)],
    "Z": [(0, 0), (1, 0), (1, 1), (2, 1)],
    "J": [(0, 0), (0, 1), (1, 1), (2, 1)],
    "L": [(2, 0), (0, 1), (1, 1), (2, 1)],
}
BOX = {"I": 4, "O": 4, "T": 3, "S": 3, "Z": 3, "J": 3, "L": 3}
PIECE_NAMES = ("I", "O", "T", "S", "Z", "J", "L")

# SRS wall kicks, converted to screen coordinates (y grows downward).
KICKS_JLSTZ = {
    (0, 1): ((0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2)),
    (1, 0): ((0, 0), (1, 0), (1, 1), (0, -2), (1, -2)),
    (1, 2): ((0, 0), (1, 0), (1, 1), (0, -2), (1, -2)),
    (2, 1): ((0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2)),
    (2, 3): ((0, 0), (1, 0), (1, -1), (0, 2), (1, 2)),
    (3, 2): ((0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2)),
    (3, 0): ((0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2)),
    (0, 3): ((0, 0), (1, 0), (1, -1), (0, 2), (1, 2)),
}
KICKS_I = {
    (0, 1): ((0, 0), (-2, 0), (1, 0), (-2, 1), (1, -2)),
    (1, 0): ((0, 0), (2, 0), (-1, 0), (2, -1), (-1, 2)),
    (1, 2): ((0, 0), (-1, 0), (2, 0), (-1, -2), (2, 1)),
    (2, 1): ((0, 0), (1, 0), (-2, 0), (1, 2), (-2, -1)),
    (2, 3): ((0, 0), (2, 0), (-1, 0), (2, -1), (-1, 2)),
    (3, 2): ((0, 0), (-2, 0), (1, 0), (-2, 1), (1, -2)),
    (3, 0): ((0, 0), (1, 0), (-2, 0), (1, 2), (-2, -1)),
    (0, 3): ((0, 0), (-1, 0), (2, 0), (-1, -2), (2, 1)),
}

# Scoring and timing
LINE_SCORES = {1: 100, 2: 300, 3: 500, 4: 800}
SOFT_DROP_POINTS = 1
HARD_DROP_POINTS = 2
LOCK_DELAY_MS = 500
MAX_LOCK_RESETS = 15
DAS_DELAY_MS = 170      # hold left/right this long before it auto-repeats
DAS_RATE_MS = 50
SOFT_RATE_MS = 40

# Settings, adjustable in-game and persisted beside the exe
DEFAULT_START_LEVEL = 1.0
DEFAULT_LINES_PER_LEVEL = 10.0
DEFAULT_GHOST = 1.0
SETTINGS_FILE = "settings.json"

# key, label, min, max, step, kind
SETTING_ROWS = (
    ("start_level", "Start level", 1.0, 15.0, 1.0, "num"),
    ("lines_per_level", "Lines per level", 1.0, 20.0, 1.0, "num"),
    ("ghost_piece", "Ghost piece", 0.0, 1.0, 1.0, "bool"),
)

# Shared layout with the WPF build — same numbers as the Snake pair.
FOOTER_Y = 876
MENU_TITLE_Y = 225
MENU_LINE_Y = (420, 473, 578, 630, 682)
OVER_TITLE_Y = 270
OVER_SCORE_Y = 390
OVER_BEST_Y = 465
OVER_HINT_Y = 600
PAUSE_TITLE_Y = 380
PAUSE_HINT_Y = 480
HUD_SCORE_POS = (30, 9)
HUD_BEST_MARGIN = 30
HUD_BEST_Y = 21
SET_TITLE_Y = 200
SET_ROW_Y = (350, 440, 530)
SET_MARKER_X = 460
SET_LABEL_X = 520
SET_VALUE_X = 1100
SET_HINT_Y = (700, 750)


def fall_interval_ms(level):
    """Tetris guideline gravity: (0.8 - (level-1)*0.007) ** (level-1) seconds."""
    lvl = max(1, min(int(level), 20))
    seconds = (0.8 - (lvl - 1) * 0.007) ** (lvl - 1)
    return max(seconds * 1000.0, 16.0)


def resource_path(relative_path):
    """Absolute path to a bundled resource, for dev and PyInstaller alike."""
    try:
        base_path = sys._MEIPASS
    except AttributeError:
        base_path = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(base_path, relative_path)


class Settings:
    """Game settings, persisted to settings.json beside the exe."""

    def __init__(self):
        self.start_level = DEFAULT_START_LEVEL
        self.lines_per_level = DEFAULT_LINES_PER_LEVEL
        self.ghost_piece = DEFAULT_GHOST
        self.load()

    @staticmethod
    def path():
        if getattr(sys, "frozen", False):
            base = os.path.dirname(sys.executable)
        else:
            base = os.path.dirname(os.path.abspath(__file__))
        return os.path.join(base, SETTINGS_FILE)

    def load(self):
        try:
            with open(self.path(), "r", encoding="utf-8") as f:
                data = json.load(f)
        except (OSError, ValueError):
            return  # missing or corrupt file: keep the defaults
        if not isinstance(data, dict):
            return
        for key, _, lo, hi, _, _ in SETTING_ROWS:
            value = data.get(key)
            if isinstance(value, (int, float)) and not isinstance(value, bool):
                setattr(self, key, min(max(float(value), lo), hi))

    def save(self):
        data = {key: getattr(self, key) for key, *_ in SETTING_ROWS}
        try:
            with open(self.path(), "w", encoding="utf-8") as f:
                json.dump(data, f, indent=2)
        except OSError:
            pass  # read-only folder: carry on with in-memory values

    def adjust(self, index, direction):
        key, _, lo, hi, step, _ = SETTING_ROWS[index]
        value = round(getattr(self, key) + direction * step, 1)
        setattr(self, key, min(max(value, lo), hi))

    def display(self, index):
        key, _, _, _, _, kind = SETTING_ROWS[index]
        value = getattr(self, key)
        if kind == "bool":
            return "On" if value >= 0.5 else "Off"
        return f"{value:.0f}"

    @property
    def ghost(self):
        return self.ghost_piece >= 0.5


class Bag:
    """7-bag randomizer: each piece appears once per bag, order shuffled."""

    def __init__(self):
        self.queue = []

    def _refill(self):
        bag = list(PIECE_NAMES)
        random.shuffle(bag)
        self.queue.extend(bag)

    def peek(self, count):
        while len(self.queue) < count:
            self._refill()
        return list(self.queue[:count])

    def take(self):
        if not self.queue:
            self._refill()
        return self.queue.pop(0)


class Piece:
    def __init__(self, name):
        self.name = name
        self.rot = 0
        self.x = 3
        self.y = 0

    def cells(self, rot=None, x=None, y=None):
        """Absolute board cells, optionally for a hypothetical rotation/position."""
        rot = self.rot if rot is None else rot
        x = self.x if x is None else x
        y = self.y if y is None else y
        n = BOX[self.name]
        turns = 0 if self.name == "O" else rot % 4
        out = []
        for cx, cy in SHAPES[self.name]:
            for _ in range(turns):
                cx, cy = n - 1 - cy, cx
            out.append((x + cx, y + cy))
        return out


class Game:
    def __init__(self):
        pygame.init()
        self.screen = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT))
        pygame.display.set_caption("Tetris")
        self.clock = pygame.time.Clock()

        # Fonts — sizes match the WPF build one-for-one.
        self.font_title = pygame.font.SysFont("Segoe UI", 78, bold=True)
        self.font_large = pygame.font.SysFont("Segoe UI", 72, bold=True)
        self.font_medium_bold = pygame.font.SysFont("Segoe UI", 42, bold=True)
        self.font_medium = pygame.font.SysFont("Segoe UI", 42)
        self.font_small = pygame.font.SysFont("Segoe UI", 30)
        self.font_tiny = pygame.font.SysFont("Segoe UI", 18)

        self.settings = Settings()
        self.state = "menu"     # menu, settings, playing, paused, gameover
        self.sel = 0            # highlighted settings row
        self.best = 0
        self.prev_best = 0
        self.reset_game()

    # --- game state ---------------------------------------------------------

    def reset_game(self):
        self.board = [[None] * COLS for _ in range(ROWS)]
        self.bag = Bag()
        self.hold = None
        self.hold_used = False
        self.score = 0
        self.lines = 0
        self.level = int(self.settings.start_level)
        self.gravity_accum = 0.0
        self.lock_timer = 0.0
        self.lock_resets = 0
        self.grounded = False
        self.das_dir = 0
        self.das_timer = 0.0
        self.das_charged = False
        self.soft_timer = 0.0
        self.piece = Piece(self.bag.take())

    def start_game(self):
        self.reset_game()
        self.state = "playing"

    def spawn_piece(self, name=None):
        self.piece = Piece(name or self.bag.take())
        self.gravity_accum = 0.0
        self.lock_timer = 0.0
        self.lock_resets = 0
        self.grounded = False
        if not self.fits(self.piece.cells()):
            self.top_out()

    def top_out(self):
        self.prev_best = self.best
        self.best = max(self.best, self.score)
        self.state = "gameover"

    def fits(self, cells):
        """Cells above the well (y < 0) are allowed, so pieces can spawn there."""
        for x, y in cells:
            if x < 0 or x >= COLS or y >= ROWS:
                return False
            if y >= 0 and self.board[y][x] is not None:
                return False
        return True

    def touch_lock_reset(self):
        # Any successful move or rotation on the floor buys more time, up to 15.
        if self.grounded and self.lock_resets < MAX_LOCK_RESETS:
            self.lock_timer = 0.0
            self.lock_resets += 1

    def move(self, dx, dy=0):
        if self.fits(self.piece.cells(x=self.piece.x + dx, y=self.piece.y + dy)):
            self.piece.x += dx
            self.piece.y += dy
            self.touch_lock_reset()
            return True
        return False

    def rotate(self, turn):
        if self.piece.name == "O":
            return False
        new_rot = (self.piece.rot + turn) % 4
        table = KICKS_I if self.piece.name == "I" else KICKS_JLSTZ
        for dx, dy in table.get((self.piece.rot, new_rot), ((0, 0),)):
            if self.fits(self.piece.cells(rot=new_rot,
                                          x=self.piece.x + dx,
                                          y=self.piece.y + dy)):
                self.piece.rot = new_rot
                self.piece.x += dx
                self.piece.y += dy
                self.touch_lock_reset()
                return True
        return False

    def ghost_offset(self):
        drop = 0
        while self.fits(self.piece.cells(y=self.piece.y + drop + 1)):
            drop += 1
        return drop

    def soft_drop(self):
        if self.move(0, 1):
            self.score += SOFT_DROP_POINTS
            return True
        return False

    def hard_drop(self):
        drop = self.ghost_offset()
        self.piece.y += drop
        self.score += drop * HARD_DROP_POINTS
        self.lock_piece()

    def hold_piece(self):
        if self.hold_used:
            return
        swap = self.hold
        self.hold = self.piece.name
        self.hold_used = True
        self.spawn_piece(swap)   # None pulls the next piece from the bag

    def lock_piece(self):
        for x, y in self.piece.cells():
            if 0 <= y < ROWS and 0 <= x < COLS:
                self.board[y][x] = self.piece.name

        cleared = self.clear_lines()
        if cleared:
            self.score += LINE_SCORES[cleared] * self.level
            self.lines += cleared
            per = max(1, int(self.settings.lines_per_level))
            self.level = int(self.settings.start_level) + self.lines // per

        self.hold_used = False
        self.spawn_piece()

    def clear_lines(self):
        kept = [row for row in self.board if any(cell is None for cell in row)]
        cleared = ROWS - len(kept)
        if cleared:
            self.board = [[None] * COLS for _ in range(cleared)] + kept
        return cleared

    def update(self, dt):
        if self.state != "playing":
            return

        interval = fall_interval_ms(self.level)
        self.gravity_accum += dt
        while self.gravity_accum >= interval:
            self.gravity_accum -= interval
            if not self.move(0, 1):
                break

        self.grounded = not self.fits(self.piece.cells(y=self.piece.y + 1))
        if self.grounded:
            self.lock_timer += dt
            if self.lock_timer >= LOCK_DELAY_MS:
                self.lock_piece()
        else:
            self.lock_timer = 0.0

    # --- drawing ------------------------------------------------------------

    def draw_centered(self, font, text, color, y):
        surf = font.render(text, True, color)
        self.screen.blit(surf, (WINDOW_WIDTH // 2 - surf.get_width() // 2, y))

    def draw_cell(self, px, py, color, size):
        rect = pygame.Rect(px + CELL_INSET, py + CELL_INSET,
                           size - 2 * CELL_INSET, size - 2 * CELL_INSET)
        pygame.draw.rect(self.screen, color, rect, border_radius=RADIUS_CELL)

    def draw_frame(self, box):
        x, y, w, h = box
        pygame.draw.rect(self.screen, PANEL_BORDER,
                         pygame.Rect(x - BORDER_W, y - BORDER_W,
                                     w + 2 * BORDER_W, h + 2 * BORDER_W),
                         BORDER_W, border_radius=RADIUS_BOARD)

    def draw_well(self):
        pygame.draw.rect(self.screen, WELL_BG,
                         pygame.Rect(BOARD_X, BOARD_Y, BOARD_W, BOARD_H))
        for c in range(1, COLS):
            x = BOARD_X + c * CELL
            pygame.draw.line(self.screen, WELL_LINE, (x, BOARD_Y), (x, BOARD_Y + BOARD_H))
        for r in range(1, ROWS):
            y = BOARD_Y + r * CELL
            pygame.draw.line(self.screen, WELL_LINE, (BOARD_X, y), (BOARD_X + BOARD_W, y))
        self.draw_frame((BOARD_X, BOARD_Y, BOARD_W, BOARD_H))

    def draw_stack(self):
        for r in range(ROWS):
            for c in range(COLS):
                name = self.board[r][c]
                if name is not None:
                    self.draw_cell(BOARD_X + c * CELL, BOARD_Y + r * CELL,
                                   PIECE_COLORS[name], CELL)

    def draw_ghost(self):
        drop = self.ghost_offset()
        if drop == 0:
            return
        color = PIECE_COLORS[self.piece.name]
        dim = (color[0] // 3, color[1] // 3, color[2] // 3)
        for x, y in self.piece.cells(y=self.piece.y + drop):
            if y < 0:
                continue
            rect = pygame.Rect(BOARD_X + x * CELL + CELL_INSET,
                               BOARD_Y + y * CELL + CELL_INSET,
                               CELL - 2 * CELL_INSET, CELL - 2 * CELL_INSET)
            pygame.draw.rect(self.screen, dim, rect, 2, border_radius=RADIUS_CELL)

    def draw_piece(self):
        color = PIECE_COLORS[self.piece.name]
        for x, y in self.piece.cells():
            if y < 0:
                continue   # the part still above the well stays hidden
            self.draw_cell(BOARD_X + x * CELL, BOARD_Y + y * CELL, color, CELL)

    def draw_preview(self, name, cx, cy):
        cells = SHAPES[name]
        xs = [c[0] for c in cells]
        ys = [c[1] for c in cells]
        w = (max(xs) - min(xs) + 1) * MINI
        h = (max(ys) - min(ys) + 1) * MINI
        ox = cx - w // 2 - min(xs) * MINI
        oy = cy - h // 2 - min(ys) * MINI
        for gx, gy in cells:
            self.draw_cell(ox + gx * MINI, oy + gy * MINI, PIECE_COLORS[name], MINI)

    def draw_panels(self):
        label = self.font_small.render("HOLD", True, GRAY_TEXT)
        self.screen.blit(label, HOLD_LABEL_POS)
        self.draw_frame(HOLD_BOX)
        if self.hold is not None:
            hx, hy, hw, hh = HOLD_BOX
            self.draw_preview(self.hold, hx + hw // 2, hy + hh // 2)

        label = self.font_small.render("NEXT", True, GRAY_TEXT)
        self.screen.blit(label, NEXT_LABEL_POS)
        self.draw_frame(NEXT_BOX)
        nx, ny, nw, _ = NEXT_BOX
        for i, name in enumerate(self.bag.peek(NEXT_COUNT)):
            self.draw_preview(name, nx + nw // 2, ny + NEXT_SLOT_H // 2 + i * NEXT_SLOT_H)

        for (caption, value), y in zip(
                (("LEVEL", self.level), ("LINES", self.lines)), STAT_ROW_Y):
            text = self.font_small.render(caption, True, GRAY_TEXT)
            self.screen.blit(text, (STAT_LABEL_X, y))
            val = self.font_small.render(str(value), True, WHITE)
            self.screen.blit(val, (STAT_VALUE_RIGHT - val.get_width(), y))

    def draw_hud(self):
        score_text = self.font_medium_bold.render(f"Score: {self.score}", True, WHITE)
        self.screen.blit(score_text, HUD_SCORE_POS)
        best_text = self.font_small.render(f"Best: {self.best}", True, GRAY_TEXT)
        self.screen.blit(best_text,
                         (WINDOW_WIDTH - best_text.get_width() - HUD_BEST_MARGIN,
                          HUD_BEST_Y))

    def draw_footer(self):
        self.draw_centered(
            self.font_tiny,
            "A D = Move    W = Rotate    SPACE = Hard drop    "
            "C = Hold    P = Pause    R = Restart    ESC = Back",
            FOOTER_TEXT, FOOTER_Y)

    def draw_playfield(self):
        self.screen.fill(DARK_BG)
        self.draw_well()
        self.draw_stack()
        if self.state == "playing":
            if self.settings.ghost:
                self.draw_ghost()
            self.draw_piece()
        self.draw_panels()
        self.draw_hud()
        self.draw_footer()

    def dim_screen(self):
        overlay = pygame.Surface((WINDOW_WIDTH, WINDOW_HEIGHT), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 150))
        self.screen.blit(overlay, (0, 0))

    def draw_menu(self):
        self.screen.fill(DARK_BG)
        self.draw_centered(self.font_title, "TETRIS", YELLOW, MENU_TITLE_Y)
        self.draw_centered(self.font_small, "Arrow keys or A / D to move",
                           WHITE, MENU_LINE_Y[0])
        self.draw_centered(self.font_small, "W rotates, SPACE hard-drops, C holds",
                           WHITE, MENU_LINE_Y[1])
        self.draw_centered(self.font_small, "Press SPACE or ENTER to start",
                           GREEN, MENU_LINE_Y[2])
        self.draw_centered(self.font_small, "Press S for settings",
                           WHITE, MENU_LINE_Y[3])
        self.draw_centered(self.font_small, "Press ESC to quit",
                           GRAY_TEXT, MENU_LINE_Y[4])

    def draw_settings(self):
        self.screen.fill(DARK_BG)
        self.draw_centered(self.font_large, "SETTINGS", YELLOW, SET_TITLE_Y)

        for i, row in enumerate(SETTING_ROWS):
            label = row[1]
            selected = (i == self.sel)
            y = SET_ROW_Y[i]

            if selected:
                marker = self.font_medium.render(">", True, YELLOW)
                self.screen.blit(marker, (SET_MARKER_X, y))

            text = self.font_medium.render(label, True, YELLOW if selected else WHITE)
            self.screen.blit(text, (SET_LABEL_X, y))

            value = self.font_medium.render(self.settings.display(i), True,
                                            YELLOW if selected else GRAY_TEXT)
            self.screen.blit(value, (SET_VALUE_X, y))

        self.draw_centered(self.font_small,
                           "UP / DOWN = select     LEFT / RIGHT = adjust",
                           WHITE, SET_HINT_Y[0])
        self.draw_centered(self.font_small,
                           "ESC = back to menu (saved automatically)",
                           GRAY_TEXT, SET_HINT_Y[1])

    def draw_paused(self):
        self.dim_screen()
        self.draw_centered(self.font_large, "PAUSED", YELLOW, PAUSE_TITLE_Y)
        self.draw_centered(self.font_small, "P or SPACE to resume  |  ESC for menu",
                           GREEN, PAUSE_HINT_Y)

    def draw_gameover(self):
        self.dim_screen()
        self.draw_centered(self.font_large, "GAME OVER", RED, OVER_TITLE_Y)
        self.draw_centered(self.font_medium, f"Score: {self.score}", WHITE, OVER_SCORE_Y)
        if self.score > self.prev_best and self.score > 0:
            self.draw_centered(self.font_medium_bold, "New record!", YELLOW, OVER_BEST_Y)
        self.draw_centered(self.font_small, "SPACE to play again  |  ESC for menu",
                           GREEN, OVER_HINT_Y)

    def draw(self):
        if self.state == "menu":
            self.draw_menu()
        elif self.state == "settings":
            self.draw_settings()
        else:
            self.draw_playfield()
            if self.state == "paused":
                self.draw_paused()
            elif self.state == "gameover":
                self.draw_gameover()

        pygame.display.flip()

    # --- input --------------------------------------------------------------

    def handle_input(self, event):
        if event.type != pygame.KEYDOWN:
            return

        if self.state == "menu":
            if event.key in (pygame.K_SPACE, pygame.K_RETURN):
                self.start_game()
            elif event.key == pygame.K_s:
                self.sel = 0
                self.state = "settings"
            elif event.key == pygame.K_ESCAPE:
                pygame.quit()
                sys.exit()

        elif self.state == "settings":
            if event.key in (pygame.K_UP, pygame.K_w):
                self.sel = (self.sel - 1) % len(SETTING_ROWS)
            elif event.key in (pygame.K_DOWN, pygame.K_s):
                self.sel = (self.sel + 1) % len(SETTING_ROWS)
            elif event.key in (pygame.K_LEFT, pygame.K_a):
                self.settings.adjust(self.sel, -1)
            elif event.key in (pygame.K_RIGHT, pygame.K_d):
                self.settings.adjust(self.sel, 1)
            elif event.key in (pygame.K_ESCAPE, pygame.K_RETURN, pygame.K_SPACE):
                self.settings.save()
                self.state = "menu"

        elif self.state == "playing":
            if event.key in (pygame.K_LEFT, pygame.K_a):
                self.move(-1)
            elif event.key in (pygame.K_RIGHT, pygame.K_d):
                self.move(1)
            elif event.key in (pygame.K_DOWN, pygame.K_s):
                self.soft_drop()
            elif event.key in (pygame.K_UP, pygame.K_w, pygame.K_x):
                self.rotate(1)
            elif event.key == pygame.K_z:
                self.rotate(-1)
            elif event.key == pygame.K_SPACE:
                self.hard_drop()
            elif event.key in (pygame.K_c, pygame.K_LSHIFT, pygame.K_RSHIFT):
                self.hold_piece()
            elif event.key == pygame.K_p:
                self.state = "paused"
            elif event.key == pygame.K_r:
                self.start_game()
            elif event.key == pygame.K_ESCAPE:
                self.state = "menu"

        elif self.state == "paused":
            if event.key in (pygame.K_p, pygame.K_SPACE, pygame.K_RETURN):
                self.state = "playing"
            elif event.key == pygame.K_ESCAPE:
                self.state = "menu"

        elif self.state == "gameover":
            if event.key in (pygame.K_SPACE, pygame.K_RETURN, pygame.K_r):
                self.start_game()
            elif event.key == pygame.K_ESCAPE:
                self.state = "menu"

    def handle_held_keys(self, dt):
        """Auto-repeat for left/right (DAS) and soft drop, from held key state."""
        if self.state != "playing":
            self.das_dir = 0
            self.soft_timer = 0.0
            return

        keys = pygame.key.get_pressed()
        left = keys[pygame.K_LEFT] or keys[pygame.K_a]
        right = keys[pygame.K_RIGHT] or keys[pygame.K_d]
        direction = -1 if (left and not right) else (1 if (right and not left) else 0)

        if direction == 0:
            self.das_dir = 0
            self.das_timer = 0.0
            self.das_charged = False
        elif direction != self.das_dir:
            # Fresh press: the KEYDOWN already moved it once, so just start charging.
            self.das_dir = direction
            self.das_timer = 0.0
            self.das_charged = False
        else:
            self.das_timer += dt
            threshold = DAS_RATE_MS if self.das_charged else DAS_DELAY_MS
            while self.das_timer >= threshold:
                self.das_timer -= threshold
                self.das_charged = True
                threshold = DAS_RATE_MS
                if not self.move(direction):
                    break

        if keys[pygame.K_DOWN] or keys[pygame.K_s]:
            self.soft_timer += dt
            while self.soft_timer >= SOFT_RATE_MS:
                self.soft_timer -= SOFT_RATE_MS
                if not self.soft_drop():
                    break
        else:
            self.soft_timer = 0.0

    def run(self):
        running = True
        while running:
            dt = self.clock.tick(60)

            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False
                self.handle_input(event)

            self.handle_held_keys(dt)
            self.update(dt)
            self.draw()

        pygame.quit()
        sys.exit()


if __name__ == "__main__":
    Game().run()
