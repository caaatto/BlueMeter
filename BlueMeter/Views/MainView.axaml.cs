using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Main application window code-behind.
///
/// Port notes (WPF -> Avalonia):
///   - WPF's <c>Loaded</c> routed event has no Window equivalent — use
///     <see cref="Window.Opened"/> which fires once when the window first
///     appears. The handler kicks off tray initialization.
///   - WPF used <c>StateChanged</c> + a switch on <c>WindowState</c> to detect
///     minimize. Avalonia exposes <see cref="Window.WindowStateProperty"/> via
///     observable subscription instead, with the same effect.
///   - <c>Closing</c> → <see cref="OnClosing"/> override. Default behavior is
///     "minimize to tray instead of exit" — the user explicitly Exits via the
///     tray menu (which calls <c>ExitFromTrayCommand</c> on the VM).
///   - Footer's confirm button is wired in XAML to <see cref="OnFooterConfirmClick"/>
///     to minimize the window (matches the WPF "OK button = minimize" behavior;
///     the cancel button binds to ShutdownCommand via the Footer's CancelCommand
///     styled property).
///   - <see cref="IsDebugContentVisible"/> is a #if DEBUG-gated bool exposed to
///     the XAML so the debug toggle is hidden in Release builds without
///     conditional compilation in the XAML itself.
///   - <see cref="MainViewModel.Dispose"/> is invoked from <see cref="OnClosed"/>
///     to release the LocalizationManager / ConfigManager event subscriptions
///     so transient VM instances can be GC'd.
/// </summary>
public partial class MainView : Window
{
    private MainViewModel? _viewModel;

    public MainView()
    {
        InitializeComponent();

        Opened += OnOpened;
    }

    public MainView(MainViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (_viewModel?.InitializeTrayCommand.CanExecute(null) == true)
        {
            _viewModel.InitializeTrayCommand.Execute(null);
        }
    }

    /// <summary>
    /// WPF used a <c>StateChanged</c> handler to detect minimize. Avalonia's
    /// <c>Window</c> raises property changes for <see cref="WindowStateProperty"/>
    /// through the standard <see cref="OnPropertyChanged(AvaloniaPropertyChangedEventArgs)"/>
    /// pipeline; check for the minimized transition there.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WindowStateProperty &&
            change.GetNewValue<WindowState>() == WindowState.Minimized &&
            _viewModel is not null)
        {
            _viewModel.MinimizeToTrayCommand.Execute(null);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Close-button behavior: minimize to tray instead of exit. The user
        // explicitly leaves the app via the tray menu's Exit entry, which
        // routes through ExitFromTrayCommand and calls IApplicationControlService.Shutdown().
        e.Cancel = true;
        _viewModel?.MinimizeToTrayCommand.Execute(null);
        base.OnClosing(e);
    }

    private void OnUpdateBannerTapped(object? sender, TappedEventArgs e)
    {
        if (_viewModel?.OpenReleaseUrlCommand.CanExecute(null) == true)
        {
            _viewModel.OpenReleaseUrlCommand.Execute(null);
        }
    }

    private void OnFooterConfirmClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel?.Dispose();
        base.OnClosed(e);
    }

    /// <summary>
    /// Hardcoded #if DEBUG flag exposed to XAML so the debug-tab toggle and
    /// debug content panel can be hidden in Release builds without conditional
    /// compilation inside the XAML file.
    /// </summary>
    public bool IsDebugContentVisible { get; } =
#if DEBUG
        true;
#else
        false;
#endif
}
