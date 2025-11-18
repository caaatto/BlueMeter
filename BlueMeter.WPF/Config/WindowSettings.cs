using System.IO;
using System.Text.Json;

namespace BlueMeter.WPF.Config;

/// <summary>
/// Simple window position storage (similar to WoW Details addon SavedVariables)
/// Saved separately from main config to avoid reload issues
/// </summary>
public class WindowSettings
{
    public double? DpsWindowLeft { get; set; }
    public double? DpsWindowTop { get; set; }
    public double? DpsWindowWidth { get; set; }
    public double? DpsWindowHeight { get; set; }
    public bool SaveDpsWindowPosition { get; set; } = true;

    private static readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BlueMeter",
        "WindowSettings.json");

    /// <summary>
    /// Load window settings from disk
    /// </summary>
    public static WindowSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<WindowSettings>(json) ?? new WindowSettings();
            }
        }
        catch
        {
            // Silently fall back to defaults if loading fails
        }

        return new WindowSettings();
    }

    /// <summary>
    /// Save window settings to disk
    /// </summary>
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently ignore save failures
        }
    }
}
