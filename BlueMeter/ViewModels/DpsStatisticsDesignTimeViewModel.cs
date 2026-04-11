using System.Collections.ObjectModel;
using Avalonia.Controls;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Serilog.Events;
using BlueMeter.Config;
using BlueMeter.Core.Analyze.Models;
using BlueMeter.Core.Data;
using BlueMeter.Core.Data.Models;
using BlueMeter.Localization;
using BlueMeter.Models;
using BlueMeter.Services;
using BlueMeter.Views;
using BlueMeter.WPF.Data; // IDataStorage still lives in this legacy namespace inside BlueMeter.Core

namespace BlueMeter.ViewModels;

public sealed class DpsStatisticsDesignTimeViewModel : DpsStatisticsViewModel
{
    public DpsStatisticsDesignTimeViewModel() : base(
        new DesignAppControlService(),
        new DesignDataStorage(),
        NullLogger<DpsStatisticsViewModel>.Instance,
        new DesignConfigManager(),
        new DesignWindowManagementService(),
        new DesignTopmostService(),
        new DesignTrayService(),
        new DebugFunctions(
            NullLogger<DebugFunctions>.Instance,
            new DesignLogObservable(),
            new DesignOptionsMonitor(),
            null!,
            LocalizationManager.Instance),
        new DesignChartDataService(),
        NullLoggerFactory.Instance,
        new DesignServiceProvider(),
        new DesignMessageDialogService())
    {
        // Populate with a few sample entries so the previewer shows something.
        try
        {
            for (var i = 0; i < 3; i++)
            {
                AddTestItem();
            }
        }
        catch
        {
            /* swallow design-time exceptions */
        }
    }

    #region Stub Implementations

    private sealed class DesignServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return null; // Design-time stub returns null
        }
    }

    private sealed class DesignChartDataService : IChartDataService
    {
        public bool IsRunning => false;

        // Event required by interface - design-time stub never fires this
#pragma warning disable CS0067 // Event is never used (design-time only)
        public event EventHandler<ChartHistoryClearingEventArgs>? BeforeHistoryCleared;
#pragma warning restore CS0067

        public void Start() { }
        public void Stop() { }
        public ObservableCollection<ChartDataPoint>? GetDpsHistory(long playerId) => null;
        public ObservableCollection<ChartDataPoint>? GetHpsHistory(long playerId) => null;
        public IReadOnlyCollection<long> GetTrackedPlayerIds() => Array.Empty<long>();
        public Dictionary<long, List<ChartDataPoint>> GetDpsHistorySnapshot() => new();
        public Dictionary<long, List<ChartDataPoint>> GetHpsHistorySnapshot() => new();
        public void LoadHistoricalChartData(
            Dictionary<long, List<ChartDataPoint>> dpsHistory,
            Dictionary<long, List<ChartDataPoint>> hpsHistory) { }
        public void Dispose() { }
    }

    private sealed class DesignTopmostService : ITopmostService
    {
        public void SetTopmost(Window window, bool enable)
        {
            // no-op at design time
        }

        public bool ToggleTopmost(Window window)
        {
            // Return current state at design time
            return window.Topmost = !window.Topmost;
        }
    }

    private sealed class DesignTrayService : ITrayService
    {
        public void Initialize(string? toolTip = null)
        {
            // no-op at design time
        }

        public void MinimizeToTray()
        {
            // no-op at design time
        }

        public void Restore()
        {
            // no-op at design time
        }

        public void Exit()
        {
            // no-op at design time
        }
    }

    private sealed class DesignAppControlService : IApplicationControlService
    {
        public void Shutdown()
        {
        }
    }

    private sealed class DesignWindowManagementService : IWindowManagementService
    {
        public void MinimizeDpsStatisticsWindow() { }
        public void ShowDpsStatistics() { }
        public void ShowSettings() { }
        public void ShowSettingsAndHighlightUidField() { }
        public void ShowAbout() { }
        public void ShowDamageReference() { }
        public void ShowSkillBreakdown() { }
        public void ShowModuleSolve() { }
        public void ShowBossTracker() { }
        public void ShowChecklist() { }
        public void ShowCharts() { }
        public void ShowChartsForFocusedPlayer(long playerUid) { }
        public Task LoadHistoricalEncounterInChartsAsync(string encounterId) => Task.CompletedTask;
        public void ShowEncounterHistory(EncounterHistoryViewModel viewModel) { }
        public Window GetDpsStatisticsWindow() => throw new NotSupportedException();
    }

    private sealed class DesignMessageDialogService : IMessageDialogService
    {
        public Task ShowInformationAsync(string title, string message) => Task.CompletedTask;
        public Task ShowWarningAsync(string title, string message) => Task.CompletedTask;
        public Task ShowErrorAsync(string title, string message) => Task.CompletedTask;
        public Task<bool> ShowConfirmationAsync(string title, string message) => Task.FromResult(true);
    }

    private sealed class DesignDataStorage : IDataStorage
    {
        public PlayerInfo CurrentPlayerInfo { get; } = new();

        public ReadOnlyDictionary<long, PlayerInfo> ReadOnlyPlayerInfoDatas { get; } =
            new(new Dictionary<long, PlayerInfo>());

        public ReadOnlyDictionary<long, DpsData> ReadOnlyFullDpsDatas => ReadOnlySectionedDpsDatas;
        public IReadOnlyList<DpsData> ReadOnlyFullDpsDataList { get; } = [];

        public ReadOnlyDictionary<long, DpsData> ReadOnlySectionedDpsDatas { get; } =
            new(new Dictionary<long, DpsData>());

        public IReadOnlyList<DpsData> ReadOnlySectionedDpsDataList { get; } = [];
        public TimeSpan SectionTimeout { get; set; } = TimeSpan.FromSeconds(5);
        bool IDataStorage.IsServerConnected { get; set; }
        public long CurrentPlayerUUID { get; set; }
        public bool IsServerConnected => false;

        // Boss tracking (design-time no-ops)
        public long ActiveBossUuid { get; set; }
        public DateTime? BossDeathTime { get; set; }

        public void RegisterBossEngagement(long enemyUuid) { }
        public void RegisterBossDeath(long enemyUuid) { }
        public bool ShouldEndBattleSection() => false;

#pragma warning disable CS0067
        public event ServerConnectionStateChangedEventHandler? ServerConnectionStateChanged;
        public event PlayerInfoUpdatedEventHandler? PlayerInfoUpdated;
        public event NewSectionCreatedEventHandler? NewSectionCreated;
        public event BattleLogCreatedEventHandler? BattleLogCreated;
        public event DpsDataUpdatedEventHandler? DpsDataUpdated;
        public event DataUpdatedEventHandler? DataUpdated;
        public event ServerChangedEventHandler? ServerChanged;
#pragma warning restore

        public void LoadPlayerInfoFromFile()
        {
        }

        public void SavePlayerInfoToFile()
        {
        }

        public Dictionary<long, PlayerInfoFileData> BuildPlayerDicFromBattleLog(List<BattleLog> battleLogs)
        {
            return new Dictionary<long, PlayerInfoFileData>();
        }

        public void ClearAllDpsData()
        {
        }

        public void ClearDpsData()
        {
        }

        public void ClearCurrentPlayerInfo()
        {
        }

        public void ClearPlayerInfos()
        {
        }

        public void ClearAllPlayerInfos()
        {
        }

        public void RaiseServerChanged(string currentServerStr, string prevServer)
        {
        }

        public void SetPlayerLevel(long playerUid, int tmpLevel)
        {
        }

        public bool EnsurePlayer(long playerUid)
        {
            return true;
        }

        public void SetPlayerHP(long playerUid, long hp)
        {
        }

        public void SetPlayerMaxHP(long playerUid, long maxHp)
        {
        }

        public void SetPlayerName(long playerUid, string playerName)
        {
        }

        public void SetPlayerCombatPower(long playerUid, int combatPower)
        {
        }

        public void SetPlayerProfessionID(long playerUid, int professionId)
        {
        }

        public void AddBattleLog(BattleLog log)
        {
        }

        public void SetPlayerRankLevel(long playerUid, int readInt32)
        {
        }

        public void SetPlayerCritical(long playerUid, int readInt32)
        {
        }

        public void SetPlayerLucky(long playerUid, int readInt32)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class DesignLogObservable : IObservable<LogEvent>
    {
        public IDisposable Subscribe(IObserver<LogEvent> observer)
        {
            return new DummyDisp();
        }

        private sealed class DummyDisp : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    private sealed class DesignOptionsMonitor : IOptionsMonitor<AppConfig>
    {
        public AppConfig CurrentValue { get; } = new() { DebugEnabled = true };

        public AppConfig Get(string? name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<AppConfig, string?> listener)
        {
            listener(CurrentValue, null);
            return new DummyDisp();
        }

        private sealed class DummyDisp : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    #endregion
}
