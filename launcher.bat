@echo off
REM BlueMeter Launcher
REM Simple launcher to start BlueMeter after it's been built

setlocal enabledelayedexpansion
cd /d "%~dp0"

REM Check if EXE exists
set EXE_PATH=%CD%\BlueMeter.WPF\bin\Release\net8.0-windows\BlueMeter.WPF.exe

if not exist "%EXE_PATH%" (
    echo.
    echo ======================================
    echo BlueMeter Launcher
    echo ======================================
    echo.
    echo Error: BlueMeter has not been built yet!
    echo.
    echo Please run setup.bat first to build BlueMeter.
    echo.
    pause
    exit /b 1
)

REM Start BlueMeter without waiting
start "" "%EXE_PATH%"

REM Close this window
exit /b 0
