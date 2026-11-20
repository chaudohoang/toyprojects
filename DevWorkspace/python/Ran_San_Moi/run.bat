@echo off
cd /d "%~dp0"
python -c "import pygame" 2>nul || python -m pip install "pygame-ce>=2.5.0" -q
python snake_game.py
if errorlevel 1 pause
