# Quick Reference

## Common Commands

### Development

```bash
# Run the app in debug mode
dotnet run --project src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj

# Run with hot reload
dotnet watch --project src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj

# Build debug
dotnet build

# Build release
dotnet build -c Release

# Clean
dotnet clean
```

### Publishing

```bash
# Windows self-contained
dotnet publish src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64

# Linux self-contained
dotnet publish src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64
```

### Build Scripts

```bash
# Windows
.\build-windows.ps1

# Linux
./build-linux.sh
```

### Release

```bash
# Tag and push to trigger CI release
git tag v0.1.0
git push origin v0.1.0
```

## Project Structure

| Path | Description |
|------|-------------|
| `src/LibreLinkupDesktop-Unofficial/` | Main application source |
| `src/LibreLinkupDesktop-Unofficial/Models/` | Data models |
| `src/LibreLinkupDesktop-Unofficial/Services/` | Business logic services |
| `src/LibreLinkupDesktop-Unofficial/ViewModels/` | MVVM ViewModels |
| `src/LibreLinkupDesktop-Unofficial/Views/` | Avalonia UI views |
| `Docs/` | Documentation |
| `.github/workflows/` | CI/CD workflows |

## Settings File Location

- **Windows**: `%APPDATA%\LibreLinkupDesktop-Unofficial\settings.json`
- **Linux**: `~/.config/LibreLinkupDesktop-Unofficial/settings.json`

## Key Intervals

| Action | Interval |
|--------|----------|
| CGM data refresh | 30 seconds |
| Error auto-retry | 10 seconds |
| Always-on-top re-enforce | 1 second |
| Login timeout | 30 seconds |
