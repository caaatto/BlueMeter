@echo off
REM BlueMeter Portable Release Creator
REM This script creates a fully portable, pre-built version that doesn't require building

setlocal enabledelayedexpansion
cd /d "%~dp0"

echo.
echo ======================================
echo BlueMeter Portable Release Creator
echo ======================================
echo.

REM Check if .NET SDK is installed
where dotnet >nul 2>&1
if errorlevel 1 (
    echo Error: .NET SDK not found!
    echo Please install .NET 8.0 SDK first.
    pause
    exit /b 1
)

REM Get version from Installer.iss
for /f "tokens=2 delims==" %%a in ('findstr /c:"#define MyAppVersion" setup\Installer.iss') do (
    set VERSION=%%a
    set VERSION=!VERSION:"=!
    set VERSION=!VERSION: =!
)

if "%VERSION%"=="" (
    echo Error: Could not determine version from setup\Installer.iss
    pause
    exit /b 1
)

echo Building version: %VERSION%
echo.

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean -c Release >nul 2>&1

REM Publish with self-contained .NET runtime
echo Publishing self-contained build...
dotnet publish BlueMeter.WPF/BlueMeter.WPF.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=false ^
    -p:DebugType=None ^
    -p:DebugSymbols=false ^
    -o "release-portable\BlueMeter-v%VERSION%-portable"

if errorlevel 1 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

REM Create portable launcher
echo Creating portable launcher...
(
echo @echo off
echo cd /d "%%~dp0"
echo start "" "BlueMeter.WPF.exe"
echo exit
) > "release-portable\BlueMeter-v%VERSION%-portable\BlueMeter.bat"

REM Copy README
if exist "README.md" (
    copy "README.md" "release-portable\BlueMeter-v%VERSION%-portable\README.md" >nul
)

REM Create portable-specific README
(
echo # BlueMeter Portable Edition
echo.
echo This is a pre-built, portable version of BlueMeter that does NOT require building.
echo.
echo ## Requirements
echo.
echo - Npcap ^(for packet capture^)
echo   Download from: https://npcap.com/#download
echo.
echo ## How to Run
echo.
echo 1. Make sure Npcap is installed
echo 2. Double-click `BlueMeter.bat` to launch
echo.
echo ## Running in Offline/VM Environment
echo.
echo This portable version works offline! Just:
echo 1. Install Npcap once ^(requires internet for download^)
echo 2. Copy this entire folder to your offline machine/VM
echo 3. Run `BlueMeter.bat`
echo.
echo All combat data stays local - no internet connection needed for operation.
echo.
echo ## Troubleshooting
echo.
echo If BlueMeter doesn't start:
echo 1. Check that Npcap is installed
echo 2. Try running `BlueMeter.WPF.exe` directly as administrator
echo 3. Check Windows Defender isn't blocking the application
echo.
echo For more information, see the main README.md
) > "release-portable\BlueMeter-v%VERSION%-portable\PORTABLE-README.txt"

REM Create ZIP
echo Creating ZIP archive...
powershell -NoProfile -Command "Compress-Archive -Path 'release-portable\BlueMeter-v%VERSION%-portable' -DestinationPath 'release-portable\BlueMeter-v%VERSION%-portable.zip' -Force"

if errorlevel 1 (
    echo Warning: Failed to create ZIP automatically
    echo You can manually ZIP the folder: release-portable\BlueMeter-v%VERSION%-portable
)

echo.
echo ======================================
echo Portable release created successfully!
echo ======================================
echo.
echo Location: release-portable\BlueMeter-v%VERSION%-portable
echo ZIP file: release-portable\BlueMeter-v%VERSION%-portable.zip
echo.
echo This version:
echo   - Includes .NET runtime ^(no separate installation needed^)
echo   - Works completely offline
echo   - Only requires Npcap
echo   - Ready to run with BlueMeter.bat
echo.
pause
