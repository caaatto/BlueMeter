using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace BlueMeter.WPF.Behaviors.Checklist;

/// <summary>
/// Keyboard Navigation für Task Lists
/// Up/Down: Navigate zwischen Tasks
/// Enter/Space: Toggle Task Completion
/// Ctrl+F: Focus Search Box
/// </summary>
public class KeyboardNavigationBehavior : Behavior<ItemsControl>
{
    private int _currentIndex = -1;
    private FrameworkElement? _currentElement;

    /// <summary>
    /// Command zum Toggle von Tasks
    /// </summary>
    public static readonly DependencyProperty ToggleCommandProperty =
        DependencyProperty.Register(
            nameof(ToggleCommand),
            typeof(ICommand),
            typeof(KeyboardNavigationBehavior),
            new PropertyMetadata(null));

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    /// <summary>
    /// TextBox für Search (um Fokus zu setzen bei Ctrl+F)
    /// </summary>
    public static readonly DependencyProperty SearchBoxProperty =
        DependencyProperty.Register(
            nameof(SearchBox),
            typeof(TextBox),
            typeof(KeyboardNavigationBehavior),
            new PropertyMetadata(null));

    public TextBox? SearchBox
    {
        get => (TextBox?)GetValue(SearchBoxProperty);
        set => SetValue(SearchBoxProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.Focusable = true;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null)
        {
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Ctrl+F: Focus Search Box
        if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            SearchBox?.Focus();
            SearchBox?.SelectAll();
            e.Handled = true;
            return;
        }

        // Arrow Navigation
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

        // Toggle Task
        if ((e.Key == Key.Enter || e.Key == Key.Space) && _currentElement != null)
        {
            var task = _currentElement.DataContext;
            if (task != null && ToggleCommand?.CanExecute(task) == true)
            {
                ToggleCommand.Execute(task);
            }
            e.Handled = true;
            return;
        }
    }

    private void NavigateNext()
    {
        if (AssociatedObject == null) return;

        var items = GetVisibleItems();
        if (items.Count == 0) return;

        _currentIndex++;
        if (_currentIndex >= items.Count)
        {
            _currentIndex = 0; // Wrap around
        }

        FocusItem(items[_currentIndex]);
    }

    private void NavigatePrevious()
    {
        if (AssociatedObject == null) return;

        var items = GetVisibleItems();
        if (items.Count == 0) return;

        _currentIndex--;
        if (_currentIndex < 0)
        {
            _currentIndex = items.Count - 1; // Wrap around
        }

        FocusItem(items[_currentIndex]);
    }

    private List<FrameworkElement> GetVisibleItems()
    {
        var items = new List<FrameworkElement>();

        if (AssociatedObject == null) return items;

        for (int i = 0; i < AssociatedObject.Items.Count; i++)
        {
            var container = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
            if (container != null && container.Visibility == Visibility.Visible)
            {
                items.Add(container);
            }
        }

        return items;
    }

    private void FocusItem(FrameworkElement element)
    {
        _currentElement = element;

        // Remove focus from previous
        RemoveFocusVisual();

        // Set focus to new element
        element.Focus();

        // Optional: Add visual indicator
        AddFocusVisual(element);

        // Scroll into view
        element.BringIntoView();
    }

    private void AddFocusVisual(FrameworkElement element)
    {
        // Optional: Add a visual indicator border
        if (element is Border border)
        {
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = System.Windows.Media.Brushes.CornflowerBlue;
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
