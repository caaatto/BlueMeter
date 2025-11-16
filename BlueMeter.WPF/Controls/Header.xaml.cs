using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlueMeter.WPF.Controls;

/// <summary>
/// Header.xaml 的交互逻辑
/// </summary>
public partial class Header : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(Header),
            new FrameworkPropertyMetadata("Header", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MinimizeToTrayCommandProperty =
        DependencyProperty.Register(
            nameof(MinimizeToTrayCommand),
            typeof(ICommand),
            typeof(Header),
            new PropertyMetadata(null));

    public Header()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand? MinimizeToTrayCommand
    {
        get => (ICommand?)GetValue(MinimizeToTrayCommandProperty);
        set => SetValue(MinimizeToTrayCommandProperty, value);
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            Window.GetWindow(this)?.DragMove();
    }
}