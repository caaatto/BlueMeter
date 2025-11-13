# BlueMeter Professional Installer Script
# This script serves as a professional installer without requiring NSIS

param(
    [switch]$Silent
)

$ErrorActionPreference = 'Stop'

# Configuration
$AppName = "BlueMeter"
$Version = "1.2.1"
$InstallDir = "C:\Program Files\BlueMeter"
$StartMenuDir = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\BlueMeter"

if (-not $Silent) {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "$AppName Installer v$Version" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Installation directory: $InstallDir" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to continue or Ctrl+C to cancel"
}

try {
    # Check for Administrator privileges
    $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object System.Security.Principal.WindowsPrincipal($currentUser)
    if (-not $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "This installer requires Administrator privileges. Please run as Administrator."
    }

    # Create installation directory
    Write-Host "Creating installation directory..." -ForegroundColor Yellow
    if (Test-Path $InstallDir) {
        try {
            Remove-Item $InstallDir -Recurse -Force
        }
        catch {
            Write-Host "Warning: Could not remove existing installation. Will overwrite files." -ForegroundColor Yellow
        }
    }
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null

    # Copy application files
    Write-Host "Copying application files..." -ForegroundColor Yellow
    $sourceDir = Join-Path $PSScriptRoot "BlueMeter.WPF\bin\Release\net8.0-windows"

    if (-not (Test-Path $sourceDir)) {
        throw "Release build not found. Run build-installer.bat first."
    }

    Copy-Item -Path "$sourceDir\*" -Destination $InstallDir -Recurse -Force

    # Create Start Menu shortcut
    Write-Host "Creating Start Menu shortcuts..." -ForegroundColor Yellow
    if (-not (Test-Path $StartMenuDir)) {
        New-Item -ItemType Directory -Path $StartMenuDir -Force | Out-Null
    }

    $WshShell = New-Object -ComObject WScript.Shell

    # Main application shortcut
    $shortcut = $WshShell.CreateShortcut("$StartMenuDir\BlueMeter.lnk")
    $shortcut.TargetPath = "$InstallDir\BlueMeter.WPF.exe"
    $shortcut.WorkingDirectory = $InstallDir
    $shortcut.IconLocation = "$InstallDir\BlueMeter.WPF.exe"
    $shortcut.Save()

    # Uninstall shortcut
    $uninstallScript = Join-Path $InstallDir "uninstall.ps1"

@"
# BlueMeter Uninstaller
\$InstallDir = '$InstallDir'
\$StartMenuDir = '$StartMenuDir'

Write-Host 'Removing BlueMeter...' -ForegroundColor Yellow

Remove-Item \$StartMenuDir -Recurse -ErrorAction SilentlyContinue
Remove-Item \$InstallDir -Recurse -ErrorAction SilentlyContinue

Write-Host 'BlueMeter uninstalled successfully!' -ForegroundColor Green
"@ | Set-Content $uninstallScript

    $uninstallShortcut = $WshShell.CreateShortcut("$StartMenuDir\Uninstall BlueMeter.lnk")
    $uninstallShortcut.TargetPath = "powershell.exe"
    $uninstallShortcut.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$uninstallScript`""
    $uninstallShortcut.WorkingDirectory = $InstallDir
    $uninstallShortcut.Save()

    # Create Desktop shortcut
    Write-Host "Creating Desktop shortcut..." -ForegroundColor Yellow
    $desktopShortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\BlueMeter.lnk")
    $desktopShortcut.TargetPath = "$InstallDir\BlueMeter.WPF.exe"
    $desktopShortcut.WorkingDirectory = $InstallDir
    $desktopShortcut.IconLocation = "$InstallDir\BlueMeter.WPF.exe"
    $desktopShortcut.Save()

    # Registry entry for Add/Remove Programs
    Write-Host "Registering in Add/Remove Programs..." -ForegroundColor Yellow
    $regPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\BlueMeter"
    if (-not (Test-Path $regPath)) {
        New-Item -Path $regPath -Force | Out-Null
    }

    Set-ItemProperty -Path $regPath -Name "DisplayName" -Value "BlueMeter"
    Set-ItemProperty -Path $regPath -Name "DisplayVersion" -Value $Version
    Set-ItemProperty -Path $regPath -Name "UninstallString" -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$uninstallScript`""
    Set-ItemProperty -Path $regPath -Name "InstallLocation" -Value $InstallDir

    if (-not $Silent) {
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "Installation Successful!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "BlueMeter has been installed to:" -ForegroundColor Yellow
        Write-Host $InstallDir -ForegroundColor Cyan
        Write-Host ""
        Write-Host "You can find BlueMeter in:" -ForegroundColor Yellow
        Write-Host "- Start Menu: BlueMeter" -ForegroundColor Cyan
        Write-Host "- Desktop: BlueMeter shortcut" -ForegroundColor Cyan
        Write-Host ""

        Read-Host "Press Enter to exit"
    }
}
catch {
    Write-Host ""
    Write-Host "Installation failed!" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
