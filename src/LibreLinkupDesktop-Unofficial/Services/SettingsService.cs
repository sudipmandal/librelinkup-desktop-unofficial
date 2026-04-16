using System;
using System.IO;
using System.Text.Json;
using LibreLinkupDesktopUnofficial.Models;

namespace LibreLinkupDesktopUnofficial.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;

    public SettingsService()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LibreLinkupDesktop-Unofficial");
        Directory.CreateDirectory(appDataDir);
        _settingsPath = Path.Combine(appDataDir, "settings.json");
        _settings = Load();
    }

    public AppSettings Settings => _settings;

    public void Save()
    {
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json);
    }

    private AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // If settings file is corrupted, start fresh
        }

        return new AppSettings();
    }
}
