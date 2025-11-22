# How to Compile and Run LibreLink Desktop

This guide provides step-by-step instructions to compile and run the application from source.

## Prerequisites Installation

### Step 1: Install Node.js

1. Download Node.js 20.x or later from https://nodejs.org/
2. Run the installer and follow the prompts
3. Verify installation:
```powershell
node --version
npm --version
```

### Step 2: Install Rust

1. Download and run rustup-init.exe from https://rustup.rs/
   - Or use winget: `winget install Rustlang.Rustup`
2. Follow the installation prompts (choose default options)
3. **Important**: After installation, you MUST do ONE of the following:
   - **Option A (Recommended)**: Close and reopen PowerShell/VS Code completely
   - **Option B**: Reload environment variables in current session:
   ```powershell
   $env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
   ```
4. Verify installation:
```powershell
rustc --version
cargo --version
```
Both should show version numbers (e.g., `rustc 1.91.1`)

### Step 3: Install Visual Studio Build Tools (Windows)

Rust on Windows requires Microsoft C++ build tools.

**Option A: Visual Studio 2022 (if you have it)**
- Make sure "Desktop development with C++" workload is installed

**Option B: Build Tools Only (recommended for smaller download)**
1. Download from https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
2. Run the installer
3. Select "Desktop development with C++" workload
4. Install (this may take 15-30 minutes)

### Step 4: Verify All Prerequisites

Run this command to check everything:
```powershell
node --version; npm --version; rustc --version; cargo --version
```

All commands should return version numbers without errors.

## Compilation Steps

### Step 1: Navigate to Project Directory

```powershell
cd C:\sources\LibreLinkupDesktop-TauriApp
```

### Step 2: Install Node.js Dependencies

```powershell
npm install
```

This installs:
- Vue.js and related frontend libraries
- Vite build tool
- Tauri CLI
- TypeScript compiler

**Expected output**: `added X packages` (takes 30-60 seconds)

### Step 3: Build the Application (Development Mode)

To compile and run in development mode:

```powershell
npm run tauri dev
```

**What happens:**
1. Vue.js frontend compiles (Vite dev server starts on port 1420)
2. Rust/Tauri wrapper compiles (first time takes 5-10 minutes)
3. Desktop application window opens with borderless custom titlebar
4. Login form appears ready for LibreLinkUp credentials

**First-time compilation**: Takes 5-15 minutes as Rust downloads and compiles all dependencies.
**Subsequent runs**: Takes 30-60 seconds.

### Step 4: Test the Application

When the window opens:
1. Select your country from the dropdown
2. Enter your LibreLinkUp email and password
3. Click "Login" button
4. Window should resize to compact mode (249×58px) and show:
   - Current glucose reading with color-coded background
   - Trend arrow (↑↑, ↑, →, ↓, ↓↓)
   - Last updated timestamp
   - Gear icon (⚙) for logout
5. Data should auto-refresh every 30 seconds

### Step 5: Build for Production (Optional)

To create installer packages:

```powershell
npm run tauri build
```

**What happens:**
1. Vue.js frontend is built with production optimizations
2. Rust/Tauri creates MSI and NSIS installers
3. Output appears in `src-tauri\target\release\bundle\`
   - MSI: `msi\LibreLinkup Desktop - Unofficial_*.msi`
   - NSIS: `nsis\LibreLinkup Desktop - Unofficial_*_x64-setup.exe`

**Build time**: 3-8 minutes (depending on system)

## Running the Application

### Development Mode

Start the development server:
```powershell
npm run tauri dev
```

**Features in development mode:**
- ✅ Hot reload for Vue.js/TypeScript changes (edit and save, app updates automatically)
- ✅ Rust console output visible in terminal
- ✅ Browser DevTools available (F12) for debugging
- ❌ Requires restarting for Rust code changes

### Production Mode

After building, run the installer:
```powershell
# Install via MSI
.\src-tauri\target\release\bundle\msi\LibreLinkup*.msi

# Or run NSIS installer
.\src-tauri\target\release\bundle\nsis\LibreLinkup*setup.exe
```

Or run the executable directly without installing:
```powershell
.\src-tauri\target\release\librelinkup-desktop-unofficial.exe
```

## Quick Reference

| Action | Command |
|--------|---------|
| Install dependencies | `npm install` |
| Run in development | `npm run tauri dev` |
| Build for production | `npm run tauri build` |
| Clean Rust builds | `cd src-tauri; cargo clean` |
| Frontend dev server only | `npm run dev` |
| Check for errors | `npm run build` (TypeScript check) |

## Troubleshooting

### Error: "cargo: command not found" or "program not found"

**Problem**: Rust is not installed OR the terminal hasn't loaded the updated PATH

**Solution 1 (If Rust not installed)**:
```powershell
winget install Rustlang.Rustup
```

**Solution 2 (If just installed - PATH not loaded - CRITICAL FIX)**:
After installing Rust, you MUST reload the PATH. Choose one option:

```powershell
# Option A: Close and reopen PowerShell/VS Code completely (recommended)

# Option B: Reload PATH in current session (faster):
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Then verify:
rustc --version
cargo --version
```

**Solution 3 (Check if Rust is actually installed)**:
```powershell
# Check if Rust binaries exist
Test-Path "$env:USERPROFILE\.cargo\bin\rustc.exe"
# Should return "True" if installed
```

### Error: "Port 1420 is already in use"

**Problem**: Vite dev server from previous run is still running

**Solution**: Kill the process using port 1420:
```powershell
$port1420 = Get-NetTCPConnection -LocalPort 1420 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -First 1
if ($port1420) { Stop-Process -Id $port1420 -Force }

# Then try again:
npm run tauri dev
```

### Error: "MSBUILD error" or "link.exe not found"

**Solution**: Visual Studio Build Tools not installed
1. Download from https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
2. Install "Desktop development with C++" workload
3. Restart terminal and try again

### Error: First build takes forever (Rust compilation)

**Solution**: This is normal!
- First build: 5-15 minutes (downloads and compiles 100+ Rust dependencies)
- Subsequent builds: 30-60 seconds (only recompiles changed code)
- Be patient and let it complete

### Error: LibreLinkUp login fails

**Solution**: Check credentials and connections
```powershell
# Common causes:
# 1. Wrong email/password - verify in LibreLinkUp app
# 2. No active connections - ensure you have an approved follower connection
# 3. Wrong country selected - select the correct region
# 4. Network issue - check internet connection
```

### Error: CORS when calling API

**Solution**: Make sure you're using Tauri's fetch, not browser fetch
```typescript
// Correct (from @tauri-apps/plugin-http):
import { fetch } from '@tauri-apps/plugin-http'

// Wrong (browser fetch has CORS):
// import { fetch } from 'window'
```

### Error: Window opens but shows login form only

**Solutions**:
1. Enter your LibreLinkUp credentials
2. Make sure you have an active follower connection set up in the LibreLinkUp app
3. Check browser console (F12) for any error messages
4. Verify internet connection

### Error: Changes not appearing

**Solution**: Depends on what you changed
- **Vue.js/TypeScript files**: Should hot-reload automatically (no restart needed)
- **Rust files**: Must stop (`Ctrl+C`) and restart `npm run tauri dev`
- **Config files**: Must stop and restart

### Error: Store data not persisting

**Solution**: Check permissions for AppData folder
```powershell
# Settings are stored in:
# %APPDATA%\Local\com.sudipmandal.lldunofficial\settings.json

# Check if file exists:
Test-Path "$env:LOCALAPPDATA\com.sudipmandal.lldunofficial\settings.json"
```

## File Locations

After compilation, key files are located at:

```
LibreLinkupDesktop-TauriApp/
├── node_modules/              # NPM packages (after npm install)
├── dist/                      # Built Vue.js frontend
├── src-tauri/
│   ├── target/
│   │   ├── debug/           # Development builds
│   │   └── release/         # Production builds
│   │       ├── librelinkup-desktop-unofficial.exe
│   │       └── bundle/      # Installers
│   │           ├── msi/     # Windows MSI installers
│   │           ├── nsis/    # Windows NSIS installers
│   │           ├── deb/     # Linux .deb packages (if built on Linux)
│   │           ├── appimage/# Linux AppImage (if built on Linux)
│   │           ├── dmg/     # macOS DMG (if built on macOS)
│   │           └── macos/   # macOS .app bundle (if built on macOS)
└── %LOCALAPPDATA%\com.sudipmandal.lldunofficial\
    └── settings.json          # User settings (email, password, country, etc.)
```

## Performance Tips

1. **Use an SSD**: Rust compilation is I/O intensive
2. **Close other applications**: Compilation uses significant CPU/RAM
3. **Use development mode**: Much faster than rebuilding for production each time
4. **Incremental builds**: Don't run `cargo clean` unless necessary

## Next Steps After Successful Compilation

1. ✅ Application runs successfully
2. 📝 Start customizing `src/App.vue` for your UI
3. 🎨 Modify glucose thresholds and colors
4. ✨ Add new features (historical graphs, data export, etc.)
5. 📦 Build production installers when ready to distribute

## Common Development Workflow

```powershell
# 1. Start development server
npm run tauri dev

# 2. Make changes to Vue/TypeScript files (src/App.vue, src/lib/*.ts)
#    - Changes appear automatically in the running app (hot reload)

# 3. Make changes to Rust files (src-tauri/src/*.rs)
#    - Stop the app (Ctrl+C)
#    - Restart: npm run tauri dev

# 4. Test your changes with real LibreLinkUp data

# 5. When ready to distribute:
npm run tauri build
```

## Estimated Times

| Task | First Time | Subsequent |
|------|-----------|-----------|
| npm install | 1-2 min | 10-30 sec |
| npm run tauri dev (compile) | 5-15 min | 30-60 sec |
| npm run build:windows | 10-20 min | 5-10 min |
| Vue.js hot reload | Instant | Instant |

## System Requirements

**Minimum:**
- Windows 10 or later / Linux (Ubuntu 20.04+)
- 8 GB RAM
- 10 GB free disk space
- Dual-core CPU

**Recommended:**
- Windows 11 / Linux (Ubuntu 22.04+)
- 16 GB RAM
- 20 GB free disk space (for all build artifacts)
- Quad-core CPU or better
- SSD storage

## Getting Help

If you encounter issues:

1. Check this guide's Troubleshooting section
2. Review `BUILD-GUIDE.md` for platform-specific issues
3. Check Tauri documentation: https://tauri.app/
4. Check LibreLinkUp API docs: https://github.com/DiaKEM/libre-link-up-api-client
5. Check Tauri plugins: https://v2.tauri.app/plugin/

## Success Indicators

You'll know everything is working when:
- ✅ `npm run tauri dev` opens a window
- ✅ Login form displays with country dropdown
- ✅ Can login with LibreLinkUp credentials
- ✅ Window resizes to compact mode after login
- ✅ Glucose data displays with correct colors and trend arrows
- ✅ Data auto-refreshes every 30 seconds
- ✅ Console shows no error messages
- ✅ App responds to Vue.js file changes automatically

**You're now ready to develop!** 🎉
