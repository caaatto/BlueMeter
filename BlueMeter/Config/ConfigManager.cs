using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace BlueMeter.Config;

/// <summary>
/// Persists <see cref="AppConfig"/> to <c>%APPDATA%\BlueMeter\config.json</c>
/// and notifies subscribers when the underlying <see cref="IOptionsMonitor{TOptions}"/>
/// detects an external change. The on-disk file overlays the bundled
/// <c>appsettings.json</c> via the configuration pipeline wired in <c>App.axaml.cs</c>.
/// </summary>
public class ConfigManager : IConfigManager
{
    private readonly string _configFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IOptionsMonitor<AppConfig> _optionsMonitor;

    public ConfigManager(IOptionsMonitor<AppConfig> optionsMonitor,
        IOptions<JsonSerializerOptions> jsonOptions)
    {
        _optionsMonitor = optionsMonitor;
        _jsonOptions = jsonOptions.Value;

        // Save user settings to AppData to persist across updates
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BlueMeter");
        Directory.CreateDirectory(appDataPath);
        _configFilePath = Path.Combine(appDataPath, "config.json");

        _optionsMonitor.OnChange(OnConfigurationChanged);
    }

    public async Task SaveAsync(AppConfig? newConfig = null)
    {
        try
        {
            newConfig ??= CurrentConfig;

            // Save only the Config section to AppData. This file overlays the default appsettings.json.
            var rootDict = new Dictionary<string, object>
            {
                ["Config"] = newConfig
            };

            var updatedJson = JsonSerializer.Serialize(rootDict, _jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, updatedJson);

            // Manually notify subscribers in addition to the file watcher.
            OnConfigurationChanged(newConfig);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update configuration: {ex.Message}", ex);
        }
    }

    public event EventHandler<AppConfig>? ConfigurationUpdated;

    public AppConfig CurrentConfig => _optionsMonitor.CurrentValue;

    private void OnConfigurationChanged(AppConfig newConfig)
    {
        ConfigurationUpdated?.Invoke(this, newConfig);
    }
}
