# LibreLinkUp Desktop - Unofficial

[![GitHub Release](https://img.shields.io/github/v/release/sudipmandal/librelinkup-desktop-unofficial?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/releases)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/sudipmandal/librelinkup-desktop-unofficial/release.yml?style=flat-square&label=build)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/actions)
[![License](https://img.shields.io/github/license/sudipmandal/librelinkup-desktop-unofficial?style=flat-square)](LICENSE)
[![GitHub Downloads](https://img.shields.io/github/downloads/sudipmandal/librelinkup-desktop-unofficial/total?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/releases)
[![GitHub Stars](https://img.shields.io/github/stars/sudipmandal/librelinkup-desktop-unofficial?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/stargazers)
[![Contributions Welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat-square)](https://github.com/sudipmandal/librelinkup-desktop-unofficial/issues)

> ⚠️ **Disclaimer**: This is an unofficial, community-developed desktop application and is not affiliated with, endorsed by, or connected to Abbott Laboratories or LibreLink/LibreLinkUp. Use at your own risk.

A lightweight, cross-platform desktop application for monitoring your LibreLinkUp CGM (Continuous Glucose Monitor) data. Built with modern web technologies, this app provides a compact, always-accessible widget displaying real-time glucose readings on your desktop.

## ✨ Key Features

- 🩸 **Real-time CGM Monitoring**: Display current glucose levels with trend arrows
- 🌍 **Multi-region Support**: Works with 11 LibreLinkUp regions worldwide
- 📦 **Compact Widget Mode**: Minimal 249×58px window shows only essential data
- 🎨 **Color-coded Alerts**: Visual feedback with green/orange/red backgrounds
- 🔄 **Auto-refresh**: Updates every 30 seconds automatically
- 📌 **Always on Top**: Optional setting to keep window visible
- 🔐 **Credential Storage**: Remembers login for quick access
- 🌓 **Dark Mode Support**: Adapts to system theme preferences
- 💻 **Cross-platform**: Windows (MSI/NSIS) and Linux (.deb/AppImage)

## 📥 Installation & Usage

### Download Pre-built App

The easiest way to get started is to download the pre-built application from the [Releases Page](https://github.com/sudipmandal/librelinkup-desktop-unofficial/releases).

**Windows:**
- Download `LibreLinkup Desktop - Unofficial_*_x64_en-US.msi` (MSI installer)
- Or `LibreLinkup Desktop - Unofficial_*_x64-setup.exe` (NSIS installer)
- Run the installer and follow the prompts

**Linux:**
- Download `librelinkup-desktop-unofficial_*_amd64.deb` (Debian/Ubuntu)
- Or `librelinkup-desktop-unofficial_*_amd64.AppImage` (Universal)
- For .deb: `sudo dpkg -i librelinkup-desktop-unofficial_*_amd64.deb`
- For AppImage: `chmod +x *.AppImage && ./librelinkup-desktop-unofficial_*.AppImage`

**macOS:**
- Download `LibreLinkup Desktop - Unofficial_*_x64.dmg`
- Open the DMG and drag the app to Applications

### First Run

1. Launch the application
2. Select your LibreLinkUp region from the dropdown
3. Enter your LibreLinkUp email and password
4. Click "Login"
5. The window will resize to compact mode and display your glucose data

The app will auto-login on subsequent launches and refresh data every 30 seconds.

## 🏗️ Technical Architecture

- **Frontend**: Vue.js 3 + TypeScript + Vite (Options API)
- **Desktop Framework**: Tauri 2.x (Rust)
- **API Integration**: LibreLinkUp API via Tauri HTTP Plugin
- **Storage**: Tauri Store Plugin (JSON-based preferences)
- **Build Output**: Native executables (4-6 MB installed size)

## 📋 Prerequisites

To build from source, you need:

1. **Node.js** (v18+) - JavaScript runtime
2. **Rust** - Systems programming language for Tauri
3. **Visual Studio Build Tools** (Windows only) - For Rust compilation

**For detailed setup instructions, see [SETUP.md](Docs/SETUP.md)** or [COMPILE-AND-RUN.md](Docs/COMPILE-AND-RUN.md) for step-by-step guidance.

## 📚 Documentation

- [SETUP.md](Docs/SETUP.md) - Initial setup guide
- [COMPILE-AND-RUN.md](Docs/COMPILE-AND-RUN.md) - Step-by-step compilation
- [BUILD-GUIDE.md](Docs/BUILD-GUIDE.md) - Cross-platform builds
- [API_IMPLEMENTATION.md](Docs/API_IMPLEMENTATION.md) - LibreLinkUp API details
- [STORAGE-GUIDE.md](Docs/STORAGE-GUIDE.md) - Data storage system
- [VUE-STYLE-GUIDE.md](Docs/VUE-STYLE-GUIDE.md) - Vue.js coding standards
- [QUICK-REFERENCE.md](Docs/QUICK-REFERENCE.md) - Command cheat sheet

## ⚖️ License

This project is provided as-is for educational and personal use. Always consult with healthcare professionals for medical decisions.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.
