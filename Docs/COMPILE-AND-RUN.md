# Compile and Run

## Quick Start

```bash
# Clone the repository
git clone https://github.com/sudipmandal/librelinkup-desktop-unofficial.git
cd librelinkup-desktop-unofficial

# Restore dependencies
dotnet restore

# Run in development mode
dotnet run --project src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj
```

## Build for Release

### Windows
```powershell
# Self-contained single-file executable
dotnet publish src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64

# Or use the build script
.\build-windows.ps1
```

### Linux
```bash
# Self-contained single-file executable
dotnet publish src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64

# Or use the build script
chmod +x build-linux.sh
./build-linux.sh
```

## Running the Development Build

```bash
# Debug mode with hot reload
dotnet watch --project src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj
```

## Troubleshooting

### Linux: Missing Libraries
If you get errors about missing native libraries on Linux:
```bash
# Ubuntu/Debian
sudo apt-get install -y libx11-dev libxi-dev libxcursor-dev libxrandr-dev libgl1-mesa-dev
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```
