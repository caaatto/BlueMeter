using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace BlueMeter.Controls;

/// <summary>
/// Minimize / Maximize / Close buttons used by chromeless windows. The
/// minimize button can optionally invoke a <see cref="MinimizeToTrayCommand"/>
/// for main-window tray behavior; otherwise it falls back to
/// <see cref="WindowState.Minimized"/>.
/// </summary>
public partial class ControlBox : UserControl
{
    public const double BUTTON_WIDTH = 50;

    public static readonly StyledProperty<bool> UseMinimizeButtonProperty =
        AvaloniaProperty.Register<ControlBox, bool>(
            nameof(UseMinimizeButton),
            defaultValue: true,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<bool> UseMaximizeButtonProperty =
        AvaloniaProperty.Register<ControlBox, bool>(
            nameof(UseMaximizeButton),
            defaultValue: true,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<ICommand?> MinimizeToTrayCommandProperty =
        AvaloniaProperty.Register<ControlBox, ICommand?>(nameof(MinimizeToTrayCommand));

    public ControlBox()
    {
        InitializeComponent();
    }

    public bool UseMinimizeButton
    {
        get => GetValue(UseMinimizeButtonProperty);
        set => SetValue(UseMinimizeButtonProperty, value);
    }

    public bool UseMaximizeButton
    {
        get => GetValue(UseMaximizeButtonProperty);
        set => SetValue(UseMaximizeButtonProperty, value);
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UseMinimizeButtonProperty || change.Property == UseMaximizeButtonProperty)
        {
            var minBtn = this.FindControl<Button>("MinimizeButton");
            var maxBtn = this.FindControl<Button>("MaximizeButton");
            if (minBtn is not null)
            {
                minBtn.IsVisible = UseMinimizeButton;
            }
            if (maxBtn is not null)
            {
                maxBtn.IsVisible = UseMaximizeButton;
            }
        }
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (MinimizeToTrayCommand is not null && MinimizeToTrayCommand.CanExecute(null))
        {
            MinimizeToTrayCommand.Execute(null);
            return;
        }

        if (this.GetVisualRoot() is Window window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (this.GetVisualRoot() is not Window window)
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (this.GetVisualRoot() is Window window)
        {
            window.Close();
        }
    }
}
