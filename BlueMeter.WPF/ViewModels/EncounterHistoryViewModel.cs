using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueMeter.Core.Data;
using BlueMeter.Core.Data.Database;
using BlueMeter.Core.Data.Models;
using System.Collections.Generic;
using BlueMeter.Core.Models;
using Newtonsoft.Json;

namespace BlueMeter.WPF.ViewModels;

/// <summary>
/// ViewModel for encounter history window
/// </summary>
public partial class EncounterHistoryViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<EncounterSummaryViewModel> _encounters = new();

    [ObservableProperty]
    private EncounterSummaryViewModel? _selectedEncounter;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Loading encounters...";

    public event Action? RequestClose;
    public event Action<EncounterData>? LoadEncounterRequested;

    [RelayCommand]
    private async Task LoadedAsync()
    {
        await RefreshEncountersAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await RefreshEncountersAsync();
    }

    private async Task RefreshEncountersAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading encounters...";

        try
        {
            var encounterService = DataStorageExtensions.GetEncounterService();
            if (encounterService == null)
            {
                StatusMessage = "Database not initialized. Please ensure the database has been set up correctly.";
                MessageBox.Show(
                    "The database is not initialized.\n\n" +
                    "The encounter history database may not have been set up yet. " +
                    "Try running a few battles first to create some encounters.",
                    "Database Not Initialized",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var encounters = await DataStorageExtensions.GetRecentEncountersAsync(100);

            Encounters.Clear();
            foreach (var encounter in encounters)
            {
                Encounters.Add(new EncounterSummaryViewModel(encounter));
            }

            if (Encounters.Count == 0)
            {
                StatusMessage = "No encounters found. Run some battles to create encounter history.";
            }
            else
            {
                StatusMessage = $"Loaded {Encounters.Count} encounter(s)";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading encounters: {ex.Message}";
            MessageBox.Show(
                $"An error occurred while loading encounters:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadSelectedEncounterAsync()
    {
        if (SelectedEncounter == null) return;

        IsLoading = true;
        StatusMessage = "Loading encounter data...";

        try
        {
            var encounterData = await DataStorageExtensions.LoadEncounterAsync(SelectedEncounter.EncounterId);
            if (encounterData != null)
            {
                LoadEncounterRequested?.Invoke(encounterData);
                RequestClose?.Invoke();
            }
            else
            {
                StatusMessage = "Failed to load encounter data";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading encounter: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task DeleteSelectedEncounterAsync()
    {
        if (SelectedEncounter == null) return Task.CompletedTask;

        var result = MessageBox.Show(
            $"Are you sure you want to delete this encounter?\n\n{SelectedEncounter.DisplayName}",
            "Delete Encounter",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return Task.CompletedTask;

        // TODO: Implement delete functionality in EncounterService
        StatusMessage = "Delete functionality not yet implemented";
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeleteAllHistoryAsync()
    {
        var result = MessageBox.Show(
            $"⚠️ WARNING ⚠️\n\n" +
            $"This will delete ALL {Encounters.Count} encounters from the database!\n\n" +
            "This action CANNOT be undone.\n\n" +
            "Are you absolutely sure you want to delete the entire history?",
            "Delete All History",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            StatusMessage = "Delete cancelled";
            return;
        }

        IsLoading = true;
        StatusMessage = "Deleting all encounters...";

        try
        {
            var deletedCount = await DataStorageExtensions.DeleteAllEncountersAsync();

            StatusMessage = $"Successfully deleted {deletedCount} encounter(s)";

            // Refresh the list
            await RefreshEncountersAsync();

            MessageBox.Show(
                $"Successfully deleted {deletedCount} encounter(s) from the database.",
                "History Cleared",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting encounters: {ex.Message}";
            MessageBox.Show(
                $"An error occurred while deleting encounters:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke();
    }
}

/// <summary>
/// ViewModel wrapper for EncounterSummary
/// </summary>
public partial class EncounterSummaryViewModel : BaseViewModel
{
    private readonly EncounterSummary _encounter;

    public EncounterSummaryViewModel(EncounterSummary encounter)
    {
        _encounter = encounter;
    }

    public string EncounterId => _encounter.EncounterId;
    public DateTime StartTime => _encounter.StartTime;
    public DateTime? EndTime => _encounter.EndTime;
    public long DurationMs => _encounter.DurationMs;
    public long TotalDamage => _encounter.TotalDamage;
    public long TotalHealing => _encounter.TotalHealing;
    public int PlayerCount => _encounter.PlayerCount;
    public bool IsActive => _encounter.IsActive;
    public string BossName => _encounter.BossName;

    public string DisplayName => _encounter.DisplayName;
    public string FormattedStartTime => StartTime.ToString("yyyy-MM-dd HH:mm:ss");
    public string FormattedDuration => TimeSpan.FromMilliseconds(DurationMs).ToString(@"mm\:ss");

    public string FormattedTotalDamage => FormatNumber(TotalDamage);
    public string FormattedTotalHealing => FormatNumber(TotalHealing);

    private string FormatNumber(long value)
    {
        if (value >= 100_000_000)
            return $"{value / 100_000_000.0:F1}亿";
        if (value >= 10_000)
            return $"{value / 10_000.0:F1}万";
        return value.ToString("N0");
    }
}
