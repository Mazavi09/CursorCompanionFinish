@echo off
echo === ПРОВЕРКА ФОНОВ ===
echo.

set BG_PATH=Backgrounds

echo Содержимое папки Backgrounds:
dir "%BG_PATH%\*.png" /b

echo.
echo Детальная информация:
for %%f in ("%BG_PATH%\*.png") do (
    echo Файл: %%~nxf
    echo   Размер: %%~zf байт
    echo   Дата: %%~tf
    echo.
)

echo.
echo Проверка имен файлов:
if exist "%BG_PATH%\lizard_bg.png" echo ✅ lizard_bg.png существует
if exist "%BG_PATH%\rat_bg.png" echo ✅ rat_bg.png существует
if exist "%BG_PATH%\snake_bg.png" echo ✅ snake_bg.png существует
if exist "%BG_PATH%\axolotl_bg.png" echo ✅ axolotl_bg.png существует
if exist "%BG_PATH%\cat_bg.png" echo ✅ cat_bg.png существует
if exist "%BG_PATH%\ferret_bg.png" echo ✅ ferret_bg.png существует
if exist "%BG_PATH%\dragon_bg.png" echo ✅ dragon_bg.png существует

echo.
pause