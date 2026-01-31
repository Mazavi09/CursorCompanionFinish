@echo off
echo Копирование файлов в bin\Debug...
xcopy /E /Y "Backgrounds" "bin\Debug\Backgrounds\"
xcopy /E /Y "Skins" "bin\Debug\Skins\"
if not exist "bin\Debug\Data" mkdir "bin\Debug\Data"
if not exist "bin\Debug\Logs" mkdir "bin\Debug\Logs"
echo Готово!
pause