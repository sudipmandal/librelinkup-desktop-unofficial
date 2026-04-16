using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibreLinkupDesktopUnofficial.Models;
using LibreLinkupDesktopUnofficial.Services;

namespace LibreLinkupDesktopUnofficial.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly LogService _logService;
    private readonly LinkupService _linkupService;

    private Timer? _refreshTimer;
    private Timer? _retryTimer;
    private Timer? _alwaysOnTopTimer;

    [ObservableProperty] private string _country = "global";
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool _alwaysOnTop;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isLoggedIn;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private CgmConnectionData? _cgmData;
    [ObservableProperty] private IBrush _glucoseBackgroundBrush = new SolidColorBrush(Color.Parse("#e81123"));

    [ObservableProperty] private string _glucoseValue = "--";
    [ObservableProperty] private string _glucoseUnit = "mmol/L";
    [ObservableProperty] private string _glucoseTrend = "?";
    [ObservableProperty] private string _glucoseTimestamp = "";

    [ObservableProperty] private int _selectedCountryIndex;

    public ObservableCollection<string> LogLines => _logService.Lines;

    public event Action? RequestLoginSize;
    public event Action? RequestCompactSize;
    public event Action<bool>? RequestAlwaysOnTop;

    public List<CountryOption> Countries { get; } = new()
    {
        new() { Code = "global", Name = "Global" },
        new() { Code = "de", Name = "Germany" },
        new() { Code = "eu", Name = "European Union" },
        new() { Code = "eu2", Name = "European Union 2" },
        new() { Code = "us", Name = "United States" },
        new() { Code = "ap", Name = "Asia/Pacific" },
        new() { Code = "ca", Name = "Canada" },
        new() { Code = "jp", Name = "Japan" },
        new() { Code = "ae", Name = "United Arab Emirates" },
        new() { Code = "fr", Name = "France" },
        new() { Code = "au", Name = "Australia" },
    };

    public bool IsLoginEnabled => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !IsLoading;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasLogLines => _logService.Lines.Count > 0;

    public MainWindowViewModel(SettingsService settingsService, LogService logService, LinkupService linkupService)
    {
        _settingsService = settingsService;
        _logService = logService;
        _linkupService = linkupService;

        _logService.Lines.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasLogLines));

        LoadSettings();
    }

    partial void OnEmailChanged(string value) => OnPropertyChanged(nameof(IsLoginEnabled));
    partial void OnPasswordChanged(string value) => OnPropertyChanged(nameof(IsLoginEnabled));
    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsLoginEnabled));
        OnPropertyChanged(nameof(LoginButtonText));
    }
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    public string LoginButtonText => IsLoading ? "Logging in..." : "Login";

    partial void OnAlwaysOnTopChanged(bool value)
    {
        RequestAlwaysOnTop?.Invoke(value);
        StopAlwaysOnTopTimer();

        if (value)
        {
            _alwaysOnTopTimer = new Timer(_ =>
            {
                Dispatcher.UIThread.Post(() => RequestAlwaysOnTop?.Invoke(true));
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }
    }

    private void LoadSettings()
    {
        var s = _settingsService.Settings;
        Country = s.Country;
        Email = s.Email;
        Password = s.Password;
        AlwaysOnTop = s.AlwaysOnTop;

        SelectedCountryIndex = Countries.FindIndex(c => c.Code == Country);
        if (SelectedCountryIndex < 0) SelectedCountryIndex = 0;
    }

    public async Task InitializeAsync()
    {
        _logService.Log($"Platform: {Environment.OSVersion.Platform}");

        if (!string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
        {
            _logService.Log("Auto-login: credentials found, attempting login...");
            await HandleLoginAsync();
        }
    }

    [RelayCommand]
    private async Task HandleLoginAsync()
    {
        _logService.Log($"Login attempt: country={Country}, email={Email}");

        ErrorMessage = string.Empty;
        StopRetryTimer();

        SaveFormValues();

        try
        {
            IsLoading = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            _logService.Log("Calling getAuthToken...");

            var (result, error) = await _linkupService.GetAuthTokenAsync(Country, Email, Password, cts.Token);

            if (error != null || result == null)
            {
                _logService.Log($"Login failed: {error}");
                await HandleErrorAsync(error ?? "Login failed. Please check your credentials.");
                return;
            }

            _logService.Log($"Login successful, country={result.AccountCountry}");

            _settingsService.Settings.Token = result.Token;
            _settingsService.Settings.AccountId = result.AccountId;
            _settingsService.Settings.AccountCountry = result.AccountCountry;
            _settingsService.Save();

            _logService.Log("Token stored, fetching CGM data...");
            await FetchCgmDataAsync();
        }
        catch (Exception ex)
        {
            _logService.Log($"Login error: {ex.Message}");
            await HandleErrorAsync(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task FetchCgmDataAsync()
    {
        try
        {
            var token = _settingsService.Settings.Token;
            var accountId = _settingsService.Settings.AccountId;
            var accountCountry = _settingsService.Settings.AccountCountry;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(accountId))
            {
                _logService.Log("Missing token or account ID");
                return;
            }

            _logService.Log($"Fetching CGM data for country={accountCountry}...");

            var (data, error) = await _linkupService.GetCgmDataAsync(
                token, accountCountry ?? Country, accountId);

            if (error != null || data == null)
            {
                _logService.Log($"Failed to fetch CGM data: {error}");
                await HandleErrorAsync(error ?? "Failed to fetch CGM data. Retrying...");
                return;
            }

            _logService.Log("CGM data received successfully");
            CgmData = data;
            UpdateGlucoseDisplay();

            IsLoggedIn = true;
            RequestCompactSize?.Invoke();

            StopRefreshTimer();
            _refreshTimer = new Timer(async _ =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () => await FetchCgmDataAsync());
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }
        catch (Exception ex)
        {
            _logService.Log($"CGM data error: {ex.Message}");
            await HandleErrorAsync(ex.Message);
        }
    }

    private void UpdateGlucoseDisplay()
    {
        if (CgmData?.GlucoseMeasurement == null)
        {
            GlucoseValue = "--";
            GlucoseUnit = "mmol/L";
            GlucoseTrend = "?";
            GlucoseTimestamp = "";
            GlucoseBackgroundBrush = new SolidColorBrush(Color.Parse("#e81123"));
            return;
        }

        var gm = CgmData.GlucoseMeasurement;
        GlucoseValue = gm.Value.ToString("F1");
        GlucoseUnit = gm.ValueInMgPerDl ? "mg/dL" : "mmol/L";
        GlucoseTrend = GetTrendArrow(gm.TrendArrow);

        try
        {
            var dt = DateTime.Parse(gm.Timestamp);
            GlucoseTimestamp = dt.ToString("g");
        }
        catch
        {
            GlucoseTimestamp = gm.Timestamp;
        }

        var value = gm.Value;
        if (value >= 4.2 && value <= 10)
            GlucoseBackgroundBrush = new SolidColorBrush(Color.Parse("#28a745"));
        else if (value >= 10.1 && value <= 13.9)
            GlucoseBackgroundBrush = new SolidColorBrush(Color.Parse("#fd7e14"));
        else
            GlucoseBackgroundBrush = new SolidColorBrush(Color.Parse("#e81123"));
    }

    private static string GetTrendArrow(int trend) => trend switch
    {
        1 => "↓↓",
        2 => "↓",
        3 => "→",
        4 => "↑",
        5 => "↑↑",
        _ => "?"
    };

    private async Task HandleErrorAsync(string errorMessage)
    {
        _logService.Log($"Error: {errorMessage}");
        ErrorMessage = errorMessage;

        StopRefreshTimer();
        IsLoggedIn = false;
        CgmData = null;

        RequestLoginSize?.Invoke();

        if (_retryTimer == null)
        {
            _logService.Log("Setting up auto-retry every 10 seconds...");
            _retryTimer = new Timer(async _ =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    _logService.Log("Auto-retrying login...");
                    await HandleLoginAsync();
                });
            }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }
    }

    [RelayCommand]
    private void Logout()
    {
        ErrorMessage = string.Empty;
        StopRetryTimer();
        StopRefreshTimer();

        IsLoggedIn = false;
        CgmData = null;
        RequestLoginSize?.Invoke();
    }

    public void OnCountrySelectionChanged(int index)
    {
        if (index >= 0 && index < Countries.Count)
        {
            Country = Countries[index].Code;
        }
    }

    private void SaveFormValues()
    {
        _settingsService.Settings.Country = Country;
        _settingsService.Settings.Email = Email;
        _settingsService.Settings.Password = Password;
        _settingsService.Settings.AlwaysOnTop = AlwaysOnTop;
        _settingsService.Save();

        _logService.Log($"Form values saved: country={Country}, email={Email}, alwaysOnTop={AlwaysOnTop}");
    }

    private void StopRefreshTimer()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    private void StopRetryTimer()
    {
        _retryTimer?.Dispose();
        _retryTimer = null;
    }

    private void StopAlwaysOnTopTimer()
    {
        _alwaysOnTopTimer?.Dispose();
        _alwaysOnTopTimer = null;
    }

    public void Cleanup()
    {
        StopRefreshTimer();
        StopRetryTimer();
        StopAlwaysOnTopTimer();
    }
}
