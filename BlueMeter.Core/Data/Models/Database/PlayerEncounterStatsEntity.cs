using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlueMeter.Core.Data.Models.Database;

/// <summary>
/// Represents a player's statistics for a specific encounter
/// </summary>
[Table("PlayerEncounterStats")]
public class PlayerEncounterStatsEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Player
    /// </summary>
    [Required]
    public long PlayerUID { get; set; }

    /// <summary>
    /// Foreign key to Encounter
    /// </summary>
    [Required]
    public int EncounterId { get; set; }

    /// <summary>
    /// Total attack damage dealt
    /// </summary>
    public long TotalAttackDamage { get; set; }

    /// <summary>
    /// Total damage taken
    /// </summary>
    public long TotalTakenDamage { get; set; }

    /// <summary>
    /// Total healing done
    /// </summary>
    public long TotalHeal { get; set; }

    /// <summary>
    /// First action timestamp (Windows ticks)
    /// </summary>
    public long StartLoggedTick { get; set; }

    /// <summary>
    /// Last action timestamp (Windows ticks)
    /// </summary>
    public long LastLoggedTick { get; set; }

    /// <summary>
    /// Whether this is NPC data
    /// </summary>
    public bool IsNpcData { get; set; }

    /// <summary>
    /// Serialized skill statistics (JSON)
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? SkillDataJson { get; set; }

    /// <summary>
    /// DPS history time-series data (JSON: List&lt;ChartDataPoint&gt;)
    /// Used for chart visualization of DPS over time
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? DpsHistoryJson { get; set; }

    /// <summary>
    /// HPS history time-series data (JSON: List&lt;ChartDataPoint&gt;)
    /// Used for chart visualization of HPS over time
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? HpsHistoryJson { get; set; }

    /// <summary>
    /// Player's combat power at the time of this encounter
    /// </summary>
    public int CombatPowerSnapshot { get; set; }

    /// <summary>
    /// Player's level at the time of this encounter
    /// </summary>
    public int LevelSnapshot { get; set; }

    /// <summary>
    /// Player's name at the time of this encounter
    /// </summary>
    [MaxLength(100)]
    public string NameSnapshot { get; set; } = string.Empty;

    // ===== Aggregate Statistics =====

    /// <summary>
    /// Total number of hits (sum of all skill UseTimes)
    /// </summary>
    public int TotalHits { get; set; }

    /// <summary>
    /// Total number of critical hits (sum of all skill CritTimes)
    /// </summary>
    public int TotalCrits { get; set; }

    /// <summary>
    /// Total number of lucky hits (sum of all skill LuckyTimes)
    /// </summary>
    public int TotalLuckyHits { get; set; }

    /// <summary>
    /// Average damage per hit (TotalAttackDamage / TotalHits)
    /// </summary>
    public double AvgDamagePerHit { get; set; }

    /// <summary>
    /// Critical hit rate (TotalCrits / TotalHits)
    /// </summary>
    public double CritRate { get; set; }

    /// <summary>
    /// Lucky hit rate (TotalLuckyHits / TotalHits)
    /// </summary>
    public double LuckyRate { get; set; }

    /// <summary>
    /// Damage per second (TotalAttackDamage / encounter duration in seconds)
    /// </summary>
    public double DPS { get; set; }

    /// <summary>
    /// Healing per second (TotalHeal / encounter duration in seconds)
    /// </summary>
    public double HPS { get; set; }

    /// <summary>
    /// Highest critical hit damage
    /// </summary>
    public long HighestCrit { get; set; }

    /// <summary>
    /// Minimum damage dealt in a single hit
    /// </summary>
    public long MinDamage { get; set; }

    /// <summary>
    /// Maximum damage dealt in a single hit
    /// </summary>
    public long MaxDamage { get; set; }

    /// <summary>
    /// Navigation property to Player
    /// </summary>
    [ForeignKey(nameof(PlayerUID))]
    public virtual PlayerEntity Player { get; set; } = null!;

    /// <summary>
    /// Navigation property to Encounter
    /// </summary>
    [ForeignKey(nameof(EncounterId))]
    public virtual EncounterEntity Encounter { get; set; } = null!;
}
