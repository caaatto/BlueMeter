using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using BlueMeter.Core.Analyze;
using BlueMeter.Core.Analyze.Models;
using BlueMeter.Core.Data.Models;
using BlueMeter.Core.Extends.Data;
using BlueMeter.Core.Tools;
using BlueMeter.WPF.Data;

namespace BlueMeter.Core.Data;

/// <summary>
/// 数据存储
/// </summary>
public sealed partial class DataStorageV2(ILogger<DataStorageV2> logger) : IDataStorage
{
    // ===== Event Batching Support =====
    private readonly object _eventBatchLock = new();
    private readonly List<BattleLog> _pendingBattleLogs = new(100);
    private readonly HashSet<long> _pendingPlayerUpdates = new();
    private readonly object _sectionTimeoutLock = new();
    private bool _disposed;
    private bool _hasPendingBattleLogEvents;
    private bool _hasPendingDataEvents;
    private bool _hasPendingDpsEvents;
    private bool _hasPendingPlayerInfoEvents;
    private bool _isServerConnected;
    private DateTime _lastLogWallClockAtUtc = DateTime.MinValue;

    // ===== Section timeout monitor =====
    private Timer? _sectionTimeoutTimer;
    private bool _timeoutSectionClearedOnce; // avoid repeated clear/events until next log arrives

    // ===== Boss tracking for battle section management =====
    private long _activeBossUuid = 0;
    private DateTime? _bossDeathTime = null;
    private const int BossDeathDelaySeconds = 8;

    /// <summary>
    /// 玩家信息字典 (Key: UID)
    /// </summary>
    private Dictionary<long, PlayerInfo> PlayerInfoData { get; } = [];

    /// <summary>
    /// 最后一次战斗日志
    /// </summary>
    private BattleLog? LastBattleLog { get; set; }

    /// <summary>
    /// 全程玩家DPS字典 (Key: UID)
    /// </summary>
    private Dictionary<long, DpsData> FullDpsData { get; } = [];

    /// <summary>
    /// 阶段性玩家DPS字典 (Key: UID)
    /// </summary>
    private Dictionary<long, DpsData> SectionedDpsData { get; } = [];

    /// <summary>
    /// 强制新分段标记
    /// </summary>
    /// <remarks>
    /// 设置为 true 后将在下一次添加战斗日志时, 强制创建一个新的分段之后重置为 false
    /// </remarks>
    private bool ForceNewBattleSection { get; set; }

    /// <summary>
    /// 当前玩家UUID
    /// </summary>
    public long CurrentPlayerUUID { get; set; }

    /// <summary>
    /// 当前玩家信息
    /// </summary>
    public PlayerInfo CurrentPlayerInfo { get; private set; } = new();

    /// <summary>
    /// 只读玩家信息字典 (Key: UID)
    /// </summary>
    public ReadOnlyDictionary<long, PlayerInfo> ReadOnlyPlayerInfoDatas => PlayerInfoData.AsReadOnly();

    /// <summary>
    /// 只读全程玩家DPS字典 (Key: UID)
    /// </summary>
    public ReadOnlyDictionary<long, DpsData> ReadOnlyFullDpsDatas => FullDpsData.AsReadOnly();

    /// <summary>
    /// 只读全程玩家DPS列表; 注意! 频繁读取该属性可能会导致性能问题!
    /// </summary>
    public IReadOnlyList<DpsData> ReadOnlyFullDpsDataList => FullDpsData.Values.ToList().AsReadOnly();

    /// <summary>
    /// 阶段性只读玩家DPS字典 (Key: UID)
    /// </summary>
    public ReadOnlyDictionary<long, DpsData> ReadOnlySectionedDpsDatas => SectionedDpsData.AsReadOnly();

    /// <summary>
    /// 阶段性只读玩家DPS列表; 注意! 频繁读取该属性可能会导致性能问题!
    /// </summary>
    public IReadOnlyList<DpsData> ReadOnlySectionedDpsDataList => SectionedDpsData.Values.ToList().AsReadOnly();

    /// <summary>
    /// Inactivity timeout for creating new sections (phase transitions, pauses)
    /// Short timeout (1s) so boss phases don't count towards DPS
    /// </summary>
    public TimeSpan InactivityTimeout { get; set; } = TimeSpan.FromSeconds(1); // 1s for phase breaks

    /// <summary>
    /// 战斗日志分段超时时间 (默认: 15s for dungeon fights and zone changes)
    /// Used for final encounter ending and saving to history
    /// </summary>
    public TimeSpan SectionTimeout { get; set; } = TimeSpan.FromSeconds(15); // 15s timeout for dungeons

    /// <summary>
    /// 是否正在监听服务器
    /// </summary>
    public bool IsServerConnected
    {
        get => _isServerConnected;
        set
        {
            if (_isServerConnected == value) return;
            _isServerConnected = value;

            // ensure background timeout monitor is running when connected
            if (value) EnsureSectionMonitorStarted();

            RaiseServerConnectionStateChanged(value);
        }
    }

    /// <summary>
    /// Current active boss/enemy being fought (0 = no active boss)
    /// </summary>
    public long ActiveBossUuid
    {
        get => _activeBossUuid;
        set => _activeBossUuid = value;
    }

    /// <summary>
    /// Time when the active boss died (null = boss not dead yet)
    /// </summary>
    public DateTime? BossDeathTime
    {
        get => _bossDeathTime;
        set => _bossDeathTime = value;
    }

    /// <summary>
    /// Registers a new boss fight when first damage is dealt to an enemy
    /// </summary>
    public void RegisterBossEngagement(long enemyUuid)
    {
        if (_activeBossUuid == 0 || _activeBossUuid != enemyUuid)
        {
            _activeBossUuid = enemyUuid;
            _bossDeathTime = null;
            logger.LogInformation("Boss fight started: Enemy {EnemyUid}", enemyUuid);

            // Start encounter for this boss fight
            _ = Task.Run(async () =>
            {
                try
                {
                    await DataStorageExtensions.StartNewEncounterAsync();
                    logger.LogInformation("Encounter started for boss fight: Enemy {EnemyUid}", enemyUuid);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to start encounter for boss fight");
                }
            });
        }
    }

    /// <summary>
    /// Registers boss death and starts the post-death timer
    /// </summary>
    public void RegisterBossDeath(long enemyUuid)
    {
        if (_activeBossUuid == enemyUuid)
        {
            _bossDeathTime = DateTime.UtcNow;
            logger.LogInformation("Boss defeated: Enemy {EnemyUid}. Section will end in {Delay}s if no new combat.", enemyUuid, BossDeathDelaySeconds);
        }
    }

    /// <summary>
    /// Checks if we should end the battle section (boss dead + delay passed)
    /// </summary>
    public bool ShouldEndBattleSection()
    {
        if (_bossDeathTime.HasValue && _activeBossUuid != 0)
        {
            var timeSinceDeath = DateTime.UtcNow - _bossDeathTime.Value;
            if (timeSinceDeath.TotalSeconds >= BossDeathDelaySeconds)
            {
                logger.LogInformation("Boss death delay passed ({Seconds}s). Ending battle section.", timeSinceDeath.TotalSeconds);
                // Reset boss tracking
                _activeBossUuid = 0;
                _bossDeathTime = null;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get boss name from UUID
    /// </summary>
    private string GetBossName(long bossUuid)
    {
        if (bossUuid == 0) return "Unknown Boss";

        // Try to get from PlayerInfoData
        if (PlayerInfoData.TryGetValue(bossUuid, out var playerInfo))
        {
            return playerInfo.Name ?? $"Boss {bossUuid}";
        }

        return $"Boss {bossUuid}";
    }

    /// <summary>
    /// 从文件加载缓存玩家信息
    /// </summary>
    public void LoadPlayerInfoFromFile()
    {
        PlayerInfoCacheFileV3_0_0 playerInfoCaches;
        try
        {
            playerInfoCaches = PlayerInfoCacheReader.ReadFile();
        }
        catch (FileNotFoundException)
        {
            logger.LogInformation("Player info cache file not exist, abort load");
            return;
        }

        foreach (var playerInfoCache in playerInfoCaches.PlayerInfos)
        {
            if (!PlayerInfoData.TryGetValue(playerInfoCache.UID, out var playerInfo))
            {
                playerInfo = new PlayerInfo();
            }

            playerInfo.UID = playerInfoCache.UID;
            playerInfo.ProfessionID ??= playerInfoCache.ProfessionID;
            playerInfo.CombatPower ??= playerInfoCache.CombatPower;
            playerInfo.Critical ??= playerInfoCache.Critical;
            playerInfo.Lucky ??= playerInfoCache.Lucky;
            playerInfo.MaxHP ??= playerInfoCache.MaxHP;

            if (string.IsNullOrEmpty(playerInfo.Name))
            {
                playerInfo.Name = playerInfoCache.Name;
            }

            if (string.IsNullOrEmpty(playerInfo.SubProfessionName))
            {
                playerInfo.SubProfessionName = playerInfoCache.SubProfessionName;
            }

            PlayerInfoData[playerInfo.UID] = playerInfo;
        }
    }

    /// <summary>
    /// 保存缓存玩家信息到文件
    /// </summary>
    public void SavePlayerInfoToFile()
    {
        try
        {
            LoadPlayerInfoFromFile();
        }
        catch (FileNotFoundException)
        {
            // 无缓存或缓存篡改直接无视重新保存新文件
            // File not exist, ignore and write new file
            logger.LogInformation("Player info cache file not exist, write new file");
        }

        var list = PlayerInfoData.Values.ToList();
        PlayerInfoCacheWriter.WriteToFile([.. list]);
    }

    /// <summary>
    /// 通过战斗日志构建玩家信息字典
    /// </summary>
    /// <param name="battleLogs">战斗日志</param>
    /// <returns></returns>
    public Dictionary<long, PlayerInfoFileData> BuildPlayerDicFromBattleLog(List<BattleLog> battleLogs)
    {
        var playerDic = new Dictionary<long, PlayerInfoFileData>();
        foreach (var log in battleLogs)
        {
            if (!playerDic.ContainsKey(log.AttackerUuid) &&
                PlayerInfoData.TryGetValue(log.AttackerUuid, out var attackerPlayerInfo))
            {
                playerDic.Add(log.AttackerUuid, attackerPlayerInfo);
            }

            if (!playerDic.ContainsKey(log.TargetUuid) &&
                PlayerInfoData.TryGetValue(log.TargetUuid, out var targetPlayerInfo))
            {
                playerDic.Add(log.TargetUuid, targetPlayerInfo);
            }
        }

        return playerDic;
    }
    /// <summary>
    /// 检查或创建玩家信息
    /// </summary>
    /// <param name="uid"></param>
    /// <returns>是否已经存在; 是: true, 否: false</returns>
    /// <remarks>
    /// 如果传入的 UID 已存在, 则不会进行任何操作;
    /// 否则会创建一个新的 PlayerInfo 并触发 PlayerInfoUpdated 事件
    /// </remarks>
    public bool EnsurePlayer(long uid)
    {
        /*
         * 因为修改 PlayerInfo 必须触发 PlayerInfoUpdated 事件,
         * 所以不能用 GetOrCreate 的方式来返回 PlayerInfo 对象,
         * 否则会造成外部使用 PlayerInfo 对象后没有触发事件的问题
         * * * * * * * * * * * * * * * * * * * * * * * * * * */

        if (PlayerInfoData.ContainsKey(uid))
        {
            return true;
        }

        PlayerInfoData[uid] = new PlayerInfo { UID = uid };

        TriggerPlayerInfoUpdatedImmediate(uid);

        return false;
    }

    /// <summary>
    /// 添加战斗日志 (会自动创建日志分段)
    /// Public method for backwards compatibility - fires events immediately
    /// </summary>
    /// <param name="log">战斗日志</param>
    public void AddBattleLog(BattleLog log)
    {
        ProcessBattleLogCore(log, out var sectionFlag);

        // Fire events immediately for backwards compatibility
        if (sectionFlag)
        {
            RaiseNewSectionCreated();
        }

        RaiseBattleLogCreated(log);
        RaiseDpsDataUpdated();
        RaiseDataUpdated();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _sectionTimeoutTimer?.Dispose();
        }
        catch (Exception ex)
        {
            // ignored
            logger.LogError(ex, "An error occurred during Dispose");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }


    private void EnsureSectionMonitorStarted()
    {
        if (_sectionTimeoutTimer != null) return;
        try
        {
            _sectionTimeoutTimer = new Timer(static s => ((DataStorageV2)s!).SectionTimeoutTick(), this,
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during EnsureSectionMonitorStarted");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }

    private void SectionTimeoutTick()
    {
        CheckSectionTimeout();
    }

    private void CheckSectionTimeout()
    {
        if (_disposed) return;
        DateTime last;
        bool alreadyCleared;
        lock (_sectionTimeoutLock)
        {
            last = _lastLogWallClockAtUtc;
            alreadyCleared = _timeoutSectionClearedOnce;
        }

        if (alreadyCleared) return;
        if (last == DateTime.MinValue) return; // no logs yet

        // Check if boss died and delay has passed
        // Capture boss info BEFORE calling ShouldEndBattleSection (which resets the values)
        var currentBossUuid = _activeBossUuid;
        var currentBossDeathTime = _bossDeathTime;

        if (ShouldEndBattleSection())
        {
            // Get boss name from captured UUID
            var bossName = GetBossName(currentBossUuid);

            try
            {
                // Calculate actual duration from boss death time
                var durationMs = currentBossDeathTime.HasValue
                    ? (long)(DateTime.UtcNow - currentBossDeathTime.Value).TotalMilliseconds + (BossDeathDelaySeconds * 1000)
                    : (long)SectionTimeout.TotalMilliseconds;

                // CRITICAL FIX: Save encounter data synchronously BEFORE clearing
                // We must block here to ensure chart data is captured before it's deleted
                try
                {
                    DataStorageExtensions.EndCurrentEncounterAsync(durationMs, bossName, currentBossUuid).GetAwaiter().GetResult();
                    logger.LogInformation("Encounter ended for boss: {BossName} (UID={BossUid}), Duration={DurationMs}ms",
                        bossName, currentBossUuid, durationMs);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to end encounter for boss fight");
                }

                // Now safe to clear - data has been saved
                PrivateClearDpsData(); // raises DpsDataUpdated & DataUpdated
                RaiseNewSectionCreated();
            }
            finally
            {
                lock (_sectionTimeoutLock)
                {
                    _timeoutSectionClearedOnce = true;
                }
            }
            return;
        }

        var now = DateTime.UtcNow;
        if (now - last <= SectionTimeout) return;

        // Timeout reached: save and clear section
        try
        {
            // CRITICAL FIX: Also save encounter on timeout (training dummy, wipe, etc.)
            // Calculate duration from last activity
            var durationMs = (long)(now - last).TotalMilliseconds;

            try
            {
                // Save encounter before clearing (blocking to ensure data is captured)
                DataStorageExtensions.EndCurrentEncounterAsync(durationMs, bossName: null, bossUuid: null)
                    .GetAwaiter().GetResult();
                logger.LogInformation("Encounter ended by timeout after {DurationMs}ms of inactivity", durationMs);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to end encounter on timeout");
            }

            // Now safe to clear
            PrivateClearDpsData(); // raises DpsDataUpdated & DataUpdated
            RaiseNewSectionCreated();
        }
        finally
        {
            lock (_sectionTimeoutLock)
            {
                _timeoutSectionClearedOnce = true;
            }
        }
    }

    /// <summary>
    /// 触发玩家信息更新事件
    /// </summary>
    /// <param name="uid">UID</param>
    private void TriggerPlayerInfoUpdated(long uid)
    {
        lock (_eventBatchLock)
        {
            _pendingPlayerUpdates.Add(uid);
            _hasPendingPlayerInfoEvents = true;
            _hasPendingDataEvents = true;
        }
    }

    /// <summary>
    /// Immediately fire player info updated event (used when not in batch mode)
    /// </summary>
    private void TriggerPlayerInfoUpdatedImmediate(long uid)
    {
        try
        {
            PlayerInfoUpdated?.Invoke(PlayerInfoData[uid]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"An error occurred during trigger event(PlayerInfoUpdated) => {ex.Message}\r\n{ex.StackTrace}");
        }

        try
        {
            DataUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"An error occurred during trigger event(DataUpdated) => {ex.Message}\r\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 检查或创建玩家战斗日志列表
    /// </summary>
    /// <param name="uid">UID</param>
    /// <returns>是否已经存在; 是: true, 否: false</returns>
    /// <remarks>
    /// 如果传入的 UID 已存在, 则不会进行任何操作;
    /// 否则会创建一个新的对应 UID 的 <![CDATA[List<BattleLog>]]>
    /// </remarks>
    public (DpsData fullData, DpsData sectionedData) GetOrCreateDpsDataByUid(long uid)
    {
        var fullDpsDataFlag = FullDpsData.TryGetValue(uid, out var fullDpsData);
        if (!fullDpsDataFlag)
        {
            fullDpsData = new DpsData { UID = uid };
        }

        var sectionedDpsDataFlag = SectionedDpsData.TryGetValue(uid, out var sectionedDpsData);
        if (!sectionedDpsDataFlag)
        {
            sectionedDpsData = new DpsData { UID = uid };
        }

        SectionedDpsData[uid] = sectionedDpsData!;
        FullDpsData[uid] = fullDpsData!;

        return (fullDpsData!, sectionedDpsData!);
    }

    /// <summary>
    /// Internal method for queue processing - does NOT fire events immediately
    /// Used by BattleLogQueue for batched processing
    /// </summary>
    internal void AddBattleLogInternal(BattleLog log)
    {
        // Process the core logic without firing events
        ProcessBattleLogCore(log, out _);

        // Queue events instead of firing immediately
        lock (_eventBatchLock)
        {
            _pendingBattleLogs.Add(log);
            _hasPendingBattleLogEvents = true;
            _hasPendingDpsEvents = true;
            _hasPendingDataEvents = true;
        }
    }

    /// <summary>
    /// Flush all pending batched events
    /// Called by BattleLogQueue after processing a batch
    /// </summary>
    internal void FlushPendingEvents()
    {
        List<BattleLog> logsToFire;
        HashSet<long> playerUpdates;
        bool hasBattle, hasDps, hasData, hasPlayerInfo;

        lock (_eventBatchLock)
        {
            if (!_hasPendingBattleLogEvents && !_hasPendingDpsEvents &&
                !_hasPendingDataEvents && !_hasPendingPlayerInfoEvents)
                return;

            hasBattle = _hasPendingBattleLogEvents;
            hasDps = _hasPendingDpsEvents;
            hasData = _hasPendingDataEvents;
            hasPlayerInfo = _hasPendingPlayerInfoEvents;

            logsToFire = new List<BattleLog>(_pendingBattleLogs);
            playerUpdates = new HashSet<long>(_pendingPlayerUpdates);

            _pendingBattleLogs.Clear();
            _pendingPlayerUpdates.Clear();
            _hasPendingBattleLogEvents = false;
            _hasPendingDpsEvents = false;
            _hasPendingDataEvents = false;
            _hasPendingPlayerInfoEvents = false;
        }

        // Fire events outside of lock
        if (hasBattle && logsToFire.Count > 0)
        {
            foreach (var log in logsToFire)
            {
                RaiseBattleLogCreated(log);
            }
        }

        if (hasPlayerInfo && playerUpdates.Count > 0)
        {
            foreach (var uid in playerUpdates)
            {
                if (PlayerInfoData.TryGetValue(uid, out var info))
                {
                    RaisePlayerInfoUpdated(info);
                }
            }
        }

        if (hasDps)
        {
            RaiseDpsDataUpdated();
        }

        if (hasData)
        {
            RaiseDataUpdated();
        }
    }

    /// <summary>
    /// Core battle log processing logic (extracted to avoid duplication)
    /// Processes data without firing events
    /// </summary>
    private void ProcessBattleLogCore(BattleLog log, out bool sectionFlag)
    {
        var tt = new TimeSpan(log.TimeTicks);
        sectionFlag = false;

        if (LastBattleLog != null)
        {
            var prevTt = new TimeSpan(LastBattleLog.Value.TimeTicks);
            var gap = tt - prevTt;

            // SectionTimeout (15s) for creating new sections when combat fully stops
            // This handles: normal enemy deaths, dungeon transitions, etc.
            if (gap > SectionTimeout || ForceNewBattleSection)
            {
                PrivateClearDpsDataNoEvents();
                sectionFlag = true;
                ForceNewBattleSection = false;
            }

            // Track active combat time (exclude downtime >1s)
            // This ensures DPS calculations don't include boss phase transitions, running between packs, etc.
            if (gap <= InactivityTimeout)
            {
                // Short gap - this is active combat, add to all active players
                long gapTicks = gap.Ticks;
                foreach (var dpsData in SectionedDpsData.Values)
                {
                    dpsData.ActiveCombatTicks += gapTicks;
                }
            }
            // else: Gap >1s is downtime, don't add to ActiveCombatTicks
        }

        // Track healing done by players (regardless of target type)
        // This matches Resonance's approach: track all healing from player healers
        if (log.IsHeal && log.IsAttackerPlayer)
        {
            var (fullData, sectionedData) = SetLogInfos(log.AttackerUuid, log);
            TrySetSubProfessionBySkillId(log.AttackerUuid, log.SkillID);
            fullData.TotalHeal += log.Value;
            sectionedData.TotalHeal += log.Value;

            // Real-time windowing for charts (Phase 2B)
            sectionedData.AddHealToWindow(log.Value);
        }

        // Track damage dealt by players (regardless of target type)
        if (!log.IsHeal && log.IsAttackerPlayer)
        {
            var (fullData, sectionedData) = SetLogInfos(log.AttackerUuid, log);
            TrySetSubProfessionBySkillId(log.AttackerUuid, log.SkillID);
            fullData.TotalAttackDamage += log.Value;
            sectionedData.TotalAttackDamage += log.Value;

            // Real-time windowing for charts (Phase 2B)
            sectionedData.AddDamageToWindow(log.Value);
        }

        // Track damage taken by players
        if (!log.IsHeal && log.IsTargetPlayer)
        {
            var (fullData, sectionedData) = SetLogInfos(log.TargetUuid, log);
            fullData.TotalTakenDamage += log.Value;
            sectionedData.TotalTakenDamage += log.Value;
        }

        // Track damage/effects on NPCs/monsters (non-player targets)
        if (!log.IsTargetPlayer)
        {
            var (fullData, sectionedData) = SetLogInfos(log.TargetUuid, log);
            fullData.TotalTakenDamage += log.Value;
            sectionedData.TotalTakenDamage += log.Value;
            fullData.IsNpcData = true;
            sectionedData.IsNpcData = true;
        }

        LastBattleLog = log;

        // Update wall clock timestamp and unlock next section timeout clear
        lock (_sectionTimeoutLock)
        {
            _lastLogWallClockAtUtc = DateTime.UtcNow;
            _timeoutSectionClearedOnce = false;
        }

        // Ensure monitor is running once we have activity
        EnsureSectionMonitorStarted();
    }

    /// <summary>
    /// Private method to clear DPS data without firing events
    /// Used internally by event batching
    /// </summary>
    private void PrivateClearDpsDataNoEvents()
    {
        // Clear sliding windows for charts (Phase 2B)
        foreach (var dpsData in SectionedDpsData.Values)
        {
            dpsData.ClearWindows();
        }

        SectionedDpsData.Clear();
    }

    /// <summary>
    /// 设置通用基础信息
    /// </summary>
    private (DpsData fullData, DpsData sectionedData) SetLogInfos(long uid, BattleLog log)
    {
        // 检查或创建玩家信息
        EnsurePlayer(uid);

        // 检查或创建玩家战斗日志列表
        var (fullData, sectionedData) = GetOrCreateDpsDataByUid(uid);

        fullData.StartLoggedTick ??= log.TimeTicks;
        fullData.LastLoggedTick = log.TimeTicks;

        fullData.UpdateSkillData(log.SkillID, skillData =>
        {
            skillData.TotalValue += log.Value;
            skillData.UseTimes += 1;
            skillData.CritTimes += log.IsCritical ? 1 : 0;
            skillData.LuckyTimes += log.IsLucky ? 1 : 0;
            // Track min/max damage (excluding misses)
            if (!log.IsMiss && log.Value > 0)
            {
                skillData.MinDamage = Math.Min(skillData.MinDamage, log.Value);
                skillData.MaxDamage = Math.Max(skillData.MaxDamage, log.Value);
                // Track highest crit
                if (log.IsCritical)
                {
                    skillData.HighestCrit = Math.Max(skillData.HighestCrit, log.Value);
                }
            }
        });

        sectionedData.StartLoggedTick ??= log.TimeTicks;
        sectionedData.LastLoggedTick = log.TimeTicks;

        sectionedData.UpdateSkillData(log.SkillID, skillData =>
        {
            skillData.TotalValue += log.Value;
            skillData.UseTimes += 1;
            skillData.CritTimes += log.IsCritical ? 1 : 0;
            skillData.LuckyTimes += log.IsLucky ? 1 : 0;
            // Track min/max damage (excluding misses)
            if (!log.IsMiss && log.Value > 0)
            {
                skillData.MinDamage = Math.Min(skillData.MinDamage, log.Value);
                skillData.MaxDamage = Math.Max(skillData.MaxDamage, log.Value);
                // Track highest crit
                if (log.IsCritical)
                {
                    skillData.HighestCrit = Math.Max(skillData.HighestCrit, log.Value);
                }
            }
        });

        return (fullData, sectionedData);
    }

    private void PrivateClearDpsData()
    {
        // Clear sliding windows for charts (Phase 2B)
        foreach (var dpsData in SectionedDpsData.Values)
        {
            dpsData.ClearWindows();
        }

        SectionedDpsData.Clear();

        // Reset boss tracking when section clears
        _activeBossUuid = 0;
        _bossDeathTime = null;

        RaiseDpsDataUpdated();
        RaiseDataUpdated();
    }

    #region SetPlayerProperties

    private void TrySetSubProfessionBySkillId(long uid, long skillId)
    {
        if (!PlayerInfoData.TryGetValue(uid, out var playerInfo))
        {
            return;
        }

        var subProfessionName = skillId.GetSubProfessionBySkillId();
        var spec = skillId.GetClassSpecBySkillId();
        if (!string.IsNullOrEmpty(subProfessionName))
        {
            playerInfo.SubProfessionName = subProfessionName;
            playerInfo.Spec = spec;
            TriggerPlayerInfoUpdated(uid);
        }
    }

    /// <summary>
    /// 设置玩家名称
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="name">玩家名称</param>
    public void SetPlayerName(long uid, string name)
    {
        PlayerInfoData[uid].Name = name;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家职业ID
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="professionId">职业ID</param>
    public void SetPlayerProfessionID(long uid, int professionId)
    {
        PlayerInfoData[uid].ProfessionID = professionId;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家战力
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="combatPower">战力</param>
    public void SetPlayerCombatPower(long uid, int combatPower)
    {
        PlayerInfoData[uid].CombatPower = combatPower;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家等级
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="level">等级</param>
    public void SetPlayerLevel(long uid, int level)
    {
        PlayerInfoData[uid].Level = level;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家 RankLevel
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="rankLevel">RankLevel</param>
    /// <remarks>
    /// 暂不清楚 RankLevel 的具体含义...
    /// </remarks>
    public void SetPlayerRankLevel(long uid, int rankLevel)
    {
        PlayerInfoData[uid].RankLevel = rankLevel;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家暴击
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="critical">暴击值</param>
    public void SetPlayerCritical(long uid, int critical)
    {
        PlayerInfoData[uid].Critical = critical;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家幸运
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="lucky">幸运值</param>
    public void SetPlayerLucky(long uid, int lucky)
    {
        PlayerInfoData[uid].Lucky = lucky;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家当前HP
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="hp">当前HP</param>
    public void SetPlayerHP(long uid, long hp)
    {
        PlayerInfoData[uid].HP = hp;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    /// <summary>
    /// 设置玩家最大HP
    /// </summary>
    /// <param name="uid">UID</param>
    /// <param name="maxHp">最大HP</param>
    public void SetPlayerMaxHP(long uid, long maxHp)
    {
        PlayerInfoData[uid].MaxHP = maxHp;

        TriggerPlayerInfoUpdatedImmediate(uid);
    }

    #endregion

    #region Clear data

    /// <summary>
    /// 清除所有DPS数据 (包括全程和阶段性)
    /// </summary>
    public void ClearAllDpsData()
    {
        ForceNewBattleSection = true;
        SectionedDpsData.Clear();
        FullDpsData.Clear();

        RaiseDpsDataUpdated();
        RaiseDataUpdated();
    }

    /// <summary>
    /// 标记新的战斗日志分段 (清空阶段性Dps数据)
    /// </summary>
    public void ClearDpsData()
    {
        ForceNewBattleSection = true;

        PrivateClearDpsData();
    }

    /// <summary>
    /// 清除当前玩家信息
    /// </summary>
    public void ClearCurrentPlayerInfo()
    {
        CurrentPlayerInfo = new PlayerInfo();

        RaiseDataUpdated();
    }

    /// <summary>
    /// 清除所有玩家信息
    /// </summary>
    public void ClearPlayerInfos()
    {
        PlayerInfoData.Clear();

        RaiseDataUpdated();
    }

    /// <summary>
    /// 清除所有数据 (包括缓存历史)
    /// </summary>
    public void ClearAllPlayerInfos()
    {
        CurrentPlayerInfo = new PlayerInfo();
        PlayerInfoData.Clear();

        RaiseDataUpdated();
    }


    #endregion
}

public partial class DataStorageV2
{
    #region Events

    /// <summary>
    /// 服务器的监听连接状态变更事件
    /// </summary>
    public event ServerConnectionStateChangedEventHandler? ServerConnectionStateChanged;

    /// <summary>
    /// 玩家信息更新事件
    /// </summary>
    public event PlayerInfoUpdatedEventHandler? PlayerInfoUpdated;

    /// <summary>
    /// 战斗日志新分段创建事件
    /// </summary>
    public event NewSectionCreatedEventHandler? NewSectionCreated;

    /// <summary>
    /// 战斗日志更新事件
    /// </summary>
    public event BattleLogCreatedEventHandler? BattleLogCreated;

    /// <summary>
    /// DPS数据更新事件
    /// </summary>
    public event DpsDataUpdatedEventHandler? DpsDataUpdated;

    /// <summary>
    /// 数据更新事件 (玩家信息或战斗日志更新时触发)
    /// </summary>
    public event DataUpdatedEventHandler? DataUpdated;

    /// <summary>
    /// 服务器变更事件 (地图变更)
    /// </summary>
    public event ServerChangedEventHandler? ServerChanged;

    public void RaiseServerChanged(string currentServer, string prevServer)
    {
        try
        {
            ServerChanged?.Invoke(currentServer, prevServer);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"An error occurred during trigger event(ServerChanged) => {ex.Message}\r\n{ex.StackTrace}");
        }
    }

    private void RaiseDataUpdated()
    {
        try
        {
            DataUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during trigger event(DataUpdated)");
            Console.WriteLine(
                $"An error occurred during trigger event(DataUpdated) => {ex.Message}\r\n{ex.StackTrace}");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }

    private void RaiseDpsDataUpdated()
    {
        try
        {
            DpsDataUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during trigger event(DpsDataUpdated)");
            Console.WriteLine(
                $"An error occurred during trigger event(DpsDataUpdated) => {ex.Message}\r\n{ex.StackTrace}");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }

    private void RaiseServerConnectionStateChanged(bool value)
    {
        try
        {
            ServerConnectionStateChanged?.Invoke(value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during trigger event(ServerConnectionStateChanged)");
            Console.WriteLine(
                $"An error occurred during trigger event(ServerConnectionStateChanged) => {ex.Message}\r\n{ex.StackTrace}");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }

    private void RaisePlayerInfoUpdated(PlayerInfo info)
    {
        try
        {
            PlayerInfoUpdated?.Invoke(info);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during trigger event(PlayerInfoUpdated)");
            Console.WriteLine(
                $"An error occurred during trigger event(PlayerInfoUpdated) => {ex.Message}\r\n{ex.StackTrace}");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }

    private void RaiseBattleLogCreated(BattleLog log)
    {
        try
        {
            BattleLogCreated?.Invoke(log);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during trigger event(BattleLogCreated)");
            Console.WriteLine(
                $"An error occurred during trigger event(BattleLogCreated) => {ex.Message}\r\n{ex.StackTrace}");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }

    private void RaiseNewSectionCreated()
    {
        try
        {
            NewSectionCreated?.Invoke();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during trigger event(NewSectionCreated)");
            Console.WriteLine(
                $"An error occurred during trigger event(NewSectionCreated) => {ex.Message}\r\n{ex.StackTrace}");
            ExceptionHelper.ThrowIfDebug(ex);
        }
    }

    #endregion
}