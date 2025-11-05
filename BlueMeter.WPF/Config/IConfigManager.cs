namespace BlueMeter.WPF.Config;

public interface IConfigManager
{
    Task SaveAsync(AppConfig? newConfig = null);
    event EventHandler<AppConfig>? ConfigurationUpdated;
    AppConfig CurrentConfig { get; }
}