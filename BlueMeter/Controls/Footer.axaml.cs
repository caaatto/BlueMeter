using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace BlueMeter.Controls;

/// <summary>
/// Footer with OK / Cancel buttons. Consumers can either wire the
/// <see cref="ConfirmCommand"/> / <see cref="CancelCommand"/> styled properties
/// or subscribe to the <see cref="ConfirmClick"/> / <see cref="CancelClick"/>
/// events — both paths run when a button is clicked.
/// </summary>
public partial class Footer : UserControl
{
    public static readonly StyledProperty<ICommand?> ConfirmCommandProperty =
        AvaloniaProperty.Register<Footer, ICommand?>(nameof(ConfirmCommand));

    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<Footer, ICommand?>(nameof(CancelCommand));

    public Footer()
    {
        InitializeComponent();
    }

    public ICommand? ConfirmCommand
    {
        get => GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? ConfirmClick;
    public event EventHandler<RoutedEventArgs>? CancelClick;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        ConfirmClick?.Invoke(sender, e);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelClick?.Invoke(sender, e);
    }
}
