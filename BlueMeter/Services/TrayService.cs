using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using BlueMeter.Logging;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

/// <summary>
/// Avalonia implementation of <see cref="ITrayService"/> built on the framework's
/// built-in <see cref="TrayIcon"/> + <see cref="NativeMenu"/> APIs (no third-party
/// notify-icon dependency).
/// </summary>
public sealed class TrayService : ITrayService, IDisposable
{
    private readonly ILogger<TrayService>? _logger;
    private TrayIcon? _tray;
    private bool _initialized;

    public TrayService(ILogger<TrayService>? logger = null)
    {
        _logger = logger;
    }

    public void Initialize(string? toolTip = null)
    {
        if (_initialized) return;
        _initialized = true;

        _tray = new TrayIcon
        {
            ToolTipText = toolTip ?? GetMainWindow()?.Title ?? "BlueMeter",
            IsVisible = true
        };

        try
        {
            var iconUri = new Uri("avares://BlueMeter/Assets/Images/ApplicationIcon.ico");
            using var stream = AssetLoader.Open(iconUri);
            _tray.Icon = new WindowIcon(stream);
        }
        catch (Exception ex)
        {
            // Fall back to the default system icon rather than crash startup,
            // but make the failure visible — a silent catch here is how we
            // ended up with an invisible tray glyph for the whole smoke test.
            _logger?.LogWarning(ex, "Failed to load tray icon; falling back to default");
        }

        var menu = new NativeMenu();
        var miShow = new NativeMenuItem("Show");
        miShow.Click += (_, _) => Restore();
        var miExit = new NativeMenuItem("Exit");
        miExit.Click += (_, _) => Exit();
        menu.Add(miShow);
        menu.Add(new NativeMenuItemSeparator());
        menu.Add(miExit);
        _tray.Menu = menu;
        _tray.Clicked += (_, _) => Restore();

        _logger?.LogInformation(LogEvents.TrayInit, "Tray initialized");
    }

    public void MinimizeToTray()
    {
        var main = GetMainWindow();
        if (main == null) return;
        Dispatcher.UIThread.Post(() => main.Hide());
        _logger?.LogDebug(LogEvents.TrayMinimize, "Window minimized to tray");
    }

    public void Restore()
    {
        var main = GetMainWindow();
        if (main == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            main.Show();
            if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
            main.Activate();
        });
        _logger?.LogDebug(LogEvents.TrayRestore, "Window restored from tray");
    }

    public void Exit()
    {
        _logger?.LogInformation(LogEvents.TrayExit, "Tray exit requested");
        Dispose();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    public void Dispose()
    {
        try { _tray?.Dispose(); }
        catch
        {
            // Ignore
        }
        _tray = null;
    }

    private static Window? GetMainWindow()
    {
        return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
    }
}
