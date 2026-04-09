using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace BlueMeter.Controls;

public class SortedDpsControl : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SortedDpsControl, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<SortedDpsControl, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<SortedDpsControl, double>(nameof(ItemHeight));

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<SortedDpsControl, object?>(nameof(SelectedItem));

    public static readonly StyledProperty<ICommand?> ItemClickCommandProperty =
        AvaloniaProperty.Register<SortedDpsControl, ICommand?>(nameof(ItemClickCommand));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand? ItemClickCommand
    {
        get => GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }
}
