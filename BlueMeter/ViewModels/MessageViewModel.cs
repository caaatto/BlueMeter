using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.ViewModels;

public partial class MessageViewModel : BaseViewModel
{
    [ObservableProperty] private string? _title;
    [ObservableProperty] private string? _content;
}
