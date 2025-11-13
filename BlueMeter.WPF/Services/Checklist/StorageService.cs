using System.IO;
using BlueMeter.WPF.Models.Checklist;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace BlueMeter.WPF.Services.Checklist;

/// <summary>
/// Implementierung des Storage-Service für Persistierung
/// </summary>
public class StorageService : IStorageService
{
    private readonly ILogger<StorageService> _logger;
    private readonly string _dataPath;
    private readonly JsonSerializerSettings _jsonSettings;

    public StorageService(ILogger<StorageService> logger)
    {
        _logger = logger;

        // Speicherpfad: %AppData%/BlueMeter/Checklist
        _dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BlueMeter",
            "Checklist");

        // Erstelle Verzeichnis falls nicht vorhanden
        Directory.CreateDirectory(_dataPath);

        // JSON-Settings mit schöner Formatierung
        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };
    }

    public async Task SaveConfigAsync(ChecklistConfig config)
    {
        try
        {
            var filePath = Path.Combine(_dataPath, "config.json");
            var json = JsonConvert.SerializeObject(config, _jsonSettings);
            await File.WriteAllTextAsync(filePath, json);
            _logger.LogDebug("Saved config to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving config");
            throw;
        }
    }

    public async Task<ChecklistConfig?> LoadConfigAsync()
    {
        try
        {
            var filePath = Path.Combine(_dataPath, "config.json");

            if (!File.Exists(filePath))
            {
                _logger.LogInformation("Config file not found at {FilePath}", filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var config = JsonConvert.DeserializeObject<ChecklistConfig>(json, _jsonSettings);
            _logger.LogDebug("Loaded config from {FilePath}", filePath);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading config");
            return null;
        }
    }

    public async Task SaveProfileAsync(ChecklistProfile profile)
    {
        try
        {
            var filePath = Path.Combine(_dataPath, $"profile_{profile.ProfileId}.json");
            var json = JsonConvert.SerializeObject(profile, _jsonSettings);
            await File.WriteAllTextAsync(filePath, json);
            _logger.LogDebug("Saved profile {ProfileName} to {FilePath}", profile.ProfileName, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving profile {ProfileId}", profile.ProfileId);
            throw;
        }
    }

    public async Task<ChecklistProfile?> LoadProfileAsync(string profileId)
    {
        try
        {
            var filePath = Path.Combine(_dataPath, $"profile_{profileId}.json");

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Profile file not found at {FilePath}", filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var profile = JsonConvert.DeserializeObject<ChecklistProfile>(json, _jsonSettings);
            _logger.LogDebug("Loaded profile from {FilePath}", filePath);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profile {ProfileId}", profileId);
            return null;
        }
    }

    public async Task<List<ChecklistProfile>> LoadAllProfilesAsync()
    {
        var profiles = new List<ChecklistProfile>();

        try
        {
            var profileFiles = Directory.GetFiles(_dataPath, "profile_*.json");

            foreach (var filePath in profileFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var profile = JsonConvert.DeserializeObject<ChecklistProfile>(json, _jsonSettings);
                    if (profile != null)
                    {
                        profiles.Add(profile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading profile from {FilePath}", filePath);
                }
            }

            _logger.LogInformation("Loaded {Count} profiles", profiles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profiles");
        }

        return profiles;
    }

    public async Task<bool> DeleteProfileAsync(string profileId)
    {
        try
        {
            var filePath = Path.Combine(_dataPath, $"profile_{profileId}.json");

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Profile file not found at {FilePath}", filePath);
                return false;
            }

            await Task.Run(() => File.Delete(filePath));
            _logger.LogInformation("Deleted profile {ProfileId}", profileId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile {ProfileId}", profileId);
            return false;
        }
    }

    public async Task<string?> ExportProfileAsync(ChecklistProfile profile)
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"BlueMeter_Checklist_{profile.ProfileName}_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                DefaultExt = ".json",
                Title = "Export Checklist Profile"
            };

            if (dialog.ShowDialog() == true)
            {
                var json = SerializeProfile(profile);
                await File.WriteAllTextAsync(dialog.FileName, json);
                _logger.LogInformation("Exported profile to {FilePath}", dialog.FileName);
                return dialog.FileName;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting profile");
            throw;
        }
    }

    public async Task<ChecklistProfile?> ImportProfileAsync()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",
                Title = "Import Checklist Profile"
            };

            if (dialog.ShowDialog() == true)
            {
                var json = await File.ReadAllTextAsync(dialog.FileName);
                var profile = DeserializeProfile(json);

                if (profile != null)
                {
                    // Generiere neue ID um Konflikte zu vermeiden
                    profile.ProfileId = Guid.NewGuid().ToString();
                    profile.ProfileName += " (Imported)";

                    await SaveProfileAsync(profile);
                    _logger.LogInformation("Imported profile from {FilePath}", dialog.FileName);
                    return profile;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing profile");
            throw;
        }
    }

    public string SerializeProfile(ChecklistProfile profile)
    {
        return JsonConvert.SerializeObject(profile, _jsonSettings);
    }

    public ChecklistProfile? DeserializeProfile(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<ChecklistProfile>(json, _jsonSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing profile");
            return null;
        }
    }
}
