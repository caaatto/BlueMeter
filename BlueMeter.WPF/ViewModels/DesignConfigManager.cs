using BlueMeter.WPF.Config;

namespace BlueMeter.WPF.ViewModels;

internal sealed class DesignConfigManager : IConfigManager
{
    public event EventHandler<AppConfig>? ConfigurationUpdated;
    public AppConfig CurrentConfig { get; } = new();

    public Task SaveAsync(AppConfig? newConfig)
    {
        return Task.CompletedTask;
    }

    private void OnConfigurationUpdated(AppConfig e)
    {
        ConfigurationUpdated?.Invoke(this, e);
    }
}