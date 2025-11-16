using System.Linq;
using BlueMeter.WPF.Config;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.ViewModels;

public partial class SkillBreakdownViewModel : BaseViewModel
{
    private readonly ILogger<SkillBreakdownViewModel> _logger;
    private readonly IConfigManager _configManager;

    [ObservableProperty]
    private StatisticDataViewModel? _playerData;

    [ObservableProperty]
    private AppConfig _appConfig;

    public SkillBreakdownViewModel(
        ILogger<SkillBreakdownViewModel> logger,
        IConfigManager configManager)
    {
        _logger = logger;
        _configManager = configManager;
        _appConfig = configManager.CurrentConfig;

        // Subscribe to config changes
        _configManager.ConfigurationUpdated += OnConfigurationUpdated;
    }

    private void OnConfigurationUpdated(object? sender, AppConfig newConfig)
    {
        AppConfig = newConfig;
    }

    [RelayCommand]
    private void Confirm()
    {
        _logger.LogDebug("Confirm SkillBreakDown");
    }

    [RelayCommand]
    private void Cancel()
    {
        _logger.LogDebug("Cancel SkillBreakDown");
    }

    public void SetPlayerData(StatisticDataViewModel? player)
    {
        PlayerData = player;
        _logger.LogDebug("SkillBreakdownViewModel updated with player: {PlayerName}", player?.Player.Name ?? "null");

        // Notify all calculated properties to update
        OnPropertyChanged(nameof(TotalDamage));
        OnPropertyChanged(nameof(Dps));
        OnPropertyChanged(nameof(TotalHits));
        OnPropertyChanged(nameof(CritRate));
        OnPropertyChanged(nameof(CritCount));
        OnPropertyChanged(nameof(TotalCritDamage));
        OnPropertyChanged(nameof(TotalNormalDamage));
        OnPropertyChanged(nameof(FormattedDuration));
    }

    // Calculated properties for damage statistics
    public string TotalDamage => PlayerData?.Value.ToString("N0") ?? "0";

    public string Dps
    {
        get
        {
            if (PlayerData == null || PlayerData.Duration == 0)
                return "0";
            var durationSeconds = PlayerData.Duration / 1000.0; // Convert milliseconds to seconds
            var dps = PlayerData.Value / durationSeconds;
            return dps.ToString("N0");
        }
    }

    public string TotalHits
    {
        get
        {
            if (PlayerData == null) return "0";
            var total = PlayerData.SkillList.Sum(s => s.HitCount);
            return total.ToString("N0");
        }
    }

    public string CritCount
    {
        get
        {
            if (PlayerData == null) return "0";
            var total = PlayerData.SkillList.Sum(s => s.CritCount);
            return total.ToString("N0");
        }
    }

    public string CritRate
    {
        get
        {
            if (PlayerData == null) return "0%";
            var totalHits = PlayerData.SkillList.Sum(s => s.HitCount);
            var totalCrits = PlayerData.SkillList.Sum(s => s.CritCount);
            if (totalHits == 0) return "0%";
            var rate = (totalCrits / (double)totalHits) * 100;
            return rate.ToString("F1") + "%";
        }
    }

    public string TotalCritDamage
    {
        get
        {
            if (PlayerData == null) return "0";
            // Approximate: use highest crit * crit count for each skill
            var total = PlayerData.SkillList.Sum(s => (long)s.CritCount * s.HighestCrit);
            return total.ToString("N0");
        }
    }

    public string TotalNormalDamage
    {
        get
        {
            if (PlayerData == null) return "0";
            var total = (long)PlayerData.Value;
            var critDamage = PlayerData.SkillList.Sum(s => (long)s.CritCount * s.HighestCrit);
            var normalDamage = total - critDamage;
            return Math.Max(0, normalDamage).ToString("N0");
        }
    }

    public string FormattedDuration
    {
        get
        {
            if (PlayerData == null) return "00:00";
            return TimeSpan.FromMilliseconds(PlayerData.Duration).ToString(@"mm\:ss");
        }
    }
}