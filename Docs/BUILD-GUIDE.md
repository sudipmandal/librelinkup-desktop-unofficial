# Build Guide

## Overview

The application is built using .NET 8 and Avalonia UI. It produces self-contained single-file executables for Windows and Linux.

## Build Targets

| Platform | Runtime ID | Output |
|----------|-----------|--------|
| Windows x64 | win-x64 | `LibreLinkupDesktop-Unofficial.exe` |
| Linux x64 | linux-x64 | `LibreLinkupDesktop-Unofficial` |

## Build Scripts

### Windows
```powershell
.\build-windows.ps1
```
Output: `publish\win-x64\LibreLinkupDesktop-Unofficial.exe`

### Linux
```bash
chmod +x build-linux.sh
./build-linux.sh
```
Output: `publish/linux-x64/LibreLinkupDesktop-Unofficial`

## Manual Build Commands

### Debug Build
```bash
dotnet build LibreLinkupDesktop-Unofficial.sln
```

### Release Build (Framework-dependent)
```bash
dotnet build LibreLinkupDesktop-Unofficial.sln -c Release
```

### Self-Contained Publish
```bash
# Windows
dotnet publish src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/win-x64

# Linux
dotnet publish src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/linux-x64
```

## CI/CD

The GitHub Actions workflow (`.github/workflows/release.yml`) automatically builds for both platforms when a tag starting with `v` is pushed:

```bash
git tag v0.1.0
git push origin v0.1.0
```

This creates a draft GitHub release with platform-specific archives:
- `LibreLinkupDesktop-Unofficial-win-x64.zip`
- `LibreLinkupDesktop-Unofficial-linux-x64.tar.gz`

## Build Configuration

Key project settings in `LibreLinkupDesktop-Unofficial.csproj`:

| Setting | Value | Purpose |
|---------|-------|---------|
| TargetFramework | net8.0 | .NET 8 runtime |
| OutputType | WinExe | Windows executable (no console) |
| PublishSingleFile | true | Single-file deployment |
| SelfContained | true | No .NET runtime required on target |
