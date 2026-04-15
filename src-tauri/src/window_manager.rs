// KDE Plasma Window Management for Always-On-Top functionality
use tauri::{AppHandle, Runtime, WebviewWindow};

#[cfg(target_os = "linux")]
use std::process::Command;

#[cfg(target_os = "linux")]
fn is_kde_plasma() -> bool {
    // Check for KDE Plasma desktop environment
    if let Ok(desktop) = std::env::var("XDG_CURRENT_DESKTOP") {
        if desktop.to_lowercase().contains("kde") {
            return true;
        }
    }
    
    if let Ok(desktop) = std::env::var("DESKTOP_SESSION") {
        if desktop.to_lowercase().contains("plasma") || desktop.to_lowercase().contains("kde") {
            return true;
        }
    }
    
    false
}

#[cfg(target_os = "linux")]
fn set_kde_always_on_top_x11(window_title: &str, enable: bool) -> Result<(), String> {
    // Use wmctrl to set the window state on KDE
    // wmctrl uses EWMH (Extended Window Manager Hints) which KDE respects
    
    // Skip if no X display is available - wmctrl would hang
    if std::env::var("DISPLAY").unwrap_or_default().is_empty() {
        return Err("No X display available, wmctrl cannot be used".to_string());
    }
    
    let state_action = if enable { "add" } else { "remove" };
    
    // First, try to find the window by title
    let output = Command::new("wmctrl")
        .args(&["-l"])
        .output();
    
    if output.is_err() {
        return Err("wmctrl not found. Please install it: sudo apt install wmctrl".to_string());
    }
    
    let output = output.unwrap();
    let window_list = String::from_utf8_lossy(&output.stdout);
    let mut window_id: Option<String> = None;
    
    for line in window_list.lines() {
        if line.contains(window_title) {
            // Window ID is the first column
            if let Some(id) = line.split_whitespace().next() {
                window_id = Some(id.to_string());
                break;
            }
        }
    }
    
    if let Some(id) = window_id {
        // Set the window to always on top using _NET_WM_STATE_ABOVE
        let result = Command::new("wmctrl")
            .args(&["-i", "-r", &id, "-b", &format!("{},above", state_action)])
            .output();
        
        match result {
            Ok(_) => Ok(()),
            Err(e) => Err(format!("Failed to set window state: {}", e)),
        }
    } else {
        Err(format!("Window '{}' not found", window_title))
    }
}

#[cfg(target_os = "linux")]
fn set_kde_always_on_top_wayland(enable: bool) -> Result<(), String> {
    // On Wayland, we have limited options
    // KWin (KDE's compositor) can be controlled via D-Bus or KWin scripts
    
    // For now, return an informative error
    Err("Wayland does not support programmatic always-on-top. Please set it manually in KDE window rules.".to_string())
}

#[tauri::command]
pub fn kde_set_always_on_top<R: Runtime>(
    _app: AppHandle<R>,
    _window: WebviewWindow<R>,
    _enable: bool,
) -> Result<String, String> {
    #[cfg(target_os = "linux")]
    {
        let enable = _enable;
        // Check if we're on KDE Plasma
        if !is_kde_plasma() {
            return Ok("Not running on KDE Plasma, skipping KDE-specific window management".to_string());
        }
        
        // Check if we're on X11 or Wayland
        let session_type = std::env::var("XDG_SESSION_TYPE").unwrap_or_default();
        
        if session_type == "wayland" {
            return set_kde_always_on_top_wayland(enable)
                .map(|_| "KDE Wayland always-on-top applied".to_string());
        } else {
            // Assume X11
            let window_title = _window.title().unwrap_or_default();
            return set_kde_always_on_top_x11(&window_title, enable)
                .map(|_| "KDE X11 always-on-top applied successfully".to_string());
        }
    }
    
    #[cfg(not(target_os = "linux"))]
    {
        Ok("KDE always-on-top is only available on Linux".to_string())
    }
}

#[cfg(target_os = "linux")]
fn is_gnome() -> bool {
    if let Ok(desktop) = std::env::var("XDG_CURRENT_DESKTOP") {
        let lower = desktop.to_lowercase();
        if lower.contains("gnome") || lower.contains("unity") || lower.contains("cinnamon") {
            return true;
        }
    }
    
    if let Ok(desktop) = std::env::var("DESKTOP_SESSION") {
        let lower = desktop.to_lowercase();
        if lower.contains("gnome") || lower.contains("ubuntu") {
            return true;
        }
    }
    
    false
}

#[cfg(target_os = "linux")]
fn set_gnome_always_on_top_dbus(window_title: &str, enable: bool) -> Result<(), String> {
    // Use GNOME Shell's D-Bus Eval interface to set/unset always-on-top
    // This works on GNOME Shell versions that expose org.gnome.Shell.Eval
    let action = if enable { "make_above" } else { "unmake_above" };
    
    // Escape the window title for safe embedding in JavaScript
    let safe_title = window_title.replace('\\', "\\\\").replace('\'', "\\'");
    
    let js_code = format!(
        "global.get_window_actors().forEach(function(w) {{ let m = w.get_meta_window(); if (m && m.get_title() && m.get_title().includes('{}')) m.{}(); }});",
        safe_title, action
    );
    
    let output = Command::new("gdbus")
        .args(&[
            "call", "--session",
            "--timeout", "3",
            "--dest", "org.gnome.Shell",
            "--object-path", "/org/gnome/Shell",
            "--method", "org.gnome.Shell.Eval",
            &js_code,
        ])
        .output();
    
    match output {
        Ok(out) => {
            if out.status.success() {
                let stdout = String::from_utf8_lossy(&out.stdout);
                // GNOME Shell Eval returns (true, '') on success
                if stdout.contains("true") {
                    Ok(())
                } else {
                    Err(format!("GNOME Shell Eval returned unexpected result: {}", stdout))
                }
            } else {
                let stderr = String::from_utf8_lossy(&out.stderr);
                Err(format!("gdbus call failed: {}", stderr))
            }
        }
        Err(e) => Err(format!("Failed to execute gdbus: {}", e)),
    }
}

#[cfg(target_os = "linux")]
fn set_gnome_always_on_top_wmctrl(window_title: &str, enable: bool) -> Result<(), String> {
    // Fallback: use wmctrl which works on X11 and sometimes via XWayland
    // Skip if no X display is available (pure Wayland) - wmctrl would hang
    if std::env::var("DISPLAY").unwrap_or_default().is_empty() {
        return Err("No X display available (pure Wayland session), wmctrl cannot be used".to_string());
    }
    
    let state_action = if enable { "add" } else { "remove" };
    
    let output = Command::new("wmctrl")
        .args(&["-l"])
        .output();
    
    if output.is_err() {
        return Err("wmctrl not available".to_string());
    }
    
    let output = output.unwrap();
    let window_list = String::from_utf8_lossy(&output.stdout);
    let mut window_id: Option<String> = None;
    
    for line in window_list.lines() {
        if line.contains(window_title) {
            if let Some(id) = line.split_whitespace().next() {
                window_id = Some(id.to_string());
                break;
            }
        }
    }
    
    if let Some(id) = window_id {
        let result = Command::new("wmctrl")
            .args(&["-i", "-r", &id, "-b", &format!("{},above", state_action)])
            .output();
        
        match result {
            Ok(_) => Ok(()),
            Err(e) => Err(format!("wmctrl failed: {}", e)),
        }
    } else {
        Err(format!("Window '{}' not found via wmctrl", window_title))
    }
}

#[tauri::command]
pub fn gnome_set_always_on_top<R: Runtime>(
    _app: AppHandle<R>,
    _window: WebviewWindow<R>,
    _enable: bool,
) -> Result<String, String> {
    #[cfg(target_os = "linux")]
    {
        let enable = _enable;
        if !is_gnome() {
            return Ok("Not running on GNOME, skipping GNOME-specific window management".to_string());
        }
        
        let window_title = _window.title().unwrap_or_default();
        let session_type = std::env::var("XDG_SESSION_TYPE").unwrap_or_default();
        
        // On Wayland, try D-Bus Eval first, then wmctrl as fallback
        // On X11, try wmctrl first, then D-Bus as fallback
        if session_type == "wayland" {
            // Try GNOME Shell D-Bus Eval (works on GNOME versions with Eval enabled)
            match set_gnome_always_on_top_dbus(&window_title, enable) {
                Ok(_) => return Ok("GNOME Wayland always-on-top applied via D-Bus".to_string()),
                Err(dbus_err) => {
                    // Fallback to wmctrl via XWayland
                    match set_gnome_always_on_top_wmctrl(&window_title, enable) {
                        Ok(_) => return Ok("GNOME Wayland always-on-top applied via wmctrl (XWayland)".to_string()),
                        Err(wmctrl_err) => {
                            return Err(format!(
                                "GNOME Wayland always-on-top failed. D-Bus: {}. wmctrl: {}. \
                                 On GNOME 41+, Shell Eval may be disabled. Try: install gnome-shell-extension 'Allow Unsafe Mode' or use X11 session.",
                                dbus_err, wmctrl_err
                            ));
                        }
                    }
                }
            }
        } else {
            // X11 session - try wmctrl first
            match set_gnome_always_on_top_wmctrl(&window_title, enable) {
                Ok(_) => return Ok("GNOME X11 always-on-top applied via wmctrl".to_string()),
                Err(wmctrl_err) => {
                    // Fallback to D-Bus
                    match set_gnome_always_on_top_dbus(&window_title, enable) {
                        Ok(_) => return Ok("GNOME X11 always-on-top applied via D-Bus".to_string()),
                        Err(dbus_err) => {
                            return Err(format!(
                                "GNOME X11 always-on-top failed. wmctrl: {}. D-Bus: {}. \
                                 Please install wmctrl: sudo apt install wmctrl",
                                wmctrl_err, dbus_err
                            ));
                        }
                    }
                }
            }
        }
    }
    
    #[cfg(not(target_os = "linux"))]
    {
        Ok("GNOME always-on-top is only available on Linux".to_string())
    }
}

#[tauri::command]
pub fn get_desktop_environment() -> String {
    #[cfg(target_os = "linux")]
    {
        let desktop = std::env::var("XDG_CURRENT_DESKTOP").unwrap_or_default();
        let session = std::env::var("DESKTOP_SESSION").unwrap_or_default();
        let session_type = std::env::var("XDG_SESSION_TYPE").unwrap_or_default();
        
        format!("Desktop: {}, Session: {}, Type: {}", desktop, session, session_type)
    }
    
    #[cfg(not(target_os = "linux"))]
    {
        std::env::consts::OS.to_string()
    }
}
