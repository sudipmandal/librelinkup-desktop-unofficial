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
- **C# Backend**: http://localhost:5000

To change C# backend port:
1. Edit `src-csharp/Program.cs` → `UseUrls("http://localhost:YOUR_PORT")`
2. Update all fetch URLs in `src/App.vue`

## Build Output Locations

- **Development**: 
  - Rust: `src-tauri/target/debug/`
  - C#: `src-tauri/binaries/`
  
- **Production**: 
  - Windows MSI: `src-tauri/target/release/bundle/msi/`
  - Windows NSIS: `src-tauri/target/release/bundle/nsis/`
  - Linux .deb: `src-tauri/target/release/bundle/deb/`
  - Linux AppImage: `src-tauri/target/release/bundle/appimage/`
  - Raw executable: `src-tauri/target/release/`

## Debugging

### VS Code Launch Configurations
- `Tauri Dev` - Start full app with debugger
- `C# Backend Debug` - Debug C# backend separately

### Console Logging
- **Rust**: `println!("message");`
- **C#**: `Console.WriteLine("message");`
- **Vue**: `console.log("message");`

### Check Backend Status
```powershell
# Test if C# backend is running
curl http://localhost:5000/health
```

## Common Tasks

### Update Dependencies
```powershell
# NPM packages
npm update

# Rust crates
cd src-tauri
cargo update

# NuGet packages
cd src-csharp
dotnet list package --outdated
dotnet add package PackageName
```

### Clean Everything
```powershell
# Clean Rust build
cd src-tauri
cargo clean

# Clean C# build
cd src-csharp
dotnet clean

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
