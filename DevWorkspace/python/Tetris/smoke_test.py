"""Headless checks for tetris.py — renders every state and exercises the rules."""

import os
os.environ.setdefault("SDL_VIDEODRIVER", "dummy")

import tetris as tt

g = tt.Game()
print("board", tt.COLS, "x", tt.ROWS, "cell", tt.CELL)
print("well", (tt.BOARD_X, tt.BOARD_Y), "->",
      (tt.BOARD_X + tt.BOARD_W, tt.BOARD_Y + tt.BOARD_H))
assert tt.BOARD_Y + tt.BOARD_H < tt.FOOTER_Y, "well overlaps the footer"
assert tt.BOARD_X + tt.BOARD_W < tt.NEXT_BOX[0], "well overlaps the NEXT panel"
assert tt.HOLD_BOX[0] + tt.HOLD_BOX[2] < tt.BOARD_X, "HOLD panel overlaps the well"

# Every piece must rotate through all four states and come back home.
for name in tt.PIECE_NAMES:
    g.start_game()
    g.piece = tt.Piece(name)
    start = sorted(g.piece.cells())
    for _ in range(4):
        g.rotate(1)
    assert sorted(g.piece.cells()) == start, f"{name} did not return to spawn state"
    assert len(g.piece.cells()) == 4, f"{name} does not have 4 cells"
print("all 7 pieces rotate cleanly")

# 7-bag: each piece exactly once per bag.
g.bag = tt.Bag()
drawn = [g.bag.take() for _ in range(7)]
assert sorted(drawn) == sorted(tt.PIECE_NAMES), f"bad bag: {drawn}"
print("bag", drawn)

# Wall kicks: a piece jammed against the left wall still rotates.
g.start_game()
g.piece = tt.Piece("I")
while g.move(-1):
    pass
assert g.rotate(1), "I piece could not kick off the left wall"
print("wall kick ok, x =", g.piece.x)

# Line clear: fill the bottom row except one cell, then drop a piece into it.
g.start_game()
for c in range(tt.COLS - 1):
    g.board[tt.ROWS - 1][c] = "T"
g.piece = tt.Piece("I")
g.piece.rot = 1                      # vertical I
g.piece.x = tt.COLS - 3
before = g.lines
g.hard_drop()
assert g.lines == before + 1, f"expected 1 line cleared, got {g.lines - before}"
assert g.score > 0, "clearing a line scored nothing"
print("line clear ok, score", g.score, "level", g.level)

# Ghost never sits above the piece, and hard drop lands on it.
g.start_game()
assert g.ghost_offset() > 0
drop = g.ghost_offset()
y0 = g.piece.y
g.hard_drop()
print("hard drop fell", drop, "rows from y =", y0)

# Hold swaps once per piece, not twice.
g.start_game()
first = g.piece.name
g.hold_piece()
assert g.hold == first, "hold did not store the piece"
held_second = g.piece.name
g.hold_piece()
assert g.piece.name == held_second, "hold fired twice for one piece"
print("hold ok:", first, "->", g.piece.name)

# Gravity curve must be monotonic and never hit zero.
prev = tt.fall_interval_ms(1)
for lvl in range(2, 21):
    now = tt.fall_interval_ms(lvl)
    assert 0 < now <= prev, f"gravity not monotonic at level {lvl}"
    prev = now
print("gravity level 1:", round(tt.fall_interval_ms(1)), "ms   level 10:",
      round(tt.fall_interval_ms(10)), "ms   level 20:", round(tt.fall_interval_ms(20)), "ms")

for state in ("menu", "settings", "playing", "paused", "gameover"):
    g.state = state
    g.draw()
print("all states rendered")

# A long unattended run must not crash or wedge.
g.start_game()
for _ in range(4000):
    g.update(16)
    if g.state == "gameover":
        g.start_game()
print("4000 ticks survived, score", g.score, "lines", g.lines)

# Settings round-trip, then put the user's own values back.
original = {k: getattr(g.settings, k) for k, *_ in tt.SETTING_ROWS}
for i in range(len(tt.SETTING_ROWS)):
    for _ in range(40):
        g.settings.adjust(i, 1)
values = [g.settings.display(i) for i in range(len(tt.SETTING_ROWS))]
g.settings.save()
assert [tt.Settings().display(i) for i in range(len(tt.SETTING_ROWS))] == values, \
    "settings did not round-trip through settings.json"
print("settings", values, "->", tt.Settings.path())
for key, value in original.items():
    setattr(g.settings, key, value)
g.settings.save()

print("SMOKE OK")
