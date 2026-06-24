@echo off
echo ================================
echo  Беларусь: Знай свой край
echo  Запуск СЕРВЕРА...
echo ================================
echo.
cd /d "%~dp0BelarusQuiz.Server"
dotnet run
pause
