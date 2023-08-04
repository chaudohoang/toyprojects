@echo OFF
for %%B in (%~dp0\.) do set c=%%~dpB
echo %c%
pause