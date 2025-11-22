# Quick Reference - Tauri + Vue Commands

## Development
```powershell
# Start development server (frontend + desktop app)
npm run tauri dev

# Frontend only (for testing)
npm run dev
```

## Production Builds
```powershell
# Build for Windows (MSI + NSIS installers)
npm run build:windows

# Build for Linux (.deb + AppImage)
npm run build:linux

# Build for current platform
npm run tauri build

# Build specific format only
npm run tauri build -- --bundles msi      # Windows MSI only
npm run tauri build -- --bundles deb      # Linux .deb only
npm run tauri build -- --bundles appimage # Linux AppImage only
```

## Tauri Commands
```powershell
# Check Tauri environment
npm run tauri info

# Build for specific platform
npm run tauri build -- --target x86_64-pc-windows-msvc

# Clean build
cd src-tauri
cargo clean
```

## File Locations

### Frontend
- `src/App.vue` - Main Vue component with login & CGM display
- `src/lib/linkup.ts` - LibreLinkUp API integration
- `src/lib/utils.ts` - Utility functions (SHA-256 hashing)
- `src/main.ts` - Vue entry point
- `vite.config.ts` - Vite configuration

### Backend (Rust)
- `src-tauri/src/main.rs` - Entry point
- `src-tauri/src/lib.rs` - Tauri setup & plugin initialization
- `src-tauri/build.rs` - Build script
- `src-tauri/tauri.conf.json` - Tauri configuration
- `src-tauri/capabilities/default.json` - Permission definitions

## Adding LibreLinkUp API Functions

1. **Add to linkup.ts** (`src/lib/linkup.ts`):
```typescript
export async function myNewApiCall(params: MyParams) {
  const baseUrl = getBaseUrl(params.country);
  const response = await fetch(`${baseUrl}/endpoint`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${params.token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ /* your data */ })
  });
  return response.json();
}
```

2. **Call from Vue** (Options API style in `App.vue`):
```typescript
import { myNewApiCall } from './lib/linkup';

methods: {
  async callMyNewApi() {
    const result = await myNewApiCall({
      token: this.token,
      country: this.country
    });
    console.log(result);
  }
}
    const result = await response.json();
    this.myData = result; // Update component data
  }
}
```

## Creating a New Vue Component (Options API)

Use `src/components/ExampleComponent.vue` as a template:

```vue
<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "MyComponent",
  props: {
    title: String
  },
  data() {
    return {
      message: ""
    };
  },
  methods: {
    async myMethod() {
      // Your logic here
      this.message = "Updated!";
    }
  },
  mounted() {
    // Initialization code
  }
});
</script>

<template>
  <div>
    <h2>{{ title }}</h2>
    <p>{{ message }}</p>
  </div>
</template>

<style scoped>
/* Component styles */
</style>
```

## Port Configuration

- **Vue Dev Server**: http://localhost:1420
- **LibreLinkUp API**: https://api-{country}.libreview.io/llu

## Build Output Locations

- **Development**: 
  - Rust: `src-tauri/target/debug/librelinkup-desktop-unofficial.exe`
  
- **Production**: 
  - Windows MSI: `src-tauri/target/release/bundle/msi/LibreLinkup Desktop - Unofficial_*.msi`
  - Windows NSIS: `src-tauri/target/release/bundle/nsis/LibreLinkup Desktop - Unofficial_*_x64-setup.exe`
  - Linux .deb: `src-tauri/target/release/bundle/deb/librelinkup-desktop-unofficial_*_amd64.deb`
  - Linux AppImage: `src-tauri/target/release/bundle/appimage/librelinkup-desktop-unofficial_*_amd64.AppImage`
  - macOS DMG: `src-tauri/target/release/bundle/dmg/LibreLinkup Desktop - Unofficial_*_x64.dmg`
  - Raw executable: `src-tauri/target/release/librelinkup-desktop-unofficial.exe`

## User Data Locations

- **Settings**: `%LOCALAPPDATA%\com.sudipmandal.lldunofficial\settings.json`
  - Stores: email, password, country, alwaysOnTop preference
  - Format: Plain text JSON

## Debugging

### VS Code Launch Configurations
- `Tauri Dev` - Start full app with debugger attached

### Console Logging
- **Rust**: `println!("message");`
- **Vue**: `console.log("message");`
- **Browser DevTools**: Press F12 in the app window

### Check LibreLinkUp API Status
```powershell
# Test API endpoint (replace with your country and token)
curl https://api-us.libreview.io/llu/connections `
  -H "Authorization: Bearer YOUR_TOKEN" `
  -H "product: llu.android" `
  -H "version: 4.16.0"
```

## Common Tasks

### Update Dependencies
```powershell
# NPM packages
npm update

# Rust crates
cd src-tauri
cargo update
```

### Clean Everything
```powershell
# Clean Rust build
cd src-tauri
cargo clean

# Clean npm packages
npm run clean
rm -rf node_modules
npm install

# Clean Node modules
Remove-Item node_modules -Recurse -Force
npm install
```

## Environment Variables

### Development
Set in your shell before running `npm run tauri dev`:
```powershell
$env:RUST_LOG="debug"
$env:ASPNETCORE_ENVIRONMENT="Development"
```

### Production
Set during build:
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
npm run tauri build
```
