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
2. Rust backend compiles
3. Rust/Tauri wrapper compiles (first time takes 5-10 minutes)
4. Desktop application window opens
5. C# backend starts on http://localhost:5000

**First-time compilation**: Takes 5-15 minutes as Rust downloads and compiles all dependencies.
**Subsequent runs**: Takes 30-60 seconds.

### Step 4: Test the Application

When the window opens:
1. Check "C# Backend Status" shows "Connected ✓"
2. Enter your name in any input field
3. Click "Greet from Rust" - should show greeting from Rust
4. Click "Greet from C#" - should show greeting from C# backend
5. Enter name and age, click "Get Person Info" - should show JSON response

### Step 5: Build for Production (Optional)

To create installer packages:

```powershell
npm run build:windows
```

**What happens:**
1. Vue.js frontend is built with production optimizations
2. C# backend is compiled as self-contained executable
3. Rust/Tauri creates MSI and NSIS installers
4. Output appears in `src-tauri\target\release\bundle\`

**Build time**: 5-15 minutes (depending on system)

## Running the Application

### Development Mode

Start the development server:
```powershell
npm run tauri dev
```

**Features in development mode:**
- ✅ Hot reload for Vue.js changes (edit and save, app updates automatically)
- ✅ Rust console output visible in terminal
- ✅ C# backend console output visible
- ❌ Requires restarting for Rust or C# changes

### Production Mode

After building, run the installer:
```powershell
# Install via MSI
.\src-tauri\target\release\bundle\msi\*.msi

# Or run NSIS installer
.\src-tauri\target\release\bundle\nsis\*.exe
```

Or run the executable directly without installing:
```powershell
.\src-tauri\target\release\tauri-app.exe
```

## Quick Reference

| Action | Command |
|--------|---------|
| Install dependencies | `npm install` |
| Run in development | `npm run tauri dev` |
| Build for production | `npm run build:windows` |
| Build C# only | `cd src-csharp; dotnet build` |
| Run C# backend standalone | `cd src-csharp; dotnet run` |
| Clean all builds | `cd src-tauri; cargo clean` |
| Frontend dev server only | `npm run dev` |

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

### Error: "dotnet: command not found"

**Solution**: .NET SDK not installed
```powershell
# Install .NET 9.0
winget install Microsoft.DotNet.SDK.9

# Verify:
dotnet --version
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

### Error: "Port 5000 already in use"

**Solution**: Another application is using port 5000
```powershell
# Find what's using port 5000
netstat -ano | findstr :5000

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or change the port in src-csharp/Program.cs
```

### Error: C# backend not starting

**Solution**: Check C# compilation
```powershell
cd src-csharp
dotnet build

# Should show "Build succeeded"
# If it fails, check error messages
```

### Error: Window opens but shows "Disconnected"

**Solutions**:
1. Check if C# backend is running (look for console output showing "C# Backend is running on http://localhost:5000")
2. Check Windows Firewall isn't blocking localhost connections
3. Try running C# backend manually:
```powershell
cd src-csharp
dotnet run
# Should show "C# Backend is running on http://localhost:5000"
```

### Error: Changes not appearing

**Solution**: Depends on what you changed
- **Vue.js files**: Should hot-reload automatically (no restart needed)
- **Rust files**: Must stop (`Ctrl+C`) and restart `npm run tauri dev`
- **C# files**: Must stop and restart `npm run tauri dev`
- **Config files**: Must stop and restart

## File Locations

After compilation, key files are located at:

```
LibreLinkupDesktop-TauriApp/
├── node_modules/              # NPM packages (after npm install)
├── dist/                      # Built Vue.js frontend
├── src-tauri/
│   ├── binaries/             # Compiled C# backend
│   ├── target/
│   │   ├── debug/           # Development builds
│   │   └── release/         # Production builds
│   │       └── bundle/      # Installers (MSI, NSIS)
│   │           ├── msi/     # Windows MSI installers
│   │           └── nsis/    # Windows NSIS installers
└── src-csharp/
    ├── bin/                  # C# debug builds
    └── obj/                  # C# intermediate files
```

## Performance Tips

1. **Use an SSD**: Rust compilation is I/O intensive
2. **Close other applications**: Compilation uses significant CPU/RAM
3. **Use development mode**: Much faster than rebuilding for production each time
4. **Incremental builds**: Don't run `cargo clean` unless necessary

## Next Steps After Successful Compilation

1. ✅ Application runs successfully
2. 📝 Start customizing `src/App.vue` for your frontend
3. 🔧 Add C# services in `src-csharp/`
4. 🎨 Modify styles and UI components
5. 📦 Build production installers when ready to distribute

## Common Development Workflow

```powershell
# 1. Start development server
npm run tauri dev

# 2. Make changes to Vue files (src/App.vue, etc.)
#    - Changes appear automatically in the running app

# 3. Make changes to C# backend (src-csharp/*.cs)
#    - Stop the app (Ctrl+C)
#    - Restart: npm run tauri dev

# 4. Test your changes

# 5. When ready to distribute:
npm run build:windows
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
4. Check .NET documentation: https://docs.microsoft.com/dotnet/
5. Check TauriDotNetBridge: https://github.com/FelixKahle/TauriDotNetBridge

## Success Indicators

You'll know everything is working when:
- ✅ `npm run tauri dev` opens a window
- ✅ Window shows "C# Backend Status: Connected ✓"
- ✅ All three sections respond to button clicks
- ✅ Console shows no error messages
- ✅ App responds to Vue.js file changes automatically

**You're now ready to develop!** 🎉
