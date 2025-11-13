using BlueMeter.WPF.Models.Checklist;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.Services.Checklist;

/// <summary>
/// Implementierung des Checklist-Service
/// </summary>
public class ChecklistService : IChecklistService
{
    private readonly IStorageService _storageService;
    private readonly ILogger<ChecklistService> _logger;
    private ChecklistConfig? _config;
    private ChecklistProfile? _currentProfile;

    public ChecklistService(
        IStorageService storageService,
        ILogger<ChecklistService> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Initialisiert den Service (lädt Config)
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _config = await _storageService.LoadConfigAsync();

            if (_config == null)
            {
                _logger.LogInformation("No config found, creating default configuration");
                _config = ChecklistConfig.CreateDefault();
                await _storageService.SaveConfigAsync(_config);
            }

            // Lade aktuelles Profil
            if (!string.IsNullOrEmpty(_config.CurrentProfileId))
            {
                _currentProfile = await _storageService.LoadProfileAsync(_config.CurrentProfileId);
            }

            // Fallback: Erstes Profil oder neues Default-Profil
            if (_currentProfile == null && _config.Profiles.Count > 0)
            {
                _currentProfile = _config.Profiles[0];
            }
            else if (_currentProfile == null)
            {
                _currentProfile = ChecklistProfile.CreateDefault("Main Character");
                _config.Profiles.Add(_currentProfile);
                await _storageService.SaveProfileAsync(_currentProfile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ChecklistService");
            _config = ChecklistConfig.CreateDefault();
            _currentProfile = _config.Profiles.First();
        }
    }

    public ChecklistProfile? GetCurrentProfile()
    {
        return _currentProfile;
    }

    public void SetCurrentProfile(string profileId)
    {
        if (_config == null) return;

        var profile = _config.Profiles.FirstOrDefault(p => p.ProfileId == profileId);
        if (profile != null)
        {
            _currentProfile = profile;
            _config.CurrentProfileId = profileId;
            _storageService.SaveConfigAsync(_config).ConfigureAwait(false);
        }
    }

    public async Task SaveProfileAsync(ChecklistProfile profile)
    {
        await _storageService.SaveProfileAsync(profile);
    }

    public async Task<List<ChecklistProfile>> LoadAllProfilesAsync()
    {
        return await _storageService.LoadAllProfilesAsync();
    }

    public ChecklistProfile CreateNewProfile(string profileName)
    {
        var profile = ChecklistProfile.CreateDefault(profileName);

        if (_config != null)
        {
            _config.Profiles.Add(profile);
            _storageService.SaveConfigAsync(_config).ConfigureAwait(false);
        }

        _storageService.SaveProfileAsync(profile).ConfigureAwait(false);
        return profile;
    }

    public async Task<bool> DeleteProfileAsync(string profileId)
    {
        if (_config == null) return false;

        // Verhindere Löschen des letzten Profils
        if (_config.Profiles.Count <= 1)
        {
            _logger.LogWarning("Cannot delete last profile");
            return false;
        }

        var profile = _config.Profiles.FirstOrDefault(p => p.ProfileId == profileId);
        if (profile == null) return false;

        _config.Profiles.Remove(profile);

        // Wenn aktuelles Profil gelöscht wird, wechsle zum ersten
        if (_currentProfile?.ProfileId == profileId)
        {
            _currentProfile = _config.Profiles.First();
            _config.CurrentProfileId = _currentProfile.ProfileId;
        }

        await _storageService.SaveConfigAsync(_config);
        return await _storageService.DeleteProfileAsync(profileId);
    }

    public async Task CheckAndResetTasksAsync()
    {
        if (_currentProfile == null || _config == null) return;

        var timezone = _config.GetTimeZone();
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timezone);
        var resetTime = _config.DailyResetTime;

        // Berechne letzten Reset-Zeitpunkt (heute um resetTime)
        var todayReset = now.Date.Add(resetTime);
        if (now < todayReset)
        {
            // Noch nicht resettet heute, also war letzter Reset gestern
            todayReset = todayReset.AddDays(-1);
        }

        // Daily Reset
        if (_currentProfile.LastDailyReset < todayReset)
        {
            _logger.LogInformation("Performing daily reset");
            ResetTasks(_currentProfile.DailyTasks);
            _currentProfile.LastDailyReset = now;
            await SaveProfileAsync(_currentProfile);
        }

        // Weekly Reset (Montag)
        var lastWeeklyResetDay = GetLastWeeklyResetDate(now, _config.WeeklyResetDay, resetTime);
        if (_currentProfile.LastWeeklyReset < lastWeeklyResetDay)
        {
            _logger.LogInformation("Performing weekly reset");
            ResetTasks(_currentProfile.WeeklyTasks);
            _currentProfile.LastWeeklyReset = now;
            await SaveProfileAsync(_currentProfile);
        }
    }

    private static DateTime GetLastWeeklyResetDate(DateTime now, DayOfWeek resetDay, TimeSpan resetTime)
    {
        var daysToSubtract = ((int)now.DayOfWeek - (int)resetDay + 7) % 7;
        var lastResetDay = now.Date.AddDays(-daysToSubtract).Add(resetTime);

        // Wenn der Reset-Zeitpunkt in der Zukunft liegt, gehe eine Woche zurück
        if (lastResetDay > now)
        {
            lastResetDay = lastResetDay.AddDays(-7);
        }

        return lastResetDay;
    }

    private static void ResetTasks(IEnumerable<ChecklistTask> tasks)
    {
        foreach (var task in tasks)
        {
            task.IsCompleted = false;
            task.CurrentProgress = 0;
        }
    }

    public IEnumerable<ChecklistTask> FilterTasks(
        IEnumerable<ChecklistTask> tasks,
        string? searchQuery,
        bool showCompleted)
    {
        var filtered = tasks.AsEnumerable();

        // Filter nach Completion-Status
        if (!showCompleted)
        {
            filtered = filtered.Where(t => !t.IsCompleted);
        }

        // Filter nach Suchbegriff
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            filtered = filtered.Where(t =>
                t.Label.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
        }

        return filtered;
    }

    public void ToggleTaskCompletion(ChecklistTask task)
    {
        task.IsCompleted = !task.IsCompleted;

        // Wenn Task wieder uncompleted wird, setze Progress zurück
        if (!task.IsCompleted && task.IsIncremental)
        {
            task.CurrentProgress = 0;
        }

        // Auto-Save
        if (_currentProfile != null)
        {
            SaveProfileAsync(_currentProfile).ConfigureAwait(false);
        }
    }

    public void IncrementTaskProgress(ChecklistTask task, int amount = 1)
    {
        if (!task.IsIncremental) return;

        task.CurrentProgress = Math.Min(task.CurrentProgress + amount, task.MaxProgress);

        // Auto-Save
        if (_currentProfile != null)
        {
            SaveProfileAsync(_currentProfile).ConfigureAwait(false);
        }
    }

    public void DecrementTaskProgress(ChecklistTask task, int amount = 1)
    {
        if (!task.IsIncremental) return;

        task.CurrentProgress = Math.Max(task.CurrentProgress - amount, 0);

        // Wenn Progress < Max, uncomplete den Task
        if (task.CurrentProgress < task.MaxProgress)
        {
            task.IsCompleted = false;
        }

        // Auto-Save
        if (_currentProfile != null)
        {
            SaveProfileAsync(_currentProfile).ConfigureAwait(false);
        }
    }

    public void SetAllTasksCompletion(IEnumerable<ChecklistTask> tasks, bool completed)
    {
        foreach (var task in tasks)
        {
            task.IsCompleted = completed;

            // Wenn completed, setze Progress auf Max
            if (completed && task.IsIncremental)
            {
                task.CurrentProgress = task.MaxProgress;
            }
            // Wenn uncompleted, setze Progress auf 0
            else if (!completed && task.IsIncremental)
            {
                task.CurrentProgress = 0;
            }
        }

        // Auto-Save
        if (_currentProfile != null)
        {
            SaveProfileAsync(_currentProfile).ConfigureAwait(false);
        }
    }
}
