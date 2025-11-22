# Cross-Platform Build Guide

This guide explains how to build LibreLink Desktop for Windows, Linux, and macOS platforms.

## Supported Platforms

- **Windows**: MSI Installer, NSIS Installer
- **Linux**: AppImage (universal), .deb package (Debian/Ubuntu)
- **macOS**: DMG, .app bundle

## Prerequisites

### All Platforms
- Node.js 18+ 
- Rust (latest stable)

### Windows-Specific
- Visual Studio 2022 Build Tools with C++ workload
- Or Visual Studio 2022 Community/Professional

### macOS-Specific
- Xcode Command Line Tools (`xcode-select --install`)

### Linux-Specific (Debian/Ubuntu)
Install required dependencies:
```bash
sudo apt-get update
sudo apt-get install -y \
    libwebkit2gtk-4.1-dev \
    libgtk-3-dev \
    libayatana-appindicator3-dev \
    librsvg2-dev \
    patchelf \
    build-essential \
    curl \
    wget \
    file
```

For other distributions, see: https://tauri.app/start/prerequisites/#linux

## Building for Windows (on Windows)

### Option 1: Using Build Script
```powershell
npm run build:windows
```

### Option 2: Manual Build
```powershell
npm run tauri build
```

**Output Files:**
- MSI Installer: `src-tauri\target\release\bundle\msi\librelinkup-desktop-unofficial_*.msi`
- NSIS Installer: `src-tauri\target\release\bundle\nsis\librelinkup-desktop-unofficial_*_x64-setup.exe`
- Executable: `src-tauri\target\release\librelinkup-desktop-unofficial.exe`

### MSI vs NSIS
- **MSI**: Native Windows installer format, better for enterprise deployment
- **NSIS**: More customizable, smaller file size

## Building for Linux (on Linux)

### Option 1: Using Build Script
```bash
chmod +x build-linux.sh
npm run build:linux
```

### Option 2: Manual Build
```bash
npm run tauri build
```

**Output Files:**
- AppImage: `src-tauri/target/release/bundle/appimage/librelinkup-desktop-unofficial_*_amd64.AppImage`
- DEB Package: `src-tauri/target/release/bundle/deb/librelinkup-desktop-unofficial_*_amd64.deb`
- Executable: `src-tauri/target/release/librelinkup-desktop-unofficial`

### AppImage vs .deb
- **AppImage**: Universal format, runs on any Linux distribution without installation
- **.deb**: Native package for Debian-based systems (Ubuntu, Linux Mint, etc.)

## Cross-Compilation

### Building Linux Binaries on Windows (via WSL)

1. Install WSL2 with Ubuntu:
```powershell
wsl --install -d Ubuntu
```

2. Inside WSL, install prerequisites:
```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Install Node.js
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install Tauri dependencies
sudo apt-get install -y libwebkit2gtk-4.1-dev libgtk-3-dev \
    libayatana-appindicator3-dev librsvg2-dev patchelf
```

3. Navigate to project and build:
```bash
cd /mnt/c/sources/LibreLinkupDesktop-TauriApp
npm install
npm run tauri build
```

### Building Windows Binaries on Linux (Not Recommended)

Cross-compiling Windows binaries from Linux is complex due to Rust and Windows WebView2 requirements. 
**Recommended**: Use Windows for Windows builds, Linux for Linux builds, or use CI/CD.

## CI/CD Cross-Platform Builds

For automated builds, use GitHub Actions:

```yaml
name: Build
on: [push]

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
      - uses: dtolnay/rust-toolchain@stable
      - run: npm install
      - run: npm run tauri build
      - uses: actions/upload-artifact@v4
        with:
          name: windows-installers
          path: src-tauri/target/release/bundle/

  build-linux:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
      - uses: dtolnay/rust-toolchain@stable
      - run: |
          sudo apt-get update
          sudo apt-get install -y libwebkit2gtk-4.1-dev libgtk-3-dev \
            libayatana-appindicator3-dev librsvg2-dev patchelf
      - run: npm install
      - run: npm run tauri build
      - uses: actions/upload-artifact@v4
        with:
          name: linux-packages
          path: src-tauri/target/release/bundle/

  build-macos:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
      - uses: dtolnay/rust-toolchain@stable
      - run: npm install
      - run: npm run tauri build
      - uses: actions/upload-artifact@v4
        with:
          name: macos-packages
          path: src-tauri/target/release/bundle/
```

## Build Configuration

### Customizing Bundle Settings

Edit `src-tauri/tauri.conf.json`:

```json
{
  "bundle": {
    "targets": ["msi", "nsis", "deb", "appimage", "dmg", "app"],
    "linux": {
      "deb": {
        "depends": []  // Add system dependencies here
      }
    }
  }
}
```

### Target-Specific Builds

Build only specific formats:

**Windows MSI only:**
```powershell
npm run tauri build -- --bundles msi
```

**Linux .deb only:**
```bash
npm run tauri build -- --bundles deb
```

**AppImage only:**
```bash
npm run tauri build -- --bundles appimage
```

## Platform-Specific Code

### Runtime Detection

Tauri automatically handles platform-specific builds:
1. Detects the target platform during compilation
2. Links appropriate system WebView libraries (WebView2 on Windows, WebKit2GTK on Linux, WKWebView on macOS)
3. Sets executable permissions on Unix-based systems
4. Packages the correct bundle format for each platform

### Architecture

The application is frontend-only:
- **Vue 3 + TypeScript** (Options API) for UI
- **Tauri 2.x + Rust** for native desktop wrapper
- **tauri-plugin-http** for LibreLinkUp API calls (bypasses CORS)
- **tauri-plugin-store** for settings persistence (plain text JSON)
- **tauri-plugin-opener** for opening external links

## Testing Builds

### Windows
```powershell
# Install MSI
msiexec /i src-tauri\target\release\bundle\msi\librelinkup-desktop-unofficial_*.msi

# Or run NSIS installer
.\src-tauri\target\release\bundle\nsis\librelinkup-desktop-unofficial_*_x64-setup.exe

# Or run executable directly (no installation)
.\src-tauri\target\release\librelinkup-desktop-unofficial.exe
```

### Linux
```bash
# Install .deb
sudo dpkg -i src-tauri/target/release/bundle/deb/librelinkup-desktop-unofficial_*_amd64.deb

# Or run AppImage directly
chmod +x src-tauri/target/release/bundle/appimage/librelinkup-desktop-unofficial_*_amd64.AppImage
./src-tauri/target/release/bundle/appimage/librelinkup-desktop-unofficial_*_amd64.AppImage

# Or run executable directly (no installation)
./src-tauri/target/release/librelinkup-desktop-unofficial
```

## Troubleshooting

### Windows: "MSBUILD error"
- Install Visual Studio Build Tools with "Desktop development with C++" workload

### Linux: "WebKit2GTK not found"
- Run: `sudo apt-get install libwebkit2gtk-4.1-dev`

### Linux: AppImage won't run
- Make it executable: `chmod +x *.AppImage`
- Install FUSE: `sudo apt-get install libfuse2`

### Build Fails with HTTP Errors
- The app uses tauri-plugin-http for API calls
- Ensure Rust dependencies are up to date: `cd src-tauri && cargo update`

### Application Won't Start
- Check WebView2 is installed on Windows (usually pre-installed on Windows 10/11)
- On Linux, verify WebKit2GTK: `sudo apt-get install libwebkit2gtk-4.1-0`

## Distribution

### Windows
- Upload MSI/NSIS to your website
- Submit to Microsoft Store (requires additional configuration)
- Use Chocolatey for package management

### Linux
- Upload .deb to your website
- Submit to Ubuntu PPA
- Upload AppImage to GitHub Releases
- Use Snapcraft or Flatpak (requires additional configuration)

### macOS
- Upload DMG to your website
- Submit to Mac App Store (requires Apple Developer account and additional configuration)
- Notarize the app for Gatekeeper

## File Sizes (Approximate)

- Windows MSI: ~8-12 MB
- Windows NSIS: ~8-12 MB
- Linux .deb: ~10-15 MB
- Linux AppImage: ~15-20 MB
- macOS DMG: ~10-15 MB

Sizes include:
- Vue.js frontend (bundled and minified)
- Tauri runtime (WebView wrapper)
- Rust binary

Note: Actual runtime uses system WebView (WebView2/WebKit2GTK/WKWebView), keeping the bundle size small.

## Reducing Build Size

1. **Enable compression** in `src-tauri/tauri.conf.json`:
```json
{
  "bundle": {
    "resources": {
      "compress": true
    }
  }
}
```

2. **Optimize Vite build** in `vite.config.ts`:
```typescript
build: {
  minify: 'terser',
  terserOptions: {
    compress: {
      drop_console: true,
    },
  },
}
```

3. **Remove unused dependencies** from `package.json`

4. **Enable Rust optimization** in `src-tauri/Cargo.toml`:
```toml
[profile.release]
strip = true
lto = true
codegen-units = 1
```

## Security Considerations

- All builds include code signing capabilities
- Configure signing in `tauri.conf.json`
- Windows: Use Authenticode certificate
- Linux: Sign .deb with GPG key
- AppImage: Include signature file

## Next Steps

- Set up automated builds with GitHub Actions
- Configure code signing
- Create update mechanism with Tauri's updater
- Set up download page for releases
