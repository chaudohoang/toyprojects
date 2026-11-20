"""
🐍 Rắn Săn Mồi - Snake Game
By Vivian 💕
"""

import pygame
import random
import sys
import os

# --- Constants ---
WINDOW_WIDTH = 800
WINDOW_HEIGHT = 600
GRID_SIZE = 20
HUD_HEIGHT = 44
MARGIN = 12

# Playfield sits below the HUD band, so the score never overlaps the snake.
GRID_WIDTH = (WINDOW_WIDTH - 2 * MARGIN) // GRID_SIZE
GRID_HEIGHT = (WINDOW_HEIGHT - HUD_HEIGHT - 2 * MARGIN) // GRID_SIZE
PLAY_WIDTH = GRID_WIDTH * GRID_SIZE
PLAY_HEIGHT = GRID_HEIGHT * GRID_SIZE
OFFSET_X = (WINDOW_WIDTH - PLAY_WIDTH) // 2
OFFSET_Y = HUD_HEIGHT + (WINDOW_HEIGHT - HUD_HEIGHT - PLAY_HEIGHT) // 2

FPS = 12
MAX_FPS = 24

# Colors
BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
RED = (220, 50, 50)
GREEN = (50, 200, 50)
DARK_GREEN = (30, 150, 30)
GRAY = (40, 40, 40)
YELLOW = (255, 215, 0)
BLUE = (50, 150, 255)
DARK_BG = (15, 15, 25)

# Directions
UP = (0, -1)
DOWN = (0, 1)
LEFT = (-1, 0)
RIGHT = (1, 0)


def resource_path(relative_path):
    """Get absolute path to resource, works for dev and PyInstaller."""
    try:
        base_path = sys._MEIPASS
    except AttributeError:
        base_path = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(base_path, relative_path)


class Snake:
    def __init__(self):
        self.reset()

    def reset(self):
        center_x = GRID_WIDTH // 2
        center_y = GRID_HEIGHT // 2
        self.body = [
            (center_x, center_y),
            (center_x - 1, center_y),
            (center_x - 2, center_y),
        ]
        self.direction = RIGHT
        self.pending = []
        self.grow = False

    def move(self):
        # Apply at most one queued turn per step.
        if self.pending:
            self.direction = self.pending.pop(0)

        head_x, head_y = self.body[0]
        dir_x, dir_y = self.direction
        new_head = (head_x + dir_x, head_y + dir_y)

        self.body.insert(0, new_head)
        if not self.grow:
            self.body.pop()
        else:
            self.grow = False

    def change_direction(self, new_dir):
        # Queue turns, and validate each against the last queued one. Comparing
        # only against the current direction let two keys in a single frame
        # reverse the snake into its own neck.
        last = self.pending[-1] if self.pending else self.direction
        if new_dir == last:
            return
        if new_dir == (-last[0], -last[1]):
            return
        if len(self.pending) < 2:
            self.pending.append(new_dir)

    def check_collision(self):
        head = self.body[0]
        # Wall collision
        if head[0] < 0 or head[0] >= GRID_WIDTH:
            return True
        if head[1] < 0 or head[1] >= GRID_HEIGHT:
            return True
        # Self collision
        if head in self.body[1:]:
            return True
        return False

    def eat(self):
        self.grow = True


class Food:
    def __init__(self, snake_body):
        self.spawn(snake_body)

    def spawn(self, snake_body):
        while True:
            self.position = (
                random.randint(0, GRID_WIDTH - 1),
                random.randint(0, GRID_HEIGHT - 1),
            )
            if self.position not in snake_body:
                break

    def draw(self, surface, offset_x, offset_y):
        x = offset_x + self.position[0] * GRID_SIZE + GRID_SIZE // 2
        y = offset_y + self.position[1] * GRID_SIZE + GRID_SIZE // 2
        # Glow effect
        glow_surf = pygame.Surface((GRID_SIZE * 2, GRID_SIZE * 2), pygame.SRCALPHA)
        pygame.draw.circle(glow_surf, (255, 50, 50, 40), (GRID_SIZE, GRID_SIZE), GRID_SIZE)
        surface.blit(glow_surf, (x - GRID_SIZE, y - GRID_SIZE))
        # Apple
        pygame.draw.circle(surface, RED, (x, y), GRID_SIZE // 2 - 1)
        # Shine
        pygame.draw.circle(surface, (255, 120, 120), (x - 2, y - 3), 3)


class Game:
    def __init__(self):
        pygame.init()
        self.screen = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT))
        pygame.display.set_caption("🐍 Rắn Săn Mồi")
        self.clock = pygame.time.Clock()

        # Fonts
        self.font_large = pygame.font.SysFont("Segoe UI", 48, bold=True)
        self.font_medium = pygame.font.SysFont("Segoe UI", 28)
        self.font_small = pygame.font.SysFont("Segoe UI", 20)

        self.snake = Snake()
        self.food = Food(self.snake.body)
        self.score = 0
        self.high_score = 0
        self.prev_high = 0
        self.state = "menu"  # menu, playing, gameover
        self.speed = FPS

    def draw_grid(self, offset_x, offset_y):
        for x in range(GRID_WIDTH):
            for y in range(GRID_HEIGHT):
                rect = pygame.Rect(
                    offset_x + x * GRID_SIZE,
                    offset_y + y * GRID_SIZE,
                    GRID_SIZE,
                    GRID_SIZE,
                )
                if (x + y) % 2 == 0:
                    pygame.draw.rect(self.screen, (20, 20, 35), rect)
                else:
                    pygame.draw.rect(self.screen, (25, 25, 42), rect)

    def draw_snake(self, offset_x, offset_y):
        for i, (x, y) in enumerate(self.snake.body):
            rect = pygame.Rect(
                offset_x + x * GRID_SIZE + 1,
                offset_y + y * GRID_SIZE + 1,
                GRID_SIZE - 2,
                GRID_SIZE - 2,
            )
            if i == 0:
                # Head
                pygame.draw.rect(self.screen, YELLOW, rect, border_radius=6)
                # Eyes
                dir_x, dir_y = self.snake.direction
                eye_size = 3
                cx = offset_x + x * GRID_SIZE + GRID_SIZE // 2
                cy = offset_y + y * GRID_SIZE + GRID_SIZE // 2
                if self.snake.direction == RIGHT:
                    pygame.draw.circle(self.screen, BLACK, (cx + 4, cy - 4), eye_size)
                    pygame.draw.circle(self.screen, BLACK, (cx + 4, cy + 4), eye_size)
                elif self.snake.direction == LEFT:
                    pygame.draw.circle(self.screen, BLACK, (cx - 4, cy - 4), eye_size)
                    pygame.draw.circle(self.screen, BLACK, (cx - 4, cy + 4), eye_size)
                elif self.snake.direction == UP:
                    pygame.draw.circle(self.screen, BLACK, (cx - 4, cy - 4), eye_size)
                    pygame.draw.circle(self.screen, BLACK, (cx + 4, cy - 4), eye_size)
                elif self.snake.direction == DOWN:
                    pygame.draw.circle(self.screen, BLACK, (cx - 4, cy + 4), eye_size)
                    pygame.draw.circle(self.screen, BLACK, (cx + 4, cy + 4), eye_size)
            else:
                # Body
                t = i / len(self.snake.body)
                g = int(200 - t * 100)
                color = (50, g, 50)
                pygame.draw.rect(self.screen, color, rect, border_radius=4)

    def draw_hud(self):
        # Score
        score_text = self.font_medium.render(f"Score: {self.score}", True, WHITE)
        self.screen.blit(score_text, (15, 10))

        # High score
        hs_text = self.font_small.render(f"Best: {self.high_score}", True, (150, 150, 150))
        self.screen.blit(hs_text, (WINDOW_WIDTH - hs_text.get_width() - 15, 15))

    def draw_menu(self):
        self.screen.fill(DARK_BG)

        # Title
        title = self.font_large.render("Rắn Săn Mồi", True, YELLOW)
        self.screen.blit(title, (WINDOW_WIDTH // 2 - title.get_width() // 2, 180))

        # Instructions
        lines = [
            "Sử dụng phím mũi tên hoặc WASD để điều khiển",
            "Ăn táo đỏ để điểm cao!",
            "",
            "Nhấn SPACE hoặc ENTER để bắt đầu",
            "Nhấn ESC để thoát",
        ]
        for i, line in enumerate(lines):
            color = GREEN if "SPACE" in line or "ENTER" in line else WHITE
            text = self.font_small.render(line, True, color)
            self.screen.blit(text, (WINDOW_WIDTH // 2 - text.get_width() // 2, 300 + i * 35))

    def draw_gameover(self):
        # Dim overlay
        overlay = pygame.Surface((WINDOW_WIDTH, WINDOW_HEIGHT), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, 150))
        self.screen.blit(overlay, (0, 0))

        # Game Over text
        go_text = self.font_large.render("GAME OVER", True, RED)
        self.screen.blit(go_text, (WINDOW_WIDTH // 2 - go_text.get_width() // 2, 180))

        # Score
        score_text = self.font_medium.render(f"Điểm: {self.score}", True, WHITE)
        self.screen.blit(score_text, (WINDOW_WIDTH // 2 - score_text.get_width() // 2, 260))

        # High score
        if self.score > self.prev_high and self.score > 0:
            new_best = self.font_medium.render("Kỷ lục mới!", True, YELLOW)
            self.screen.blit(new_best, (WINDOW_WIDTH // 2 - new_best.get_width() // 2, 310))

        # Restart
        restart = self.font_small.render("Nhấn SPACE để chơi lại | ESC để thoát", True, GREEN)
        self.screen.blit(restart, (WINDOW_WIDTH // 2 - restart.get_width() // 2, 400))

    def handle_input(self, event):
        if event.type == pygame.KEYDOWN:
            if self.state == "menu":
                if event.key in (pygame.K_SPACE, pygame.K_RETURN):
                    self.state = "playing"
                    self.snake.reset()
                    self.food = Food(self.snake.body)
                    self.score = 0
                    self.speed = FPS
                elif event.key == pygame.K_ESCAPE:
                    pygame.quit()
                    sys.exit()

            elif self.state == "playing":
                key_map = {
                    pygame.K_UP: UP,
                    pygame.K_w: UP,
                    pygame.K_DOWN: DOWN,
                    pygame.K_s: DOWN,
                    pygame.K_LEFT: LEFT,
                    pygame.K_a: LEFT,
                    pygame.K_RIGHT: RIGHT,
                    pygame.K_d: RIGHT,
                }
                if event.key in key_map:
                    self.snake.change_direction(key_map[event.key])
                elif event.key == pygame.K_ESCAPE:
                    self.state = "menu"

            elif self.state == "gameover":
                if event.key in (pygame.K_SPACE, pygame.K_RETURN):
                    self.state = "playing"
                    self.snake.reset()
                    self.food = Food(self.snake.body)
                    self.score = 0
                    self.speed = FPS
                elif event.key == pygame.K_ESCAPE:
                    self.state = "menu"

    def update(self):
        if self.state != "playing":
            return

        self.snake.move()

        # Check eat
        if self.snake.body[0] == self.food.position:
            self.snake.eat()
            self.food.spawn(self.snake.body)
            self.score += 10
            # Speed up every 5 apples, independent of points-per-apple
            self.speed = min(FPS + len(self.snake.body) // 5, MAX_FPS)

        # Check death
        if self.snake.check_collision():
            self.prev_high = self.high_score
            self.high_score = max(self.high_score, self.score)
            self.state = "gameover"

    def draw(self):
        if self.state == "menu":
            self.draw_menu()
        elif self.state == "playing":
            self.screen.fill(DARK_BG)
            offset_x, offset_y = OFFSET_X, OFFSET_Y

            # Border
            border_rect = pygame.Rect(offset_x - 2, offset_y - 2,
                                       GRID_WIDTH * GRID_SIZE + 4,
                                       GRID_HEIGHT * GRID_SIZE + 4)
            pygame.draw.rect(self.screen, (60, 60, 80), border_rect, 2, border_radius=4)

            self.draw_grid(offset_x, offset_y)
            self.food.draw(self.screen, offset_x, offset_y)
            self.draw_snake(offset_x, offset_y)
            self.draw_hud()

        elif self.state == "gameover":
            self.screen.fill(DARK_BG)
            offset_x, offset_y = OFFSET_X, OFFSET_Y
            self.draw_grid(offset_x, offset_y)
            self.food.draw(self.screen, offset_x, offset_y)
            self.draw_snake(offset_x, offset_y)
            self.draw_hud()
            self.draw_gameover()

        pygame.display.flip()

    def run(self):
        running = True
        while running:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False
                self.handle_input(event)

            self.update()
            self.draw()
            self.clock.tick(self.speed if self.state == "playing" else 30)

        pygame.quit()
        sys.exit()


if __name__ == "__main__":
    game = Game()
    game.run()
