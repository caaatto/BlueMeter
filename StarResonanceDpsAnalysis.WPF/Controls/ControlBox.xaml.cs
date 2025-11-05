using System.Windows;
using System.Windows.Controls;

namespace StarResonanceDpsAnalysis.WPF.Controls;

/// <summary>
/// ControlBox.xaml 的交互逻辑
/// </summary>
public partial class ControlBox : UserControl
{
    public const double BUTTON_WIDTH = 50;

    public static readonly DependencyProperty UseMinimizeButtonProperty =
        DependencyProperty.Register(
            nameof(UseMinimizeButton),
            typeof(bool),
            typeof(ControlBox),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty UseMaximizeButtonProperty =
        DependencyProperty.Register(
            nameof(UseMaximizeButton),
            typeof(bool),
            typeof(ControlBox),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public ControlBox()
    {
        InitializeComponent();

        DisableWidthHeight();
    }

    public bool UseMinimizeButton
    {
        get => (bool)GetValue(UseMinimizeButtonProperty);
        set => SetValue(UseMinimizeButtonProperty, value);
    }

    public bool UseMaximizeButton
    {
        get => (bool)GetValue(UseMaximizeButtonProperty);
        set => SetValue(UseMaximizeButtonProperty, value);
    }

    private void DisableWidthHeight()
    {
        WidthProperty.OverrideMetadata(typeof(ControlBox),
            new FrameworkPropertyMetadata(double.NaN,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                null,
                (_, _) => BUTTON_WIDTH * (1 + (UseMinimizeButton ? 1 : 0) + (UseMaximizeButton ? 1 : 0))));

        HeightProperty.OverrideMetadata(typeof(ControlBox),
            new FrameworkPropertyMetadata(double.NaN,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                null,
                (_, _) => double.NaN));
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window == null)
        {
            return;
        }

        window.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window == null)
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}