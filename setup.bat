@echo off
REM BlueMeter Setup Script (Batch version for easier execution)
REM This script automatically installs .NET 8.0 SDK, builds, and runs BlueMeter

setlocal enabledelayedexpansion
cd /d "%~dp0"

echo.
echo ======================================
echo BlueMeter Setup ^& Launch
echo ======================================
echo.

REM Check if .NET SDK is installed
echo Checking .NET SDK installation...
where dotnet >nul 2>&1

if errorlevel 1 (
    echo .NET SDK not found. Installing .NET 8.0 SDK...
    echo Please wait, this may take a few minutes...
    echo.

    REM Download and execute .NET installer
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

    echo.
    echo .NET 8.0 SDK installed successfully!
    echo Adding .NET to PATH...

    REM Add .NET to PATH for current session
    set PATH=%ProgramFiles%\dotnet;!PATH!

    echo Refreshing environment...
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo .NET SDK found: !DOTNET_VERSION!
)

echo.
echo Building BlueMeter...
echo.

REM Navigate to WPF project and build
if not exist "BlueMeter.WPF" (
    echo Error: BlueMeter.WPF directory not found!
    echo Make sure you're running this script from the BlueMeter root directory.
    pause
    exit /b 1
)

cd BlueMeter.WPF

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean -c Release >nul 2>&1

REM Build the project
echo Building in Release mode... (this may take a minute)
dotnet build -c Release

if errorlevel 1 (
    echo.
    echo Build failed!
    echo Please check the errors above and try again.
    pause
    exit /b 1
)

echo.
echo ======================================
echo Build completed successfully!
echo ======================================
echo.
echo You can now use launcher.bat to start BlueMeter anytime.
echo.

cd ..

REM Ask user if they want to launch now
set /p LAUNCH="Launch BlueMeter now? (y/n): "
if /i "%LAUNCH%"=="y" (
    echo.
    echo Launching BlueMeter...
    echo.
    start "" "BlueMeter.WPF\bin\Release\net8.0-windows\BlueMeter.WPF.exe"
) else (
    echo.
    echo Setup complete! To launch BlueMeter later, double-click launcher.bat
    echo.
)

pause
