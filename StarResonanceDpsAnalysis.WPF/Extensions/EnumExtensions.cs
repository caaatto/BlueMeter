using System.Diagnostics;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.Extensions;

/// <summary>
///     针对 StatisticType 枚举的扩展方法集合
/// </summary>
internal static class EnumExtensions
{
    private static T Roll<T>(this T value, int offset, Dictionary<T, int> map) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        Debug.Assert(values.Length > 0, "Enum values length needs to be greater than 0");
        for (var i = 0; i < values.Length; i++) map[values[i]] = i;
        if (!map.TryGetValue(value, out var index))
        {
            // Unexpected case: not found in enum values list, return original value to ensure predictable behavior
            // 意外情况：未在枚举值列表中找到，返回原值以保证行为可预测
            return value;
        }

        var len = values.Length;
        // Normalize offset into range [-len+1, len-1] via modulo
        // 将偏移量归一化到范围 [-len+1, len-1] 通过取模运算
        var normalized = offset % len;
        // newIndex corrects for negative offsets by adding len then modulo
        // newIndex 通过加上 len 然后取模来纠正负偏移
        var newIndex = (index + normalized + len) % len;
        return values[newIndex];
    }

    #region StatisticType Roll Extension Methods

    /// <summary>
    /// 延迟缓存值到索引的映射，避免每次 Roll 时使用 Array.IndexOf（O(n)）
    /// </summary>
    private static readonly Lazy<Dictionary<StatisticType, int>> StatisticTypeIndexMap =
        new(() =>
        {
            var vals = (StatisticType[])Enum.GetValues(typeof(StatisticType));
            var map = new Dictionary<StatisticType, int>(vals.Length);
            for (var i = 0; i < vals.Length; i++) map[vals[i]] = i;
            return map;
        });

    /// <summary>
    ///     将当前枚举值按指定偏移量进行循环滚动（向前或向后），支持负偏移。
    ///     使用预构建的索引映射以获得常数时间查找。
    /// </summary>
    /// <param name="value">当前 StatisticType 值</param>
    /// <param name="offset">偏移量，正数表示下n个</param>
    /// <returns>滚动后的 StatisticType 值（若找不到当前值则返回原值）</returns>
    public static StatisticType Roll(this StatisticType value, int offset)
    {
        return value.Roll(offset, StatisticTypeIndexMap.Value);
    }

    /// <summary>获取当前枚举值的下一个值（循环）</summary>
    public static StatisticType Next(this StatisticType value)
    {
        return value.Roll(1);
    }

    /// <summary>获取当前枚举值的上一个值（循环）</summary>
    public static StatisticType Previous(this StatisticType value)
    {
        return value.Roll(-1);
    }

    #endregion

    #region ScopeTime Roll Extension Methods

    /// <summary>
    /// 延迟缓存值到索引的映射，避免每次 Roll 时使用 Array.IndexOf（O(n)）
    /// </summary>
    private static readonly Lazy<Dictionary<ScopeTime, int>> ScopeTimeIndexMap =
        new(() =>
        {
            var vals = (ScopeTime[])Enum.GetValues(typeof(ScopeTime));
            var map = new Dictionary<ScopeTime, int>(vals.Length);
            for (var i = 0; i < vals.Length; i++) map[vals[i]] = i;
            return map;
        });

    /// <summary>
    ///     将当前枚举值按指定偏移量进行循环滚动（向前或向后），支持负偏移。
    ///     使用预构建的索引映射以获得常数时间查找。
    /// </summary>
    /// <param name="value">当前 ScopeTime 值</param>
    /// <param name="offset">偏移量，正数表示下n个</param>
    /// <returns>滚动后的 ScopeTime 值（若找不到当前值则返回原值）</returns>
    public static ScopeTime Roll(this ScopeTime value, int offset)
    {
        return value.Roll(offset, ScopeTimeIndexMap.Value);
    }

    /// <summary>获取当前枚举值的下一个值（循环）</summary>
    public static ScopeTime Next(this ScopeTime value)
    {
        return value.Roll(1);
    }

    /// <summary>获取当前枚举值的上一个值（循环）</summary>
    public static ScopeTime Previous(this ScopeTime value)
    {
        return value.Roll(-1);
    }

    #endregion
}