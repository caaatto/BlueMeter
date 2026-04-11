namespace BlueMeter.Models;

/// <summary>
/// 统计时间范围（全程/当前）
/// </summary>
public enum ScopeTime
{
    [LocalizedDescription("ScopeTime_Total")]
    Total,

    [LocalizedDescription("ScopeTime_Current")]
    Current
}
