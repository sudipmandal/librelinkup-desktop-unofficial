# Project Setup Verification Checklist

Use this checklist to verify your Tauri + Vue project is set up correctly.

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

## ✅ Rust Crates
```powershell
cd src-tauri
cargo tree --depth 1
```

Should include:
- tauri
- tauri-plugin-opener
- tauri-plugin-store
- tauri-plugin-http
- serde
- serde_json

## 🧪 Test the Setup

### Test 1: Build Rust Project
```powershell
cd src-tauri
cargo build
```
✅ Should compile without errors

### Test 2: Run Development Server
```powershell
cd c:\sources\LibreLinkupDesktop-TauriApp
npm run tauri dev
```
✅ Should open a desktop window with borderless custom titlebar
✅ Should show login form with country dropdown, email, and password fields
✅ Should be able to login and see CGM data display

### Test 3: Test LibreLinkUp Integration
In the app window:
1. Select your country from dropdown
2. Enter your LibreLinkUp email and password
3. Click "Login"
✅ Should resize to compact mode (249×58px)
✅ Should display current glucose reading
✅ Should show trend arrow and timestamp
✅ Should show color-coded background (green/orange/red)
✅ Should auto-refresh every 30 seconds

### Test 4: Test Window Controls
In the compact mode:
1. Click the gear icon (⚙) to logout
✅ Should resize back to full window (800×600px)
✅ Should show login form again
✅ Window minimize/maximize/close buttons should work

## 🔧 Troubleshooting

If any test fails, check:

1. **Rust not found**: Restart terminal after installing Rust
2. **Login fails**: Verify LibreLinkUp credentials are correct
3. **CORS errors**: Ensure tauri-plugin-http is properly configured
4. **Window resize fails**: Check capabilities/default.json has window resize permissions
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
- [x] Login form displays correctly
- [x] LibreLinkUp login works and shows CGM data
- [x] Window resizing works (compact/full modes)
- [x] Auto-refresh works every 30 seconds
- [x] No errors in console

## Next Steps

Once everything is verified:
1. Read `README.md` for architecture overview
2. Check `QUICK-REFERENCE.md` for common commands
3. Modify the Vue frontend to add features
4. Customize glucose thresholds and colors
5. Add additional CGM data visualizations!

---

**Project Status**: ✅ Ready for Development

**Stack**:
- Frontend: Vue 3 + TypeScript + Vite (Options API)
- Desktop: Tauri 2.x + Rust
- API Integration: LibreLinkUp API via tauri-plugin-http
- Storage: tauri-plugin-store (plain text JSON)

**Platforms**:
- Windows: MSI, NSIS installers
- Linux: .deb packages, AppImage
- macOS: DMG, .app bundles

**Communication Flow**:
Vue (Options API) → Tauri HTTP Plugin → LibreLinkUp API
Vue (Options API) → Tauri Store Plugin → settings.json
