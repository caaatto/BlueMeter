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
    public async Task EndEncounterAsync(string encounterId, DateTime endTime, long durationMs)
    {
        var encounter = await _context.Encounters
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter != null)
        {
            encounter.EndTime = endTime;
            encounter.DurationMs = durationMs;
            encounter.IsActive = false;
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
    public async Task SavePlayerStatsAsync(string encounterId, PlayerInfo playerInfo, DpsData dpsData)
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

        if (encounter == null) return;

        encounter.TotalDamage = encounter.PlayerStats.Sum(s => s.TotalAttackDamage);
        encounter.TotalHealing = encounter.PlayerStats.Sum(s => s.TotalHeal);
        encounter.PlayerCount = encounter.PlayerStats.Count;

        await _context.SaveChangesAsync();
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
