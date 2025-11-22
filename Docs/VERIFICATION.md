# Project Setup Verification Checklist

Use this checklist to verify your Tauri + Vue + C# project is set up correctly.

## ✅ File Structure

- [x] `src/` - Vue.js frontend source code
  - [x] `App.vue` - Main component with LibreLinkUp integration
  - [x] `lib/linkup.ts` - LibreLinkUp API functions
  - [x] `lib/utils.ts` - Utility functions
  - [x] `main.ts` - Vue entry point
  
- [x] `src-tauri/` - Tauri/Rust application
  - [x] `src/main.rs` - Rust entry point
  - [x] `src/lib.rs` - Plugin initialization
  - [x] `build.rs` - Build script
  - [x] `tauri.conf.json` - Tauri configuration
  - [x] `capabilities/default.json` - Permission definitions
  - [x] `Cargo.toml` - Rust dependencies (opener, store, http plugins)

- [x] `.vscode/` - VS Code configuration
  - [x] `launch.json` - Debug configurations
  - [x] `tasks.json` - Build tasks
  - [x] `extensions.json` - Recommended extensions

- [x] Root files
  - [x] `README.md` - Project overview and documentation
  - [x] `SETUP.md` - Detailed setup instructions
  - [x] `QUICK-REFERENCE.md` - Command reference
  - [x] `BUILD-GUIDE.md` - Cross-platform build instructions
  - [x] `VUE-STYLE-GUIDE.md` - Vue.js Options API coding standards
  - [x] `build-windows.ps1` - Windows build script
  - [x] `build-linux.sh` - Linux build script
  - [x] `package.json` - NPM dependencies and scripts
  - [x] `.gitignore` - Git ignore rules

## ✅ Configuration Verification

### Tauri Configuration
- [x] `tauri.conf.json` configured for cross-platform builds (msi, nsis, deb, appimage)
- [x] `tauri.conf.json` has borderless window settings
- [x] `build.rs` calls tauri_build::build()
- [x] `lib.rs` initializes plugins (opener, store, http)
- [x] `capabilities/default.json` defines HTTP permissions for LibreLinkUp API

### Frontend Configuration
- [x] `App.vue` uses Options API (defineComponent)
- [x] `App.vue` imports Tauri API and plugins
- [x] `App.vue` has LibreLinkUp login and CGM data display
- [x] `App.vue` uses Tauri's fetch to call LibreLinkUp API
- [x] `App.vue` displays custom borderless window with titlebar
- [x] `lib/linkup.ts` contains API integration functions
- [x] `lib/utils.ts` contains SHA-256 hashing for Account-Id header

## ✅ Dependencies Installed

Run these commands to verify:

```powershell
# Node.js and NPM
node --version    # Should show v18+ 
npm --version     # Should show 9+

# Rust
rustc --version   # Should show rustc 1.70+
cargo --version   # Should show cargo 1.70+
```

## ✅ NPM Packages
```powershell
cd c:\sources\LibreLinkupDesktop-TauriApp
npm list --depth=0
```

Should show:
- vue
- @tauri-apps/api
- @tauri-apps/plugin-opener
- @tauri-apps/plugin-store
- @tauri-apps/plugin-http
- @tauri-apps/cli (dev)
- vite (dev)
- typescript (dev)
- vue-tsc (dev)

## ✅ Rust Dependencies
```powershell
cd src-csharp
dotnet list package
```

Should show:
- TauriDotNetBridge (v2.2.0)
- Microsoft.Extensions.DependencyInjection (v8.0.1)

## ✅ Rust Crates
```powershell
cd src-tauri
cargo tree --depth 1
```

Should include:
- tauri
- tauri-plugin-opener
- tauri-plugin-shell
- serde
- serde_json

## 🧪 Test the Setup

### Test 1: Build C# Backend Standalone
```powershell
cd src-csharp
dotnet build
```
✅ Should build without errors

### Test 2: Build Rust Project
```powershell
cd src-tauri
cargo build
```
✅ Should compile C# backend and Rust project
✅ Should create `binaries/tauri-backend.exe` (or `tauri-backend` on Linux/Mac)

### Test 3: Run Development Server
```powershell
cd c:\sources\LibreLinkupDesktop-TauriApp
npm run tauri dev
```
✅ Should open a desktop window
✅ Should show "C# Backend Status: Connected ✓"
✅ Should be able to click buttons and see responses

### Test 4: Test Rust Backend
In the app window:
1. Enter your name
2. Click "Greet from Rust"
✅ Should show: "Hello, [name]! You've been greeted from Rust!"

### Test 5: Test C# Backend
In the app window:
1. Enter your name
2. Click "Greet from C#"
✅ Should show: "Hello, [name]! Greetings from C# backend via TauriDotNetBridge!"

### Test 6: Test Complex Data
In the app window:
1. Enter name and age
2. Click "Get Person Info"
✅ Should show JSON with name, age, and message

## 🔧 Troubleshooting

If any test fails, check:

1. **Rust not found**: Restart terminal after installing Rust
2. **C# build fails**: Run `dotnet restore` in src-csharp folder
3. **Port 5000 in use**: Change port in Program.cs and App.vue
4. **Backend not connecting**: Check if C# backend is running (look for console output)
5. **Build errors**: Check SETUP.md for prerequisite installation steps

## 📦 Ready for Production Build?

Before building for production, ensure all tests pass:

**On Windows:**
```powershell
npm run build:windows
```
✅ Should create MSI and NSIS installers in `src-tauri/target/release/bundle/`

**On Linux:**
```bash
npm run build:linux
```
✅ Should create .deb and AppImage packages in `src-tauri/target/release/bundle/`

See `BUILD-GUIDE.md` for cross-compilation and CI/CD setup.

## 🎉 Success Criteria

Your setup is complete when:
- [x] All files are in correct locations
- [x] All dependencies are installed
- [x] `npm run tauri dev` launches successfully
- [x] C# Backend Status shows "Connected ✓"
- [x] All three test sections work correctly
- [x] No errors in console

## Next Steps

Once everything is verified:
1. Read `README.md` for architecture overview
2. Check `QUICK-REFERENCE.md` for common commands
3. Start customizing the C# backend services
4. Modify the Vue frontend to match your needs
5. Add your application logic!

---

**Project Status**: ✅ Ready for Development

**Stack**:
- Frontend: Vue 3 + TypeScript + Vite (Options API)
- Desktop: Tauri 2.0 + Rust
- Backend: C# .NET 9.0 + TauriDotNetBridge 2.2.0

**Platforms**:
- Windows: MSI, NSIS installers
- Linux: .deb packages, AppImage

**Communication Flow**:
Vue (Options API) → HTTP → C# Backend (via TauriDotNetBridge)
Vue (Options API) → invoke() → Rust Backend (via Tauri)
