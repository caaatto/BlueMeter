using System.Globalization;
using BlueMeter.Localization;
using BlueMeter.Plugins.Interfaces;
using BlueMeter.Properties;
using BlueMeter.Services;

namespace BlueMeter.Plugins.BuiltIn;

/// <summary>
/// Built-in plugin that exposes the DPS meter / statistics window as a top-level
/// entry in the main view's plugin list. The underlying DpsStatisticsView and
/// supporting analysis services were ported in earlier phases — this shell only
/// wires those into the IPlugin contract.
/// </summary>
internal sealed class DpsPlugin(
    IWindowManagementService windowManagementService,
    LocalizationManager localizationManager) : IPlugin
{
    public string PackageName => "BlueMeter.Plugins.BuiltIn.DpsPlugin";

    public string PackageVersion => "3.0.0";

    public string GetPluginName(CultureInfo cultureInfo) =>
        localizationManager.GetString(ResourcesKeys.MainView_Plugin_DpsTool_Title, cultureInfo);

    public string GetPluginDescription(CultureInfo cultureInfo) =>
        localizationManager.GetString(ResourcesKeys.MainView_Plugin_DpsTool_Description, cultureInfo);

    public void OnRequestRun() => windowManagementService.ShowDpsStatistics();

    public void OnRequestSetting() => windowManagementService.ShowSettings();
}
