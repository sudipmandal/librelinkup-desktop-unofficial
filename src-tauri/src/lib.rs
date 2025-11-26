mod window_manager;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_store::Builder::new().build())
        .plugin(tauri_plugin_http::init())
        .invoke_handler(tauri::generate_handler![
            window_manager::kde_set_always_on_top,
            window_manager::get_desktop_environment
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
