# BlueMeter Installation Guide

## Quick Start (Recommended)

### Option A: Installer (Recommended - Windows only)

If you have the `BlueMeter-Setup-1.2.1.exe` installer:

1. **Download** the installer from [Releases](https://github.com/caaatto/BlueMeter/releases)
2. **Run** the `.exe` installer
3. **Follow** the installation wizard
4. **Launch** BlueMeter from Start Menu or Desktop shortcut

**Advantages:**
- No command prompt or configuration needed
- Professional Windows installer experience
- Automatic .NET 8.0 runtime detection
- Start Menu and Desktop shortcuts
- Easy uninstall via Add/Remove Programs

### Option B: Setup Script

If you have the source files or want to build from scratch:

Simply run the setup script from the extracted folder:

```
Double-click: setup.bat
```

Or via PowerShell:

```powershell
.\setup.ps1
```

**That's it!** The script will:
1. Check if .NET 8.0 SDK is installed (install it if needed)
2. Build the BlueMeter application
3. Launch BlueMeter automatically

## Manual Installation

If the automated setup doesn't work, follow these steps:

### Prerequisites
- **Operating System:** Windows 10 or later
- **.NET 8.0 SDK:** Download from https://dotnet.microsoft.com/download/dotnet/8.0

### Installation Steps

1. **Extract the ZIP file** to a folder of your choice

2. **Open Command Prompt or PowerShell** in the extracted folder

3. **Navigate to the WPF project:**
   ```bash
   cd BlueMeter.WPF
   ```

4. **Build the project:**
   ```bash
   dotnet build -c Release
   ```

5. **Run BlueMeter:**
   ```bash
   dotnet run --no-build -c Release
   ```

## Troubleshooting

### "dotnet: command not found"
- **.NET SDK is not installed** - Download and install it from https://dotnet.microsoft.com/download/dotnet/8.0
- **After installation**, restart PowerShell/Command Prompt and try again

### Build fails with "project not found"
- Make sure you're in the correct directory (BlueMeter.WPF folder)
- Check that the `.sln` file exists in the parent directory

### Application won't start
- Ensure you have .NET 8.0 SDK installed (not just Runtime)
- Try running from the Release build folder: `BlueMeter.WPF/bin/Release/net8.0-windows/`

### Port already in use
- Another instance of BlueMeter might be running
- Close other BlueMeter windows and try again

## System Requirements

- **OS:** Windows 10 or later (x64)
- **.NET:** .NET 8.0 Runtime (automatically installed with SDK)
- **RAM:** 2GB minimum
- **Disk Space:** 500MB for .NET SDK + 200MB for BlueMeter

## Getting Help

If you encounter issues:
1. Check the log files in `BlueMeter.WPF/bin/Release/net8.0-windows/logs/`
2. Open an issue on GitHub: https://github.com/caaatto/BlueMeter/issues
3. Join the Discord community for support

## Building from Source (Advanced)

Clone the repository and build:
```bash
git clone https://github.com/caaatto/BlueMeter.git
cd BlueMeter
.\setup.bat
```

Enjoy BlueMeter! 🎮
