using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BlueMeter.WPF.Controls;

/// <summary>
/// MultiSelector.xaml 的交互逻辑
/// </summary>
[ContentProperty(nameof(Items))]
public partial class MultiSelector : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(MultiSelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(MultiSelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty ActiveIndexProperty =
        DependencyProperty.Register(
            nameof(ActiveIndex),
            typeof(int),
            typeof(MultiSelector),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public MultiSelector()
    {
        InitializeComponent();
    }

    public ItemCollection Items => SelectionItems.Items;

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set
        {
            SetValue(ItemsSourceProperty, value);
            SelectionItems.ItemsSource = value;
        }
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set
        {
            SetValue(ItemTemplateProperty, value);
            SelectionItems.ItemTemplate = value;
        }
    }

    public int ActiveIndex
    {
        get => (int)GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }
}