using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using BlueMeter.Core;
using BlueMeter.Core.Analyze;
using BlueMeter.Core.Analyze.Exceptions;
using BlueMeter.Core.Data.Models;
using BlueMeter.Core.Extends.Data;
using BlueMeter.Core.Models;
using BlueMeter.WPF.Config;
using BlueMeter.WPF.Data;
using BlueMeter.WPF.Extensions;
using BlueMeter.WPF.Models;
using BlueMeter.WPF.Services;
using BlueMeter.WPF.Logging;

namespace BlueMeter.WPF.ViewModels;

public partial class DpsStatisticsOptions : BaseViewModel
{
    [ObservableProperty] private int _minimalDurationInSeconds = -1;

    public bool IsRecordAll => MinimalDurationInSeconds == -1;
    public bool IsSkip2Sec => MinimalDurationInSeconds == 2;
    public bool IsSkip5Sec => MinimalDurationInSeconds == 5;

    [RelayCommand]
    private void SetMinimalDuration(int duration)
    {
        MinimalDurationInSeconds = duration;
        OnPropertyChanged(nameof(IsRecordAll));
        OnPropertyChanged(nameof(IsSkip2Sec));
        OnPropertyChanged(nameof(IsSkip5Sec));
    }
}

public partial class DpsStatisticsViewModel : BaseViewModel, IDisposable
{
    private readonly IApplicationControlService _appControlService;
    private readonly IConfigManager _configManager;
    private readonly Dispatcher _dispatcher;
    // Use a single stopwatch for both total and section durations
    private readonly Stopwatch _timer = new();
    // Snapshot of elapsed time at the moment a new section starts
    private TimeSpan _sectionStartElapsed = TimeSpan.Zero;
    // Whether we are waiting for the first datapoint of a new section
    private bool _awaitingSectionStart;
    // Captured elapsed of the last section to freeze UI until new data arrives
    private TimeSpan _lastSectionElapsed = TimeSpan.Zero;
    private readonly ILogger<DpsStatisticsViewModel> _logger;
    private readonly IDataStorage _storage;
    private readonly IWindowManagementService _windowManagement;
    private readonly ITopmostService _topmostService;
    private readonly ITrayService _trayService;
    private readonly IChartDataService _chartDataService;
    private readonly ILoggerFactory _loggerFactory;
    private DispatcherTimer? _durationTimer;
    private bool _isInitialized;
    // UI update throttling to prevent freezing during intense combat
    // Increased from 100ms to 200ms to improve performance in high-activity scenarios (raids/WBC)
    private DateTime _lastUiUpdate = DateTime.MinValue;
    private bool _pendingUiUpdate;
    private readonly TimeSpan _uiUpdateThrottle = TimeSpan.FromMilliseconds(200);
    // Combat pause detection - pause timer when no damage (but don't archive!)
    private DateTime _lastDamageTime = DateTime.MinValue;
    private readonly TimeSpan _combatPauseThreshold = TimeSpan.FromSeconds(1); // Stop meter after 1s of no combat
    private ulong _lastKnownMaxTick;
    [ObservableProperty] private ScopeTime _scopeTime = ScopeTime.Current;
    [ObservableProperty] private bool _showContextMenu;
    [ObservableProperty] private SortDirectionEnum _sortDirection = SortDirectionEnum.Descending;
    [ObservableProperty] private string _sortMemberPath = "Value";
    [ObservableProperty] private StatisticType _statisticIndex;
    [ObservableProperty] private AppConfig _appConfig;
    [ObservableProperty] private TimeSpan _battleDuration;
    [ObservableProperty] private bool _isHistoryMode;
    [ObservableProperty] private string _historyModeLabel = string.Empty;
    [ObservableProperty] private bool _isShowingLastBattle;
    [ObservableProperty] private string _battleStatusLabel = string.Empty;
    private Dictionary<long, PlayerInfo>? _historicalPlayerInfos;
    private Dictionary<long, DpsData>? _historicalDpsData;
    // Snapshot of Last Battle's raw data to enable filtering during Last Battle view
    private IReadOnlyList<DpsData>? _lastBattleDataSnapshot;

    /// <inheritdoc/>
    public DpsStatisticsViewModel(IApplicationControlService appControlService,
        IDataStorage storage,
        ILogger<DpsStatisticsViewModel> logger,
        IConfigManager configManager,
        IWindowManagementService windowManagement,
        ITopmostService topmostService,
        ITrayService trayService,
        DebugFunctions debugFunctions,
        Dispatcher dispatcher,
        IChartDataService chartDataService,
        ILoggerFactory loggerFactory)
    {
        StatisticData = new Dictionary<StatisticType, DpsStatisticsSubViewModel>
        {
            {
                StatisticType.Damage,
                new DpsStatisticsSubViewModel(logger, dispatcher, StatisticType.Damage, storage, debugFunctions)
            },
            {
                StatisticType.TakenDamage,
                new DpsStatisticsSubViewModel(logger, dispatcher, StatisticType.TakenDamage, storage, debugFunctions)
            },
            {
                StatisticType.Healing,
                new DpsStatisticsSubViewModel(logger, dispatcher,StatisticType.Healing, storage, debugFunctions)
            },
            {
                StatisticType.NpcTakenDamage,
                new DpsStatisticsSubViewModel(logger, dispatcher,StatisticType.TakenDamage, storage, debugFunctions)
            }
        };
        _configManager = configManager;
        _configManager.ConfigurationUpdated += ConfigManagerOnConfigurationUpdated;
        _appConfig = _configManager.CurrentConfig;

        DebugFunctions = debugFunctions;
        _appControlService = appControlService;
        _storage = storage;
        _logger = logger;

        // Log loaded configuration for debugging
        _logger.LogInformation("[STARTUP] Configuration loaded. TrainingMode={TrainingMode}, ManualPlayerUid={ManualPlayerUid}",
            _appConfig.TrainingMode, _appConfig.ManualPlayerUid);

        // IMPORTANT: Always reset TrainingMode to None on startup (but keep ManualPlayerUid)
        // User must manually activate Solo Training each session
        if (_appConfig.TrainingMode != Models.TrainingMode.None)
        {
            _logger.LogInformation("[STARTUP] Resetting TrainingMode from {OldMode} to None", _appConfig.TrainingMode);
            _appConfig.TrainingMode = Models.TrainingMode.None;
        }
        _windowManagement = windowManagement;
        _topmostService = topmostService;
        _trayService = trayService;
        _dispatcher = dispatcher;
        _chartDataService = chartDataService;
        _loggerFactory = loggerFactory;

        // Subscribe to DebugFunctions events to handle sample data requests
        DebugFunctions.SampleDataRequested += OnSampleDataRequested;
        _storage.PlayerInfoUpdated += StorageOnPlayerInfoUpdated;

        // set config
    }

    public Dictionary<StatisticType, DpsStatisticsSubViewModel> StatisticData { get; }

    public DpsStatisticsSubViewModel CurrentStatisticData => StatisticData[StatisticIndex];

    public DebugFunctions DebugFunctions { get; }

    public DpsStatisticsOptions Options { get; } = new();

    public void Dispose()
    {
        // Unsubscribe from DebugFunctions events
        DebugFunctions.SampleDataRequested -= OnSampleDataRequested;
        _configManager.ConfigurationUpdated -= ConfigManagerOnConfigurationUpdated;

        if (_durationTimer != null)
        {
            _durationTimer.Stop();
            _durationTimer.Tick -= DurationTimerOnTick;
        }

        _storage.DpsDataUpdated -= DataStorage_DpsDataUpdated;
        _storage.NewSectionCreated -= StorageOnNewSectionCreated;
        _storage.PlayerInfoUpdated -= StorageOnPlayerInfoUpdated;
        _storage.Dispose();

        // MEMORY LEAK FIX: Dispose all DpsStatisticsSubViewModel instances to clean up their event subscriptions.
        // Previously only set Initialized = false, but this left CollectionChanged event subscriptions active,
        // causing the BulkObservableCollection to hold references to these ViewModels indefinitely.
        foreach (var dpsStatisticsSubViewModel in StatisticData.Values)
        {
            dpsStatisticsSubViewModel.Initialized = false;
            dpsStatisticsSubViewModel.Dispose();
        }

        _isInitialized = false;
    }

    private void ConfigManagerOnConfigurationUpdated(object? sender, AppConfig newConfig)
    {
        if (_dispatcher.CheckAccess())
        {
            AppConfig = newConfig;
        }
        else
        {
            _dispatcher.Invoke(() => AppConfig = newConfig);
        }
    }

    private void OnSampleDataRequested(object? sender, EventArgs e)
    {
        // Handle the event from DebugFunctions
        AddRandomData();
    }

    /// <summary>
    /// 切换窗口置顶状态（命令）。
    /// 通过绑定 Window.Topmost 到 AppConfig.TopmostEnabled 实现。
    /// </summary>
    [RelayCommand]
    private async Task ToggleTopmost()
    {
        AppConfig.TopmostEnabled = !AppConfig.TopmostEnabled;
        try
        {
            await _configManager.SaveAsync(AppConfig);
        }
        catch(InvalidOperationException ex)
        {
            // Ignore
            _logger.LogError(ex, "Failed to save AppConfig");
        }
    }

    [RelayCommand]
    public void ResetAll()
    {
        _storage.ClearAllDpsData();
        _timer.Reset();
        _sectionStartElapsed = TimeSpan.Zero;
        _awaitingSectionStart = false;
        _lastSectionElapsed = TimeSpan.Zero;
        IsShowingLastBattle = false;
        BattleStatusLabel = string.Empty;
        _lastBattleDataSnapshot = null;

        // Reset combat tracking
        _lastDamageTime = DateTime.MinValue;
        _lastKnownMaxTick = 0;

        // Clear current UI data for all statistic types and rebuild from the new section snapshot
        foreach (var subVm in StatisticData.Values)
        {
            subVm.Reset();
        }
    }

    [RelayCommand]
    public void ResetSection()
    {
        _storage.ClearDpsData();
        // Move section start to current elapsed so section duration becomes zero
        _sectionStartElapsed = _timer.Elapsed;

        // Reset combat tracking for new section
        _lastDamageTime = DateTime.MinValue;
        _lastKnownMaxTick = 0;

        // Note: ManualPlayerUid is NOT reset here - it should persist in settings
        _logger.LogInformation("[RESET] Section reset complete. TrainingMode={Mode}, ManualUID={UID}",
            AppConfig.TrainingMode, AppConfig.ManualPlayerUid);
    }

    /// <summary>
    /// 读取用户缓存
    /// </summary>
    private void LoadPlayerCache()
    {
        try
        {
            _storage.LoadPlayerInfoFromFile();
        }
        catch (FileNotFoundException)
        {
            // 没有缓存
        }
        catch (DataTamperedException)
        {
            _storage.ClearAllPlayerInfos();
            _storage.SavePlayerInfoToFile();
        }
    }

    [RelayCommand]
    private void OnLoaded()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        foreach (var vm in StatisticData.Values)
        {
            vm.Initialized = true;
        }

        _logger.LogDebug(WpfLogEvents.VmLoaded, "DpsStatisticsViewModel loaded");
        LoadPlayerCache();

        EnsureDurationTimerStarted();
        UpdateBattleDuration();

        // 开始监听DPS更新事件
        _storage.DpsDataUpdated += DataStorage_DpsDataUpdated;
        _storage.NewSectionCreated += StorageOnNewSectionCreated;
    }

    [RelayCommand]
    private void OnUnloaded()
    {
    }

    [RelayCommand]
    private void OnResize()
    {
        _logger.LogDebug("Window Resized");
    }

    private void DataStorage_DpsDataUpdated()
    {
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(DataStorage_DpsDataUpdated);
            return;
        }

        // Don't update UI if we're viewing history
        if (IsHistoryMode)
        {
            return;
        }

        // Throttle UI updates to prevent freezing during intense combat
        var now = DateTime.UtcNow;
        var timeSinceLastUpdate = now - _lastUiUpdate;

        if (timeSinceLastUpdate < _uiUpdateThrottle)
        {
            // Too soon since last update - schedule a delayed update if not already pending
            if (!_pendingUiUpdate)
            {
                _pendingUiUpdate = true;
                var delay = _uiUpdateThrottle - timeSinceLastUpdate;
                Task.Delay(delay).ContinueWith(_ =>
                {
                    _dispatcher.BeginInvoke(() =>
                    {
                        _pendingUiUpdate = false;
                        PerformUiUpdate();
                    });
                });
            }
            return;
        }

        PerformUiUpdate();
    }

    private void PerformUiUpdate()
    {
        _lastUiUpdate = DateTime.UtcNow;

        // When showing Last Battle and user toggles filter, use the snapshot
        IReadOnlyList<DpsData> dpsList;
        if (IsShowingLastBattle && _lastBattleDataSnapshot != null && ScopeTime == ScopeTime.Current)
        {
            dpsList = _lastBattleDataSnapshot;
            _logger.LogInformation("[LAST BATTLE] Using snapshot with {Count} players for filtering", dpsList.Count);
        }
        else
        {
            dpsList = ScopeTime == ScopeTime.Total
                ? _storage.ReadOnlyFullDpsDataList
                : _storage.ReadOnlySectionedDpsDataList;

            // Capture snapshot of active combat data (before it gets cleared by NewSectionCreated)
            // Only capture when we have valid section data during active combat
            if (ScopeTime == ScopeTime.Current && !IsShowingLastBattle && dpsList.Count > 0)
            {
                _lastBattleDataSnapshot = dpsList.ToList();
                _logger.LogDebug("[SNAPSHOT] Captured active combat data with {Count} players", _lastBattleDataSnapshot.Count);
            }
        }

        // Track new damage to update _lastDamageTime
        var maxTick = dpsList.Any() ? (ulong)dpsList.Max(d => d.LastLoggedTick) : 0UL;
        var hasNewDamage = maxTick > _lastKnownMaxTick;

        if (hasNewDamage)
        {
            // New damage received - update tracking
            _lastKnownMaxTick = maxTick;
            _lastDamageTime = DateTime.UtcNow;

            // Start/resume timer if not running
            if (!_timer.IsRunning)
            {
                _timer.Start();
                _logger.LogDebug("Combat resumed - timer started");
            }
        }

        // If a new section was created, wait until first datapoint to reset UI and mark section start
        var hasSectionDamage = HasDamageData(_storage.ReadOnlySectionedDpsDataList);
        if (_awaitingSectionStart && hasSectionDamage)
        {
            foreach (var subVm in StatisticData.Values)
            {
                subVm.Reset();
            }
            _sectionStartElapsed = _timer.Elapsed;
            _lastSectionElapsed = TimeSpan.Zero;
            _awaitingSectionStart = false;
            IsShowingLastBattle = false;
            BattleStatusLabel = string.Empty;
            // Clear Last Battle snapshot when new combat starts
            _lastBattleDataSnapshot = null;
            _logger.LogInformation("[LAST BATTLE] Cleared snapshot - new combat started");
        }

        UpdateData(dpsList);
        UpdateBattleDuration();
    }

    private static bool HasDamageData(IReadOnlyList<DpsData> data)
    {
        return data.Any(t => t.TotalAttackDamage > 0);
    }

    private void UpdateData(IReadOnlyList<DpsData> data)
    {
        _logger.LogTrace(WpfLogEvents.VmUpdateData, "Update data requested: {Count} entries", data.Count);

        var currentPlayerUid = _storage.CurrentPlayerInfo.UID;

        // Pre-process data once for all statistic types
        var processedDataByType = PreProcessDataForAllTypes(data);

        // Update each subViewModel with its pre-processed data
        foreach (var (statisticType, processedData) in processedDataByType)
        {
            if (!StatisticData.TryGetValue(statisticType, out var subViewModel)) continue;
            subViewModel.ScopeTime = ScopeTime;
            subViewModel.UpdateDataOptimized(processedData, currentPlayerUid);
        }
    }

    /// <summary>
    /// Pre-processes data once for all statistic types to avoid redundant iterations
    /// </summary>
    private Dictionary<StatisticType, Dictionary<long, DpsDataProcessed>> PreProcessDataForAllTypes(
        IReadOnlyList<DpsData> data)
    {
        var result = new Dictionary<StatisticType, Dictionary<long, DpsDataProcessed>>
        {
            [StatisticType.Damage] = new(),
            [StatisticType.Healing] = new(),
            [StatisticType.TakenDamage] = new(),
            [StatisticType.NpcTakenDamage] = new()
        };

        // Get current player UID for Personal training mode filtering
        // Use ManualPlayerUid if set, otherwise fall back to auto-detected UID
        var currentPlayerUid = AppConfig.ManualPlayerUid > 0
            ? AppConfig.ManualPlayerUid
            : _storage.CurrentPlayerInfo.UID;
        var isPersonalMode = AppConfig.TrainingMode == Models.TrainingMode.Personal;

        // ALWAYS log TrainingMode and filtering status for debugging
        _logger.LogInformation("[FILTER] PreProcessDataForAllTypes called. TrainingMode={TrainingMode}, IsPersonalMode={IsPersonalMode}, CurrentPlayerUID={CurrentPlayerUID}, ManualUID={ManualUID}, DataCount={DataCount}",
            AppConfig.TrainingMode, isPersonalMode, currentPlayerUid, AppConfig.ManualPlayerUid, data.Count);

        // Log Personal mode status for debugging
        if (isPersonalMode)
        {
            _logger.LogInformation("[PERSONAL MODE] Active! CurrentPlayerUID={CurrentPlayerUID}, DataCount={DataCount}",
                currentPlayerUid, data.Count);

            if (currentPlayerUid == 0)
            {
                _logger.LogWarning("[PERSONAL MODE] CurrentPlayerUID is 0! Player not detected yet. Disabling filter.");
            }
        }

        // Single pass through the data
        foreach (var dpsData in data)
        {
            // Filter: In Personal training mode, only show current player's data
            // Skip if: Personal mode is active AND CurrentPlayerUID is valid (not 0) AND it's not the current player AND it's not an NPC
            if (isPersonalMode && currentPlayerUid > 0 && dpsData.UID != currentPlayerUid && !dpsData.IsNpcData)
            {
                _logger.LogInformation("[PERSONAL MODE] Filtering out player UID={FilteredUID} (CurrentPlayer={CurrentUID}, IsNpc={IsNpc})",
                    dpsData.UID, currentPlayerUid, dpsData.IsNpcData);
                continue;
            }
            // Calculate common values once
            var duration = (dpsData.LastLoggedTick - (dpsData.StartLoggedTick ?? 0)).ConvertToUnsigned();
            var skillList = BuildSkillListSnapshot(dpsData);

            // Get player info once
            string playerName;
            Classes playerClass;
            ClassSpec playerSpec;
            int powerLevel = 0;


            if (_storage.ReadOnlyPlayerInfoDatas.TryGetValue(dpsData.UID, out var playerInfo))
            {
                playerName = playerInfo.Name ?? $"UID: {dpsData.UID}";
                playerClass = playerInfo.ProfessionID.GetClassNameById();
                playerSpec = playerInfo.Spec;
                powerLevel = playerInfo.CombatPower ?? 0;
            }
            else
            {
                playerName = $"UID: {dpsData.UID}";
                playerClass = Classes.Unknown;
                playerSpec = ClassSpec.Unknown;
            }

            // Process Damage (only for players, not NPCs)
            var damageValue = dpsData.TotalAttackDamage.ConvertToUnsigned();
            if (damageValue > 0 && !dpsData.IsNpcData)
            {
                result[StatisticType.Damage][dpsData.UID] = new DpsDataProcessed(
                    dpsData, damageValue, duration, skillList, playerName, playerClass, playerSpec,
                    powerLevel);
            }

            // Process Healing (only for players, not NPCs)
            var healingValue = dpsData.TotalHeal.ConvertToUnsigned();
            if (healingValue > 0 && !dpsData.IsNpcData)
            {
                result[StatisticType.Healing][dpsData.UID] = new DpsDataProcessed(
                    dpsData, healingValue, duration, skillList, playerName, playerClass, playerSpec, powerLevel);
            }

            // Process TakenDamage
            var takenDamageValue = dpsData.TotalTakenDamage.ConvertToUnsigned();
            if (takenDamageValue > 0)
            {
                // Process NpcTakenDamage (only for NPCs)
                if (dpsData.IsNpcData)
                {
                    result[StatisticType.NpcTakenDamage][dpsData.UID] = new DpsDataProcessed(
                        dpsData, takenDamageValue, duration, skillList, playerName, playerClass, playerSpec, powerLevel);
                }
                else
                {
                    result[StatisticType.TakenDamage][dpsData.UID] = new DpsDataProcessed(
                        dpsData, takenDamageValue, duration, skillList, playerName, playerClass, playerSpec, powerLevel);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Builds skill list snapshot
    /// </summary>
    private List<SkillItemViewModel> BuildSkillListSnapshot(DpsData dpsData)
    {
        var skills = dpsData.ReadOnlySkillDataList;
        if (skills.Count == 0)
        {
            return [];
        }

        var orderedSkills = skills.OrderByDescending(static s => s.TotalValue);

        var skillDisplayLimit = CurrentStatisticData?.SkillDisplayLimit ?? 8;

        var projected = orderedSkills.Select(skill =>
        {
            var average = skill.UseTimes > 0
                ? Math.Round(skill.TotalValue / (double)skill.UseTimes)
                : 0d;

            var avgDamage = average > int.MaxValue
                ? int.MaxValue
                : (int)average;

            var skillIdText = skill.SkillId.ToString();
            var skillName = EmbeddedSkillConfig.TryGet(skillIdText, out var definition)
                ? definition.Name
                : skillIdText;

            // Translate skill name using DeepL if available
            var translatedSkillName = DpsStatisticsSubViewModel.GetTranslator()?.Translate(skillName) ?? skillName;

            return new SkillItemViewModel
            {
                SkillName = translatedSkillName,
                TotalDamage = skill.TotalValue,
                HitCount = skill.UseTimes,
                CritCount = skill.CritTimes,
                AvgDamage = avgDamage,
                MinDamage = skill.MinDamage == long.MaxValue ? 0 : skill.MinDamage,
                MaxDamage = skill.MaxDamage,
                HighestCrit = skill.HighestCrit
            };
        });

        return skillDisplayLimit > 0
            ? projected.Take(skillDisplayLimit).ToList()
            : projected.ToList();
    }

    [RelayCommand]
    public void AddRandomData()
    {
        UpdateData();
    }

    [RelayCommand]
    private void SetSkillDisplayLimit(int limit)
    {
        var clampedLimit = Math.Max(0, limit);
        foreach (var vm in StatisticData.Values)
        {
            vm.SkillDisplayLimit = clampedLimit;
        }

        // Notify that current data's SkillDisplayLimit changed
        OnPropertyChanged(nameof(CurrentStatisticData));
    }

    protected void AddTestItem()
    {
        CurrentStatisticData.AddTestItem();
    }

    [RelayCommand]
    private void MinimizeWindow()
    {
        _windowManagement.DpsStatisticsView.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void NextMetricType()
    {
        StatisticIndex = StatisticIndex.Next();
    }

    [RelayCommand]
    private void PreviousMetricType()
    {
        StatisticIndex = StatisticIndex.Previous();
    }

    [RelayCommand]
    private void ToggleScopeTime()
    {
        ScopeTime = ScopeTime.Next();
    }

    protected void UpdateData()
    {
        DataStorage_DpsDataUpdated();
    }

    [RelayCommand]
    private void Refresh()
    {
        _logger.LogDebug(WpfLogEvents.VmRefresh, "Manual refresh requested");

        // Reload cached player details so that recent changes in the on-disk
        // cache are reflected in the UI.
        LoadPlayerCache();

        try
        {
            DataStorage_DpsDataUpdated();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh DPS statistics");
        }
    }


    [RelayCommand]
    private void OpenContextMenu()
    {
        ShowContextMenu = true;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        _windowManagement.SettingsView.Show();
    }

    [RelayCommand]
    private void OpenMainView()
    {
        // Use TrayService.Restore() which properly handles MainView restoration
        // including WindowState and visibility management
        _trayService.Restore();
    }

    [RelayCommand]
    private void Shutdown()
    {
        _appControlService.Shutdown();
    }

    [RelayCommand]
    private void OpenEncounterHistory()
    {
        var historyWindow = new Views.EncounterHistoryView
        {
            Owner = _windowManagement.DpsStatisticsView,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var viewModel = new EncounterHistoryViewModel(_chartDataService, _loggerFactory.CreateLogger<EncounterHistoryViewModel>());
        historyWindow.DataContext = viewModel;

        // Handle RequestClose event
        viewModel.RequestClose += () =>
        {
            historyWindow.Close();
        };

        // Handle LoadEncounterRequested event
        viewModel.LoadEncounterRequested += async (encounterData) =>
        {
            _logger.LogInformation("Loading encounter: {EncounterId}", encounterData.EncounterId);
            await LoadHistoricalEncounterAsync(encounterData);
        };

        historyWindow.Show();
    }

    [RelayCommand]
    private void OpenPlayerSkillBreakdown(StatisticDataViewModel? player)
    {
        if (player == null) return;

        _logger.LogInformation("Opening Advanced Combat Log for player: {PlayerName} (UID: {PlayerUid})",
            player.Player.Name, player.Player.Uid);

        // Open ChartsWindow with focused player instead of separate SkillBreakdownView
        var chartsWindow = _windowManagement.ChartsWindow;
        chartsWindow.SetFocusedPlayer(player.Player.Uid);
        chartsWindow.Show();
        chartsWindow.Activate();
    }

    [RelayCommand]
    private void OpenAdvancedCombatLog()
    {
        _logger.LogInformation("Opening Advanced Combat Log (Charts)");
        _windowManagement.ChartsWindow.Show();
        _windowManagement.ChartsWindow.Activate();
    }

    private async Task LoadHistoricalEncounterAsync(Core.Data.Database.EncounterData encounterData)
    {
        try
        {
            _logger.LogInformation("Loading historical encounter: {EncounterId} from {StartTime}",
                encounterData.EncounterId, encounterData.StartTime);

            _logger.LogInformation("Encounter has {PlayerCount} players in PlayerStats", encounterData.PlayerStats.Count);

            // Create PlayerInfo dictionary
            _historicalPlayerInfos = new Dictionary<long, PlayerInfo>();
            _historicalDpsData = new Dictionary<long, DpsData>();

            foreach (var playerStats in encounterData.PlayerStats.Values)
            {
                _logger.LogInformation("Processing player UID={UID}, Name={Name}, TotalDamage={Damage}, IsNpc={IsNpc}",
                    playerStats.UID, playerStats.Name, playerStats.TotalAttackDamage, playerStats.IsNpcData);

                // Create PlayerInfo using factory method
                var playerInfo = Core.Data.Database.EncounterService.CreatePlayerInfoFromEncounter(playerStats);

                // FINAL FALLBACK: If name is still "Unknown", check current live PlayerInfo
                if (string.IsNullOrEmpty(playerInfo.Name) || playerInfo.Name == "Unknown" || playerInfo.Name.StartsWith("UID:"))
                {
                    if (_storage.ReadOnlyPlayerInfoDatas.TryGetValue(playerStats.UID, out var livePlayerInfo))
                    {
                        if (!string.IsNullOrEmpty(livePlayerInfo.Name) && livePlayerInfo.Name != "Unknown")
                        {
                            _logger.LogInformation("Using LIVE player name for UID={UID}: '{Name}' (was: '{OldName}')",
                                playerStats.UID, livePlayerInfo.Name, playerInfo.Name);

                            // Update with live data
                            playerInfo = livePlayerInfo;
                        }
                    }
                }

                _historicalPlayerInfos[playerStats.UID] = playerInfo;

                _logger.LogInformation("Created PlayerInfo: UID={UID}, Name={Name}, Class={Class}, Spec={Spec}",
                    playerInfo.UID, playerInfo.Name, playerInfo.Class, playerInfo.Spec);

                // Create DpsData using factory method
                var dpsData = Core.Data.Database.EncounterService.CreateDpsDataFromEncounter(playerStats, encounterData.DurationMs);
                _historicalDpsData[playerStats.UID] = dpsData;

                _logger.LogInformation("Created DpsData: UID={UID}, TotalDamage={Damage}, TotalHeal={Heal}, Skills={SkillCount}",
                    dpsData.UID, dpsData.TotalAttackDamage, dpsData.TotalHeal, dpsData.ReadOnlySkillDataList.Count);
            }

            // Update UI on dispatcher thread
            await _dispatcher.InvokeAsync(() =>
            {
                IsHistoryMode = true;
                HistoryModeLabel = $"History: {encounterData.StartTime:yyyy-MM-dd HH:mm:ss}";
                BattleDuration = TimeSpan.FromMilliseconds(encounterData.DurationMs);

                // Update all statistic sub-viewmodels with historical data
                foreach (var subViewModel in StatisticData.Values)
                {
                    subViewModel.UpdateHistoricalData(_historicalDpsData, _historicalPlayerInfos);
                }

                _logger.LogInformation("Historical encounter loaded successfully");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load historical encounter");
            await _dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show($"Failed to load encounter: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    [RelayCommand]
    private void ReturnToLive()
    {
        if (!IsHistoryMode) return;

        _logger.LogInformation("Returning to live mode");

        IsHistoryMode = false;
        HistoryModeLabel = string.Empty;
        _historicalPlayerInfos = null;
        _historicalDpsData = null;

        // Check if we're showing last battle data
        if (_awaitingSectionStart)
        {
            IsShowingLastBattle = true;
            BattleStatusLabel = "Last Battle";
        }

        // Trigger a refresh to show current data
        Refresh();
    }

    private void EnsureDurationTimerStarted()
    {
        if (_durationTimer != null) return;

        _durationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _durationTimer.Tick += DurationTimerOnTick;
        _durationTimer.Start();
    }

    private void DurationTimerOnTick(object? sender, EventArgs e)
    {
        UpdateBattleDuration();
    }

    private void UpdateBattleDuration()
    {
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(UpdateBattleDuration);
            return;
        }

        // Combat pause detection - check every second if we should stop timer
        if (_timer.IsRunning && _lastDamageTime != DateTime.MinValue)
        {
            var timeSinceLastDamage = DateTime.UtcNow - _lastDamageTime;
            if (timeSinceLastDamage >= _combatPauseThreshold)
            {
                // Combat paused - stop timer to freeze DPS
                _timer.Stop();
                _logger.LogDebug("Combat paused - timer stopped after {Seconds}s of inactivity (DPS frozen)", timeSinceLastDamage.TotalSeconds);
            }
        }

        if (_timer.IsRunning)
        {
            if (ScopeTime == ScopeTime.Current && _awaitingSectionStart)
            {
                // Freeze to last section elapsed until new data arrives
                BattleDuration = _lastSectionElapsed;
                return;
            }

            var elapsed = ScopeTime == ScopeTime.Total
                ? _timer.Elapsed
                : _timer.Elapsed - _sectionStartElapsed;

            if (elapsed < TimeSpan.Zero)
            {
                elapsed = TimeSpan.Zero;
            }

            BattleDuration = elapsed;
        }
    }

    private void StorageOnNewSectionCreated()
    {
        _dispatcher.BeginInvoke(() =>
        {
            // Freeze current section duration and await first datapoint of the new section
            // Preserve BattleDuration if timer is not running (avoids DPS showing N/A)
            _lastSectionElapsed = _timer.IsRunning
                ? (_timer.Elapsed - _sectionStartElapsed)
                : (BattleDuration > TimeSpan.Zero ? BattleDuration : _lastSectionElapsed);
            _awaitingSectionStart = true;
            IsShowingLastBattle = true;
            BattleStatusLabel = "Last Battle";

            _logger.LogInformation("[LAST BATTLE] Combat ended. Snapshot has {Count} players",
                _lastBattleDataSnapshot?.Count ?? 0);

            // STOP TIMER when section ends (zone change / timeout) - this archives the fight
            if (_timer.IsRunning)
            {
                _timer.Stop();
                _logger.LogDebug("Combat ended - new section created (zone change or timeout). Fight archived as 'Last Battle'");
            }

            UpdateBattleDuration();

            // Reset combat tracking for new section
            _lastDamageTime = DateTime.MinValue;
            _lastKnownMaxTick = 0;

            // Trigger UI update to display the Last Battle snapshot data
            // This ensures the snapshot is processed and shown when entering Last Battle mode
            if (_lastBattleDataSnapshot != null && _lastBattleDataSnapshot.Count > 0)
            {
                _logger.LogInformation("[LAST BATTLE] Triggering UI update to display snapshot");
                PerformUiUpdate();
            }
        });
    }

    private void StorageOnPlayerInfoUpdated(PlayerInfo? info)
    {
        if (info == null)
        {
            return;
        }

        foreach (var subViewModel in StatisticData.Values)
        {
            if (!subViewModel.DataDictionary.TryGetValue(info.UID, out var slot))
            {
                continue;
            }

            if (_dispatcher.CheckAccess())
            {
                ApplyUpdate();
            }
            else
            {
                _dispatcher.BeginInvoke((Action)ApplyUpdate);
            }

            continue;

            void ApplyUpdate()
            {
                slot.Player.Name = info.Name ?? slot.Player.Name;
                slot.Player.Class = info.ProfessionID.GetClassNameById();
                slot.Player.Spec = info.Spec;
                slot.Player.Uid = info.UID;

                if (_storage.CurrentPlayerInfo.UID == info.UID)
                {
                    subViewModel.CurrentPlayerSlot = slot;
                }
            }
        }
    }

    partial void OnScopeTimeChanged(ScopeTime value)
    {
        foreach (var subViewModel in StatisticData.Values)
        {
            subViewModel.ScopeTime = value;
        }

        UpdateBattleDuration();
        UpdateData();

        // Notify that CurrentPlayerSlot might have changed
        OnPropertyChanged(nameof(CurrentStatisticData));
    }

    partial void OnStatisticIndexChanged(StatisticType value)
    {
        OnPropertyChanged(nameof(CurrentStatisticData));
        UpdateData();
    }
}