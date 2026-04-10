using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using BlueMeter.Config;
using BlueMeter.Extensions;
using BlueMeter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

/// <summary>
/// Avalonia implementation of <see cref="IGlobalHotkeyService"/>.
///
/// The WPF version used <c>HwndSource.AddHook</c> against the DPS statistics window
/// HWND and <c>KeyInterop.VirtualKeyFromKey</c>. The Avalonia port uses
/// <see cref="Avalonia.Controls.Win32Properties.AddWndProcHookCallback"/> for the
/// WndProc hook, <see cref="TopLevel.TryGetPlatformHandle"/> for the HWND, and a
/// hand-rolled <see cref="KeyExtension.ToVirtualKey"/> for Key → VK mapping (the
/// built-in KeyInterop in <c>Avalonia.Win32</c> is internal).
///
/// Behavior is otherwise identical: same hotkey IDs, same WM_HOTKEY (0x0312)
/// handling, same three actions (mouse-through toggle, topmost toggle, reset
/// statistics), and the same "only re-register on actual hotkey change" guard in
/// <see cref="UpdateFromConfig"/>.
/// </summary>
public sealed class GlobalHotkeyService : IGlobalHotkeyService
{
    private const uint WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID_MOUSETHROUGH = 0x1001;
    private const int HOTKEY_ID_TOPMOST = 0x1002;
    private const int HOTKEY_ID_RESET_STATISTIC = 0x1003;

    private readonly ILogger<GlobalHotkeyService> _logger;
    private readonly IWindowManagementService _windowManager;
    private readonly IConfigManager _configManager;
    private readonly IMousePenetrationService _mousePenetration;
    private readonly ITopmostService _topmostService;
    private readonly DpsStatisticsViewModel _dpsStatisticsViewModel;

    private AppConfig _config;
    private Window? _hostWindow;
    private IntPtr _hostHandle;
    private Win32Properties.CustomWndProcHookCallback? _hookCallback;

    public GlobalHotkeyService(
        ILogger<GlobalHotkeyService> logger,
        IWindowManagementService windowManager,
        IConfigManager configManager,
        IMousePenetrationService mousePenetration,
        ITopmostService topmostService,
        DpsStatisticsViewModel dpsStatisticsViewModel)
    {
        _logger = logger;
        _windowManager = windowManager;
        _configManager = configManager;
        _mousePenetration = mousePenetration;
        _topmostService = topmostService;
        _dpsStatisticsViewModel = dpsStatisticsViewModel;
        _config = configManager.CurrentConfig;
    }

    public void Start()
    {
        _hostWindow = _windowManager.GetDpsStatisticsWindow();
        AttachMessageHook();
        _configManager.ConfigurationUpdated += OnConfigUpdated;
    }

    public void Stop()
    {
        try
        {
            UnregisterAll();
        }
        finally
        {
            DetachMessageHook();
            _configManager.ConfigurationUpdated -= OnConfigUpdated;
        }
    }

    public void UpdateFromConfig(AppConfig config)
    {
        bool hotkeysChanged =
            _config.MouseThroughShortcut != config.MouseThroughShortcut ||
            _config.ClearDataShortcut != config.ClearDataShortcut ||
            _config.TopmostShortcut != config.TopmostShortcut;

        bool toggleChanged = _config.GlobalHotkeysEnabled != config.GlobalHotkeysEnabled;

        _config = config;

        if (!hotkeysChanged && !toggleChanged)
        {
            return;
        }

        // Ensure all (un)registration runs on the UI thread that owns the window.
        if (Dispatcher.UIThread.CheckAccess())
        {
            UnregisterAll();
            RegisterAll();
        }
        else
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                UnregisterAll();
                RegisterAll();
            });
        }
    }

    private void OnConfigUpdated(object? sender, AppConfig e)
    {
        UpdateFromConfig(e);
    }

    /// <summary>
    /// Attach the WndProc hook. If the host window hasn't been shown yet (its
    /// native HWND isn't available), the hook attachment and hotkey registration
    /// are deferred to <see cref="Window.Opened"/> — same shape as
    /// <see cref="MousePenetrationService"/>.
    /// </summary>
    private void AttachMessageHook()
    {
        if (_hostWindow is null || _hookCallback is not null) return;

        var handle = _hostWindow.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero)
        {
            void Handler(object? s, EventArgs e)
            {
                _hostWindow.Opened -= Handler;
                AttachMessageHook();
                RegisterAll();
            }

            _hostWindow.Opened += Handler;
            return;
        }

        _hostHandle = handle;
        _hookCallback = WndProc;
        Win32Properties.AddWndProcHookCallback(_hostWindow, _hookCallback);
        RegisterAll();
    }

    private void DetachMessageHook()
    {
        if (_hostWindow is null || _hookCallback is null) return;
        Win32Properties.RemoveWndProcHookCallback(_hostWindow, _hookCallback);
        _hookCallback = null;
        _hostHandle = IntPtr.Zero;
    }

    private void RegisterAll()
    {
        try
        {
            if (!_config.GlobalHotkeysEnabled)
            {
                _logger.LogInformation("Global hotkeys are disabled. Skipping registration.");
                return;
            }

            RegisterMouseThroughHotkey();
            RegisterTopmostHotkey();
            RegisterResetDpsStatistic();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RegisterAll hotkeys failed");
        }
    }

    private void UnregisterAll()
    {
        try
        {
            if (_hostHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_hostHandle, HOTKEY_ID_MOUSETHROUGH);
                UnregisterHotKey(_hostHandle, HOTKEY_ID_TOPMOST);
                UnregisterHotKey(_hostHandle, HOTKEY_ID_RESET_STATISTIC);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "UnregisterAll hotkeys failed");
        }
    }

    private void RegisterMouseThroughHotkey()
    {
        var key = _config.MouseThroughShortcut.Key;
        var mods = _config.MouseThroughShortcut.Modifiers;
        if (key == Key.None || _hostHandle == IntPtr.Zero) return;

        var (vk, fsMods) = ToNative(key, mods);
        TryRegisterHotKey(_hostHandle, HOTKEY_ID_MOUSETHROUGH, fsMods, vk, key, mods);
    }

    private void RegisterTopmostHotkey()
    {
        var key = _config.TopmostShortcut.Key;
        var mods = _config.TopmostShortcut.Modifiers;
        if (key == Key.None || _hostHandle == IntPtr.Zero) return;

        var (vk, fsMods) = ToNative(key, mods);
        TryRegisterHotKey(_hostHandle, HOTKEY_ID_TOPMOST, fsMods, vk, key, mods);
    }

    private void RegisterResetDpsStatistic()
    {
        var key = _config.ClearDataShortcut.Key;
        var mods = _config.ClearDataShortcut.Modifiers;
        if (key == Key.None || _hostHandle == IntPtr.Zero) return;

        var (vk, fsMods) = ToNative(key, mods);
        TryRegisterHotKey(_hostHandle, HOTKEY_ID_RESET_STATISTIC, fsMods, vk, key, mods);
    }

    private static (uint vk, uint fsMods) ToNative(Key key, KeyModifiers mods)
    {
        var vk = key.ToVirtualKey();
        uint fs = 0;
        if (mods.HasFlag(KeyModifiers.Alt)) fs |= 0x0001; // MOD_ALT
        if (mods.HasFlag(KeyModifiers.Control)) fs |= 0x0002; // MOD_CONTROL
        if (mods.HasFlag(KeyModifiers.Shift)) fs |= 0x0004; // MOD_SHIFT
        // ignore meta / windows key by design
        return (vk, fs);
    }

    private bool TryRegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk, Key key, KeyModifiers mods,
        [CallerMemberName] string? name = null)
    {
        // Always attempt to unregister first (safe even if not registered).
        UnregisterHotKey(hWnd, id);

        if (vk == 0)
        {
            _logger.LogWarning(
                "RegisterHotKey skipped for {Name}: Invalid virtual key code for {Key}. " +
                "This key is not supported by Windows (possibly a hardware-specific key like Fn or custom keyboard software key).",
                name, key);
            return false;
        }

        if (!RegisterHotKey(hWnd, id, fsModifiers, vk))
        {
            var error = Marshal.GetLastWin32Error();
            var errorDescription = error switch
            {
                1409 => "Hotkey already registered by another application",
                87 => "Invalid parameter (key not supported)",
                _ => $"Win32 error code {error}"
            };

            _logger.LogWarning(
                "RegisterHotKey failed for {Name}: {Key}+{Mods}. {ErrorDescription}. " +
                "If using special keyboard software (Razer, Corsair, Wooting, etc.), it may be intercepting the key. " +
                "Try a different key or disable keyboard software temporarily.",
                name, key, mods, errorDescription);
            return false;
        }

        _logger.LogInformation("Successfully registered hotkey {Name}: {Key}+{Mods}", name, key, mods);
        return true;
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_HOTKEY) return IntPtr.Zero;

        // Re-check the live config (matches WPF behavior — the registration-time
        // snapshot may be stale if a toggle came in between register and fire).
        if (!_configManager.CurrentConfig.GlobalHotkeysEnabled)
        {
            return IntPtr.Zero;
        }

        var id = wParam.ToInt32();

        switch (id)
        {
            case HOTKEY_ID_MOUSETHROUGH:
                ToggleMouseThrough();
                handled = true;
                break;
            case HOTKEY_ID_TOPMOST:
                ToggleTopmost();
                handled = true;
                break;
            case HOTKEY_ID_RESET_STATISTIC:
                TriggerReset();
                handled = true;
                break;
        }

        return IntPtr.Zero;
    }

    private void ToggleMouseThrough()
    {
        try
        {
            if (_hostWindow is null) return;
            var newState = !_config.MouseThroughEnabled;
            _config.MouseThroughEnabled = newState;
            _mousePenetration.SetMousePenetrate(_hostWindow, newState);
            _ = _configManager.SaveAsync(_config); // persist asynchronously
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ToggleMouseThrough failed");
        }
    }

    private void ToggleTopmost()
    {
        try
        {
            if (_hostWindow is null) return;
            var newState = !_hostWindow.Topmost; // source of truth is window state
            _topmostService.SetTopmost(_hostWindow, newState);
            _config.TopmostEnabled = newState;
            _ = _configManager.SaveAsync(_config);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ToggleTopmost failed");
        }
    }

    private void TriggerReset()
    {
        try
        {
            _logger.LogInformation("TriggerReset called - resetting ALL DPS statistics");
            _dpsStatisticsViewModel.ResetAll();
            _logger.LogInformation("TriggerReset completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TriggerReset failed");
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

public static class GlobalHotkeyServiceExtensions
{
    public static IServiceCollection AddGlobalHotkeyService(this IServiceCollection services)
    {
        services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
        return services;
    }
}
