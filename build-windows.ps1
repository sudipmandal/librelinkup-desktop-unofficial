# Build script for Windows
# This script builds the application as a self-contained Windows executable

Write-Host "Building LibreLink Desktop for Windows..." -ForegroundColor Green

# Check prerequisites
Write-Host "`nChecking prerequisites..." -ForegroundColor Cyan

$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ .NET SDK: $dotnetVersion" -ForegroundColor Green

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Cyan
dotnet clean LibreLinkupDesktop-Unofficial.sln -c Release 2>$null | Out-Null
if (Test-Path "publish") {
    Remove-Item -Path "publish" -Recurse -Force
    Write-Host "  ✓ Cleaned previous publish output" -ForegroundColor Green
}

# Restore dependencies
Write-Host "`nRestoring dependencies..." -ForegroundColor Cyan
dotnet restore LibreLinkupDesktop-Unofficial.sln

# Build and publish self-contained for Windows x64
Write-Host "`nBuilding application (win-x64, self-contained)..." -ForegroundColor Cyan
dotnet publish src\LibreLinkupDesktop-Unofficial\LibreLinkupDesktop-Unofficial.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o publish\win-x64

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBUILD FAILED!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✓ Build completed successfully!" -ForegroundColor Green
Write-Host "`nOutput files:" -ForegroundColor Cyan

if (Test-Path "publish\win-x64") {
    Get-ChildItem "publish\win-x64" -Filter "*.exe" | ForEach-Object {
        $size = [math]::Round($_.Length / 1MB, 2)
        Write-Host "  EXE: $($_.FullName) ($size MB)" -ForegroundColor Yellow
    }
}

Write-Host "`nTo run:" -ForegroundColor Cyan
Write-Host "  .\publish\win-x64\LibreLinkupDesktop-Unofficial.exe"

Write-Host "`nDone!" -ForegroundColor Green
