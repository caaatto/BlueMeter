# BlueMeter

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-brightgreen.svg)](https://www.gnu.org/licenses/agpl-3.0.txt)

This project is based on [StarResonanceDps](https://github.com/anying1073/StarResonanceDps), which implements the network packet capture technique originally developed by [StarResonanceDamageCounter](https://github.com/dmlgzs/StarResonanceDamageCounter). Many thanks to both authors for their contributions.

The tool does not require modifying the game client and does not violate the game's Terms of Service. It is intended to help players better understand combat data, avoid ineffective optimizations, and improve overall gameplay. Please do not use the results to justify power-level discrimination or any behavior that harms the community.

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

### First Time Setup
1. Go to the [Releases page](https://github.com/caaatto/BlueMeter/releases)
2. Download the latest `BlueMeter-vX.X.X-windows-x64.zip`
3. Extract the ZIP file
4. **Double-click `setup.bat`**

The setup script will:
- Automatically install .NET 8.0 SDK (if needed)
- Build BlueMeter
- Ask if you want to launch now

### Running BlueMeter
Once built, simply **double-click `launcher.bat`** to start BlueMeter anytime.

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
