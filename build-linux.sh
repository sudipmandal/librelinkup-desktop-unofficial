#!/bin/bash
# Build script for Linux
# This script builds the application as a self-contained Linux binary

set -e

echo -e "\033[32mBuilding LibreLink Desktop for Linux...\033[0m"

# Check prerequisites
echo -e "\n\033[36mChecking prerequisites...\033[0m"

if ! command -v dotnet &> /dev/null; then
    echo -e "\033[31mERROR: .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download\033[0m"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo -e "  \033[32m✓ .NET SDK: $DOTNET_VERSION\033[0m"

# Clean previous builds
echo -e "\n\033[36mCleaning previous builds...\033[0m"
dotnet clean LibreLinkupDesktop-Unofficial.sln -c Release > /dev/null 2>&1 || true
if [ -d "publish" ]; then
    rm -rf publish
    echo -e "  \033[32m✓ Cleaned previous publish output\033[0m"
fi

# Restore dependencies
echo -e "\n\033[36mRestoring dependencies...\033[0m"
dotnet restore LibreLinkupDesktop-Unofficial.sln

# Build and publish self-contained for Linux x64
echo -e "\n\033[36mBuilding application (linux-x64, self-contained)...\033[0m"
dotnet publish src/LibreLinkupDesktop-Unofficial/LibreLinkupDesktop-Unofficial.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o publish/linux-x64

echo -e "\n\033[32m✓ Build completed successfully!\033[0m"
echo -e "\n\033[36mOutput files:\033[0m"

if [ -d "publish/linux-x64" ]; then
    find publish/linux-x64 -maxdepth 1 -type f -name "LibreLinkupDesktop-Unofficial" | while read -r file; do
        SIZE=$(du -h "$file" | cut -f1)
        echo -e "  \033[33mBinary: $file ($SIZE)\033[0m"
    done
fi

echo -e "\n\033[36mTo run:\033[0m"
echo -e "  chmod +x publish/linux-x64/LibreLinkupDesktop-Unofficial"
echo -e "  ./publish/linux-x64/LibreLinkupDesktop-Unofficial"

echo -e "\n\033[32mDone!\033[0m"
