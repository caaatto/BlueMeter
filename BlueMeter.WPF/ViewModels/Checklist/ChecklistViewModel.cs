using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueMeter.WPF.Models.Checklist;
using BlueMeter.WPF.Services.Checklist;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.ViewModels.Checklist;

/// <summary>
/// ViewModel für die Checklist-View
/// </summary>
public partial class ChecklistViewModel : BaseViewModel, IDisposable
{
    private readonly IChecklistService _checklistService;
    private readonly ITimerService _timerService;
    private readonly IStorageService _storageService;
    private readonly ILogger<ChecklistViewModel> _logger;

    #region Observable Properties

    /// <summary>
    /// Aktuell aktives Profil
    /// </summary>
    [ObservableProperty]
    private ChecklistProfile? _currentProfile;

    /// <summary>
    /// Alle verfügbaren Profile
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChecklistProfile> _profiles = [];

    /// <summary>
    /// Gefilterte Daily Tasks
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChecklistTask> _filteredDailyTasks = [];

    /// <summary>
    /// Gefilterte Weekly Tasks
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChecklistTask> _filteredWeeklyTasks = [];

    /// <summary>
    /// Suchbegriff für Task-Filter
    /// </summary>
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    /// <summary>
    /// Ob abgeschlossene Tasks angezeigt werden sollen
    /// </summary>
    [ObservableProperty]
    private bool _showCompletedTasks = true;

    /// <summary>
    /// Event-Timer (direkt vom Service)
    /// </summary>
    public ObservableCollection<EventTimer> EventTimers => _timerService.ActiveTimers;

    /// <summary>
    /// Ob die Checklist-View sichtbar ist
    /// </summary>
    [ObservableProperty]
    private bool _isVisible = true;

    /// <summary>
    /// Verbleibende Zeit bis Daily Reset (Mitternacht)
    /// </summary>
    [ObservableProperty]
    private string _dailyResetTimer = "00:00:00";

    /// <summary>
    /// Verbleibende Zeit bis Weekly Reset (Montag 00:00)
    /// </summary>
    [ObservableProperty]
    private string _weeklyResetTimer = "0d 00:00:00";

    #endregion

    public ChecklistViewModel(
        IChecklistService checklistService,
        ITimerService timerService,
        IStorageService storageService,
        ILogger<ChecklistViewModel> logger)
    {
        _checklistService = checklistService;
        _timerService = timerService;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Initialisiert das ViewModel (async)
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Lade Profile
            await LoadProfilesAsync();

            // Starte Timer-Service
            _timerService.Start();

            // Prüfe auf Auto-Reset
            await _checklistService.CheckAndResetTasksAsync();

            // Registriere Property-Changed-Handler
            PropertyChanged += OnPropertyChanged;

            // Starte Reset-Timer Updates
            StartResetTimers();

            _logger.LogInformation("ChecklistViewModel initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ChecklistViewModel");
        }
    }

    private System.Threading.Timer? _resetTimer;

    private void StartResetTimers()
    {
        // Update Timer sofort
        UpdateResetTimers();

        // Update Timer jede Sekunde
        _resetTimer = new System.Threading.Timer(
            _ => UpdateResetTimers(),
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)
        );
    }

    private void UpdateResetTimers()
    {
        // GMT+1 Zeitzone
        var germanyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var nowGmt1 = TimeZoneInfo.ConvertTime(DateTime.Now, germanyTimeZone);

        // Daily Reset: bis 8:00 Uhr GMT+1
        var dailyReset = nowGmt1.Date.AddHours(8);
        if (nowGmt1.TimeOfDay >= TimeSpan.FromHours(8))
        {
            // Wenn es schon nach 8:00 ist, nehme nächsten Tag 8:00
            dailyReset = dailyReset.AddDays(1);
        }
        var dailyTimeRemaining = dailyReset - nowGmt1;
        DailyResetTimer = $"{dailyTimeRemaining.Hours:D2}:{dailyTimeRemaining.Minutes:D2}:{dailyTimeRemaining.Seconds:D2}";

        // Weekly Reset: bis Montag 8:00 Uhr GMT+1
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)nowGmt1.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0 && nowGmt1.TimeOfDay >= TimeSpan.FromHours(8))
        {
            daysUntilMonday = 7; // Nächster Montag
        }
        var nextMondayReset = nowGmt1.Date.AddDays(daysUntilMonday).AddHours(8);
        var weeklyTimeRemaining = nextMondayReset - nowGmt1;
        var days = (int)weeklyTimeRemaining.TotalDays;
        WeeklyResetTimer = $"{days}d {weeklyTimeRemaining.Hours:D2}:{weeklyTimeRemaining.Minutes:D2}:{weeklyTimeRemaining.Seconds:D2}";
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update Filter wenn SearchQuery oder ShowCompletedTasks ändert
        if (e.PropertyName == nameof(SearchQuery) || e.PropertyName == nameof(ShowCompletedTasks))
        {
            UpdateFilteredTasks();
        }

        // Update Filter wenn CurrentProfile ändert
        if (e.PropertyName == nameof(CurrentProfile))
        {
            UpdateFilteredTasks();
        }
    }

    #region Commands

    /// <summary>
    /// Toggle Task Completion
    /// </summary>
    [RelayCommand]
    private void ToggleTask(ChecklistTask task)
    {
        _checklistService.ToggleTaskCompletion(task);
        UpdateFilteredTasks();
    }

    /// <summary>
    /// Inkrementiert Task-Fortschritt
    /// </summary>
    [RelayCommand]
    private void IncrementProgress(ChecklistTask task)
    {
        _checklistService.IncrementTaskProgress(task);
    }

    /// <summary>
    /// Dekrementiert Task-Fortschritt
    /// </summary>
    [RelayCommand]
    private void DecrementProgress(ChecklistTask task)
    {
        _checklistService.DecrementTaskProgress(task);
    }

    /// <summary>
    /// Markiert alle Daily Tasks als completed
    /// </summary>
    [RelayCommand]
    private void SelectAllDaily()
    {
        if (CurrentProfile?.DailyTasks == null) return;
        _checklistService.SetAllTasksCompletion(CurrentProfile.DailyTasks, true);
        UpdateFilteredTasks();
    }

    /// <summary>
    /// Markiert alle Daily Tasks als uncompleted
    /// </summary>
    [RelayCommand]
    private void DeselectAllDaily()
    {
        if (CurrentProfile?.DailyTasks == null) return;
        _checklistService.SetAllTasksCompletion(CurrentProfile.DailyTasks, false);
        UpdateFilteredTasks();
    }

    /// <summary>
    /// Markiert alle Weekly Tasks als completed
    /// </summary>
    [RelayCommand]
    private void SelectAllWeekly()
    {
        if (CurrentProfile?.WeeklyTasks == null) return;
        _checklistService.SetAllTasksCompletion(CurrentProfile.WeeklyTasks, true);
        UpdateFilteredTasks();
    }

    /// <summary>
    /// Markiert alle Weekly Tasks als uncompleted
    /// </summary>
    [RelayCommand]
    private void DeselectAllWeekly()
    {
        if (CurrentProfile?.WeeklyTasks == null) return;
        _checklistService.SetAllTasksCompletion(CurrentProfile.WeeklyTasks, false);
        UpdateFilteredTasks();
    }

    /// <summary>
    /// Toggle Show Completed Tasks
    /// </summary>
    [RelayCommand]
    private void ToggleShowCompleted()
    {
        ShowCompletedTasks = !ShowCompletedTasks;
    }

    /// <summary>
    /// Exportiert aktuelles Profil
    /// </summary>
    [RelayCommand]
    private async Task ExportProgressAsync()
    {
        try
        {
            if (CurrentProfile == null)
            {
                _logger.LogWarning("No current profile to export");
                return;
            }

            var filePath = await _storageService.ExportProfileAsync(CurrentProfile);
            if (!string.IsNullOrEmpty(filePath))
            {
                _logger.LogInformation("Successfully exported profile to {FilePath}", filePath);
                // TODO: Zeige Success-Message
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting profile");
            // TODO: Zeige Error-Message
        }
    }

    /// <summary>
    /// Importiert ein Profil
    /// </summary>
    [RelayCommand]
    private async Task ImportProgressAsync()
    {
        try
        {
            var profile = await _storageService.ImportProfileAsync();
            if (profile != null)
            {
                Profiles.Add(profile);
                CurrentProfile = profile;
                _logger.LogInformation("Successfully imported profile {ProfileName}", profile.ProfileName);
                // TODO: Zeige Success-Message
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing profile");
            // TODO: Zeige Error-Message
        }
    }

    /// <summary>
    /// Wechselt das aktive Profil
    /// </summary>
    [RelayCommand]
    private void SwitchProfile(ChecklistProfile profile)
    {
        if (profile == null) return;

        _checklistService.SetCurrentProfile(profile.ProfileId);
        CurrentProfile = profile;
        UpdateFilteredTasks();
    }

    /// <summary>
    /// Erstellt ein neues Profil
    /// </summary>
    [RelayCommand]
    private void CreateNewProfile()
    {
        // TODO: Dialog für Profil-Namen
        var profileName = $"Character {Profiles.Count + 1}";
        var newProfile = _checklistService.CreateNewProfile(profileName);
        Profiles.Add(newProfile);
        CurrentProfile = newProfile;
    }

    /// <summary>
    /// Löscht ein Profil
    /// </summary>
    [RelayCommand]
    private async Task DeleteProfileAsync(ChecklistProfile profile)
    {
        if (profile == null) return;

        // TODO: Confirmation-Dialog
        var success = await _checklistService.DeleteProfileAsync(profile.ProfileId);
        if (success)
        {
            Profiles.Remove(profile);

            // Wenn gelöschtes Profil aktuell war, wechsle zum ersten
            if (CurrentProfile?.ProfileId == profile.ProfileId)
            {
                CurrentProfile = Profiles.FirstOrDefault();
            }
        }
    }

    /// <summary>
    /// Erzwingt Update aller Timer
    /// </summary>
    [RelayCommand]
    private void RefreshTimers()
    {
        _timerService.ForceUpdate();
    }

    #endregion

    #region Private Methods

    private async Task LoadProfilesAsync()
    {
        var profiles = await _checklistService.LoadAllProfilesAsync();

        Profiles.Clear();
        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }

        CurrentProfile = _checklistService.GetCurrentProfile();

        if (CurrentProfile == null && Profiles.Count > 0)
        {
            CurrentProfile = Profiles[0];
        }

        UpdateFilteredTasks();
    }

    private void UpdateFilteredTasks()
    {
        if (CurrentProfile == null) return;

        // Filter Daily Tasks
        var dailyFiltered = _checklistService.FilterTasks(
            CurrentProfile.DailyTasks,
            SearchQuery,
            ShowCompletedTasks);

        FilteredDailyTasks.Clear();
        foreach (var task in dailyFiltered)
        {
            FilteredDailyTasks.Add(task);
        }

        // Filter Weekly Tasks
        var weeklyFiltered = _checklistService.FilterTasks(
            CurrentProfile.WeeklyTasks,
            SearchQuery,
            ShowCompletedTasks);

        FilteredWeeklyTasks.Clear();
        foreach (var task in weeklyFiltered)
        {
            FilteredWeeklyTasks.Add(task);
        }
    }

    #endregion

    public void Dispose()
    {
        _timerService.Stop();
        _resetTimer?.Dispose();
        PropertyChanged -= OnPropertyChanged;
        GC.SuppressFinalize(this);
    }
}
