# Cross-Platform Build Guide

This guide explains how to build LibreLink Desktop for Windows and Linux platforms.

## Supported Platforms

- **Windows**: MSI Installer, NSIS Installer
- **Linux**: AppImage (universal), .deb package (Debian/Ubuntu)

## Prerequisites

### All Platforms
- Node.js 18+ 
- Rust (latest stable)

### Windows-Specific
- Visual Studio 2022 Build Tools with C++ workload
- Or Visual Studio 2022 Community/Professional

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
- MSI Installer: `src-tauri\target\release\bundle\msi\*.msi`
- NSIS Installer: `src-tauri\target\release\bundle\nsis\*.exe`

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
- AppImage: `src-tauri/target/release/bundle/appimage/*.AppImage`
- DEB Package: `src-tauri/target/release/bundle/deb/*.deb`

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
# Install .NET SDK
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

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
npm run build:linux
```

### Building Windows Binaries on Linux (Not Recommended)

Cross-compiling Windows binaries from Linux is complex due to .NET and Rust requirements. 
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
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: npm install
      - run: npm run build:windows
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
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: |
          sudo apt-get update
          sudo apt-get install -y libwebkit2gtk-4.1-dev libgtk-3-dev \
            libayatana-appindicator3-dev librsvg2-dev patchelf
      - run: npm install
      - run: npm run build:linux
      - uses: actions/upload-artifact@v4
        with:
          name: linux-packages
          path: src-tauri/target/release/bundle/
```

## Build Configuration

### Customizing Bundle Settings

Edit `src-tauri/tauri.conf.json`:

```json
{
  "bundle": {
    "targets": ["msi", "nsis", "deb", "appimage"],
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

The C# backend automatically builds for the correct platform via the `build.rs` script:

- Windows: `win-x64` runtime
- Linux: `linux-x64` runtime
- macOS: `osx-x64` runtime (if needed in future)

### Runtime Detection in Code

The Rust build script automatically:
1. Detects the target platform
2. Compiles C# backend with correct runtime identifier (RID)
3. Sets executable permissions on Linux
4. Packages the correct binary for each platform

## Testing Builds

### Windows
```powershell
# Install MSI
msiexec /i src-tauri\target\release\bundle\msi\*.msi

# Or run NSIS installer
.\src-tauri\target\release\bundle\nsis\*.exe
```

### Linux
```bash
# Install .deb
sudo dpkg -i src-tauri/target/release/bundle/deb/*.deb

# Or run AppImage directly
chmod +x src-tauri/target/release/bundle/appimage/*.AppImage
./src-tauri/target/release/bundle/appimage/*.AppImage
```

## Troubleshooting

### Windows: "MSBUILD error"
- Install Visual Studio Build Tools with "Desktop development with C++" workload

### Linux: "WebKit2GTK not found"
- Run: `sudo apt-get install libwebkit2gtk-4.1-dev`

### Linux: AppImage won't run
- Make it executable: `chmod +x *.AppImage`
- Install FUSE: `sudo apt-get install libfuse2`

### C# Backend Not Starting
- Check `dotnet --version` shows 9.0.x
- Rebuild C# manually: `cd src-csharp && dotnet build`

### Port 5000 Already in Use
- Change in `src-csharp/Program.cs`
- Update Vue components to use new port

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

## File Sizes (Approximate)

- Windows MSI: ~80-100 MB
- Windows NSIS: ~75-95 MB
- Linux .deb: ~75-95 MB
- Linux AppImage: ~80-100 MB

Sizes include:
- Vue.js frontend (bundled)
- Tauri runtime (WebView wrapper)
- C# backend (self-contained .NET runtime)

## Reducing Build Size

1. **Enable trimming** in `src-csharp/TauriBackend.csproj`:
```xml
<PublishTrimmed>true</PublishTrimmed>
```

2. **Enable compression** in `src-tauri/tauri.conf.json`:
```json
{
  "bundle": {
    "resources": {
      "compress": true
    }
  }
}
```

3. **Remove unused dependencies** from both frontend and backend

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
