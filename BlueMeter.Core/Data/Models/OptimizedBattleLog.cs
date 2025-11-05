using System.Runtime.InteropServices;
using BlueMeter.Core.Memory;

namespace BlueMeter.Core.Data.Models;

/// <summary>
/// Memory-optimized battle log structure with improved layout
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 64)] // Explicit size for cache line alignment
public readonly struct OptimizedBattleLog
{
    // Most frequently accessed fields first (hot data)
    [FieldOffset(0)] public readonly long PacketID;
    [FieldOffset(8)] public readonly long TimeTicks;
    [FieldOffset(16)] public readonly long AttackerUuid;
    [FieldOffset(24)] public readonly long TargetUuid;
    [FieldOffset(32)] public readonly long Value;
    [FieldOffset(40)] public readonly long SkillID;
    
    // Pack smaller fields together
    [FieldOffset(48)] public readonly int ValueElementType;
    [FieldOffset(52)] public readonly int DamageSourceType;
    
    // Pack all boolean flags into a single byte using bit flags
    [FieldOffset(56)] private readonly BattleLogFlags _flags;
    
    // Reserved space for future use or padding
    [FieldOffset(57)] private readonly byte _reserved1;
    [FieldOffset(58)] private readonly ushort _reserved2;
    [FieldOffset(60)] private readonly uint _reserved3;

    public OptimizedBattleLog(
        long packetId, long timeTicks, long skillId, long attackerUuid, long targetUuid, 
        long value, int valueElementType, int damageSourceType,
        bool isAttackerPlayer, bool isTargetPlayer, bool isLucky, bool isCritical, 
        bool isHeal, bool isMiss, bool isDead)
    {
        PacketID = packetId;
        TimeTicks = timeTicks;
        SkillID = skillId;
        AttackerUuid = attackerUuid;
        TargetUuid = targetUuid;
        Value = value;
        ValueElementType = valueElementType;
        DamageSourceType = damageSourceType;
        
        _flags = BattleLogFlags.None;
        if (isAttackerPlayer) _flags |= BattleLogFlags.IsAttackerPlayer;
        if (isTargetPlayer) _flags |= BattleLogFlags.IsTargetPlayer;
        if (isLucky) _flags |= BattleLogFlags.IsLucky;
        if (isCritical) _flags |= BattleLogFlags.IsCritical;
        if (isHeal) _flags |= BattleLogFlags.IsHeal;
        if (isMiss) _flags |= BattleLogFlags.IsMiss;
        if (isDead) _flags |= BattleLogFlags.IsDead;
        
        _reserved1 = 0;
        _reserved2 = 0;
        _reserved3 = 0;
    }

    // Properties using bit flag checks
    public bool IsAttackerPlayer => (_flags & BattleLogFlags.IsAttackerPlayer) != 0;
    public bool IsTargetPlayer => (_flags & BattleLogFlags.IsTargetPlayer) != 0;
    public bool IsLucky => (_flags & BattleLogFlags.IsLucky) != 0;
    public bool IsCritical => (_flags & BattleLogFlags.IsCritical) != 0;
    public bool IsHeal => (_flags & BattleLogFlags.IsHeal) != 0;
    public bool IsMiss => (_flags & BattleLogFlags.IsMiss) != 0;
    public bool IsDead => (_flags & BattleLogFlags.IsDead) != 0;

    // Implicit conversion from original BattleLog
    public static implicit operator OptimizedBattleLog(BattleLog original)
    {
        return new OptimizedBattleLog(
            original.PacketID, original.TimeTicks, original.SkillID,
            original.AttackerUuid, original.TargetUuid, original.Value,
            original.ValueElementType, original.DamageSourceType,
            original.IsAttackerPlayer, original.IsTargetPlayer,
            original.IsLucky, original.IsCritical, original.IsHeal,
            original.IsMiss, original.IsDead);
    }
}

[Flags]
internal enum BattleLogFlags : byte
{
    None = 0,
    IsAttackerPlayer = 1,
    IsTargetPlayer = 2,
    IsLucky = 4,
    IsCritical = 8,
    IsHeal = 16,
    IsMiss = 32,
    IsDead = 64
}