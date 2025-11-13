using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.WPF.Models.Checklist;

/// <summary>
/// Repräsentiert einen Event-Timer mit Countdown
/// </summary>
public partial class EventTimer : ObservableObject
{
    /// <summary>
    /// Name des Events
    /// </summary>
    [ObservableProperty]
    private string _eventName = string.Empty;

    /// <summary>
    /// Verbleibende Zeit bis zum Event
    /// </summary>
    [ObservableProperty]
    private TimeSpan _timeUntil;

    /// <summary>
    /// Ob das Event gerade aktiv ist
    /// </summary>
    [ObservableProperty]
    private bool _isActive;

    /// <summary>
    /// Zugehöriger Event-Schedule
    /// </summary>
    [ObservableProperty]
    private EventSchedule? _schedule;

    /// <summary>
    /// Formatierte Zeit-Anzeige (z.B. "2h 45m 30s")
    /// </summary>
    public string FormattedTime
    {
        get
        {
            if (TimeUntil.TotalDays >= 1)
            {
                return $"{(int)TimeUntil.TotalDays}d {TimeUntil.Hours}h {TimeUntil.Minutes}m";
            }
            else if (TimeUntil.TotalHours >= 1)
            {
                return $"{TimeUntil.Hours}h {TimeUntil.Minutes}m {TimeUntil.Seconds}s";
            }
            else if (TimeUntil.TotalMinutes >= 1)
            {
                return $"{TimeUntil.Minutes}m {TimeUntil.Seconds}s";
            }
            else
            {
                return $"{TimeUntil.Seconds}s";
            }
        }
    }

    /// <summary>
    /// Status-Text für UI (z.B. "Active" oder "Starts in")
    /// </summary>
    public string StatusText => IsActive ? "Active - Ends in" : "Starts in";

    partial void OnTimeUntilChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(FormattedTime));
    }

    partial void OnIsActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusText));
    }
}
