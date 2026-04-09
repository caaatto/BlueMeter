using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.ViewModels;

[DebuggerDisplay("Name:{Player.Name};Value:{Value}")]
public partial class StatisticDataViewModel(DebugFunctions debug) : BaseViewModel, IComparable<StatisticDataViewModel>
{
    [ObservableProperty] private ulong _duration;
    [ObservableProperty] private long _index;
    [ObservableProperty] private double _percent;
    [ObservableProperty] private double _percentOfMax;
    [ObservableProperty] private PlayerInfoViewModel _player = new();

    [ObservableProperty] private IReadOnlyList<SkillItemViewModel> _skillList = [];

    [ObservableProperty] private ulong _value;

    // Tank/Mitigation stats
    [ObservableProperty] private ulong _damageTaken;  // HP damage actually taken
    [ObservableProperty] private ulong _damageMitigated;  // Shield/mitigation absorbed

    /// <summary>
    /// Total effective damage (HP + Shield) - represents total threat
    /// </summary>
    public ulong EffectiveDamage => DamageTaken + DamageMitigated;

    /// <summary>
    /// Mitigation percentage: (Mitigated / Effective) × 100
    /// </summary>
    public double MitigationPercent => EffectiveDamage > 0
        ? (double)DamageMitigated / EffectiveDamage * 100.0
        : 0.0;

    /// <summary>
    /// Effective TPS (Threat Per Second) including shields
    /// </summary>
    public double EffectiveTps => Duration > 0
        ? (double)EffectiveDamage / Duration
        : 0.0;

    public DebugFunctions Debug { get; } = debug;

    public int CompareTo(StatisticDataViewModel? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }
}
