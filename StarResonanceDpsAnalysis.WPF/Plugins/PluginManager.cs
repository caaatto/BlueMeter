using System.Collections.ObjectModel;
using StarResonanceDpsAnalysis.WPF.Plugins.Interfaces;

namespace StarResonanceDpsAnalysis.WPF.Plugins;

internal sealed class PluginManager : IPluginManager
{
    private readonly List<IPlugin> _plugins = [];
    private readonly Dictionary<IPlugin, PluginState> _pluginStates = [];

    public PluginManager(IEnumerable<IPlugin> plugins)
    {
        foreach (var plugin in plugins)
        {
            RegisterPlugin(plugin);
        }
    }

    public ReadOnlyCollection<IPlugin> GetPlugins() => _plugins.AsReadOnly();

    public ReadOnlyDictionary<IPlugin, PluginState> GetPluginStates() => _pluginStates.AsReadOnly();

    public void RegisterPlugin(IPlugin plugin, bool isAutoStart = false, bool isRunning = false)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        if (_plugins.Contains(plugin))
        {
            return;
        }

        _plugins.Add(plugin);
        _pluginStates[plugin] = new PluginState(isAutoStart, isRunning);
    }
}
