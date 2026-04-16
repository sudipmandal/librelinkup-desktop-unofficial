# Setup Guide

## Prerequisites

### .NET 8 SDK

Download and install the .NET 8 SDK from [Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0).

**Windows:**
- Download the Windows x64 installer
- Run the installer and follow prompts
- Verify: `dotnet --version`

**Linux (Ubuntu/Debian):**
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 8 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

**Linux (Fedora):**
```bash
sudo dnf install dotnet-sdk-8.0
```

Verify installation:
```bash
dotnet --version
```

## IDE Setup (Optional)

### Visual Studio Code
1. Install [VS Code](https://code.visualstudio.com/)
2. Install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension

### JetBrains Rider
1. Install [Rider](https://www.jetbrains.com/rider/)
2. Open `LibreLinkupDesktop-Unofficial.sln`

### Visual Studio (Windows)
1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/)
2. Include ".NET desktop development" workload
3. Open `LibreLinkupDesktop-Unofficial.sln`
