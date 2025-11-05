using System.Globalization;

namespace BlueMeter.WPF.Plugins.Interfaces;

public interface IPlugin
{
    public string PackageName { get; }
    public string PackageVersion { get; }
    public string GetPluginName(CultureInfo cultureInfo);
    public string GetPluginDescription(CultureInfo cultureInfo);

    void OnRequestRun();

    void OnRequestSetting();
}