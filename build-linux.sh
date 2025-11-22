#!/bin/bash
# Build script for Linux
# This script builds the application for Linux (AppImage and .deb)

set -e

echo -e "\033[32mBuilding LibreLink Desktop for Linux...\033[0m"

# Check prerequisites
echo -e "\n\033[36mChecking prerequisites...\033[0m"

if ! command -v rustc &> /dev/null; then
    echo -e "\033[31mERROR: Rust not found. Please install Rust from rustup.rs\033[0m"
    exit 1
fi
RUST_VERSION=$(rustc --version)
echo -e "  \033[32m✓ Rust: $RUST_VERSION\033[0m"

if ! command -v node &> /dev/null; then
    echo -e "\033[31mERROR: Node.js not found. Please install Node.js\033[0m"
    exit 1
fi
NODE_VERSION=$(node --version)
echo -e "  \033[32m✓ Node.js: $NODE_VERSION\033[0m"

# Check for required Linux dependencies
echo -e "\n\033[36mChecking Linux build dependencies...\033[0m"

MISSING_DEPS=()

# Check for required system packages
if ! dpkg -l | grep -q libwebkit2gtk-4.1-dev; then
    if ! dpkg -l | grep -q libwebkit2gtk-4.0-dev; then
        MISSING_DEPS+=("libwebkit2gtk-4.1-dev or libwebkit2gtk-4.0-dev")
    fi
fi

if ! dpkg -l | grep -q libgtk-3-dev; then
    MISSING_DEPS+=("libgtk-3-dev")
fi

if ! dpkg -l | grep -q libayatana-appindicator3-dev; then
    if ! dpkg -l | grep -q libappindicator3-dev; then
        MISSING_DEPS+=("libayatana-appindicator3-dev or libappindicator3-dev")
    fi
fi

if [ ${#MISSING_DEPS[@]} -ne 0 ]; then
    echo -e "\033[33mWARNING: Missing dependencies detected:\033[0m"
    for dep in "${MISSING_DEPS[@]}"; do
        echo "  - $dep"
    done
    echo -e "\nInstall them with:"
    echo "  sudo apt-get update"
    echo "  sudo apt-get install -y libwebkit2gtk-4.1-dev libgtk-3-dev libayatana-appindicator3-dev librsvg2-dev patchelf"
    echo ""
    read -p "Continue anyway? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    echo -e "  \033[32m✓ All required dependencies found\033[0m"
fi

# Clean previous builds
echo -e "\n\033[36mCleaning previous builds...\033[0m"
if [ -d "src-tauri/target/release/bundle" ]; then
    rm -rf src-tauri/target/release/bundle
    echo -e "  \033[32m✓ Cleaned previous bundles\033[0m"
fi

# Build the application
echo -e "\n\033[36mBuilding application...\033[0m"
npm run tauri build

echo -e "\n\033[32m✓ Build completed successfully!\033[0m"
echo -e "\n\033[36mOutput files:\033[0m"

BUNDLE_DIR="src-tauri/target/release/bundle"

if [ -d "$BUNDLE_DIR/deb" ]; then
    find "$BUNDLE_DIR/deb" -name "*.deb" | while read -r file; do
        echo -e "  \033[33mDEB: $file\033[0m"
    done
fi

if [ -d "$BUNDLE_DIR/appimage" ]; then
    find "$BUNDLE_DIR/appimage" -name "*.AppImage" | while read -r file; do
        echo -e "  \033[33mAppImage: $file\033[0m"
    done
fi

echo -e "\n\033[32mDone!\033[0m"
