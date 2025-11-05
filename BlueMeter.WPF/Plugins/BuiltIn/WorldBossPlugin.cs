using System.Globalization;
using BlueMeter.WPF.Localization;
using BlueMeter.WPF.Plugins.Interfaces;
using BlueMeter.WPF.Properties;
using BlueMeter.WPF.Services;

namespace BlueMeter.WPF.Plugins.BuiltIn;

internal class WorldBossPlugin : IPlugin
{
    private readonly IWindowManagementService _windowManagementService;
    private readonly LocalizationManager _localizationManager;

    public WorldBossPlugin(IWindowManagementService windowManagementService, LocalizationManager localizationManager)
    {
        _windowManagementService = windowManagementService;
        _localizationManager = localizationManager;
    }

    public string PackageName => "BlueMeter.WPF.Plugins.BuiltIn.WorldBossPlugin";

    public string PackageVersion => "3.0.0";

    public string GetPluginName(CultureInfo cultureInfo) =>
        _localizationManager.GetString(ResourcesKeys.MainView_Plugin_WorldBoss_Title, cultureInfo);

    public string GetPluginDescription(CultureInfo cultureInfo) =>
        _localizationManager.GetString(ResourcesKeys.MainView_Plugin_WorldBoss_Description, cultureInfo);

    public void OnRequestRun()
    {
        _windowManagementService.BossTrackerView.Show();
    }

    public void OnRequestSetting()
    {
        _windowManagementService.SettingsView.Show();
    }
}
