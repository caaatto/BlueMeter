using BlueMeter.WPF.Models.Checklist;

namespace BlueMeter.WPF.Services.Checklist;

/// <summary>
/// Service für Datenpersistierung (Save/Load/Import/Export)
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Speichert die Checklist-Konfiguration
    /// </summary>
    Task SaveConfigAsync(ChecklistConfig config);

    /// <summary>
    /// Lädt die Checklist-Konfiguration
    /// </summary>
    Task<ChecklistConfig?> LoadConfigAsync();

    /// <summary>
    /// Speichert ein einzelnes Profil
    /// </summary>
    Task SaveProfileAsync(ChecklistProfile profile);

    /// <summary>
    /// Lädt ein einzelnes Profil
    /// </summary>
    Task<ChecklistProfile?> LoadProfileAsync(string profileId);

    /// <summary>
    /// Lädt alle gespeicherten Profile
    /// </summary>
    Task<List<ChecklistProfile>> LoadAllProfilesAsync();

    /// <summary>
    /// Löscht ein Profil
    /// </summary>
    Task<bool> DeleteProfileAsync(string profileId);

    /// <summary>
    /// Exportiert ein Profil als JSON (mit File-Dialog)
    /// </summary>
    Task<string?> ExportProfileAsync(ChecklistProfile profile);

    /// <summary>
    /// Importiert ein Profil aus JSON (mit File-Dialog)
    /// </summary>
    Task<ChecklistProfile?> ImportProfileAsync();

    /// <summary>
    /// Exportiert ein Profil als JSON-String (ohne Dialog)
    /// </summary>
    string SerializeProfile(ChecklistProfile profile);

    /// <summary>
    /// Importiert ein Profil aus JSON-String (ohne Dialog)
    /// </summary>
    ChecklistProfile? DeserializeProfile(string json);
}
