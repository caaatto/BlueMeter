using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlueMeter.Core.Data.Models.Database;

/// <summary>
/// Represents a combat encounter/battle session in the database
/// </summary>
[Table("Encounters")]
public class EncounterEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Unique identifier for the encounter
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string EncounterId { get; set; } = string.Empty;

    /// <summary>
    /// When the encounter started
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// When the encounter ended
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Server name or instance
    /// </summary>
    [MaxLength(100)]
    public string? ServerName { get; set; }

    /// <summary>
    /// Whether this encounter is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Section number for multi-section battles
    /// </summary>
    public int SectionNumber { get; set; }

    /// <summary>
    /// Total damage dealt in this encounter
    /// </summary>
    public long TotalDamage { get; set; }

    /// <summary>
    /// Total healing done in this encounter
    /// </summary>
    public long TotalHealing { get; set; }

    /// <summary>
    /// Number of players in this encounter
    /// </summary>
    public int PlayerCount { get; set; }

    /// <summary>
    /// Primary target/boss name (NPC with most damage taken)
    /// </summary>
    [MaxLength(200)]
    public string? BossName { get; set; }

    /// <summary>
    /// UID of the primary target/boss
    /// </summary>
    public long? BossUID { get; set; }

    /// <summary>
    /// Navigation property for player statistics
    /// </summary>
    public virtual ICollection<PlayerEncounterStatsEntity> PlayerStats { get; set; } = new List<PlayerEncounterStatsEntity>();
}
