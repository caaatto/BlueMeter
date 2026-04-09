using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace BlueMeter.Behaviors;

/// <summary>
/// Opens a <see cref="Button.ContextMenu"/> (if any) when the button is clicked.
/// </summary>
public class OpenContextMenuBehavior : Behavior<Button>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not null)
        {
            AssociatedObject.Click += OnButtonClick;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.Click -= OnButtonClick;
        }

        base.OnDetaching();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject?.ContextMenu is { } menu)
        {
            menu.PlacementTarget = AssociatedObject;
            menu.Open(AssociatedObject);
            e.Handled = true;
        }
    }
}
