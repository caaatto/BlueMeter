using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlueMeter.Services.ModuleSolver.Models;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services.ModuleSolver;

/// <summary>
/// Service for saving and loading module data to/from JSON files
/// </summary>
public class ModulePersistenceService
{
    private readonly ILogger<ModulePersistenceService> _logger;
    private readonly string _dataDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ModulePersistenceService(ILogger<ModulePersistenceService> logger)
    {
        _logger = logger;

        // Store module data in AppData/BlueMeter/ModuleData
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BlueMeter",
            "ModuleData");

        _dataDirectory = appDataPath;

        // Create directory if it doesn't exist
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
            _logger.LogInformation("Created module data directory: {Path}", _dataDirectory);
        }

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Save modules to a JSON file
    /// </summary>
    /// <param name="modules">List of modules to save</param>
    /// <param name="fileName">Optional filename (defaults to timestamp)</param>
    /// <returns>Path to saved file</returns>
    public async Task<string> SaveModulesAsync(List<ModuleInfo> modules, string? fileName = null)
    {
        try
        {
            if (modules == null || modules.Count == 0)
            {
                throw new ArgumentException("No modules to save");
            }

            // Generate filename if not provided
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"modules_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
            }
            else if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }

            var filePath = Path.Combine(_dataDirectory, fileName);

            // Create DTO for saving
            var saveData = new ModuleSaveData
            {
                SavedAt = DateTime.Now,
                ModuleCount = modules.Count,
                Modules = modules
            };

            // Serialize and save
            var json = JsonSerializer.Serialize(saveData, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);

            _logger.LogInformation("Saved {Count} modules to {Path}", modules.Count, filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving modules");
            throw;
        }
    }

    /// <summary>
    /// Load modules from a JSON file
    /// </summary>
    /// <param name="fileName">Filename to load (without path)</param>
    /// <returns>List of loaded modules</returns>
    public async Task<List<ModuleInfo>> LoadModulesAsync(string fileName)
    {
        try
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }

            var filePath = Path.Combine(_dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Module file not found: {fileName}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var saveData = JsonSerializer.Deserialize<ModuleSaveData>(json, _jsonOptions);

            if (saveData?.Modules == null)
            {
                throw new InvalidDataException("Invalid module data file");
            }

            _logger.LogInformation("Loaded {Count} modules from {Path}", saveData.Modules.Count, filePath);
            return saveData.Modules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading modules from {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Load modules from a full file path
    /// </summary>
    public async Task<List<ModuleInfo>> LoadModulesFromPathAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var saveData = JsonSerializer.Deserialize<ModuleSaveData>(json, _jsonOptions);

            if (saveData?.Modules == null)
            {
                throw new InvalidDataException("Invalid module data file");
            }

            _logger.LogInformation("Loaded {Count} modules from {Path}", saveData.Modules.Count, filePath);
            return saveData.Modules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading modules from {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Get list of all saved module files
    /// </summary>
    /// <returns>List of filenames</returns>
    public List<string> GetSavedFiles()
    {
        try
        {
            var files = Directory.GetFiles(_dataDirectory, "*.json");
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                fileNames.Add(Path.GetFileName(file));
            }

            return fileNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting saved files");
            return new List<string>();
        }
    }

    /// <summary>
    /// Delete a saved module file
    /// </summary>
    public bool DeleteFile(string fileName)
    {
        try
        {
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }

            var filePath = Path.Combine(_dataDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted module file: {FileName}", fileName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName}", fileName);
            return false;
        }
    }

    /// <summary>
    /// Get the data directory path
    /// </summary>
    public string GetDataDirectory() => _dataDirectory;
}

/// <summary>
/// DTO for saving module data with metadata
/// </summary>
public class ModuleSaveData
{
    public DateTime SavedAt { get; set; }
    public int ModuleCount { get; set; }
    public List<ModuleInfo> Modules { get; set; } = new();
}
