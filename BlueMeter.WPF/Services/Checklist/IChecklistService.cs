using BlueMeter.WPF.Models.Checklist;

namespace BlueMeter.WPF.Services.Checklist;

/// <summary>
/// Service für Checklist-Management
/// </summary>
public interface IChecklistService
{
    /// <summary>
    /// Holt das aktuell aktive Profil
    /// </summary>
    ChecklistProfile? GetCurrentProfile();

    /// <summary>
    /// Setzt ein Profil als aktiv
    /// </summary>
    void SetCurrentProfile(string profileId);

    /// <summary>
    /// Speichert ein Profil
    /// </summary>
    Task SaveProfileAsync(ChecklistProfile profile);

    /// <summary>
    /// Lädt alle Profile
    /// </summary>
    Task<List<ChecklistProfile>> LoadAllProfilesAsync();

    /// <summary>
    /// Erstellt ein neues Profil
    /// </summary>
    ChecklistProfile CreateNewProfile(string profileName);

    /// <summary>
    /// Löscht ein Profil
    /// </summary>
    Task<bool> DeleteProfileAsync(string profileId);

    /// <summary>
    /// Prüft und führt Auto-Reset bei Bedarf durch (Daily/Weekly)
    /// </summary>
    Task CheckAndResetTasksAsync();

    /// <summary>
    /// Filtert Tasks basierend auf Suchbegriff und Show-Completed-Flag
    /// </summary>
    IEnumerable<ChecklistTask> FilterTasks(
        IEnumerable<ChecklistTask> tasks,
        string? searchQuery,
        bool showCompleted);

    /// <summary>
    /// Toggle Task Completion-Status
    /// </summary>
    void ToggleTaskCompletion(ChecklistTask task);

    /// <summary>
    /// Inkrementiert den Fortschritt eines Tasks
    /// </summary>
    void IncrementTaskProgress(ChecklistTask task, int amount = 1);

    /// <summary>
    /// Dekrementiert den Fortschritt eines Tasks
    /// </summary>
    void DecrementTaskProgress(ChecklistTask task, int amount = 1);

    /// <summary>
    /// Markiert alle Tasks als completed/uncompleted
    /// </summary>
    void SetAllTasksCompletion(IEnumerable<ChecklistTask> tasks, bool completed);
}
