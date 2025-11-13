using System.Collections.ObjectModel;
using BlueMeter.WPF.Models.Checklist;

namespace BlueMeter.WPF.Services.Checklist;

/// <summary>
/// Service für Event-Timer-Management
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Observable Collection aller aktiven Timer
    /// </summary>
    ObservableCollection<EventTimer> ActiveTimers { get; }

    /// <summary>
    /// Startet die Timer-Updates
    /// </summary>
    void Start();

    /// <summary>
    /// Stoppt die Timer-Updates
    /// </summary>
    void Stop();

    /// <summary>
    /// Setzt das Update-Intervall
    /// </summary>
    void SetUpdateInterval(TimeSpan interval);

    /// <summary>
    /// Fügt einen Event-Schedule hinzu
    /// </summary>
    void AddEventSchedule(EventSchedule schedule);

    /// <summary>
    /// Entfernt einen Event-Schedule
    /// </summary>
    void RemoveEventSchedule(string eventName);

    /// <summary>
    /// Erzwingt ein sofortiges Update aller Timer
    /// </summary>
    void ForceUpdate();

    /// <summary>
    /// Event das gefeuert wird, wenn ein Timer abläuft
    /// </summary>
    event EventHandler<EventTimer>? TimerExpired;
}
