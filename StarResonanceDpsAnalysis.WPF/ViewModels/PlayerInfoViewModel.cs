using CommunityToolkit.Mvvm.ComponentModel;
using StarResonanceDpsAnalysis.Core.Models;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class PlayerInfoViewModel : BaseViewModel
{
    [ObservableProperty] private Classes _class = Classes.Unknown;
    [ObservableProperty] private string _guild = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private ClassSpec _spec = ClassSpec.Unknown;
    [ObservableProperty] private long _uid;
    [ObservableProperty] private bool _isNpc = false;
    [ObservableProperty] private int _powerLevel = 0;
}