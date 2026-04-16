using System.Text.Json.Serialization;

namespace LibreLinkupDesktopUnofficial.Models;

public class AppSettings
{
    public string Country { get; set; } = "global";
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool AlwaysOnTop { get; set; }
    public string? Token { get; set; }
    public string? AccountId { get; set; }
    public string? AccountCountry { get; set; }
}
