# LibreLinkUp Desktop - Unofficial

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

## Project Structure

```
LibreLinkupDesktop-TauriApp/
├── Docs/                   # Documentation
│   ├── BUILD-GUIDE.md      # Platform-specific build instructions
│   ├── COMPILE-AND-RUN.md  # Step-by-step compilation guide
│   ├── QUICK-REFERENCE.md  # Quick command reference
│   ├── SETUP.md            # Initial setup guide
│   ├── STORAGE-GUIDE.md    # Complete storage documentation
│   ├── STORAGE-IMPLEMENTATION.md  # Storage implementation summary
│   ├── STORAGE-USAGE.md    # Storage usage examples
│   ├── VERIFICATION.md     # Testing and verification guide
│   └── VUE-STYLE-GUIDE.md  # Vue Options API style guide
├── src/                    # Vue.js frontend source
│   ├── lib/                # Library modules
│   │   ├── linkup.ts       # LibreLinkUp API integration
│   │   └── utils.ts        # Utility functions (SHA-256 hashing)
│   ├── App.vue             # Main Vue component with login & CGM display
│   └── main.ts             # Application entry point
├── src-tauri/              # Tauri/Rust application
│   ├── src/
│   │   ├── main.rs         # Entry point
│   │   └── lib.rs          # App setup and plugin initialization
│   ├── capabilities/       # Permission definitions
│   │   └── default.json    # Default capabilities (HTTP, Store, Window)
│   ├── build.rs            # Build script
│   ├── tauri.conf.json     # Tauri configuration
│   └── Cargo.toml          # Rust dependencies
├── package.json            # Node.js dependencies
└── README.md               # This file
```

## 🚀 Quick Start

### Installation

```powershell
npm install
```

### Development Mode

```powershell
npm run tauri dev
```

The first build takes 5-15 minutes as Rust compiles dependencies. Subsequent builds are much faster.

**For detailed instructions, see [Docs/COMPILE-AND-RUN.md](Docs/COMPILE-AND-RUN.md)**

## 🎯 How It Works

1. **Login**: Enter your LibreLinkUp credentials (email, password, region)
2. **Auto-login**: Credentials are saved for automatic login on startup
3. **Compact View**: Window shrinks to 249×58px showing glucose data
4. **Color Feedback**: Background changes based on glucose range:
   - 🟢 Green: 4.2-10.0 (target range)
   - 🟠 Orange: 10.1-13.9 (elevated)
   - 🔴 Red: Outside ranges (requires attention)
5. **Auto-refresh**: Data updates every 30 seconds
6. **Settings Access**: Gear icon (⚙) in top-right for logout

**For API integration details, see [Docs/API_IMPLEMENTATION.md](Docs/API_IMPLEMENTATION.md)**



## 📦 Building for Production

```powershell
# Windows (MSI + NSIS installers)
npm run tauri build

# Linux (.deb + AppImage)
npm run tauri build
```

**Output locations:**
- Windows: `src-tauri/target/release/bundle/msi/` and `.../nsis/`
- Linux: `src-tauri/target/release/bundle/deb/` and `.../appimage/`

**For detailed build instructions and cross-compilation, see [Docs/BUILD-GUIDE.md](Docs/BUILD-GUIDE.md)**

## 🔧 Development & Customization

### Storage System
The app uses Tauri Store Plugin for credential and preference storage. Data is stored in:
- Windows: `%APPDATA%\com.sudipmandal.lldunofficial\settings.json`
- Linux: `~/.config/com.sudipmandal.lldunofficial/settings.json`

**For storage implementation details, see [Docs/STORAGE-GUIDE.md](Docs/STORAGE-GUIDE.md)**

### Vue.js Development
The frontend uses Vue 3 with Options API for component structure. For coding standards and best practices, see [Docs/VUE-STYLE-GUIDE.md](Docs/VUE-STYLE-GUIDE.md)

## 🐛 Troubleshooting

**Login fails silently:**
- Check browser dev tools console (F12) for API errors
- Verify your LibreLinkUp credentials work on the official app/website
- Ensure correct region is selected

**Build errors:**
- Verify Rust is installed: `rustc --version`
- Update Rust: `rustup update`
- Clear build cache: Remove `src-tauri/target/` folder

**For detailed troubleshooting and verification, see [Docs/VERIFICATION.md](Docs/VERIFICATION.md)**

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
