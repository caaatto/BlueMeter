using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using BlueMeter.Models.Checklist;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlueMeter.Services.Checklist;

/// <summary>
/// Implementierung des Storage-Service für Persistierung.
///
/// Direct port of the WPF version. The only material change is that the WPF
/// <c>Microsoft.Win32.SaveFileDialog</c> / <c>OpenFileDialog</c> used in
/// <see cref="ExportProfileAsync"/> / <see cref="ImportProfileAsync"/> are
/// rewritten to Avalonia's <see cref="IStorageProvider"/> API
/// (<see cref="IStorageProvider.SaveFilePickerAsync"/> /
/// <see cref="IStorageProvider.OpenFilePickerAsync"/>) — obtained from the
/// currently-active top-level window via
/// <see cref="IClassicDesktopStyleApplicationLifetime"/>.
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
            var topLevel = GetTopLevel();
            if (topLevel is null)
            {
                _logger.LogWarning("Cannot export profile: no top-level window available");
                return null;
            }

            var options = new FilePickerSaveOptions
            {
                Title = "Export Checklist Profile",
                SuggestedFileName = $"BlueMeter_Checklist_{profile.ProfileName}_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExtension = "json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("JSON files") { Patterns = new[] { "*.json" } },
                    FilePickerFileTypes.All
                }
            };

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
            if (file is null)
            {
                return null;
            }

            var json = SerializeProfile(profile);
            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(json);

            var path = file.TryGetLocalPath() ?? file.Name;
            _logger.LogInformation("Exported profile to {FilePath}", path);
            return path;
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
            var topLevel = GetTopLevel();
            if (topLevel is null)
            {
                _logger.LogWarning("Cannot import profile: no top-level window available");
                return null;
            }

            var options = new FilePickerOpenOptions
            {
                Title = "Import Checklist Profile",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON files") { Patterns = new[] { "*.json" } },
                    FilePickerFileTypes.All
                }
            };

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            var file = files.FirstOrDefault();
            if (file is null)
            {
                return null;
            }

            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var profile = DeserializeProfile(json);

            if (profile != null)
            {
                // Generiere neue ID um Konflikte zu vermeiden
                profile.ProfileId = Guid.NewGuid().ToString();
                profile.ProfileName += " (Imported)";

                await SaveProfileAsync(profile);
                var path = file.TryGetLocalPath() ?? file.Name;
                _logger.LogInformation("Imported profile from {FilePath}", path);
                return profile;
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

    /// <summary>
    /// Resolve a top-level window to host file pickers. Prefers the active window
    /// (matches WPF modal parenting behavior), falling back to MainWindow or the
    /// first window in the classic desktop lifetime.
    /// </summary>
    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        var active = desktop.Windows.FirstOrDefault(w => w.IsActive);
        if (active is not null)
        {
            return active;
        }

        return desktop.MainWindow ?? desktop.Windows.FirstOrDefault();
    }
}
