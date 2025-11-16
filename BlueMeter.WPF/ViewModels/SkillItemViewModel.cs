using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.WPF.ViewModels;

public partial class SkillItemViewModel : BaseViewModel
{
    [ObservableProperty] private int _avgDamage;
    [ObservableProperty] private int _critCount;
    [ObservableProperty] private int _hitCount;
    [ObservableProperty] private string _skillName = string.Empty;
    [ObservableProperty] private long _totalDamage;
<<<<<<< Updated upstream
=======
    [ObservableProperty] private long _minDamage;
    [ObservableProperty] private long _maxDamage;
    [ObservableProperty] private long _highestCrit;
>>>>>>> Stashed changes
}