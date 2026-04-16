# Application Architecture

## Overview

LibreLinkUp Desktop is built with **Avalonia UI** and **.NET 8**, following the **MVVM** (Model-View-ViewModel) pattern using **CommunityToolkit.Mvvm**.

## Project Structure

```
LibreLinkupDesktop-Unofficial.sln
src/
  LibreLinkupDesktop-Unofficial/
    Program.cs                  # Entry point
    App.axaml / App.axaml.cs    # Application setup & DI
    Models/
      CgmModels.cs              # Glucose data models
      AppSettings.cs            # Settings model
    Services/
      LinkupService.cs          # LibreLinkUp API client
      SettingsService.cs        # JSON settings persistence
      LogService.cs             # In-memory log buffer
    ViewModels/
      ViewModelBase.cs          # Base ViewModel class
      MainWindowViewModel.cs    # Main application logic
    Views/
      MainWindow.axaml          # Main window UI
      MainWindow.axaml.cs       # Window code-behind
```

## Key Components

### Services

| Service | Purpose |
|---------|---------|
| `LinkupService` | HTTP client for LibreLinkUp API authentication and CGM data fetching |
| `SettingsService` | Loads/saves `AppSettings` as JSON in the user's AppData directory |
| `LogService` | Observable log buffer (max 200 lines) displayed in the UI |

### ViewModels

**MainWindowViewModel** contains all application logic:
- Login/logout flow
- CGM data polling (30-second intervals)
- Error handling with auto-retry (10-second intervals)
- Always-on-top management
- Window size change requests via events

### Views

**MainWindow** handles:
- Custom title bar with drag support
- Minimize/maximize/close buttons
- Login form display (800×600)
- Compact CGM widget display (249×58)
- Window state management

## Data Flow

```
User Input → ViewModel → LinkupService → LibreLinkUp API
                ↓
         SettingsService → settings.json
                ↓
         Update UI Bindings → View
```

1. **Login**: User enters credentials → ViewModel calls `LinkupService.GetAuthTokenAsync()` → Token saved via `SettingsService`
2. **Data Fetch**: Token retrieved → `LinkupService.GetCgmDataAsync()` → ViewModel updates display properties
3. **Auto-refresh**: Timer fires every 30s → Repeats step 2
4. **Error**: On failure → Show login view → Auto-retry every 10s

## Glucose Color Coding

| Range (mmol/L) | Color | Meaning |
|-----------------|-------|---------|
| 4.2 – 10.0 | Green (#28a745) | In range |
| 10.1 – 13.9 | Orange (#fd7e14) | High |
| < 4.2 or > 13.9 | Red (#e81123) | Critical |

## Cross-Platform Support

The app runs on Windows and Linux via Avalonia's cross-platform rendering:
- **Windows**: Win32/DirectX backend
- **Linux**: X11/Skia backend

Self-contained publishing bundles the .NET runtime, so no prerequisites are needed on the target machine.
