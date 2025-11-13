# BlueMeter

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-brightgreen.svg)](https://www.gnu.org/licenses/agpl-3.0.txt)

This project is based on [StarResonanceDps](https://github.com/anying1073/StarResonanceDps), which implements the network packet capture technique originally developed by [StarResonanceDamageCounter](https://github.com/dmlgzs/StarResonanceDamageCounter). Many thanks to both authors for their contributions.

The tool does not require modifying the game client and does not violate the game's Terms of Service. It is intended to help players better understand combat data, avoid ineffective optimizations, and improve overall gameplay. Please do not use the results to justify power-level discrimination or any behavior that harms the community.

## 🚀 Quick Start

### Option 1: Installer (Recommended)

1. Go to the [Releases page](https://github.com/caaatto/BlueMeter/releases)
2. Download `BlueMeter-Setup-X.X.X.exe`
3. Run the installer and follow the wizard
4. Launch BlueMeter from Start Menu or Desktop

**No .NET installation or command line needed!**

### Option 2: Setup Script

1. Go to the [Releases page](https://github.com/caaatto/BlueMeter/releases)
2. Download the `BlueMeter-vX.X.X.zip` file
3. Extract the ZIP file
4. **Double-click `setup.bat`**

The setup script will:
- Automatically check and install .NET 8.0 SDK (if needed)
- Build BlueMeter
- Launch the application

**That's it!** No manual steps needed.

### Option 3: Build from Source

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
