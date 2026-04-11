using System.Globalization;
using BlueMeter.Localization;
using BlueMeter.Plugins.Interfaces;
using BlueMeter.Properties;
using BlueMeter.Services;

namespace BlueMeter.Plugins.BuiltIn;

/// <summary>
/// Built-in plugin that exposes the module solver as a top-level entry in the
/// main view's plugin list. The underlying ModuleSolveView and supporting OCR /
/// scoring services were ported in earlier phases (Phase 4 batch 4 services,
/// Phase 5 follow-up VM, Phase 10 batch 10 view) — this shell only wires those
/// into the IPlugin contract.
/// </summary>
internal sealed class ModuleSolverPlugin(
    IWindowManagementService windowManagementService,
    LocalizationManager localizationManager) : IPlugin
{
    public string PackageName => "BlueMeter.Plugins.BuiltIn.ModuleSolverPlugin";

    public string PackageVersion => "3.0.0";

    public string GetPluginName(CultureInfo cultureInfo) =>
        localizationManager.GetString(ResourcesKeys.MainView_Plugin_ModuleSolver_Title, cultureInfo);

    public string GetPluginDescription(CultureInfo cultureInfo) =>
        localizationManager.GetString(ResourcesKeys.MainView_Plugin_ModuleSolver_Description, cultureInfo);

    public void OnRequestRun() => windowManagementService.ShowModuleSolve();

    public void OnRequestSetting() => windowManagementService.ShowSettings();
}
