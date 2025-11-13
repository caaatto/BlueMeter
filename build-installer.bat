@echo off
REM BlueMeter Installer Build Script
REM This script builds the application and creates an NSIS installer

setlocal enabledelayedexpansion
cd /d "%~dp0"

echo.
echo ======================================
echo BlueMeter Installer Builder
echo ======================================
echo.

REM Check if .NET SDK is installed
echo Checking .NET SDK installation...
where dotnet >nul 2>&1

if errorlevel 1 (
    echo .NET SDK not found. Installing .NET 8.0 SDK...
    echo Please wait, this may take a few minutes...
    echo.

    powershell -NoProfile -ExecutionPolicy Bypass -Command ^
        "[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12; ^
         Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile '$env:TEMP\dotnet-install.ps1'; ^
         & '$env:TEMP\dotnet-install.ps1' -Channel 8.0 -InstallDir '$env:ProgramFiles\dotnet' -NoPath"

    if errorlevel 1 (
        echo.
        echo Failed to install .NET SDK automatically.
        echo Please download and install .NET 8.0 SDK manually from:
        echo https://dotnet.microsoft.com/download/dotnet/8.0
        echo.
        echo Then run this script again.
        pause
        exit /b 1
    )

    set PATH=%ProgramFiles%\dotnet;!PATH!
    echo .NET 8.0 SDK installed successfully!
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo .NET SDK found: !DOTNET_VERSION!
)

echo.
echo Building BlueMeter Release...
echo.

if not exist "BlueMeter.WPF" (
    echo Error: BlueMeter.WPF directory not found!
    echo Make sure you're running this script from the BlueMeter root directory.
    pause
    exit /b 1
)

cd BlueMeter.WPF

echo Cleaning previous builds...
dotnet clean -c Release >nul 2>&1

echo Building in Release mode... (this may take a minute)
dotnet build -c Release

if errorlevel 1 (
    echo.
    echo Build failed!
    echo Please check the errors above and try again.
    cd ..
    pause
    exit /b 1
)

echo Build completed successfully!

cd ..

echo.
echo Checking for NSIS installation...
where makensis >nul 2>&1

if errorlevel 1 (
    echo.
    echo NSIS not found on this system.
    echo.
    echo NSIS is required to build the installer. You can:
    echo.
    echo 1. Download NSIS from: https://nsis.sourceforge.io/
    echo 2. Install NSIS
    echo 3. Run this script again
    echo.
    echo OR
    echo.
    echo You can manually create the installer by:
    echo 1. Copying the 'BlueMeter.WPF\bin\Release\net8.0-windows' folder
    echo 2. Using the installer.nsi file with NSIS
    echo.
    pause
    exit /b 1
)

echo NSIS found! Building installer...
echo.

makensis installer.nsi

if errorlevel 1 (
    echo.
    echo Installer build failed!
    pause
    exit /b 1
)

echo.
echo ======================================
echo Installer created successfully!
echo File: BlueMeter-Setup-1.2.1.exe
echo ======================================
echo.

pause
