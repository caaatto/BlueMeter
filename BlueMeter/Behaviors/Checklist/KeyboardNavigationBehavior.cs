using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;

namespace BlueMeter.Behaviors.Checklist;

/// <summary>
/// Keyboard navigation for task lists.
///
/// <list type="bullet">
///   <item><description>Up / Down: move between items.</description></item>
///   <item><description>Enter / Space: toggle the focused task.</description></item>
///   <item><description>Ctrl+F: focus the search <see cref="SearchBox"/>.</description></item>
/// </list>
///
/// Port notes: WPF's <c>PreviewKeyDown</c> + <c>Keyboard.Modifiers</c> become
/// Avalonia's tunneling <see cref="InputElement.KeyDownEvent"/> and
/// <see cref="KeyEventArgs.KeyModifiers"/>. <c>FrameworkElement</c> targets
/// become <see cref="Control"/>.
/// </summary>
public class KeyboardNavigationBehavior : Behavior<ItemsControl>
{
    private int _currentIndex = -1;
    private Control? _currentElement;

    public static readonly StyledProperty<ICommand?> ToggleCommandProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, ICommand?>(nameof(ToggleCommand));

    public ICommand? ToggleCommand
    {
        get => GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public static readonly StyledProperty<TextBox?> SearchBoxProperty =
        AvaloniaProperty.Register<KeyboardNavigationBehavior, TextBox?>(nameof(SearchBox));

    public TextBox? SearchBox
    {
        get => GetValue(SearchBoxProperty);
        set => SetValue(SearchBoxProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        AssociatedObject.Focusable = true;
    }

    protected override void OnDetaching()
    {
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, (EventHandler<KeyEventArgs>)OnPreviewKeyDown);
        base.OnDetaching();
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+F: focus the search box.
        if (e.Key == Key.F && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            SearchBox?.Focus();
            SearchBox?.SelectAll();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Down)
        {
            NavigateNext();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            NavigatePrevious();
            e.Handled = true;
            return;
        }

        if ((e.Key == Key.Enter || e.Key == Key.Space) && _currentElement is not null)
        {
            var task = _currentElement.DataContext;
            if (task is not null && ToggleCommand?.CanExecute(task) == true)
            {
                ToggleCommand.Execute(task);
            }

            e.Handled = true;
        }
    }

    private void NavigateNext()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        var items = GetVisibleItems();
        if (items.Count == 0)
        {
            return;
        }

        _currentIndex++;
        if (_currentIndex >= items.Count)
        {
            _currentIndex = 0;
        }

        FocusItem(items[_currentIndex]);
    }

    private void NavigatePrevious()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        var items = GetVisibleItems();
        if (items.Count == 0)
        {
            return;
        }

        _currentIndex--;
        if (_currentIndex < 0)
        {
            _currentIndex = items.Count - 1;
        }

        FocusItem(items[_currentIndex]);
    }

    private List<Control> GetVisibleItems()
    {
        var items = new List<Control>();

        if (AssociatedObject is null)
        {
            return items;
        }

        for (var i = 0; i < AssociatedObject.ItemCount; i++)
        {
            if (AssociatedObject.ContainerFromIndex(i) is Control { IsVisible: true } container)
            {
                items.Add(container);
            }
        }

        return items;
    }

    private void FocusItem(Control element)
    {
        _currentElement = element;

        RemoveFocusVisual();
        element.Focus();
        AddFocusVisual(element);
        element.BringIntoView();
    }

    private static void AddFocusVisual(Control element)
    {
        if (element is Border border)
        {
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.CornflowerBlue;
        }
    }

    private void RemoveFocusVisual()
    {
        if (_currentElement is Border border)
        {
            border.BorderThickness = new Thickness(0);
        }
    }
}
