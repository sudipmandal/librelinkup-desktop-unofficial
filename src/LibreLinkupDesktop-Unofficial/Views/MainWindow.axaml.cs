using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LibreLinkupDesktopUnofficial.ViewModels;

namespace LibreLinkupDesktopUnofficial.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel != null)
        {
            _viewModel.RequestLoginSize += () =>
            {
                Width = 800;
                Height = 600;
            };
            _viewModel.RequestCompactSize += () =>
            {
                Width = 249;
                Height = 58;
            };
            _viewModel.RequestAlwaysOnTop += (enable) =>
            {
                Topmost = enable;
            };

            if (_viewModel.AlwaysOnTop)
                Topmost = true;

            await _viewModel.InitializeAsync();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _viewModel?.Cleanup();
        base.OnClosing(e);
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void OnMinimize(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximize(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnLogoutClick(object? sender, RoutedEventArgs e)
    {
        _viewModel?.LogoutCommand.Execute(null);
    }
}
