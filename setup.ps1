# BlueMeter Setup Script
# This script automatically installs .NET 8.0 SDK, builds, and runs BlueMeter

param(
    [switch]$Force
)

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "BlueMeter Setup & Launch" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Set TLS version for web requests
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK installation..." -ForegroundColor Yellow

try {
    $dotnetVersion = & dotnet --version 2>$null
    if ($dotnetVersion) {
        Write-Host ".NET SDK found: $dotnetVersion" -ForegroundColor Green
    }
    else {
        throw "dotnet not found in PATH"
    }
}
catch {
    Write-Host ".NET SDK not found. Installing .NET 8.0 SDK..." -ForegroundColor Yellow
    Write-Host "Please wait, this may take a few minutes..." -ForegroundColor Yellow
    Write-Host ""

    # Download and install .NET 8.0 SDK
    $dotnetInstallerUrl = "https://dot.net/v1/dotnet-install.ps1"
    $dotnetInstallerPath = "$env:TEMP\dotnet-install.ps1"

    try {
        Write-Host "Downloading installer..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $dotnetInstallerUrl -OutFile $dotnetInstallerPath -UseBasicParsing

        Write-Host "Running installer..." -ForegroundColor Yellow
        & $dotnetInstallerPath -Channel 8.0 -InstallDir "$env:ProgramFiles\dotnet" -NoPath

        # Add .NET to PATH for current session
        $env:PATH = "$env:ProgramFiles\dotnet;$env:PATH"

        Write-Host ".NET 8.0 SDK installed successfully!" -ForegroundColor Green
        Write-Host "Added to PATH: $env:ProgramFiles\dotnet" -ForegroundColor Green
    }
    catch {
        Write-Host "Failed to install .NET SDK automatically." -ForegroundColor Red
        Write-Host "Please download and install .NET 8.0 SDK manually from:" -ForegroundColor Yellow
        Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Then run this script again." -ForegroundColor Yellow
        Write-Host "Error: $_" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Building BlueMeter..." -ForegroundColor Yellow
Write-Host ""

# Navigate to WPF project and build
$wpfPath = Join-Path $PSScriptRoot "BlueMeter.WPF"

if (-not (Test-Path $wpfPath)) {
    Write-Host "Error: BlueMeter.WPF directory not found!" -ForegroundColor Red
    Write-Host "Make sure you're running this script from the BlueMeter root directory." -ForegroundColor Red
    exit 1
}

try {
    Push-Location $wpfPath

    # Clean previous builds
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean -c Release 2>$null

    # Build the project
    Write-Host "Building in Release mode... (this may take a minute)" -ForegroundColor Yellow
    dotnet build -c Release

    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Build failed!" -ForegroundColor Red
        Write-Host "Please check the errors above and try again." -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "Build completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "Build error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Launching BlueMeter..." -ForegroundColor Yellow
Write-Host ""

try {
    dotnet run --no-build -c Release
}
catch {
    Write-Host ""
    Write-Host "Launch error: $_" -ForegroundColor Red
    exit 1
}
