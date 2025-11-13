using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.WPF.Models.Checklist;

/// <summary>
/// Konfiguration für die Checklist
/// </summary>
public partial class ChecklistConfig : ObservableObject
{
    /// <summary>
    /// Zeitzone für Resets und Events (GMT+1 = W. Europe Standard Time)
    /// </summary>
    [ObservableProperty]
    private string _timeZoneId = "W. Europe Standard Time";

    /// <summary>
    /// Uhrzeit für Daily Reset (Standard: 08:00)
    /// </summary>
    [ObservableProperty]
    private TimeSpan _dailyResetTime = new(8, 0, 0);

    /// <summary>
    /// Wochentag für Weekly Reset (Standard: Montag)
    /// </summary>
    [ObservableProperty]
    private DayOfWeek _weeklyResetDay = DayOfWeek.Monday;

    /// <summary>
    /// ID des aktuell aktiven Profils
    /// </summary>
    [ObservableProperty]
    private string _currentProfileId = string.Empty;

    /// <summary>
    /// Liste aller gespeicherten Profile
    /// </summary>
    [ObservableProperty]
    private List<ChecklistProfile> _profiles = [];

    /// <summary>
    /// Ob abgeschlossene Tasks angezeigt werden sollen
    /// </summary>
    [ObservableProperty]
    private bool _showCompletedTasks = true;

    /// <summary>
    /// Ob Event-Timer angezeigt werden sollen
    /// </summary>
    [ObservableProperty]
    private bool _showEventTimers = true;

    /// <summary>
    /// Update-Intervall für Timer (in Sekunden)
    /// </summary>
    [ObservableProperty]
    private int _timerUpdateInterval = 1;

    /// <summary>
    /// Update-Intervall wenn Fenster nicht fokussiert (in Sekunden)
    /// </summary>
    [ObservableProperty]
    private int _timerUpdateIntervalUnfocused = 5;

    /// <summary>
    /// Aktive Event-Schedules
    /// </summary>
    [ObservableProperty]
    private List<EventSchedule> _activeEventSchedules = [];

    /// <summary>
    /// Holt die TimeZoneInfo basierend auf TimeZoneId
    /// </summary>
    public TimeZoneInfo GetTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch
        {
            // Fallback zu UTC
            return TimeZoneInfo.Utc;
        }
    }

    /// <summary>
    /// Erstellt eine Standard-Konfiguration
    /// </summary>
    public static ChecklistConfig CreateDefault()
    {
        var config = new ChecklistConfig();

        // Erstelle ein Default-Profil
        var defaultProfile = ChecklistProfile.CreateDefault("Main Character");
        config.Profiles.Add(defaultProfile);
        config.CurrentProfileId = defaultProfile.ProfileId;

        // Füge Standard-Events hinzu
        config.ActiveEventSchedules.Add(EventSchedule.Defaults.DailyReset);
        config.ActiveEventSchedules.Add(EventSchedule.Defaults.WeeklyReset);
        config.ActiveEventSchedules.Add(EventSchedule.Defaults.WorldBossCrusade);
        config.ActiveEventSchedules.Add(EventSchedule.Defaults.GuildHunt);
        config.ActiveEventSchedules.Add(EventSchedule.Defaults.GuildDance);

        return config;
    }
}
