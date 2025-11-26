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
    enable: bool,
) -> Result<String, String> {
    #[cfg(target_os = "linux")]
    {
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
