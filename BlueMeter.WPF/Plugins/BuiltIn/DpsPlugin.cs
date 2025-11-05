using System.Globalization;
using BlueMeter.WPF.Localization;
using BlueMeter.WPF.Plugins.Interfaces;
using BlueMeter.WPF.Properties;
using BlueMeter.WPF.Services;

namespace BlueMeter.WPF.Plugins.BuiltIn;

internal class DpsPlugin : IPlugin
{
    private readonly IWindowManagementService _windowManagementService;
    private readonly LocalizationManager _localizationManager;

    public DpsPlugin(IWindowManagementService windowManagementService, LocalizationManager localizationManager)
    {
        _windowManagementService = windowManagementService;
        _localizationManager = localizationManager;
    }

    public string PackageName => "BlueMeter.WPF.Plugins.BuiltIn.DpsPlugin";

    public string PackageVersion => "3.0.0";

    public string GetPluginName(CultureInfo cultureInfo) =>
        _localizationManager.GetString("MainView_Plugin_DpsTool_Title", cultureInfo);

    public string GetPluginDescription(CultureInfo cultureInfo) =>
        _localizationManager.GetString(ResourcesKeys.MainView_Plugin_DpsTool_Description, cultureInfo);

    public void OnRequestRun()
    {
        _windowManagementService.DpsStatisticsView.Show();
    }

    public void OnRequestSetting()
    {
        _windowManagementService.SettingsView.Show();
    }
}
