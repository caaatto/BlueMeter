@echo off
REM BlueMeter Installer
REM This script installs BlueMeter to Program Files with Administrator privileges

setlocal enabledelayedexpansion

REM Check if running as Administrator
openfiles >nul 2>&1
if errorlevel 1 (
    echo.
    echo ======================================
    echo BlueMeter Installer
    echo ======================================
    echo.
    echo This installer requires Administrator privileges.
    echo Requesting elevation...
    echo.

    REM Re-run the script with Administrator privileges
    powershell -Command "Start-Process cmd.exe -ArgumentList '/c \"%~f0\"' -Verb RunAs"
    exit /b 0
)

cd /d "%~dp0"

echo.
echo ======================================
echo BlueMeter Installer
echo ======================================
echo.

REM Run PowerShell installer
powershell -NoProfile -ExecutionPolicy Bypass -File "installer.ps1"

if errorlevel 1 (
    echo.
    echo Installation failed!
    pause
    exit /b 1
)
