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
    public async Task EndCurrentEncounterAsync(long durationMs)
    {
        if (!_isEncounterActive || string.IsNullOrEmpty(_currentEncounterId))
            return;

        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        await repository.EndEncounterAsync(_currentEncounterId, DateTime.UtcNow, durationMs);

        _isEncounterActive = false;
    }

    /// <summary>
    /// Save player statistics for the current encounter
    /// </summary>
    public async Task SavePlayerStatsAsync(Dictionary<long, PlayerInfo> playerInfos, Dictionary<long, DpsData> dpsDataDict)
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
                await repository.SavePlayerStatsAsync(_currentEncounterId, playerInfo, dpsData);
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

        return encounters.Select(e => new EncounterSummary
        {
            EncounterId = e.EncounterId,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            DurationMs = e.DurationMs,
            TotalDamage = e.TotalDamage,
            TotalHealing = e.TotalHealing,
            PlayerCount = e.PlayerCount,
            IsActive = e.IsActive
        }).ToList();
    }

    /// <summary>
    /// Load encounter data from database
    /// </summary>
    public async Task<EncounterData?> LoadEncounterAsync(string encounterId)
    {
        using var context = _contextFactory();
        var repository = new EncounterRepository(context);

        var encounter = await repository.GetEncounterWithStatsAsync(encounterId);
        if (encounter == null) return null;

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
            var playerData = new PlayerEncounterData
            {
                UID = stats.PlayerUID,
                Name = stats.NameSnapshot,
                CombatPower = stats.CombatPowerSnapshot,
                Level = stats.LevelSnapshot,
                TotalAttackDamage = stats.TotalAttackDamage,
                TotalTakenDamage = stats.TotalTakenDamage,
                TotalHeal = stats.TotalHeal,
                IsNpcData = stats.IsNpcData,
                Class = stats.Player.Class,
                Spec = stats.Player.Spec,
                SkillDataJson = stats.SkillDataJson
            };

            encounterData.PlayerStats[stats.PlayerUID] = playerData;
        }

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
}
