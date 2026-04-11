namespace BlueMeter.Plugins;

public class PluginState(bool isAutoStart = false, bool isRunning = false)
{
    public bool IsAutoStart { get; set; } = isAutoStart;
    public bool InRunning { get; internal set; } = isRunning;
}
