using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.WPF.Localization;
using StarResonanceDpsAnalysis.WPF.Plugins;
using StarResonanceDpsAnalysis.WPF.Plugins.Interfaces;
using StarResonanceDpsAnalysis.WPF.Properties;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public sealed partial class PluginListItemViewModel : ObservableObject
{
    private readonly PluginState _state;
    private readonly LocalizationManager _localizationManager;

    public PluginListItemViewModel(IPlugin plugin, PluginState state, LocalizationManager localizationManager)
    {
        Plugin = plugin;
        _state = state;
        _localizationManager = localizationManager;
        RunCommand = new RelayCommand(ExecuteRun);
        OpenSettingsCommand = new RelayCommand(ExecuteSettings);
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
}
