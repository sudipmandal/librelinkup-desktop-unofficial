using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LibreLinkupDesktopUnofficial.Services;
using LibreLinkupDesktopUnofficial.ViewModels;
using LibreLinkupDesktopUnofficial.Views;

namespace LibreLinkupDesktopUnofficial;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = new SettingsService();
            var logService = new LogService();
            var linkupService = new LinkupService(logService);

            var mainViewModel = new MainWindowViewModel(settingsService, logService, linkupService);
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
