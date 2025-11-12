using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueMeter.WPF.Config;
using BlueMeter.WPF.Localization;
using BlueMeter.WPF.Plugins;
using BlueMeter.WPF.Properties;
using BlueMeter.WPF.Services;
using BlueMeter.WPF.Themes;

namespace BlueMeter.WPF.ViewModels;

public partial class MainViewModel : BaseViewModel, IDisposable
{
    private readonly ApplicationThemeManager _themeManager;
    private readonly IWindowManagementService _windowManagement;
    private readonly IApplicationControlService _appControlService;
    private readonly ITrayService _trayService;
    private readonly LocalizationManager _localizationManager;
    private readonly IMessageDialogService _dialogService;
    private readonly IConfigManager _configManager;
    private readonly ThemeService _themeService;
    private readonly ObservableCollection<PluginListItemViewModel> _plugins = [];
    private PluginListItemViewModel? _lastSelectedPlugin;
    private IPluginManager _pluginManager;

    public MainViewModel(
        ApplicationThemeManager themeManager,
        DebugFunctions debugFunctions,
        IWindowManagementService windowManagement,
        IApplicationControlService appControlService,
        ITrayService trayService,
        IPluginManager pluginManager,
        LocalizationManager localizationManager,
        IMessageDialogService dialogService,
        IConfigManager configManager,
        ThemeService themeService)
    {
        _themeManager = themeManager;
        _windowManagement = windowManagement;
        _appControlService = appControlService;
        _trayService = trayService;
        _localizationManager = localizationManager;
        _dialogService = dialogService;
        _configManager = configManager;
        _themeService = themeService;
        _pluginManager = pluginManager;

        Debug = debugFunctions;
        AvailableThemes = [ApplicationTheme.Light, ApplicationTheme.Dark];
        Theme = _themeManager.GetAppTheme();

        // Expose AppConfig for theme color and background image binding
        AppConfig = _configManager.CurrentConfig;

        // Initialize ThemeService with current theme color
        _themeService.Initialize(AppConfig.ThemeColor ?? "#0047AB");
        ThemeService = _themeService;

        // Initialize header title with current theme name
        HeaderTitle = ThemeDefinitions.GetAppName(AppConfig.ThemeColor);

        var pluginStates = pluginManager.GetPluginStates();
        foreach (var plugin in pluginManager.GetPlugins())
        {
            if (!pluginStates.TryGetValue(plugin, out var state))
            {
                state = new PluginState();
            }

            var viewModel = new PluginListItemViewModel(plugin, state, localizationManager, OnPluginAutoStartChanged);

            // Load AutoStart state from config if available
            var pluginId = viewModel.GetPluginIdentifier();
            if (_configManager.CurrentConfig.PluginAutoStartStates.TryGetValue(pluginId, out var autoStart))
            {
                state.IsAutoStart = autoStart;
            }

            _plugins.Add(viewModel);
        }

        Plugins = new ReadOnlyObservableCollection<PluginListItemViewModel>(_plugins);
        SelectedPlugin = Plugins.FirstOrDefault();

        // Auto-run plugins that have AutoStart enabled
        foreach (var pluginViewModel in _plugins)
        {
            if (pluginViewModel.State.IsAutoStart)
            {
                pluginViewModel.RunCommand.Execute(null);
            }
        }

        _localizationManager.CultureChanged += OnCultureChanged;

        // FIX: Subscribe to config changes to update AppConfig when settings are saved
        // This allows live updates of theme color and background image
        _configManager.ConfigurationUpdated += OnConfigurationUpdated;

        // Subscribe to initial AppConfig for live theme header updates
        SubscribeToThemeColorChanges();
    }

    private void SubscribeToThemeColorChanges()
    {
        // Subscribe to ThemeColor changes for live header updates
        AppConfig.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AppConfig.ThemeColor) && !string.IsNullOrEmpty(AppConfig.ThemeColor))
            {
                HeaderTitle = ThemeDefinitions.GetAppName(AppConfig.ThemeColor);
            }
        };
    }

    // FIX: Handler for configuration updates - refreshes AppConfig reference
    // This is called when user saves settings, causing MainView to re-bind to new values
    private void OnConfigurationUpdated(object? sender, AppConfig e)
    {
        // Update the AppConfig reference to trigger re-binding in UI
        // The event provides the new config, but we get it from ConfigManager to ensure consistency
        AppConfig = _configManager.CurrentConfig;
        OnPropertyChanged(nameof(AppConfig));

        // Re-subscribe to the new AppConfig instance for live updates
        SubscribeToThemeColorChanges();

        // Update theme service if theme color changed
        if (!string.IsNullOrEmpty(AppConfig.ThemeColor) && _themeService.ThemeColor != AppConfig.ThemeColor)
        {
            _themeService.ThemeColor = AppConfig.ThemeColor;
        }

        // Update header title when theme changes
        if (!string.IsNullOrEmpty(AppConfig.ThemeColor))
        {
            HeaderTitle = ThemeDefinitions.GetAppName(AppConfig.ThemeColor);
        }
    }

    /// <summary>
    /// Callback when a plugin's AutoStart state changes - saves to config.
    /// </summary>
    private async Task OnPluginAutoStartChanged(PluginListItemViewModel viewModel)
    {
        var config = _configManager.CurrentConfig;
        var pluginId = viewModel.GetPluginIdentifier();
        var autoStartState = viewModel.State.IsAutoStart;

        // Update the config
        config.PluginAutoStartStates[pluginId] = autoStartState;

        // Save to file
        await _configManager.SaveAsync();
    }

    public DebugFunctions Debug { get; }

    public ReadOnlyObservableCollection<PluginListItemViewModel> Plugins { get; }

    // Exposed for theme color and background image binding in MainView
    public AppConfig AppConfig { get; private set; }

    // Exposed for dynamic header title binding (displays current theme name)
    public ThemeService ThemeService { get; private set; }

    [ObservableProperty]
    private List<ApplicationTheme> _availableThemes = [];

    [ObservableProperty]
    private ApplicationTheme _theme;

    [ObservableProperty]
    private PluginListItemViewModel? _selectedPlugin;

    [ObservableProperty]
    private bool _isDebugTabActive;

    [ObservableProperty]
    private string _headerTitle = "BlueMeter";

    partial void OnThemeChanged(ApplicationTheme value)
    {
        _themeManager.Apply(value);
    }

    [RelayCommand]
    private void InitializeTray()
    {
        _trayService.Initialize("Star Resonance DPS");
    }

    [RelayCommand]
    private void MinimizeToTray()
    {
        _trayService.MinimizeToTray();
    }

    [RelayCommand]
    private void RestoreFromTray()
    {
        _trayService.Restore();
    }

    [RelayCommand]
    private void ExitFromTray()
    {
        _trayService.Exit();
    }

    partial void OnSelectedPluginChanged(PluginListItemViewModel? value)
    {
        if (value != null)
        {
            _lastSelectedPlugin = value;
            IsDebugTabActive = false;
        }
    }

    partial void OnIsDebugTabActiveChanged(bool value)
    {
        if (value)
        {
            if (SelectedPlugin != null)
            {
                _lastSelectedPlugin = SelectedPlugin;
            }
            SelectedPlugin = null;
        }
        else if (SelectedPlugin is null && _lastSelectedPlugin != null)
        {
            if (_plugins.Contains(_lastSelectedPlugin))
            {
                SelectedPlugin = _lastSelectedPlugin;
            }
        }
    }

    private void OnCultureChanged(object? sender, CultureInfo e)
    {
        foreach (var plugin in _plugins)
        {
            plugin.RefreshLocalization();
        }
    }

    [RelayCommand]
    private void CallSettingsView()
    {
        _windowManagement.SettingsView.Show();
    }

    [RelayCommand]
    private void CallSkillBreakdownView()
    {
        _windowManagement.SkillBreakdownView.Show();
    }

    [RelayCommand]
    private void CallAboutView()
    {
        _windowManagement.AboutView.ShowDialog();
    }

    [RelayCommand]
    private void CallDamageReferenceView()
    {
        _windowManagement.DamageReferenceView.Show();
    }

    [RelayCommand]
    private void Shutdown()
    {
        var title = _localizationManager.GetString(ResourcesKeys.App_Exit_Confirm_Title);
        var content = _localizationManager.GetString(ResourcesKeys.App_Exit_Confirm_Content);

        var result = _dialogService.Show(title, content);
        if (result == true)
        {
            _appControlService.Shutdown();
        }
    }

    // MEMORY LEAK FIX: Implement IDisposable to unsubscribe from events.
    // MainViewModel subscribes to:
    // 1. LocalizationManager.CultureChanged (singleton → transient leak)
    // 2. ConfigManager.ConfigurationUpdated (singleton → transient leak)
    // Since these are singletons, they hold references to this MainViewModel instance,
    // preventing garbage collection. This is particularly problematic because MainViewModel is transient
    // and multiple instances can accumulate over the application lifetime.
    public void Dispose()
    {
        _localizationManager.CultureChanged -= OnCultureChanged;
        _configManager.ConfigurationUpdated -= OnConfigurationUpdated;
    }
}