using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging.Abstractions;
using BlueMeter.Config;
using BlueMeter.Extensions;
using BlueMeter.Localization;
using BlueMeter.Models;
using BlueMeter.Properties;
using BlueMeter.Services;
using AppConfig = BlueMeter.Config.AppConfig;
using KeyBinding = BlueMeter.Models.KeyBinding;

namespace BlueMeter.ViewModels;

public partial class SettingsViewModel(
    IConfigManager configManager,
    IDeviceManagementService deviceManagementService,
    LocalizationManager localization,
    IMessageDialogService messageDialogService,
    IGlobalHotkeyService globalHotkeyService,
    ISoundPlayerService soundPlayerService,
    IQueuePopUIDetector queuePopUIDetector)
    : BaseViewModel, IDisposable
{
    // Expose the global hotkey service so it can be accessed by behaviors
    public IGlobalHotkeyService GlobalHotkeyService => globalHotkeyService;

    [ObservableProperty] private AppConfig _appConfig = configManager.CurrentConfig.Clone(); // Initialized here with a cloned config; may be overwritten in LoadedAsync

    [ObservableProperty]
    private List<Option<Language>> _availableLanguages =
    [
        new(Language.Auto, Language.Auto.GetLocalizedDescription()),
        new(Language.ZhCn, Language.ZhCn.GetLocalizedDescription()),
        new(Language.EnUs, Language.EnUs.GetLocalizedDescription()),
        new(Language.PtBr, Language.PtBr.GetLocalizedDescription()),
    ];

    [ObservableProperty] private List<NetworkAdapterInfo> _availableNetworkAdapters = [];

    [ObservableProperty]
    private List<Option<NumberDisplayMode>> _availableNumberDisplayModes =
    [
        new(NumberDisplayMode.Wan, NumberDisplayMode.Wan.GetLocalizedDescription()),
        new(NumberDisplayMode.KMB, NumberDisplayMode.KMB.GetLocalizedDescription())
    ];

    [ObservableProperty]
    private List<QueuePopSound> _availableQueuePopSounds =
    [
        QueuePopSound.Drum,
        QueuePopSound.Harp,
        QueuePopSound.Wow,
        QueuePopSound.Yoooo,
        QueuePopSound.DungeonFound,
        QueuePopSound.QPop
    ];

    [ObservableProperty]
    private List<Option<string>> _availablePanelColorModes =
    [
        new("Light", "Light (White)"),
        new("Dark", "Dark (Gray)")
    ];

    private bool _cultureHandlerSubscribed;
    private bool _networkHandlerSubscribed;
    private bool _isLoaded; // becomes true after LoadedAsync completes
    private bool _hasUnsavedChanges; // tracks whether any property changed after load

    [ObservableProperty] private Option<Language>? _selectedLanguage;
    [ObservableProperty] private Option<NumberDisplayMode>? _selectedNumberDisplayMode;
    [ObservableProperty] private Option<string>? _selectedPanelColorMode;

    public event Action? RequestClose;

    partial void OnAppConfigChanging(AppConfig value)
    {
        // Unsubscribe from the old instance before changing
        _appConfig.PropertyChanged -= OnAppConfigPropertyChanged;
    }

    partial void OnAppConfigChanged(AppConfig value)
    {
        // Subscribe to the new instance
        value.PropertyChanged += OnAppConfigPropertyChanged;

        localization.ApplyLanguage(value.Language);
        UpdateLanguageDependentCollections();
        SyncOptions();
    }

    partial void OnSelectedNumberDisplayModeChanged(Option<NumberDisplayMode>? value)
    {
        if (value == null) return;
        AppConfig.DamageDisplayType = value.Value;
    }

    partial void OnSelectedPanelColorModeChanged(Option<string>? value)
    {
        if (value == null) return;
        AppConfig.PanelColorMode = value.Value;
    }

    partial void OnSelectedLanguageChanged(Option<Language>? value)
    {
        if (value == null) return;
        AppConfig.Language = value.Value;
        localization.ApplyLanguage(value.Value);
    }

    partial void OnAvailableNetworkAdaptersChanged(List<NetworkAdapterInfo> value)
    {
        AppConfig.PreferredNetworkAdapter ??= value.FirstOrDefault();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task LoadedAsync()
    {
        AppConfig = configManager.CurrentConfig.Clone();

        SubscribeHandlers();

        UpdateLanguageDependentCollections();
        localization.ApplyLanguage(AppConfig.Language);
        await LoadNetworkAdaptersAsync();

        _hasUnsavedChanges = false;
        _isLoaded = true;
    }

    private void SubscribeHandlers()
    {
        if (!_cultureHandlerSubscribed)
        {
            localization.CultureChanged += OnCultureChanged;
            _cultureHandlerSubscribed = true;
        }

        if (!_networkHandlerSubscribed)
        {
            NetworkChange.NetworkAvailabilityChanged += OnSystemNetworkChanged;
            NetworkChange.NetworkAddressChanged += OnSystemNetworkChanged;
            _networkHandlerSubscribed = true;
        }
    }

    private async Task LoadNetworkAdaptersAsync()
    {
        var adapters = await deviceManagementService.GetNetworkAdaptersAsync();
        AvailableNetworkAdapters = adapters.Select(a => new NetworkAdapterInfo(a.name, a.description)).ToList();
        AppConfig.PreferredNetworkAdapter =
            AvailableNetworkAdapters.FirstOrDefault(a => a.Name == AppConfig.PreferredNetworkAdapter?.Name);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task NetworkAdapterAutoSelect()
    {
        var ret = await deviceManagementService.GetAutoSelectedNetworkAdapterAsync();
        if (ret != null)
        {
            AppConfig.PreferredNetworkAdapter = ret;
            deviceManagementService.SetActiveNetworkAdapter(ret);
            return;
        }
        await messageDialogService.ShowWarningAsync(
            localization.GetString(ResourcesKeys.Settings_NetworkAdapterAutoSelect_Failed),
            localization.GetString(ResourcesKeys.Settings_NetworkAdapterAutoSelect_Failed));
    }

    private async void OnSystemNetworkChanged(object? sender, EventArgs e)
    {
        try
        {
            await LoadNetworkAdaptersAsync();
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Handle shortcut key input for mouse through shortcut
    /// </summary>
    [RelayCommand]
    private void HandleMouseThroughShortcut(object parameter)
    {
        if (parameter is KeyEventArgs e)
        {
            _ = HandleShortcutInputAsync(e, ShortcutType.MouseThrough);
        }
    }

    /// <summary>
    /// Handle shortcut key input for clear data shortcut
    /// </summary>
    /// <param name="parameter">KeyEventArgs from the view</param>
    [RelayCommand]
    private void HandleClearDataShortcut(object parameter)
    {
        if (parameter is KeyEventArgs e)
        {
            _ = HandleShortcutInputAsync(e, ShortcutType.ClearData);
        }
    }

    [RelayCommand]
    private void HandleTopMostShortcut(object parameter)
    {
        if (parameter is KeyEventArgs e)
        {
            _ = HandleShortcutInputAsync(e, ShortcutType.TopMost);
        }
    }

    private void OnAppConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AppConfig config)
        {
            return;
        }

        if (e.PropertyName == nameof(AppConfig.Language))
        {
            localization.ApplyLanguage(config.Language);
            UpdateLanguageDependentCollections();
        }
        else if (e.PropertyName == nameof(AppConfig.QueuePopSoundEnabled))
        {
            // Start or stop the queue pop UI detector based on the setting
            if (config.QueuePopSoundEnabled)
            {
                if (!queuePopUIDetector.IsRunning)
                {
                    queuePopUIDetector.Start();
                }
            }
            else
            {
                if (queuePopUIDetector.IsRunning)
                {
                    queuePopUIDetector.Stop();
                }
            }
        }
        else if (e.PropertyName == nameof(AppConfig.PreferredNetworkAdapter))
        {
            var adapter = AppConfig.PreferredNetworkAdapter;
            if (adapter != null)
            {
                deviceManagementService.SetActiveNetworkAdapter(adapter);
            }
        }
        else if (e.PropertyName == nameof(AppConfig.GlobalHotkeysEnabled))
        {
            // Save immediately when toggle changes to make it work live without Apply button
            // This triggers ConfigurationUpdated event → GlobalHotkeyService.UpdateFromConfig()
            if (_isLoaded)
            {
                _ = configManager.SaveAsync(config);
            }
        }

        if (_isLoaded)
        {
            _hasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Generic shortcut input handler
    /// </summary>
    private async Task HandleShortcutInputAsync(KeyEventArgs e, ShortcutType shortcutType)
    {
        e.Handled = true; // we'll handle the key

        var modifiers = e.KeyModifiers;
        var key = e.Key;

        // Allow Delete to clear
        if (key == Key.Delete)
        {
            ClearShortcut(shortcutType);
            return;
        }

        // Ignore modifier-only presses
        if (key.IsControlKey() || key.IsAltKey() || key.IsShiftKey())
        {
            return;
        }

        await UpdateShortcutAsync(shortcutType, key, modifiers);
    }

    /// <summary>
    /// Update a specific shortcut
    /// </summary>
    private async Task UpdateShortcutAsync(ShortcutType shortcutType, Key key, KeyModifiers modifiers)
    {
        // Validate that the key can be registered as a Windows global hotkey
        if (!IsValidHotkeyKey(key, modifiers, out var errorMessage))
        {
            await messageDialogService.ShowWarningAsync(
                "Invalid Hotkey",
                errorMessage ?? "This key cannot be used as a global hotkey. Please try a different key.\n\nNote: Function keys (F1-F12), number keys, and letter keys work best. Special keys like Fn or numpad 00 are not supported by Windows global hotkeys.");
            return;
        }

        var shortcutData = new KeyBinding(key, modifiers);

        switch (shortcutType)
        {
            case ShortcutType.MouseThrough:
                AppConfig.MouseThroughShortcut = shortcutData;
                break;
            case ShortcutType.ClearData:
                AppConfig.ClearDataShortcut = shortcutData;
                break;
            case ShortcutType.TopMost:
                AppConfig.TopmostShortcut = shortcutData;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shortcutType), shortcutType, null);
        }
    }

    /// <summary>
    /// Validates if a key can be used for Windows global hotkeys
    /// </summary>
    /// <param name="key">The key to validate</param>
    /// <param name="modifiers">The modifier keys</param>
    /// <param name="errorMessage">Error message if validation fails</param>
    /// <returns>True if valid, false otherwise</returns>
    private bool IsValidHotkeyKey(Key key, KeyModifiers modifiers, out string? errorMessage)
    {
        errorMessage = null;

        // CRITICAL: Global hotkeys without modifiers block the key system-wide!
        // This prevents normal typing/usage of the key in all applications.
        if (modifiers == KeyModifiers.None)
        {
            errorMessage = $"Global hotkeys MUST have at least one modifier key (Ctrl, Alt, or Shift).\n\nWithout modifiers, the key '{key}' would be blocked system-wide and you couldn't type it anymore!\n\nExample: Ctrl+{key} or Alt+{key}";
            return false;
        }

        // Check if we can get a valid virtual key code
        var vk = key.ToVirtualKey();
        if (vk == 0)
        {
            errorMessage = $"The key '{key}' is not recognized by Windows and cannot be registered as a global hotkey.\n\nThis often happens with:\n• Hardware-specific keys (Fn, special macro keys)\n• Custom keyboard software keys\n• Non-standard numpad keys (like 00)\n\nPlease use standard keys like F1-F12, letters, or numbers.";
            return false;
        }

        // Blacklist problematic keys that have VK codes but don't work well with RegisterHotKey
        var blacklistedKeys = new[]
        {
            Key.LWin, Key.RWin,  // Windows keys (can cause issues)
            Key.Sleep,            // System keys
            Key.None              // Invalid key
        };

        if (blacklistedKeys.Contains(key))
        {
            errorMessage = $"The key '{key}' cannot be used as a global hotkey because it is reserved by Windows.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Clear a specific shortcut
    /// </summary>
    private void ClearShortcut(ShortcutType shortcutType)
    {
        var shortCut = new KeyBinding(Key.None, KeyModifiers.None);
        switch (shortcutType)
        {
            case ShortcutType.MouseThrough:
                AppConfig.MouseThroughShortcut = shortCut;
                break;
            case ShortcutType.ClearData:
                AppConfig.ClearDataShortcut = shortCut;
                break;
            case ShortcutType.TopMost:
                AppConfig.TopmostShortcut = shortCut;
                break;
        }
    }

    public Task ApplySettingsAsync()
    {
        return configManager.SaveAsync(AppConfig);
    }

    [RelayCommand]
    private async Task Confirm()
    {
        await ApplySettingsAsync();
        UnsubscribeHandlers();
        RequestClose?.Invoke();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if (!_hasUnsavedChanges)
        {
            UnsubscribeHandlers();
            RequestClose?.Invoke();
            return;
        }

        var title = localization.GetString(ResourcesKeys.Settings_CancelConfirm_Title);
        var message = localization.GetString(ResourcesKeys.Settings_CancelConfirm_Message);

        var result = await messageDialogService.ShowConfirmationAsync(title, message);

        if (result)
        {
            // Restore to the post-load snapshot and persist it,
            // so only initialization-time changes are saved.
            if (_hasUnsavedChanges)
            {
                await ApplySettingsAsync();
            }

            _hasUnsavedChanges = false;
            UnsubscribeHandlers();
            RequestClose?.Invoke();
        }
    }

    [RelayCommand]
    private void TestQueuePopSound()
    {
        soundPlayerService.TestSound(
            AppConfig.QueuePopSound,
            AppConfig.QueuePopSoundVolume
        );
    }

    private void OnCultureChanged(object? sender, CultureInfo culture)
    {
        UpdateLanguageDependentCollections();
    }

    private void UnsubscribeHandlers()
    {
        if (_cultureHandlerSubscribed)
        {
            localization.CultureChanged -= OnCultureChanged;
            _cultureHandlerSubscribed = false;
        }

        if (_networkHandlerSubscribed)
        {
            NetworkChange.NetworkAvailabilityChanged -= OnSystemNetworkChanged;
            NetworkChange.NetworkAddressChanged -= OnSystemNetworkChanged;
            _networkHandlerSubscribed = false;
        }
    }

    // MEMORY LEAK FIX: Implement IDisposable to properly clean up event subscriptions.
    // SettingsViewModel subscribes to:
    // 1. LocalizationManager.CultureChanged (singleton → transient leak)
    // 2. NetworkChange.NetworkAvailabilityChanged (static event → memory leak)
    // 3. NetworkChange.NetworkAddressChanged (static event → memory leak)
    // 4. AppConfig.PropertyChanged (via OnAppConfigChanged)
    // Without proper disposal, these event subscriptions prevent garbage collection of this ViewModel instance.
    public void Dispose()
    {
        // Unsubscribe from all event handlers
        UnsubscribeHandlers();

        // Unsubscribe from AppConfig PropertyChanged
        if (AppConfig != null)
        {
            AppConfig.PropertyChanged -= OnAppConfigPropertyChanged;
        }

        // Unsubscribe from RequestClose (not an external event, but good practice)
        RequestClose = null;
    }
}

public partial class SettingsViewModel
{
    private static void UpdateEnumList<T>(IEnumerable<Option<T>> list) where T : Enum
    {
        foreach (var itm in list)
        {
            itm.Display = itm.Value.GetLocalizedDescription();
        }
    }

    private void UpdateLanguageDependentCollections()
    {
        UpdateEnumList(AvailableNumberDisplayModes);
        UpdateEnumList(AvailableLanguages);
    }

    private void SyncLanguageOption()
    {
        var (ret, opt) = SyncOption(SelectedLanguage, AvailableLanguages, AppConfig.Language);
        if (ret) SelectedLanguage = opt!;
    }

    private void SyncNumberDisplayModeOption()
    {
        var (ret, opt) = SyncOption(SelectedNumberDisplayMode, AvailableNumberDisplayModes,
            AppConfig.DamageDisplayType);
        if (ret) SelectedNumberDisplayMode = opt!;
    }

    private void SyncPanelColorModeOption()
    {
        var (ret, opt) = SyncOption(SelectedPanelColorMode, AvailablePanelColorModes,
            AppConfig.PanelColorMode);
        if (ret) SelectedPanelColorMode = opt!;
    }

    private void SyncOptions()
    {
        SyncLanguageOption();
        SyncNumberDisplayModeOption();
        SyncPanelColorModeOption();
    }

    private static (bool result, Option<T>? opt) SyncOption<T>(Option<T>? option, List<Option<T>> availableList,
        T origin)
    {
        if (Equal(option, origin)) return (false, null);

        var match = availableList.FirstOrDefault(l => Equal(l, origin));
        Debug.Assert(match != null);
        return (true, match);

        bool Equal(Option<T>? o1, T o2)
        {
            return o1?.Value?.Equals(o2) ?? false;
        }
    }
}

/// <summary>
/// Enum to identify shortcut types
/// </summary>
public enum ShortcutType
{
    MouseThrough,
    ClearData,
    TopMost
}

public sealed class SettingsDesignTimeViewModel : SettingsViewModel
{
    public SettingsDesignTimeViewModel() : base(new DesignConfigManager(), new DesignTimeDeviceManagementService(), new LocalizationManager(new LocalizationConfiguration(), NullLogger<LocalizationManager>.Instance), new DesignMessageDialogService(), new DesignTimeGlobalHotkeyService(), new DesignTimeSoundPlayerService(), new DesignTimeQueuePopUIDetector())
    {
        AppConfig = new AppConfig
        {
            // set friendly defaults shown in designer
            Opacity = 85,
            CombatTimeClearDelay = 5,
            ClearLogAfterTeleport = false,
            Language = Language.Auto
        };

        AvailableNetworkAdapters = new List<NetworkAdapterInfo>
        {
            new NetworkAdapterInfo("WAN Adapter", "WAN"),
            new NetworkAdapterInfo("WLAN Adapter", "WLAN")
        };

        AppConfig.MouseThroughShortcut = new KeyBinding(Key.F6, KeyModifiers.Control);
        AppConfig.ClearDataShortcut = new KeyBinding(Key.F9, KeyModifiers.None);

        AvailableLanguages = new List<Option<Language>>
        {
            new Option<Language>(Language.Auto, "Follow System"),
            new Option<Language>(Language.ZhCn, "中文 (简体)"),
            new Option<Language>(Language.EnUs, "English")
        };

        AvailableNumberDisplayModes = new List<Option<NumberDisplayMode>>
        {
            new Option<NumberDisplayMode>(NumberDisplayMode.Wan, "四位计数法 (万)"),
            new Option<NumberDisplayMode>(NumberDisplayMode.KMB, "三位计数法 (KMB)")
        };

        SelectedLanguage = AvailableLanguages[0];
        SelectedNumberDisplayMode = AvailableNumberDisplayModes[0];
    }
}

internal sealed class DesignMessageDialogService : IMessageDialogService
{
    public Task ShowInformationAsync(string title, string message) => Task.CompletedTask;
    public Task ShowWarningAsync(string title, string message) => Task.CompletedTask;
    public Task ShowErrorAsync(string title, string message) => Task.CompletedTask;
    public Task<bool> ShowConfirmationAsync(string title, string message) => Task.FromResult(true);
}

internal sealed class DesignTimeSoundPlayerService : ISoundPlayerService
{
    public void PlayQueuePopSound() { }
    public void TestSound(QueuePopSound sound, double volume) { }
    public void Dispose() { }
}

internal sealed class DesignTimeQueuePopUIDetector : IQueuePopUIDetector
{
    public bool IsRunning => false;
    public void Start() { }
    public void Stop() { }
    public void Dispose() { }
}
