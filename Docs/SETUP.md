# Setup Guide - LibreLinkup Desktop Unofficial

## Quick Start

If you have all prerequisites installed, run:

```powershell
npm run tauri dev
```

## Detailed Setup Instructions

### Step 1: Install Prerequisites

#### 1.1 Install Rust
```powershell
# Download and run rustup-init.exe from https://rustup.rs/
# Or use winget:
winget install -e --id Rustlang.Rustup
```

After installation, verify:
```powershell
rustc --version
cargo --version
```

#### 1.2 Install Node.js
```powershell
# Download from https://nodejs.org/
# Or use winget:
winget install OpenJS.NodeJS
```

Verify:
```powershell
node --version
npm --version
```

#### 1.3 Install Visual Studio Build Tools (Windows)
For Rust compilation on Windows, you need the Visual Studio C++ build tools:

```powershell
# Download Visual Studio 2022 Build Tools from:
# https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022

# Select "Desktop development with C++" workload
```

### Step 2: Verify Tauri Prerequisites

Check if everything is set up correctly:

```powershell
npm install -g @tauri-apps/cli
cargo tauri info
```

### Step 3: Install Project Dependencies

The npm dependencies should already be installed, but if needed:

```powershell
npm install
```

### Step 4: Run Development Server

```powershell
npm run tauri dev
```

This will:
1. ✅ Start Vite dev server (Vue frontend on port 1420)
2. ✅ Build Rust/Tauri wrapper
3. ✅ Launch the desktop app with borderless window

### Step 5: Verify Everything Works

When the app launches, you should see:
- A custom borderless window titled "LibreLinkup Desktop - Unofficial"
- Login form with country dropdown, email, and password fields
- Always on Top checkbox
- After login: Compact CGM data display (249×58px) with:
  - Glucose reading with color-coded background
  - Trend arrow (↑↑, ↑, →, ↓, ↓↓)
  - Last updated timestamp
  - Gear icon (⚙) for logout

Try logging in with your LibreLinkUp credentials to see live CGM data.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                   Tauri Desktop App                     │
│  ┌───────────────────────────────────────────────────┐  │
│  │         Vue.js Frontend (Port 1420)               │  │
│  │  - TypeScript + Options API                       │  │
│  │  - Vite dev server                                │  │
│  │  - LibreLinkUp API integration                    │  │
│  └───────────────────────────────────────────────────┘  │
│                          ↓                              │
│  ┌───────────────────────────────────────────────────┐  │
│  │        Tauri/Rust Core                            │  │
│  │  - Manages window (borderless custom titlebar)    │  │
│  │  - Window resize (compact/full modes)             │  │
│  │  - Plugins: HTTP, Store, Opener                   │  │
│  └───────────────────────────────────────────────────┘  │
│                          ↓                              │
│  ┌───────────────────────────────────────────────────┐  │
│  │      External APIs & Storage                      │  │
│  │  - LibreLinkUp API (via tauri-plugin-http)        │  │
│  │  - Local settings (via tauri-plugin-store)        │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## How Communication Works

### Calling LibreLinkUp API:
```typescript
// Using Tauri's fetch to bypass CORS
import { fetch } from '@tauri-apps/plugin-http'

methods: {
  async fetchCGMData() {
    const response = await fetch(
      `https://api-${country}.libreview.io/llu/connections`,
      {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'product': 'llu.android',
          'version': '4.16.0'
        }
      }
    );
    const data = await response.json();
    // Process CGM data
  }
}
```

### Using Store for Persistence:
```typescript
// Save and load settings
import { Store } from '@tauri-apps/plugin-store'

data() {
  return {
    store: null
  }
},
async mounted() {
  this.store = new Store('settings.json');
  await this.store.set('email', 'user@example.com');
  const email = await this.store.get('email');
}
```

## Development Tips

### Hot Reload
- Vue components: ✅ Auto-reload on save
- Rust code: ❌ Requires restart (`npm run tauri dev`)
- TypeScript files: ✅ Auto-reload on save

### Debugging

1. Check console logs in the dev tools (F12)
2. Check LibreLinkUp API responses in Network tab
3. Rust console output shows in terminal
4. Store data is in `AppData\Local\com.sudipmandal.lldunofficial\settings.json`

3. Or use VS Code debugging with the provided launch configuration

### Debugging Rust Code

Add console.log statements in Vue or println! in Rust:
```rust
println!("Debug message: {:?}", variable);
```

Or use the Rust debugger extension in VS Code.

## Building for Production

Create a production build:
```powershell
npm run tauri build
```

Output location: `src-tauri/target/release/bundle/`

The installer will include:
- Vue.js frontend (bundled and optimized)
- Tauri/Rust wrapper
- All required plugins (HTTP, Store, Opener)

## Common Issues

### Issue: Rust not found
**Solution**: Restart your terminal/VS Code after installing Rust

### Issue: LibreLinkUp login fails
**Solution**: 
- Verify your credentials are correct
- Check internet connection
- Ensure you have an active LibreLinkUp account with connections

### Issue: CORS errors when calling API
**Solution**: Make sure you're using Tauri's fetch from `@tauri-apps/plugin-http`, not browser fetch

### Issue: Build fails with MSBUILD error
**Solution**: Install Visual Studio Build Tools with C++ workload

## Next Steps

1. **Customize colors**: Modify glucose thresholds in `glucoseBackgroundColor()` method
2. **Add features**: Historical graphs, multiple patient support, data export
3. **Style the UI**: Modify `src/App.vue` and add custom CSS
4. **Add Tauri plugins**: Check https://tauri.app/plugin/
5. **Package for distribution**: Run `npm run tauri build`

## Cross-Platform Builds

This application supports Windows, Linux, and macOS. Build outputs:

- **Windows**: MSI and NSIS installers
- **Linux**: .deb packages and AppImage
- **macOS**: DMG and .app bundles

For detailed cross-platform build instructions, see **`BUILD-GUIDE.md`**

## Resources

- [Tauri Documentation](https://tauri.app/)
- [LibreLinkUp API (unofficial)](https://github.com/DiaKEM/libre-link-up-api-client)
- [Vue.js Options API Guide](https://vuejs.org/guide/introduction.html#options-api)
- [Tauri HTTP Plugin](https://v2.tauri.app/plugin/http-client/)
- [Tauri Store Plugin](https://v2.tauri.app/plugin/store/)
- [Vue.js TypeScript Guide](https://vuejs.org/guide/typescript/overview.html)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Cross-Platform Build Guide](./BUILD-GUIDE.md)
