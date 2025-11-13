namespace BlueMeter.WPF.Models.Checklist;

/// <summary>
/// Zeitplan für wiederkehrende Events
/// </summary>
public class EventSchedule
{
    /// <summary>
    /// Name des Events
    /// </summary>
    public required string EventName { get; init; }

    /// <summary>
    /// Startzeit (Uhrzeit im Format HH:mm)
    /// </summary>
    public TimeSpan StartTime { get; init; }

    /// <summary>
    /// Endzeit (Uhrzeit im Format HH:mm)
    /// </summary>
    public TimeSpan? EndTime { get; init; }

    /// <summary>
    /// An welchen Wochentagen das Event stattfindet (null = täglich)
    /// </summary>
    public DayOfWeek[]? ActiveDays { get; init; }

    /// <summary>
    /// Ob das Event gerade aktiv ist
    /// </summary>
    public bool IsActive(DateTime currentTime, TimeZoneInfo timezone)
    {
        var localTime = TimeZoneInfo.ConvertTime(currentTime, timezone);
        var currentTimeOfDay = localTime.TimeOfDay;
        var currentDayOfWeek = localTime.DayOfWeek;

        // Prüfe Wochentag (wenn gesetzt)
        if (ActiveDays != null && !ActiveDays.Contains(currentDayOfWeek))
        {
            return false;
        }

        // Prüfe Zeitfenster
        if (EndTime.HasValue)
        {
            // Event mit Zeitfenster
            if (EndTime.Value < StartTime)
            {
                // Über Mitternacht hinweg (z.B. 22:00 - 02:00)
                return currentTimeOfDay >= StartTime || currentTimeOfDay <= EndTime.Value;
            }
            else
            {
                // Normales Zeitfenster
                return currentTimeOfDay >= StartTime && currentTimeOfDay <= EndTime.Value;
            }
        }

        return false; // Event ohne Endzeit ist nur im Moment aktiv
    }

    /// <summary>
    /// Berechnet die verbleibende Zeit bis zum nächsten Event-Start oder -Ende
    /// </summary>
    public TimeSpan CalculateTimeUntil(DateTime currentTime, TimeZoneInfo timezone)
    {
        var localTime = TimeZoneInfo.ConvertTime(currentTime, timezone);

        if (IsActive(localTime, timezone) && EndTime.HasValue)
        {
            // Event läuft - Zeit bis zum Ende
            var endDateTime = localTime.Date.Add(EndTime.Value);
            if (EndTime.Value < localTime.TimeOfDay)
            {
                endDateTime = endDateTime.AddDays(1);
            }
            return endDateTime - localTime;
        }
        else
        {
            // Event läuft nicht - Zeit bis zum Start
            var nextStart = CalculateNextStart(localTime);
            return nextStart - localTime;
        }
    }

    /// <summary>
    /// Berechnet den nächsten Start-Zeitpunkt
    /// </summary>
    private DateTime CalculateNextStart(DateTime currentTime)
    {
        var today = currentTime.Date;
        var nextStart = today.Add(StartTime);

        // Wenn heute noch nicht gestartet und heute ein aktiver Tag
        if (nextStart > currentTime && (ActiveDays == null || ActiveDays.Contains(currentTime.DayOfWeek)))
        {
            return nextStart;
        }

        // Suche den nächsten aktiven Tag
        for (int i = 1; i <= 7; i++)
        {
            var candidateDate = today.AddDays(i);
            if (ActiveDays == null || ActiveDays.Contains(candidateDate.DayOfWeek))
            {
                return candidateDate.Add(StartTime);
            }
        }

        // Fallback: morgen zur gleichen Zeit
        return today.AddDays(1).Add(StartTime);
    }

    /// <summary>
    /// Standard Event-Schedules
    /// </summary>
    public static class Defaults
    {
        public static EventSchedule DailyReset => new()
        {
            EventName = "Daily Reset",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = null
        };

        public static EventSchedule WeeklyReset => new()
        {
            EventName = "Weekly Reset",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = null,
            ActiveDays = [DayOfWeek.Monday]
        };

        public static EventSchedule WorldBossCrusade => new()
        {
            EventName = "World Boss Crusade",
            StartTime = new TimeSpan(16, 0, 0), // 16:00
            EndTime = new TimeSpan(22, 0, 0)     // 22:00
        };

        public static EventSchedule GuildHunt => new()
        {
            EventName = "Guild Hunt",
            StartTime = new TimeSpan(14, 0, 0),  // 14:00
            EndTime = new TimeSpan(4, 0, 0),     // 04:00 (nächster Tag)
            ActiveDays = [DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday]
        };

        public static EventSchedule GuildDance => new()
        {
            EventName = "Guild Dance",
            StartTime = new TimeSpan(15, 30, 0), // 15:30
            EndTime = new TimeSpan(3, 30, 0),    // 03:30 (nächster Tag)
            ActiveDays = [DayOfWeek.Friday]
        };
    }
}
