@echo off
cd /d "%~dp0BlueMeter.WPF"
echo Building BlueMeter with new logging features...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)
echo.
echo Starting BlueMeter...
echo.
dotnet run -c Release
