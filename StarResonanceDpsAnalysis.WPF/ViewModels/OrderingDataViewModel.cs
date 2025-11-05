using CommunityToolkit.Mvvm.ComponentModel;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public abstract partial class OrderingDataViewModel : ObservableObject
{
    // ReSharper disable once InconsistentNaming
    [ObservableProperty] protected int _order;
}