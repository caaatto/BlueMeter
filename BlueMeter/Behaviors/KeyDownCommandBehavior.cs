using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace BlueMeter.Behaviors;

/// <summary>
/// Forwards TextBox key-down events to a command. The WPF version subscribed to
/// <c>PreviewKeyDown</c>; in Avalonia we subscribe to <c>KeyDownEvent</c> with
/// <see cref="RoutingStrategies.Tunnel"/> to preserve the preview semantics.
/// </summary>
public class KeyDownCommandBehavior : Behavior<TextBox>
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<KeyDownCommandBehavior, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, (EventHandler<KeyEventArgs>)OnPreviewKeyDown);
        base.OnDetaching();
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (Command?.CanExecute(e) == true)
        {
            Command.Execute(e);
        }
    }
}
