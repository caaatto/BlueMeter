using System.Collections.ObjectModel;
using BlueMeter.Plugins.Interfaces;

namespace BlueMeter.Plugins;

public interface IPluginManager
{
    ReadOnlyCollection<IPlugin> GetPlugins();
    ReadOnlyDictionary<IPlugin, PluginState> GetPluginStates();
    void RegisterPlugin(IPlugin plugin, bool isAutoStart = false, bool isRunning = false);
}
