using System.Collections.ObjectModel;
using System.Windows.Threading;
using BlueMeter.WPF.Models.Checklist;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.Services.Checklist;

/// <summary>
/// Implementierung des Timer-Service für Event-Countdowns
/// </summary>
public class TimerService : ITimerService, IDisposable
{
    private readonly ILogger<TimerService> _logger;
    private readonly DispatcherTimer _updateTimer;
    private readonly TimeZoneInfo _timezone;
    private readonly List<EventSchedule> _eventSchedules = [];
    private bool _isRunning;

    public ObservableCollection<EventTimer> ActiveTimers { get; } = [];

    public event EventHandler<EventTimer>? TimerExpired;

    public TimerService(ILogger<TimerService> logger)
    {
        _logger = logger;

        // Standard-Timezone (GMT+1)
        try
        {
            _timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
        catch
        {
            _logger.LogWarning("Could not find W. Europe Standard Time, using UTC");
            _timezone = TimeZoneInfo.Utc;
        }

        // Timer für Updates (Standard: 1 Sekunde)
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _updateTimer.Tick += OnTimerTick;
    }

    public void Start()
    {
        if (_isRunning) return;

        _logger.LogInformation("Starting TimerService");
        _isRunning = true;
        _updateTimer.Start();
        ForceUpdate(); // Initiales Update
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _logger.LogInformation("Stopping TimerService");
        _isRunning = false;
        _updateTimer.Stop();
    }

    public void SetUpdateInterval(TimeSpan interval)
    {
        var wasRunning = _updateTimer.IsEnabled;
        if (wasRunning)
        {
            _updateTimer.Stop();
        }

        _updateTimer.Interval = interval;

        if (wasRunning)
        {
            _updateTimer.Start();
        }
    }

    public void AddEventSchedule(EventSchedule schedule)
    {
        if (_eventSchedules.Any(s => s.EventName == schedule.EventName))
        {
            _logger.LogWarning("Event schedule {EventName} already exists", schedule.EventName);
            return;
        }

        _eventSchedules.Add(schedule);

        // Erstelle Timer für dieses Event
        var timer = new EventTimer
        {
            EventName = schedule.EventName,
            Schedule = schedule
        };

        ActiveTimers.Add(timer);
        UpdateTimer(timer);
    }

    public void RemoveEventSchedule(string eventName)
    {
        var schedule = _eventSchedules.FirstOrDefault(s => s.EventName == eventName);
        if (schedule != null)
        {
            _eventSchedules.Remove(schedule);
        }

        var timer = ActiveTimers.FirstOrDefault(t => t.EventName == eventName);
        if (timer != null)
        {
            ActiveTimers.Remove(timer);
        }
    }

    public void ForceUpdate()
    {
        foreach (var timer in ActiveTimers)
        {
            UpdateTimer(timer);
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        ForceUpdate();
    }

    private void UpdateTimer(EventTimer timer)
    {
        if (timer.Schedule == null) return;

        var now = DateTime.UtcNow;
        var wasActive = timer.IsActive;

        // Update Aktiv-Status
        timer.IsActive = timer.Schedule.IsActive(now, _timezone);

        // Berechne verbleibende Zeit
        timer.TimeUntil = timer.Schedule.CalculateTimeUntil(now, _timezone);

        // Feuere Event wenn Timer gerade abgelaufen ist (0 Sekunden und vorher aktiv)
        if (wasActive && timer.TimeUntil.TotalSeconds <= 0)
        {
            _logger.LogInformation("Timer {EventName} expired", timer.EventName);
            TimerExpired?.Invoke(this, timer);
        }
    }

    public void Dispose()
    {
        _updateTimer.Stop();
        _updateTimer.Tick -= OnTimerTick;
        GC.SuppressFinalize(this);
    }
}
