using CommunityToolkit.Mvvm.ComponentModel;
using BlueMeter.Core.Models;

namespace BlueMeter.WPF.ViewModels;

/// <summary>
/// 用于 DataTemplate 绑定的数据载体（挂到 ProgressBarData.Data 上）
/// </summary>
public partial class PlayerSlotViewModel : OrderingDataViewModel
{
    [ObservableProperty] private Classes _class = Classes.Unknown;
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private string _nickname = string.Empty;

    [ObservableProperty] private ulong _value;
}