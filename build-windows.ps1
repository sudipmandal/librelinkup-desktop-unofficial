# Build script for Windows
# This script builds the application for Windows (MSI and NSIS installers)

Write-Host "Building LibreLink Desktop for Windows..." -ForegroundColor Green

# Check prerequisites
Write-Host "`nChecking prerequisites..." -ForegroundColor Cyan

$rustVersion = rustc --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Rust not found. Please install Rust from rustup.rs" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ Rust: $rustVersion" -ForegroundColor Green

$nodeVersion = node --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Node.js not found. Please install Node.js" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ Node.js: $nodeVersion" -ForegroundColor Green

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Cyan
if (Test-Path "src-tauri\target\release\bundle") {
    Remove-Item -Path "src-tauri\target\release\bundle" -Recurse -Force
    Write-Host "  ✓ Cleaned previous bundles" -ForegroundColor Green
}

# Build the application
Write-Host "`nBuilding application..." -ForegroundColor Cyan
npm run tauri build

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBUILD FAILED!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✓ Build completed successfully!" -ForegroundColor Green
Write-Host "`nOutput files:" -ForegroundColor Cyan

$bundleDir = "src-tauri\target\release\bundle"
if (Test-Path "$bundleDir\msi") {
    Get-ChildItem "$bundleDir\msi" -Filter "*.msi" | ForEach-Object {
        Write-Host "  MSI: $($_.FullName)" -ForegroundColor Yellow
    }
}
if (Test-Path "$bundleDir\nsis") {
    Get-ChildItem "$bundleDir\nsis" -Filter "*.exe" | ForEach-Object {
        Write-Host "  NSIS: $($_.FullName)" -ForegroundColor Yellow
    }
}

Write-Host "`nDone!" -ForegroundColor Green
