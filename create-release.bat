@echo off
REM Create Release Package for BlueMeter
REM This script creates a zip file with all necessary files for distribution

setlocal enabledelayedexpansion
cd /d "%~dp0"

set VERSION=1.2.1
set RELEASE_DIR=BlueMeter-v%VERSION%-Release
set ZIP_NAME=BlueMeter-v%VERSION%.zip

echo.
echo ======================================
echo Creating Release Package v%VERSION%
echo ======================================
echo.

REM Check if Release build exists
if not exist "BlueMeter.WPF\bin\Release\net8.0-windows\BlueMeter.WPF.exe" (
    echo Error: Release build not found!
    echo Please run build-installer.bat first.
    pause
    exit /b 1
)

REM Create temporary release directory
echo Creating release directory...
if exist %RELEASE_DIR% (
    rmdir /s /q %RELEASE_DIR%
)
mkdir %RELEASE_DIR%

REM Copy application files
echo Copying application files...
xcopy "BlueMeter.WPF\bin\Release\net8.0-windows\*.*" "%RELEASE_DIR%\" /E /I /Y >nul

REM Copy installer scripts
echo Copying installer scripts...
copy setup.bat "%RELEASE_DIR%\" >nul
copy setup.ps1 "%RELEASE_DIR%\" >nul
copy Install.bat "%RELEASE_DIR%\" >nul
copy installer.ps1 "%RELEASE_DIR%\" >nul
copy INSTALL.md "%RELEASE_DIR%\" >nul
copy README.md "%RELEASE_DIR%\" >nul

REM Create README for release
(
    echo BlueMeter v%VERSION% Release Package
    echo.
    echo INSTALLATION INSTRUCTIONS:
    echo.
    echo Option 1 - Setup Script (Recommended for source builds^):
    echo   - Double-click: setup.bat
    echo.
    echo Option 2 - Installation Script (Recommended for distributions^):
    echo   - Double-click: Install.bat
    echo   - This will install BlueMeter to: C:\Program Files\BlueMeter
    echo   - Creates Start Menu and Desktop shortcuts
    echo.
    echo REQUIREMENTS:
    echo   - Windows 10 or later
    echo   - .NET 8.0 Runtime (scripts will offer to install if needed^)
    echo.
    echo For more information, see INSTALL.md
) > "%RELEASE_DIR%\START_HERE.txt"

REM Create ZIP file
echo Creating ZIP file...
powershell -NoProfile -Command "Compress-Archive -Path '%RELEASE_DIR%' -DestinationPath '%ZIP_NAME%' -Force"

if errorlevel 1 (
    echo.
    echo Failed to create ZIP file!
    pause
    exit /b 1
)

REM Clean up temporary directory
rmdir /s /q %RELEASE_DIR%

echo.
echo ======================================
echo Release package created successfully!
echo File: %ZIP_NAME%
echo ======================================
echo.

pause
