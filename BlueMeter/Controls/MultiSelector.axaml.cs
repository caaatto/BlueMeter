using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;

namespace BlueMeter.Controls;

/// <summary>
/// Segmented selector control: renders an ItemsControl of rounded buttons
/// and highlights the one at <see cref="ActiveIndex"/>.
///
/// Port notes: the WPF version relied on <c>ItemsControl.AlternationIndex</c>
/// inside a <c>DataTrigger</c> to drive the active-item style. Avalonia 11 has
/// no alternation API, so selection is driven imperatively from
/// <see cref="OnPropertyChanged"/>: when <see cref="ActiveIndex"/> changes, we
/// walk the host's containers and toggle
/// <see cref="MultiSelectorItem.IsActive"/>, which flips the <c>:active</c>
/// pseudo-class and lights up the style selector.
/// </summary>
public partial class MultiSelector : UserControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MultiSelector, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<MultiSelector, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<int> ActiveIndexProperty =
        AvaloniaProperty.Register<MultiSelector, int>(
            nameof(ActiveIndex),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private MultiSelectorItemsHost? _host;

    public MultiSelector()
    {
        InitializeComponent();
        _host = this.FindControl<MultiSelectorItemsHost>("SelectionItems");
        if (_host is not null)
        {
            _host.ContainerPrepared += OnContainerPrepared;
        }
    }

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

    public int ActiveIndex
    {
        get => GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_host is null)
        {
            return;
        }

        if (change.Property == ItemsSourceProperty)
        {
            _host.ItemsSource = ItemsSource;
            UpdateActiveItem();
        }
        else if (change.Property == ItemTemplateProperty)
        {
            _host.ItemTemplate = ItemTemplate;
        }
        else if (change.Property == ActiveIndexProperty)
        {
            UpdateActiveItem();
        }
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is MultiSelectorItem item)
        {
            item.IsActive = e.Index == ActiveIndex;
        }
    }

    private void UpdateActiveItem()
    {
        if (_host is null)
        {
            return;
        }

        for (int i = 0; i < _host.ItemCount; i++)
        {
            if (_host.ContainerFromIndex(i) is MultiSelectorItem item)
            {
                item.IsActive = i == ActiveIndex;
            }
        }
    }
}
