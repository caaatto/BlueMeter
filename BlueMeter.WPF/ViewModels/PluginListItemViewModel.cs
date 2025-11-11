using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueMeter.WPF.Localization;
using BlueMeter.WPF.Plugins;
using BlueMeter.WPF.Plugins.Interfaces;
using BlueMeter.WPF.Properties;

namespace BlueMeter.WPF.ViewModels;

public sealed partial class PluginListItemViewModel : ObservableObject
{
    private readonly PluginState _state;
    private readonly LocalizationManager _localizationManager;
    private readonly Func<PluginListItemViewModel, Task>? _onAutoStartChanged;

    public PluginListItemViewModel(IPlugin plugin, PluginState state, LocalizationManager localizationManager, Func<PluginListItemViewModel, Task>? onAutoStartChanged = null)
    {
        Plugin = plugin;
        _state = state;
        _localizationManager = localizationManager;
        _onAutoStartChanged = onAutoStartChanged;
        RunCommand = new RelayCommand(ExecuteRun);
        OpenSettingsCommand = new RelayCommand(ExecuteSettings);
        ToggleAutoStartCommand = new RelayCommand(ExecuteToggleAutoStart);
    }

    public IPlugin Plugin { get; }

    public PluginState State => _state;

    public string Name => Plugin.GetPluginName(CultureInfo.CurrentUICulture);

    public string Description => Plugin.GetPluginDescription(CultureInfo.CurrentUICulture);

    public string AutoStartText => _state.IsAutoStart
        ? _localizationManager.GetString(ResourcesKeys.MainView_Plugin_AutoRunState_Enabled)
        : _localizationManager.GetString(ResourcesKeys.MainView_Plugin_AutoRunState_Disabled);

    public string RunningStateText => _state.InRunning
        ? _localizationManager.GetString(ResourcesKeys.MainView_Plugin_State_Running)
        : _localizationManager.GetString(ResourcesKeys.MainView_Plugin_State_Inactive);

    public IRelayCommand RunCommand { get; }

    public IRelayCommand OpenSettingsCommand { get; }

    public IRelayCommand ToggleAutoStartCommand { get; }

    public void RefreshLocalization()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(AutoStartText));
        OnPropertyChanged(nameof(RunningStateText));
    }

    private void ExecuteRun()
    {
        try
        {
            Plugin.OnRequestRun();
        }
        catch (NotImplementedException)
        {
            // Swallow for plugins that have not implemented the action yet.
        }
    }

    private void ExecuteSettings()
    {
        try
        {
            Plugin.OnRequestSetting();
        }
        catch (NotImplementedException)
        {
            // Swallow for plugins that have not implemented the action yet.
        }
    }

    private async void ExecuteToggleAutoStart()
    {
        // AutoStart state is updated via TwoWay binding, so we just notify the change
        OnPropertyChanged(nameof(AutoStartText));

        if (_onAutoStartChanged != null)
        {
            await _onAutoStartChanged(this);
        }
    }

    /// <summary>
    /// Gets the plugin's unique identifier for config storage.
    /// </summary>
    public string GetPluginIdentifier()
    {
        return Plugin.GetType().Name;
    }
}
