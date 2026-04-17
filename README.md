# LibreLinkUp Desktop - Unofficial

[![GitHub Release](https://img.shields.io/github/v/release/sudipmandal/librelinkup-desktop-unofficial?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/releases)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/sudipmandal/librelinkup-desktop-unofficial/release.yml?style=flat-square&label=build)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/actions)
[![License](https://img.shields.io/github/license/sudipmandal/librelinkup-desktop-unofficial?style=flat-square)](LICENSE)
[![GitHub Downloads](https://img.shields.io/github/downloads/sudipmandal/librelinkup-desktop-unofficial/total?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/releases)
[![GitHub Stars](https://img.shields.io/github/stars/sudipmandal/librelinkup-desktop-unofficial?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/stargazers)
[![Contributions Welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/issues)

> ⚠️ **Disclaimer**: This is an unofficial, community-developed desktop application and is not affiliated with, endorsed by, or connected to Abbott Laboratories or LibreLink/LibreLinkUp. Use at your own risk.

Freestyle Libre 3 plus CGM sensor does not currently have any official app from Abbot that displays the real time data on PC, this is a  lightweight, cross-platform desktop application built with Avalonia UI and .NET 8 which fills this gap, it provides a compact, always-accessible widget displaying real-time glucose readings on your desktop, so you dont have to keep unlocking your phone while working on your PC or laptop.

<img width="1226" height="740" alt="image" src="https://github.com/user-attachments/assets/6416c959-46c9-4b76-98c4-ca9e15a64748" />


## ✨ Key Features

- 🩸 **Real-time CGM Monitoring**: Display current glucose levels with trend arrows
- 🌍 **Multi-region Support**: Works with 11 LibreLinkUp regions worldwide
- 📦 **Compact Widget Mode**: Minimal 249×58px window shows only essential data
- 🎨 **Color-coded Alerts**: Visual feedback with green/orange/red backgrounds
- 🔄 **Auto-refresh**: Updates every 30 seconds automatically
- 📌 **Always on Top**: Optional setting to keep window visible
- 🔐 **Credential Storage**: Remembers login for quick access
- 💻 **Cross-platform**: Windows and Linux

## 📥 Installation & Usage

### Download Pre-built App

The easiest way to get started is to download and install the pre-built application from the [Releases Page](https://github.com/sudipmandal/librelinkup-desktop-unofficial/releases).

### First Run

1. Launch the application
2. Select your LibreLinkUp region from the dropdown
3. Enter your LibreLinkUp email and password
4. Click "Login"
5. The window will resize to compact mode and display your glucose data

The app will auto-login on subsequent launches and refresh data every 30 seconds.

## 🏗️ Technical Architecture

- **UI Framework**: Avalonia UI 11.x
- **Runtime**: .NET 8
- **Pattern**: MVVM with CommunityToolkit.Mvvm
- **API Integration**: HttpClient with LibreLinkUp API
- **Storage**: JSON-based settings in AppData
- **Build Output**: Self-contained single-file executables

## 📋 Prerequisites

To build from source, you need:

1. **.NET 8 SDK** - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)

**For detailed setup instructions, see [SETUP.md](Docs/SETUP.md)** or [COMPILE-AND-RUN.md](Docs/COMPILE-AND-RUN.md) for step-by-step guidance.

## 📚 Documentation

- [SETUP.md](Docs/SETUP.md) - Initial setup guide
- [COMPILE-AND-RUN.md](Docs/COMPILE-AND-RUN.md) - Step-by-step compilation
- [BUILD-GUIDE.md](Docs/BUILD-GUIDE.md) - Cross-platform builds
- [API_IMPLEMENTATION.md](Docs/API_IMPLEMENTATION.md) - LibreLinkUp API details
- [ARCHITECTURE.md](Docs/ARCHITECTURE.md) - Application architecture
- [QUICK-REFERENCE.md](Docs/QUICK-REFERENCE.md) - Command cheat sheet

## ⚖️ License

This project is provided as-is for educational and personal use. Always consult with healthcare professionals for medical decisions.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.
