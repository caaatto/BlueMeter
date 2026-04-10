# BlueMeter

**A powerful DPS Meter and Combat Analytics Tool for Blue Protocol**

Track your damage per second (DPS), analyze combat performance, monitor tank mitigation stats, and get queue pop alerts. Features real-time DPS tracking, detailed skill breakdowns, combat history replay, and comprehensive statistics for optimizing your gameplay.

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-brightgreen.svg)](https://www.gnu.org/licenses/agpl-3.0.txt)

**Keywords:** DPS Meter, Blue Protocol DPS, Combat Tracker, Damage Meter, DPS Parser, Combat Analytics, Tank Stats, Mitigation Tracker, Performance Monitor

---

This project is based on [StarResonanceDps](https://github.com/anying1073/StarResonanceDps), which implements the network packet capture technique originally developed by [StarResonanceDamageCounter](https://github.com/dmlgzs/StarResonanceDamageCounter). Many thanks to both authors for their contributions.

The tool does not require modifying the game client and does not violate the game's Terms of Service. It is intended to help players better understand combat data, avoid ineffective optimizations, and improve overall gameplay. Please do not use the results to justify power-level discrimination or any behavior that harms the community.

## 📋 Changelog

For detailed patch notes and release information, see [PATCHNOTES.md](PATCHNOTES.md).

**Latest Version: 1.5.6**
- **CRITICAL FIX:** Eliminated race condition crashes in 20-man raids
- **DPS Refresh Rate Settings** - Customizable update frequency (10/20/30/60 FPS) to eliminate in-game lag
- Performance optimizations - Thread-safe locking, non-blocking UI sorting, improved batch processing
- Fixed ScopeTime toggle and reduced boss death delay for faster fight archiving

For full changelog history, see [PATCHNOTES.md](PATCHNOTES.md) or detailed release notes in the `/docs/` directory.

## ✨ Features

- **Real-time DPS Tracking** - See your damage per second live during combat
- **Detailed Skill Stats** - Check which skills are doing the most damage with full performance breakdown
- **Combat Replays** - Review past encounters and learn from your mistakes
- **Daily Checklists** - Manage daily/weekly tasks without alt-tabbing
- **Customizable UI** - Choose themes, set background images, and adjust transparency to play however you want
- **Overlay Mode** - Float the window above your game with F6 click-through and F7 always-on-top
- **Multiple Languages** - English and Chinese fully supported with instant switching
- **Local Storage Only** - All your combat data stays on your computer, zero online tracking

### Screenshots

<div align="center">

<img src="BlueMeter.Assets/Images/dpsMeter.png" alt="DPS Meter" width="200" />
<img src="BlueMeter.Assets/Images/menuView.png" alt="Menu" width="400" />
<img src="BlueMeter.Assets/Images/Settings.png" alt="Settings" width="300" />

**DPS Meter** • **Menu** • **Settings** *(fully customizable in Settings)*

</div>

## 🚀 Quick Start

### Installation Options

#### Option 1: Professional Installer (Recommended) ⭐
1. Go to the [Releases page](https://github.com/caaatto/BlueMeter/releases)
2. Download `BlueMeterSetup.exe`
3. **Right-click `BlueMeterSetup.exe` → "Run as administrator"**
4. If Windows SmartScreen appears, click "More info" → "Run anyway"
5. Follow the setup wizard
6. .NET 8.0 will be automatically installed if needed
7. BlueMeter will launch automatically after installation

⚠️ **Windows SmartScreen Warning**: The installer is not digitally signed. If Windows blocks it, right-click → Properties → check "Unblock" → Apply, then run as administrator.

#### Option 2: Portable Build (For Developers)
1. Go to the [Releases page](https://github.com/caaatto/BlueMeter/releases)
2. Download `BlueMeter-vX.X.X-windows-x64.zip`
3. Extract the ZIP file
4. **Double-click `setup.bat`** (NOT the .exe!)
5. The setup script will:
   - Automatically install .NET 8.0 SDK (if needed)
   - Build BlueMeter
   - Ask if you want to launch now

⚠️ **Important**: Do NOT run `BlueMeter.WPF.exe` directly on first launch. Windows SmartScreen may incorrectly flag it as suspicious. Always use the installer or `setup.bat` → `launcher.bat` instead.

⚠️ **Internet Required**: This version requires an internet connection during initial build to download NuGet packages. See Option 3 below for offline/VM usage.

#### Option 3: Pre-Built Portable (For Offline/VM Usage) 💻
**Perfect for running in VMs or offline environments!**

1. Go to the [Releases page](https://github.com/caaatto/BlueMeter/releases)
2. Download `BlueMeter-vX.X.X-portable.zip`
3. Extract the ZIP file
4. **Double-click `BlueMeter.bat`** to launch

✅ **Advantages**:
- No internet required (after downloading)
- No building needed - ready to run
- Includes .NET runtime (no separate installation)
- Perfect for isolated VMs or secure environments
- Only requires Npcap (one-time download)

⚠️ **Note**: You still need to install Npcap once. Download it from [npcap.com](https://npcap.com) on a machine with internet, then transfer the installer to your offline environment if needed.

### Running BlueMeter
Once installed, simply **double-click `launcher.bat`** (portable) or use the Start Menu shortcut (installer) to start BlueMeter anytime.

### Folder Structure
```
BlueMeter-vX.X.X-windows-x64/
├── setup.bat              (Run once to build)
├── launcher.bat           (Run anytime to launch)
├── BlueMeter.sln          (Solution file)
├── BlueMeter.WPF/         (Main application source)
├── BlueMeter.Core/        (Core logic source)
├── BlueMeter.Assets/      (Assets and fonts source)
└── lib/                   (All dependencies)
```

### Build from Source

#### Prerequisites
- .NET 8.0 SDK
- Internet connection (for NuGet package restore)

#### Build Steps
```bash
git clone https://github.com/caaatto/BlueMeter.git
cd BlueMeter
.\setup.bat
```

Or manually:
```bash
cd BlueMeter.WPF
dotnet build -c Release
dotnet run --no-build -c Release
```

For a publishable build:
```bash
dotnet publish BlueMeter.WPF/BlueMeter.WPF.csproj -c Release -o publish
```
The executable will be in the `publish` folder.

#### Creating a Pre-Built Portable Release
To create a portable version that doesn't require building (perfect for offline/VM environments):
```bash
.\create-portable-release.bat
```
This creates a self-contained package with embedded .NET runtime in `release-portable/`.

### Offline & VM Usage

If you want to run BlueMeter in an **offline VM or isolated environment** (e.g., for security/testing):

1. **On a machine with internet:**
   - Download `BlueMeter-vX.X.X-portable.zip` from [Releases](https://github.com/caaatto/BlueMeter/releases)
   - Download Npcap installer from [npcap.com](https://npcap.com/#download)

2. **Transfer to offline environment:**
   - Copy both files to your VM/offline machine

3. **Install Npcap:**
   - Run the Npcap installer once
   - Use WinPcap compatibility mode if asked
   - No admin-only mode required

4. **Run BlueMeter:**
   - Extract the portable ZIP
   - Double-click `BlueMeter.bat`
   - Works completely offline - no telemetry or online requirements

✅ **Perfect for security testing** - BlueMeter makes no outbound connections and stores all data locally.

## 📋 System Requirements

BlueMeter requires two dependencies to function properly. The installer will automatically handle both for you:

### .NET 8.0 Desktop Runtime
**Why it's needed**: BlueMeter is built as a WPF (Windows Presentation Foundation) application using .NET 8.0. The Desktop Runtime provides the core framework and UI libraries needed to run the application.

**Installation**: The installer automatically downloads and installs .NET 8.0 Desktop Runtime if it's not already present on your system.

### Npcap (Network Packet Capture Driver)
**Why it's needed**: BlueMeter works by capturing and analyzing network packets sent between your game client and the server. This packet capture approach is important because:

- **No game modification**: BlueMeter doesn't inject code, modify game files, or hook into the game process
- **Terms of Service compliant**: Reading network traffic is a passive observation technique that doesn't alter gameplay
- **Real-time accuracy**: Packet analysis provides precise combat statistics directly from the server data

**What happens with your data**:
- ✅ **100% Local Storage**: All captured data and combat statistics are stored only on your computer
- ✅ **Zero Telemetry**: BlueMeter does not send any data to external servers or third parties
- ✅ **Your Privacy Matters**: No tracking, no analytics, no data collection - everything stays on your machine
- ✅ **Offline Capable**: Works completely offline after installation (though you need to be online to play the game anyway)

**Installation**: The installer will launch the Npcap setup wizard if Npcap is not detected on your system. Simply follow the wizard to complete the installation. If you skip Npcap installation, BlueMeter will remind you to install it when you first launch the app.

**Download manually**: If needed, you can download Npcap from [https://npcap.com](https://npcap.com)

**Note**: Without Npcap, BlueMeter cannot capture network traffic and will not function.

## 📄 License

[![AGPLv3](https://www.gnu.org/graphics/agplv3-with-text-162x68.png)](LICENSE.txt)

This project is licensed under the [GNU Affero General Public License v3](LICENSE.txt).

By using this project, you agree to comply with the terms of the license.

I do not welcome individuals or projects that disregard this license, including those who modify or translate open-source code and redistribute it as closed-source, or who mirror open-source updates into closed-source derivatives.

## 👥 Contributing

Issues and pull requests to improve the project are welcome!

## ⭐ Support

If this project helps you, please consider giving it a star ⭐

---

**Disclaimer**: This tool is for learning and analysis of game data only. Do not use it in ways that violate the game's Terms of Service. You are solely responsible for any risks incurred. I am not responsible for any misuse of this tool to discriminate against other players. Please adhere to the rules and ethical standards of the game's community.

---

## 💙 Thank You!

<div align="center">

**Thanks for checking out BlueMeter!** Give it a try and let me know what you think. Every test, feedback, and bug report helps make this tool better for everyone!

<img src="BlueMeter.WPF/Assets/Images/cutedog.png" alt="Cute dog mascot" width="400" />

### Join the Community

Have questions, suggestions, or just want to hang out and talk about the game? **Come say hi!** 👋

**Issues & Feedback**: [Open an issue on GitHub](https://github.com/caaatto/BlueMeter/issues)

**Contribute**: Help improve BlueMeter - all contributions welcome!

**Happy tracking!** ✨

</div>
