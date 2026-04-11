namespace BlueMeter.Models;

/// <summary>
/// 数值类型
/// </summary>
public enum StatisticType
{
    [LocalizedDescription("StatisticType_Damage")]
    Damage = 0,

    [LocalizedDescription("StatisticType_Healing")]
    Healing = 1,

    [LocalizedDescription("StatisticType_TakenDamage")]
    TakenDamage = 2,

    [LocalizedDescription("StatisticType_NpcTakenDamage")]
    NpcTakenDamage
}
