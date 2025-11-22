# Setup Guide - Tauri + Vue Frontend

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

#### 1.4 Install Visual Studio Build Tools (Windows)
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

The C# dependencies are managed by NuGet and will be restored automatically.

### Step 3: Run Development Server

```powershell
npm run tauri dev
```

This will:
1. ✅ Start Vite dev server (Vue frontend)
2. ✅ Build Rust/Tauri wrapper
3. ✅ Launch the desktop app with borderless window

### Step 4: Verify Everything Works

When the app launches, you should see:
- A custom borderless window titled "LibreLinkup Desktop - Unofficial"
- Login form with country dropdown, email, and password fields
- Always on Top checkbox
- After login: CGM data display with glucose readings and trend arrows

Try:
1. Enter your name and click "Greet from Rust" - tests Tauri/Rust backend
2. Enter your name and click "Greet from C#" - tests C# backend via TauriDotNetBridge
3. Enter name and age, click "Get Person Info" - tests complex data from C#

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                   Tauri Desktop App                     │
│  ┌───────────────────────────────────────────────────┐  │
│  │         Vue.js Frontend (Port 1420)               │  │
│  │  - TypeScript + Options API                       │  │
│  │  - Vite dev server                                │  │
│  │  - Makes HTTP calls to C# backend                 │  │
│  └───────────────────────────────────────────────────┘  │
│                          ↓                              │
│  ┌───────────────────────────────────────────────────┐  │
│  │        Tauri/Rust Core                            │  │
│  │  - Manages window                                 │  │
│  │  - Spawns C# sidecar process                      │  │
│  │  - Rust commands available via invoke()           │  │
│  └───────────────────────────────────────────────────┘  │
│                          ↓                              │
│  ┌───────────────────────────────────────────────────┐  │
│  │   C# Backend Sidecar (Port 5000)                  │  │
│  │  - TauriDotNetBridge                              │  │
│  │  - ASP.NET Core web server                        │  │
│  │  - Exposes commands via HTTP API                  │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## How Communication Works

### Calling Rust from Vue (Options API):
```typescript
// In your component methods
import { invoke } from "@tauri-apps/api/core";

methods: {
  async callRust() {
    const result = await invoke("greet", { name: "World" });
    this.message = result;
  }
}
```

### Calling C# from Vue (Options API):
```typescript
// In your component methods
methods: {
  async callCSharp() {
    const response = await fetch("http://localhost:5000/api/invoke/Greet", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name: "World" })
    });
    const result = await response.json();
    this.message = result;
  }
}
```

## Development Tips

### Hot Reload
- Vue components: ✅ Auto-reload on save
- Rust code: ❌ Requires restart (`npm run tauri dev`)
- C# code: ❌ Requires restart

### Debugging C# Backend

1. Build the C# project separately:
```powershell
cd src-csharp
dotnet build
```

2. Run it standalone:
```powershell
dotnet run
```

3. Or use VS Code debugging with the provided launch configuration

### Debugging Rust Code

Add print statements:
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
- Vue.js frontend (bundled)
- Tauri/Rust wrapper
- C# backend (self-contained executable)

## Common Issues

### Issue: Rust not found
**Solution**: Restart your terminal/VS Code after installing Rust

### Issue: C# backend won't compile
**Solution**: 
```powershell
cd src-csharp
dotnet restore
dotnet build
```

### Issue: Port 5000 already in use
**Solution**: Change the port in both:
- `src-csharp/Program.cs` (UseUrls)
- `src/App.vue` (fetch URLs)

### Issue: Build fails with MSBUILD error
**Solution**: Install Visual Studio Build Tools with C++ workload

## Next Steps

1. **Add more C# services**: Create new interfaces and implementations in `src-csharp/`
2. **Create new Vue components**: Use `src/components/ExampleComponent.vue` as an Options API template
3. **Style the frontend**: Modify `src/App.vue` and add CSS
4. **Add Tauri plugins**: Check https://tauri.app/plugin/
5. **Package for distribution**: Run `npm run tauri build`

## Cross-Platform Builds

This application supports Windows and Linux. To build for different platforms:

- **Windows**: `npm run build:windows` (creates MSI and NSIS installers)
- **Linux**: `npm run build:linux` (creates .deb and AppImage)

For detailed cross-platform build instructions, see **`BUILD-GUIDE.md`**

## Resources

- [Tauri Documentation](https://tauri.app/)
- [TauriDotNetBridge GitHub](https://github.com/FelixKahle/TauriDotNetBridge)
- [Vue.js Options API Guide](https://vuejs.org/guide/introduction.html#options-api)
- [Vue.js TypeScript Guide](https://vuejs.org/guide/typescript/overview.html)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Cross-Platform Build Guide](./BUILD-GUIDE.md)
