using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.WPF.Models.Checklist;

/// <summary>
/// Repräsentiert einen einzelnen Task in der Checklist
/// </summary>
public partial class ChecklistTask : ObservableObject
{
    /// <summary>
    /// Eindeutige ID des Tasks
    /// </summary>
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    /// <summary>
    /// Angezeigter Label-Text
    /// </summary>
    [ObservableProperty]
    private string _label = string.Empty;

    /// <summary>
    /// Kategorie des Tasks (bestimmt Farbe)
    /// </summary>
    [ObservableProperty]
    private TaskCategory _category;

    /// <summary>
    /// Typ des Tasks (Daily/Weekly)
    /// </summary>
    [ObservableProperty]
    private TaskType _type;

    /// <summary>
    /// Ob der Task incremental ist (mit Counter)
    /// </summary>
    [ObservableProperty]
    private bool _isIncremental;

    /// <summary>
    /// Aktueller Fortschritt (für incremental tasks)
    /// </summary>
    [ObservableProperty]
    private int _currentProgress;

    /// <summary>
    /// Maximaler Fortschritt (für incremental tasks)
    /// </summary>
    [ObservableProperty]
    private int _maxProgress = 1;

    /// <summary>
    /// Ob der Task abgeschlossen ist
    /// </summary>
    [ObservableProperty]
    private bool _isCompleted;

    /// <summary>
    /// Farbcode für die Kategorie (wird dynamisch berechnet)
    /// </summary>
    public string ColorCode => Category.GetColorCode();

    /// <summary>
    /// Fortschritt in Prozent (für ProgressBar)
    /// </summary>
    public double ProgressPercentage => MaxProgress > 0
        ? (double)CurrentProgress / MaxProgress * 100
        : 0;

    /// <summary>
    /// Fortschrittstext (z.B. "6 / 9 complete")
    /// </summary>
    public string ProgressText => $"{CurrentProgress} / {MaxProgress}";

    partial void OnCategoryChanged(TaskCategory value)
    {
        OnPropertyChanged(nameof(ColorCode));
    }

    partial void OnCurrentProgressChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(ProgressText));

        // Auto-Complete wenn Max erreicht
        if (IsIncremental && value >= MaxProgress)
        {
            IsCompleted = true;
        }
    }

    partial void OnMaxProgressChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(ProgressText));
    }
}
