import os
os.environ["SDL_VIDEODRIVER"] = "dummy"
import sys
sys.path.insert(0, r"E:\Dev\python\Ran_San_Moi")
import snake_game as sg

g = sg.Game()
print("grid", sg.GRID_WIDTH, "x", sg.GRID_HEIGHT,
      "offset", sg.OFFSET_X, sg.OFFSET_Y,
      "play", sg.PLAY_WIDTH, sg.PLAY_HEIGHT)
assert sg.OFFSET_Y >= sg.HUD_HEIGHT, "playfield overlaps HUD"
assert sg.OFFSET_X >= 2 and sg.OFFSET_Y >= 2, "border clipped"
assert sg.OFFSET_Y + sg.PLAY_HEIGHT <= sg.WINDOW_HEIGHT

g.state = "playing"
g.snake.reset()
g.snake.change_direction(sg.UP)
g.snake.change_direction(sg.LEFT)
before = list(g.snake.body)
g.snake.move()
head = g.snake.body[0]
print("queued turns ->", head, "neck was", before[1])
assert head not in before[1:], "snake reversed into itself"

for state in ("menu", "playing", "gameover"):
    g.state = state
    g.draw()
print("all states rendered")

g.state = "playing"
g.snake.reset()
g.score = 0
for i in range(400):
    g.update()
    if g.state == "gameover":
        g.state = "playing"
        g.snake.reset()
print("score", g.score, "speed", g.speed, "len", len(g.snake.body))
assert g.speed <= sg.MAX_FPS
print("SMOKE OK")
