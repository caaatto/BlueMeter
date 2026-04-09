using BlueMeter.ViewModels;

namespace BlueMeter.Services;

/// <summary>
/// Service for opening, focusing and minimizing application windows.
/// In the original WPF project this exposed Window-typed properties; in the
/// Avalonia port (Phase 5+) it is a method-based contract so view models can
/// be ported before the views themselves exist (those land in Phase 10).
/// </summary>
public interface IWindowManagementService
{
    /// <summary>Minimize the main DPS statistics window.</summary>
    void MinimizeDpsStatisticsWindow();

    /// <summary>Show / focus the DPS statistics window.</summary>
    void ShowDpsStatistics();

    /// <summary>Show the settings window.</summary>
    void ShowSettings();

    /// <summary>Show the about window.</summary>
    void ShowAbout();

    /// <summary>Show the damage reference window.</summary>
    void ShowDamageReference();

    /// <summary>Show the module solver window.</summary>
    void ShowModuleSolve();

    /// <summary>Show the world boss tracker window.</summary>
    void ShowBossTracker();

    /// <summary>Show the daily / weekly checklist window.</summary>
    void ShowChecklist();

    /// <summary>Show the charts (advanced combat log) window.</summary>
    void ShowCharts();

    /// <summary>Show the charts window with a specific player focused.</summary>
    void ShowChartsForFocusedPlayer(long playerUid);

    /// <summary>Load a historical encounter into the charts window (showing it if needed).</summary>
    Task LoadHistoricalEncounterInChartsAsync(string encounterId);

    /// <summary>Show the encounter history window for the supplied view model.</summary>
    void ShowEncounterHistory(EncounterHistoryViewModel viewModel);
}
