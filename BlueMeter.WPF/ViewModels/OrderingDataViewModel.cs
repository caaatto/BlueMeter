using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.WPF.ViewModels;

public abstract partial class OrderingDataViewModel : ObservableObject
{
    // ReSharper disable once InconsistentNaming
    [ObservableProperty] protected int _order;
}