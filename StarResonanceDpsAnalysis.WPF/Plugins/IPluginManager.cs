using System.Collections.ObjectModel;
using StarResonanceDpsAnalysis.WPF.Plugins.Interfaces;

namespace StarResonanceDpsAnalysis.WPF.Plugins;

public interface IPluginManager
{
    ReadOnlyCollection<IPlugin> GetPlugins();
    ReadOnlyDictionary<IPlugin, PluginState> GetPluginStates();
    void RegisterPlugin(IPlugin plugin, bool isAutoStart = false, bool isRunning = false);
}
