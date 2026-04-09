using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.ViewModels;

/// <summary>
/// Wraps a value and a localized display string for use in selection controls.
/// </summary>
public partial class Option<T>(T value, string display) : BaseViewModel
{
    [ObservableProperty] private string _display = display;
    [ObservableProperty] private T _value = value;

    public void Deconstruct(out T value, out string display)
    {
        value = Value;
        display = Display;
    }
}
