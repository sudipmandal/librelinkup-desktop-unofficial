using System;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace LibreLinkupDesktopUnofficial.Services;

public class LogService
{
    private const int MaxLines = 200;

    public ObservableCollection<string> Lines { get; } = new();

    public void Log(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        var line = $"[{ts}] {message}";
        Lines.Add(line);
        TrimLines();
    }

    public void LogError(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        var line = $"[{ts}] [ERR] {message}";
        Lines.Add(line);
        TrimLines();
    }

    public void LogWarn(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        var line = $"[{ts}] [WARN] {message}";
        Lines.Add(line);
        TrimLines();
    }

    public void LogObject(string label, object? obj)
    {
        try
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = false });
            Log($"{label}: {json}");
        }
        catch
        {
            Log($"{label}: {obj}");
        }
    }

    private void TrimLines()
    {
        while (Lines.Count > MaxLines)
        {
            Lines.RemoveAt(0);
        }
    }
}
