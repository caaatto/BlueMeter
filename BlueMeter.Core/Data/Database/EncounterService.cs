using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlueMeter.Core.Data.Models;
using BlueMeter.Core.Data.Models.Database;
using BlueMeter.Core.Models;
using BlueMeter.Core.Extends.Data;

namespace BlueMeter.Core.Data.Database;

/// <summary>
/// Service for managing encounter tracking and history
/// </summary>
public class EncounterService
{
    private readonly Func<BlueMeterDbContext> _contextFactory;
    private string? _currentEncounterId;
    private DateTime _currentEncounterStartTime;
    private bool _isEncounterActive;

    public EncounterService(Func<BlueMeterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Gets whether an encounter is currently active
    /// </summary>
    public bool IsEncounterActive => _isEncounterActive;

    /// <summary>
    /// Gets the current encounter ID
    /// </summary>
    public string? CurrentEncounterId => _currentEncounterId;

    /// <summary>
    /// Start a new encounter
    /// </summary>
    public async Task<string> StartEncounterAsync()
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        // End any existing active encounters
        await repository.DeactivateAllEncountersAsync();

        // Create new encounter
        var encounter = await repository.CreateEncounterAsync(DateTime.UtcNow);

        _currentEncounterId = encounter.EncounterId;
        _currentEncounterStartTime = encounter.StartTime;
        _isEncounterActive = true;

        return encounter.EncounterId;
    }

    /// <summary>
    /// End the current encounter
    /// </summary>
    /// <param name="durationMs">Duration of encounter in milliseconds</param>
    /// <param name="minDurationMs">Minimum duration to save (0 = save all). Encounters shorter than this will not be saved.</param>
    /// <param name="bossName">Name of the boss (optional)</param>
    /// <param name="bossUuid">UUID of the boss (optional)</param>
    public async Task EndCurrentEncounterAsync(long durationMs, long minDurationMs = 0, string? bossName = null, long? bossUuid = null)
    {
        if (!_isEncounterActive || string.IsNullOrEmpty(_currentEncounterId))
            return;

        // Check if encounter meets minimum duration requirement
        if (minDurationMs > 0 && durationMs < minDurationMs)
        {
            DebugLogger.Log($"[EndCurrentEncounterAsync] Encounter {_currentEncounterId} duration {durationMs}ms is less than minimum {minDurationMs}ms. Not saving.");

            // Delete the encounter instead of ending it
            using var context = _contextFactory();
            var repository = new EncounterRepository(context);
            await repository.DeleteEncounterAsync(_currentEncounterId);

            _isEncounterActive = false;
            return;
        }

        using var context2 = _contextFactory();
        var repository2 = new EncounterRepository(context2);

        await repository2.EndEncounterAsync(_currentEncounterId, DateTime.UtcNow, durationMs, bossName, bossUuid);

        _isEncounterActive = false;
    }

    /// <summary>
    /// Save player statistics for the current encounter
    /// </summary>
    public async Task SavePlayerStatsAsync(
        Dictionary<long, PlayerInfo> playerInfos,
        Dictionary<long, DpsData> dpsDataDict,
        Dictionary<long, List<ChartDataPoint>>? dpsHistory = null,
        Dictionary<long, List<ChartDataPoint>>? hpsHistory = null)
    {
        if (!_isEncounterActive || string.IsNullOrEmpty(_currentEncounterId))
            return;

        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        foreach (var kvp in dpsDataDict)
        {
            var uid = kvp.Key;
            var dpsData = kvp.Value;

            if (playerInfos.TryGetValue(uid, out var playerInfo))
            {
                // Get chart history for this player
                var playerDpsHistory = dpsHistory?.GetValueOrDefault(uid);
                var playerHpsHistory = hpsHistory?.GetValueOrDefault(uid);

                await repository.SavePlayerStatsAsync(
                    _currentEncounterId,
                    playerInfo,
                    dpsData,
                    playerDpsHistory,
                    playerHpsHistory);
            }
        }
    }

    /// <summary>
    /// Get recent encounters for history
    /// </summary>
    public async Task<List<EncounterSummary>> GetRecentEncountersAsync(int count = 50)
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        var encounters = await repository.GetRecentEncountersAsync(count);

        DebugLogger.Log($"[GetRecentEncountersAsync] Found {encounters.Count} encounters in database");

        var result = encounters.Select(e =>
        {
            DebugLogger.Log($"[GetRecentEncountersAsync] Encounter {e.EncounterId}: StartTime={e.StartTime}, Duration={e.DurationMs}ms");
            DebugLogger.Log($"[GetRecentEncountersAsync]   BossName={e.BossName}, TotalDamage={e.TotalDamage}, TotalHealing={e.TotalHealing}, PlayerCount={e.PlayerCount}");

            return new EncounterSummary
            {
                EncounterId = e.EncounterId,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                DurationMs = e.DurationMs,
                TotalDamage = e.TotalDamage,
                TotalHealing = e.TotalHealing,
                PlayerCount = e.PlayerCount,
                IsActive = e.IsActive,
                BossName = e.BossName ?? "Unknown"
            };
        }).ToList();

        DebugLogger.Log($"[GetRecentEncountersAsync] Returning {result.Count} encounter summaries");
        return result;
    }

    /// <summary>
    /// Load encounter data from database
    /// </summary>
    public async Task<EncounterData?> LoadEncounterAsync(string encounterId)
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        var encounter = await repository.GetEncounterWithStatsAsync(encounterId);
        if (encounter == null)
        {
            DebugLogger.Log($"[LoadEncounterAsync] Encounter {encounterId} not found in database");
            return null;
        }

        DebugLogger.Log($"[LoadEncounterAsync] Loading encounter {encounterId}");
        DebugLogger.Log($"[LoadEncounterAsync] StartTime: {encounter.StartTime}, Duration: {encounter.DurationMs}ms");
        DebugLogger.Log($"[LoadEncounterAsync] PlayerStats count: {encounter.PlayerStats.Count}");

        var encounterData = new EncounterData
        {
            EncounterId = encounter.EncounterId,
            StartTime = encounter.StartTime,
            EndTime = encounter.EndTime,
            DurationMs = encounter.DurationMs,
            PlayerStats = new Dictionary<long, PlayerEncounterData>()
        };

        foreach (var stats in encounter.PlayerStats)
        {
            DebugLogger.Log($"[LoadEncounterAsync] Processing UID={stats.PlayerUID}, NameSnapshot='{stats.NameSnapshot}', Player.Name='{stats.Player.Name}'");
            DebugLogger.Log($"[LoadEncounterAsync]   Damage={stats.TotalAttackDamage}, Heal={stats.TotalHeal}, IsNpc={stats.IsNpcData}");
            DebugLogger.Log($"[LoadEncounterAsync]   Player.Class={stats.Player.Class}, Player.Spec={stats.Player.Spec}");

            // Use NameSnapshot first, fallback to Player.Name if snapshot is "Unknown" or empty
            var name = stats.NameSnapshot;
            if (string.IsNullOrEmpty(name) || name == "Unknown")
            {
                name = stats.Player.Name ?? $"UID: {stats.PlayerUID}";
                DebugLogger.Log($"[LoadEncounterAsync]   Name fallback applied: '{name}'");
            }

            var playerData = new PlayerEncounterData
            {
                UID = stats.PlayerUID,
                Name = name,
                CombatPower = stats.CombatPowerSnapshot,
                Level = stats.LevelSnapshot,
                TotalAttackDamage = stats.TotalAttackDamage,
                TotalTakenDamage = stats.TotalTakenDamage,
                TotalHeal = stats.TotalHeal,
                IsNpcData = stats.IsNpcData,
                Class = stats.Player.Class,
                Spec = stats.Player.Spec,
                SkillDataJson = stats.SkillDataJson,

                // Aggregate statistics
                TotalHits = stats.TotalHits,
                TotalCrits = stats.TotalCrits,
                TotalLuckyHits = stats.TotalLuckyHits,
                AvgDamagePerHit = stats.AvgDamagePerHit,
                CritRate = stats.CritRate,
                LuckyRate = stats.LuckyRate,
                DPS = stats.DPS,
                HPS = stats.HPS,
                HighestCrit = stats.HighestCrit,
                MinDamage = stats.MinDamage,
                MaxDamage = stats.MaxDamage
            };

            // Deserialize chart history data
            if (!string.IsNullOrEmpty(stats.DpsHistoryJson))
            {
                try
                {
                    playerData.DpsHistory = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ChartDataPoint>>(stats.DpsHistoryJson);
                    DebugLogger.Log($"[LoadEncounterAsync]   Loaded {playerData.DpsHistory?.Count ?? 0} DPS history points");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[LoadEncounterAsync]   Failed to deserialize DPS history: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(stats.HpsHistoryJson))
            {
                try
                {
                    playerData.HpsHistory = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ChartDataPoint>>(stats.HpsHistoryJson);
                    DebugLogger.Log($"[LoadEncounterAsync]   Loaded {playerData.HpsHistory?.Count ?? 0} HPS history points");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[LoadEncounterAsync]   Failed to deserialize HPS history: {ex.Message}");
                }
            }

            DebugLogger.Log($"[LoadEncounterAsync]   Final PlayerData: Name='{playerData.Name}', Class={playerData.Class}");

            encounterData.PlayerStats[stats.PlayerUID] = playerData;
        }

        DebugLogger.Log($"[LoadEncounterAsync] Loaded {encounterData.PlayerStats.Count} players successfully");
        return encounterData;
    }

    /// <summary>
    /// Get cached player info from database
    /// </summary>
    public async Task<PlayerInfo?> GetCachedPlayerInfoAsync(long uid)
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        var player = await repository.GetPlayerAsync(uid);
        if (player == null) return null;

        return new PlayerInfo
        {
            UID = player.UID,
            Name = player.Name,
            ProfessionID = player.ProfessionID,
            SubProfessionName = player.SubProfessionName,
            Spec = player.Spec,
            CombatPower = player.CombatPower,
            Level = player.Level,
            RankLevel = player.RankLevel,
            Critical = player.Critical,
            Lucky = player.Lucky,
            MaxHP = player.MaxHP,
            HP = player.MaxHP
        };
    }

    /// <summary>
    /// Update player cache in database
    /// </summary>
    public async Task UpdatePlayerCacheAsync(PlayerInfo playerInfo)
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        await repository.EnsurePlayerAsync(playerInfo);
    }

    /// <summary>
    /// Clean up old encounters
    /// </summary>
    public async Task CleanupOldEncountersAsync(int keepCount = 100)
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        await repository.CleanupOldEncountersAsync(keepCount);
    }

    /// <summary>
    /// Delete all encounters from database
    /// </summary>
    public async Task<int> DeleteAllEncountersAsync()
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        return await repository.DeleteAllEncountersAsync();
    }

    /// <summary>
    /// Creates a PlayerInfo object from PlayerEncounterData (for history loading)
    /// </summary>
    public static PlayerInfo CreatePlayerInfoFromEncounter(PlayerEncounterData playerData)
    {
        return new PlayerInfo
        {
            UID = playerData.UID,
            Name = playerData.Name,
            ProfessionID = playerData.Class.GetProfessionID(),
            Spec = playerData.Spec,
            CombatPower = playerData.CombatPower,
            Level = playerData.Level
        };
    }

    /// <summary>
    /// Creates a DpsData object from PlayerEncounterData (for history loading)
    /// </summary>
    public static DpsData CreateDpsDataFromEncounter(PlayerEncounterData playerData, long durationMs)
    {
        var dpsData = new DpsData
        {
            UID = playerData.UID,
            TotalAttackDamage = playerData.TotalAttackDamage,
            TotalTakenDamage = playerData.TotalTakenDamage,
            TotalHeal = playerData.TotalHeal,
            IsNpcData = playerData.IsNpcData,
            LastLoggedTick = durationMs,
            StartLoggedTick = 0
        };

        // Parse skill data from JSON
        if (!string.IsNullOrEmpty(playerData.SkillDataJson))
        {
            try
            {
                var skillDataDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<long, SkillData>>(playerData.SkillDataJson);
                if (skillDataDict != null)
                {
                    foreach (var kvp in skillDataDict)
                    {
                        dpsData.UpdateSkillData(kvp.Key, skillData =>
                        {
                            skillData.SkillId = kvp.Value.SkillId;
                            skillData.TotalValue = kvp.Value.TotalValue;
                            skillData.UseTimes = kvp.Value.UseTimes;
                            skillData.CritTimes = kvp.Value.CritTimes;
                            skillData.LuckyTimes = kvp.Value.LuckyTimes;
                        });
                    }
                }
            }
            catch
            {
                // Ignore JSON deserialization errors
            }
        }

        return dpsData;
    }
}

/// <summary>
/// Summary of an encounter for history list
/// </summary>
public class EncounterSummary
{
    public required string EncounterId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long DurationMs { get; set; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public int PlayerCount { get; set; }
    public bool IsActive { get; set; }
    public string BossName { get; set; } = "Unknown";

    public string DisplayName => $"{StartTime:yyyy-MM-dd HH:mm:ss} - {FormatDuration()} ({PlayerCount} players)";

    private string FormatDuration()
    {
        var duration = TimeSpan.FromMilliseconds(DurationMs);
        return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
    }
}

/// <summary>
/// Full encounter data loaded from database
/// </summary>
public class EncounterData
{
    public required string EncounterId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long DurationMs { get; set; }
    public Dictionary<long, PlayerEncounterData> PlayerStats { get; set; } = new();
}

/// <summary>
/// Player data for a specific encounter
/// </summary>
public class PlayerEncounterData
{
    public long UID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CombatPower { get; set; }
    public int Level { get; set; }
    public long TotalAttackDamage { get; set; }
    public long TotalTakenDamage { get; set; }
    public long TotalHeal { get; set; }
    public bool IsNpcData { get; set; }
    public Classes Class { get; set; }
    public ClassSpec Spec { get; set; }
    public string? SkillDataJson { get; set; }

    // Aggregate statistics
    public int TotalHits { get; set; }
    public int TotalCrits { get; set; }
    public int TotalLuckyHits { get; set; }
    public double AvgDamagePerHit { get; set; }
    public double CritRate { get; set; }
    public double LuckyRate { get; set; }
    public double DPS { get; set; }
    public double HPS { get; set; }
    public long HighestCrit { get; set; }
    public long MinDamage { get; set; }
    public long MaxDamage { get; set; }

    // Chart history data
    public List<ChartDataPoint>? DpsHistory { get; set; }
    public List<ChartDataPoint>? HpsHistory { get; set; }
}

/// <summary>
/// Represents a single data point for chart visualization
/// </summary>
public class ChartDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }

    public ChartDataPoint() { }

    public ChartDataPoint(DateTime timestamp, double value)
    {
        Timestamp = timestamp;
        Value = value;
    }
}
