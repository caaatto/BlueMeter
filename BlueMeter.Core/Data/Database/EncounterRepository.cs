using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlueMeter.Core.Data.Models;
using BlueMeter.Core.Data.Models.Database;
using BlueMeter.Core.Models;
using Newtonsoft.Json;

namespace BlueMeter.Core.Data.Database;

/// <summary>
/// Repository for managing encounters and player statistics in the database
/// </summary>
public class EncounterRepository
{
    private readonly BlueMeterDbContext _context;

    public EncounterRepository(BlueMeterDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create a new encounter
    /// </summary>
    public async Task<EncounterEntity> CreateEncounterAsync(DateTime startTime, int sectionNumber = 0)
    {
        var encounter = new EncounterEntity
        {
            EncounterId = Guid.NewGuid().ToString(),
            StartTime = startTime,
            IsActive = true,
            SectionNumber = sectionNumber
        };

        _context.Encounters.Add(encounter);
        await _context.SaveChangesAsync();

        return encounter;
    }

    /// <summary>
    /// End an encounter
    /// </summary>
    public async Task EndEncounterAsync(string encounterId, DateTime endTime, long durationMs, string? bossName = null, long? bossUuid = null)
    {
        var encounter = await _context.Encounters
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter != null)
        {
            encounter.EndTime = endTime;
            encounter.DurationMs = durationMs;
            encounter.IsActive = false;

            // Set boss name if provided
            if (!string.IsNullOrEmpty(bossName))
            {
                encounter.BossName = bossName;
            }

            // Set boss UID if provided
            if (bossUuid.HasValue)
            {
                encounter.BossUID = bossUuid.Value;
            }

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Get or create a player
    /// </summary>
    public async Task<PlayerEntity> EnsurePlayerAsync(PlayerInfo playerInfo, bool isNpc = false)
    {
        var player = await _context.Players.FindAsync(playerInfo.UID);

        if (player == null)
        {
            player = new PlayerEntity
            {
                UID = playerInfo.UID,
                Name = playerInfo.Name ?? "Unknown",
                FirstSeenTime = DateTime.UtcNow,
                IsNpc = isNpc
            };
            _context.Players.Add(player);
        }

        // Update player info
        player.Name = playerInfo.Name ?? "Unknown";
        player.ProfessionID = playerInfo.ProfessionID ?? 0;
        player.SubProfessionName = playerInfo.SubProfessionName ?? string.Empty;
        player.Spec = playerInfo.Spec;
        player.Class = playerInfo.Class;
        player.CombatPower = playerInfo.CombatPower ?? 0;
        player.Level = playerInfo.Level ?? 0;
        player.RankLevel = playerInfo.RankLevel ?? 0;
        player.Critical = playerInfo.Critical ?? 0;
        player.Lucky = playerInfo.Lucky ?? 0;
        player.MaxHP = playerInfo.MaxHP ?? 0;
        player.LastSeenTime = DateTime.UtcNow;
        player.IsNpc = isNpc;

        await _context.SaveChangesAsync();

        return player;
    }

    /// <summary>
    /// Save player statistics for an encounter
    /// </summary>
    public async Task SavePlayerStatsAsync(
        string encounterId,
        PlayerInfo playerInfo,
        DpsData dpsData,
        List<ChartDataPoint>? dpsHistory = null,
        List<ChartDataPoint>? hpsHistory = null)
    {
        var encounter = await _context.Encounters
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter == null) return;

        // Ensure player exists
        await EnsurePlayerAsync(playerInfo, dpsData.IsNpcData);

        // Check if stats already exist
        var stats = await _context.PlayerEncounterStats
            .FirstOrDefaultAsync(s => s.PlayerUID == playerInfo.UID && s.EncounterId == encounter.Id);

        if (stats == null)
        {
            stats = new PlayerEncounterStatsEntity
            {
                PlayerUID = playerInfo.UID,
                EncounterId = encounter.Id
            };
            _context.PlayerEncounterStats.Add(stats);
        }

        // Update stats
        stats.TotalAttackDamage = dpsData.TotalAttackDamage;
        stats.TotalTakenDamage = dpsData.TotalTakenDamage;
        stats.TotalHeal = dpsData.TotalHeal;
        stats.StartLoggedTick = dpsData.StartLoggedTick ?? 0;
        stats.LastLoggedTick = dpsData.LastLoggedTick;
        stats.IsNpcData = dpsData.IsNpcData;

        // Snapshot current player stats
        stats.CombatPowerSnapshot = playerInfo.CombatPower ?? 0;
        stats.LevelSnapshot = playerInfo.Level ?? 0;
        stats.NameSnapshot = playerInfo.Name ?? "Unknown";

        // Serialize skill data
        var skillDictionary = new Dictionary<long, SkillData>();
        foreach (var skill in dpsData.ReadOnlySkillDataList)
        {
            skillDictionary[skill.SkillId] = skill;
        }
        stats.SkillDataJson = JsonConvert.SerializeObject(skillDictionary);

        // Serialize chart history data
        if (dpsHistory != null && dpsHistory.Count > 0)
        {
            stats.DpsHistoryJson = JsonConvert.SerializeObject(dpsHistory);
            DebugLogger.Log($"[SavePlayerStatsAsync] Saved DPS history for {playerInfo.Name}: {dpsHistory.Count} data points, JSON length: {stats.DpsHistoryJson.Length}");
        }
        else
        {
            DebugLogger.Log($"[SavePlayerStatsAsync] No DPS history to save for {playerInfo.Name}");
        }

        if (hpsHistory != null && hpsHistory.Count > 0)
        {
            stats.HpsHistoryJson = JsonConvert.SerializeObject(hpsHistory);
            DebugLogger.Log($"[SavePlayerStatsAsync] Saved HPS history for {playerInfo.Name}: {hpsHistory.Count} data points, JSON length: {stats.HpsHistoryJson.Length}");
        }
        else
        {
            DebugLogger.Log($"[SavePlayerStatsAsync] No HPS history to save for {playerInfo.Name}");
        }

        // Calculate aggregate statistics
        var totalHits = 0;
        var totalCrits = 0;
        var totalLuckyHits = 0;
        var highestCrit = 0L;
        var minDamage = long.MaxValue;
        var maxDamage = 0L;

        foreach (var skill in dpsData.ReadOnlySkillDataList)
        {
            totalHits += skill.UseTimes;
            totalCrits += skill.CritTimes;
            totalLuckyHits += skill.LuckyTimes;
            highestCrit = Math.Max(highestCrit, skill.HighestCrit);

            if (skill.MinDamage < long.MaxValue)
                minDamage = Math.Min(minDamage, skill.MinDamage);

            maxDamage = Math.Max(maxDamage, skill.MaxDamage);
        }

        stats.TotalHits = totalHits;
        stats.TotalCrits = totalCrits;
        stats.TotalLuckyHits = totalLuckyHits;
        stats.HighestCrit = highestCrit;
        stats.MinDamage = minDamage == long.MaxValue ? 0 : minDamage;
        stats.MaxDamage = maxDamage;

        // Calculate rates and averages
        if (totalHits > 0)
        {
            stats.AvgDamagePerHit = (double)stats.TotalAttackDamage / totalHits;
            stats.CritRate = (double)totalCrits / totalHits;
            stats.LuckyRate = (double)totalLuckyHits / totalHits;
        }
        else
        {
            stats.AvgDamagePerHit = 0;
            stats.CritRate = 0;
            stats.LuckyRate = 0;
        }

        // Calculate DPS/HPS using active combat time (excluding downtime >1s)
        // This gives accurate DPS without counting boss phases, running between packs, etc.
        var activeCombatSeconds = dpsData.ActiveCombatTicks / 10_000_000.0; // Convert Windows ticks to seconds

        if (activeCombatSeconds > 0)
        {
            stats.DPS = stats.TotalAttackDamage / activeCombatSeconds;
            stats.HPS = stats.TotalHeal / activeCombatSeconds;
        }
        else
        {
            // Fallback: If no active combat time tracked, use total duration
            var durationTicks = stats.LastLoggedTick - stats.StartLoggedTick;
            var durationSeconds = durationTicks / 10_000_000.0;
            if (durationSeconds > 0)
            {
                stats.DPS = stats.TotalAttackDamage / durationSeconds;
                stats.HPS = stats.TotalHeal / durationSeconds;
            }
            else
            {
                stats.DPS = 0;
                stats.HPS = 0;
            }
        }

        await _context.SaveChangesAsync();

        // Update encounter totals
        await UpdateEncounterTotalsAsync(encounter.Id);
    }

    /// <summary>
    /// Update encounter total statistics
    /// </summary>
    private async Task UpdateEncounterTotalsAsync(int encounterId)
    {
        var encounter = await _context.Encounters
            .Include(e => e.PlayerStats)
            .FirstOrDefaultAsync(e => e.Id == encounterId);

        if (encounter == null)
        {
            DebugLogger.Log($"[UpdateEncounterTotalsAsync] Encounter ID {encounterId} not found!");
            return;
        }

        DebugLogger.Log($"[UpdateEncounterTotalsAsync] Updating encounter {encounter.EncounterId}");
        DebugLogger.Log($"[UpdateEncounterTotalsAsync] PlayerStats count: {encounter.PlayerStats.Count}");

        var totalDamage = encounter.PlayerStats.Sum(s => s.TotalAttackDamage);
        var totalHealing = encounter.PlayerStats.Sum(s => s.TotalHeal);
        var playerCount = encounter.PlayerStats.Count(s => !s.IsNpcData);

        DebugLogger.Log($"[UpdateEncounterTotalsAsync] Calculated: TotalDamage={totalDamage}, TotalHealing={totalHealing}, PlayerCount={playerCount}");

        encounter.TotalDamage = totalDamage;
        encounter.TotalHealing = totalHealing;
        encounter.PlayerCount = playerCount;

        // Identify primary target/boss (NPC with most damage taken)
        var primaryTarget = encounter.PlayerStats
            .Where(s => s.IsNpcData && s.TotalTakenDamage > 0)
            .OrderByDescending(s => s.TotalTakenDamage)
            .FirstOrDefault();

        if (primaryTarget != null)
        {
            encounter.BossName = primaryTarget.NameSnapshot;
            encounter.BossUID = primaryTarget.PlayerUID;

            // If name is Unknown, try to get from Player table
            if (string.IsNullOrEmpty(encounter.BossName) || encounter.BossName == "Unknown")
            {
                encounter.BossName = primaryTarget.Player.Name ?? "Unknown Target";
            }

            DebugLogger.Log($"[UpdateEncounterTotalsAsync] Identified primary target: {encounter.BossName} (UID={encounter.BossUID}, TakenDamage={primaryTarget.TotalTakenDamage})");
        }
        else
        {
            encounter.BossName = "Unknown";
            encounter.BossUID = null;
            DebugLogger.Log($"[UpdateEncounterTotalsAsync] No NPCs found, setting BossName to 'Unknown'");
        }

        // Calculate duration from player ticks if not already set
        if (encounter.DurationMs == 0 && encounter.PlayerStats.Any())
        {
            // Find the longest duration from all players
            var maxTick = encounter.PlayerStats.Max(s => s.LastLoggedTick);
            var minTick = encounter.PlayerStats.Where(s => s.StartLoggedTick > 0).Min(s => s.StartLoggedTick);

            // Convert from Windows ticks to milliseconds
            // Windows ticks are 100-nanosecond intervals
            var durationTicks = maxTick - minTick;
            encounter.DurationMs = durationTicks / 10000; // Convert to milliseconds

            DebugLogger.Log($"[UpdateEncounterTotalsAsync] Calculated DurationMs: {encounter.DurationMs}ms (from ticks {minTick} to {maxTick})");
        }

        await _context.SaveChangesAsync();

        DebugLogger.Log($"[UpdateEncounterTotalsAsync] Saved: BossName={encounter.BossName}, TotalDamage={encounter.TotalDamage}, TotalHealing={encounter.TotalHealing}, PlayerCount={encounter.PlayerCount}, Duration={encounter.DurationMs}ms");
    }

    /// <summary>
    /// Get recent encounters (for history dropdown)
    /// </summary>
    public async Task<List<EncounterEntity>> GetRecentEncountersAsync(int count = 50)
    {
        return await _context.Encounters
            .OrderByDescending(e => e.StartTime)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Get encounter with full player stats
    /// </summary>
    public async Task<EncounterEntity?> GetEncounterWithStatsAsync(string encounterId)
    {
        return await _context.Encounters
            .Include(e => e.PlayerStats)
            .ThenInclude(s => s.Player)
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);
    }

    /// <summary>
    /// Get current active encounter
    /// </summary>
    public async Task<EncounterEntity?> GetActiveEncounterAsync()
    {
        return await _context.Encounters
            .FirstOrDefaultAsync(e => e.IsActive);
    }

    /// <summary>
    /// Deactivate all encounters (cleanup)
    /// </summary>
    public async Task DeactivateAllEncountersAsync()
    {
        var activeEncounters = await _context.Encounters
            .Where(e => e.IsActive)
            .ToListAsync();

        foreach (var encounter in activeEncounters)
        {
            encounter.IsActive = false;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get player info from database (cached data)
    /// </summary>
    public async Task<PlayerEntity?> GetPlayerAsync(long uid)
    {
        return await _context.Players.FindAsync(uid);
    }

    /// <summary>
    /// Get all players
    /// </summary>
    public async Task<List<PlayerEntity>> GetAllPlayersAsync()
    {
        return await _context.Players.ToListAsync();
    }

    /// <summary>
    /// Delete a specific encounter from database
    /// </summary>
    /// <returns>True if encounter was deleted, false if not found</returns>
    public async Task<bool> DeleteEncounterAsync(string encounterId)
    {
        DebugLogger.Log($"[DeleteEncounterAsync] Deleting encounter {encounterId}...");

        var encounter = await _context.Encounters
            .Include(e => e.PlayerStats)
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter == null)
        {
            DebugLogger.Log($"[DeleteEncounterAsync] Encounter {encounterId} not found");
            return false;
        }

        // Delete player stats first (due to foreign key constraint)
        _context.PlayerEncounterStats.RemoveRange(encounter.PlayerStats);

        // Delete encounter
        _context.Encounters.Remove(encounter);

        await _context.SaveChangesAsync();
        DebugLogger.Log($"[DeleteEncounterAsync] Deleted encounter {encounterId} with {encounter.PlayerStats.Count} player stats");

        return true;
    }

    /// <summary>
    /// Delete all encounters and their statistics
    /// </summary>
    public async Task<int> DeleteAllEncountersAsync()
    {
        DebugLogger.Log("[DeleteAllEncountersAsync] Starting to delete all encounters...");

        // Get count before deletion
        var count = await _context.Encounters.CountAsync();
        DebugLogger.Log($"[DeleteAllEncountersAsync] Found {count} encounters to delete");

        // Delete all player encounter stats first (foreign key constraint)
        var statsCount = await _context.PlayerEncounterStats.CountAsync();
        _context.PlayerEncounterStats.RemoveRange(_context.PlayerEncounterStats);
        await _context.SaveChangesAsync();
        DebugLogger.Log($"[DeleteAllEncountersAsync] Deleted {statsCount} player stats");

        // Delete all encounters
        _context.Encounters.RemoveRange(_context.Encounters);
        await _context.SaveChangesAsync();
        DebugLogger.Log($"[DeleteAllEncountersAsync] Deleted {count} encounters");

        return count;
    }

    /// <summary>
    /// Clean up old encounters (keep last N)
    /// </summary>
    public async Task CleanupOldEncountersAsync(int keepCount = 100)
    {
        var encountersToKeep = await _context.Encounters
            .OrderByDescending(e => e.StartTime)
            .Take(keepCount)
            .Select(e => e.Id)
            .ToListAsync();

        var encountersToDelete = await _context.Encounters
            .Where(e => !encountersToKeep.Contains(e.Id))
            .ToListAsync();

        _context.Encounters.RemoveRange(encountersToDelete);
        await _context.SaveChangesAsync();
    }
}
