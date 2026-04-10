using Avalonia;
using Avalonia.Controls;

namespace BlueMeter.Controls;

public class MultiSelectorItemsHost : ItemsControl
{
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not MultiSelectorItem;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new MultiSelectorItem();
    }
}

/// <summary>
/// Container for a single item inside a <see cref="MultiSelector"/>.
///
/// Port notes: the WPF version compared <c>ItemsControl.AlternationIndex</c>
/// against the parent <c>MultiSelector.ActiveIndex</c> in a <c>DataTrigger</c>.
/// Avalonia 11 dropped the alternation pattern, so this replacement exposes
/// an <see cref="IsActive"/> styled property and toggles the <c>:active</c>
/// pseudo-class that XAML selectors can match on.
/// </summary>
public class MultiSelectorItem : ContentControl
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<MultiSelectorItem, bool>(nameof(IsActive));

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsActiveProperty)
        {
            PseudoClasses.Set(":active", IsActive);
        }
    }
}
