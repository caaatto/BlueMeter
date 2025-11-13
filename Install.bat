@echo off
REM BlueMeter Installer
REM This script installs BlueMeter to Program Files

setlocal enabledelayedexpansion
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
