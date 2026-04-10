using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace BlueMeter.Controls;

/// <summary>
/// Header with title text and a minimize-to-tray button. The header acts as a
/// drag handle for its host <see cref="Window"/>.
///
/// Port notes: WPF used <c>Window.GetWindow(this)?.DragMove()</c> on
/// <c>MouseLeftButtonDown</c>. Avalonia exposes
/// <see cref="Window.BeginMoveDrag"/>, which takes the triggering
/// <see cref="PointerPressedEventArgs"/> directly.
/// </summary>
public partial class Header : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<Header, string>(nameof(Title), "Header");

    public static readonly StyledProperty<ICommand?> MinimizeToTrayCommandProperty =
        AvaloniaProperty.Register<Header, ICommand?>(nameof(MinimizeToTrayCommand));

    public Header()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand? MinimizeToTrayCommand
    {
        get => GetValue(MinimizeToTrayCommandProperty);
        set => SetValue(MinimizeToTrayCommandProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (this.GetVisualRoot() is Window window)
        {
            window.BeginMoveDrag(e);
        }
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        // If a command is bound, the command path runs instead of this fallback.
        if (MinimizeToTrayCommand is not null)
        {
            return;
        }

        if (this.GetVisualRoot() is Window window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }
}
