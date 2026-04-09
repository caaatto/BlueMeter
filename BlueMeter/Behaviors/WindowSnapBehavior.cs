using Avalonia;
using Avalonia.Controls;

namespace BlueMeter.Behaviors;

/// <summary>
/// Attached property that prevents a <see cref="Window"/> from being snap-maximized
/// by Windows 11 Snap Layouts / aero drag-to-top. Whenever the window's state flips
/// to <see cref="WindowState.Maximized"/> it is restored to
/// <see cref="WindowState.Normal"/> on the next dispatcher tick.
///
/// Port notes: the WPF version hooked <c>Window.StateChanged</c>. Avalonia's
/// <see cref="Window"/> has no <c>StateChanged</c> event, so we subscribe to
/// <see cref="AvaloniaObject.PropertyChanged"/> and filter on
/// <see cref="Window.WindowStateProperty"/>.
/// </summary>
public static class WindowSnapBehavior
{
    public static readonly AttachedProperty<bool> PreventSnapMaximizeProperty =
        AvaloniaProperty.RegisterAttached<Window, bool>(
            "PreventSnapMaximize",
            typeof(WindowSnapBehavior));

    private static readonly AttachedProperty<PreventSnapHandler?> PreventSnapHandlerProperty =
        AvaloniaProperty.RegisterAttached<Window, PreventSnapHandler?>(
            "PreventSnapHandler",
            typeof(WindowSnapBehavior));

    static WindowSnapBehavior()
    {
        PreventSnapMaximizeProperty.Changed.AddClassHandler<Window>(OnPreventSnapMaximizeChanged);
    }

    public static bool GetPreventSnapMaximize(Window window)
    {
        return window.GetValue(PreventSnapMaximizeProperty);
    }

    public static void SetPreventSnapMaximize(Window window, bool value)
    {
        window.SetValue(PreventSnapMaximizeProperty, value);
    }

    private static void OnPreventSnapMaximizeChanged(Window window, AvaloniaPropertyChangedEventArgs e)
    {
        if (Equals(e.NewValue, e.OldValue))
        {
            return;
        }

        if (e.NewValue is true)
        {
            if (window.GetValue(PreventSnapHandlerProperty) is not null)
            {
                return;
            }

            window.SetValue(PreventSnapHandlerProperty, new PreventSnapHandler(window));
        }
        else
        {
            if (window.GetValue(PreventSnapHandlerProperty) is { } handler)
            {
                handler.Detach();
                window.ClearValue(PreventSnapHandlerProperty);
            }
        }
    }

    private sealed class PreventSnapHandler
    {
        private readonly Window _window;
        private bool _suppress;

        public PreventSnapHandler(Window window)
        {
            _window = window;
            _window.PropertyChanged += OnWindowPropertyChanged;
        }

        private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_suppress || e.Property != Window.WindowStateProperty)
            {
                return;
            }

            if (_window.WindowState != WindowState.Maximized)
            {
                return;
            }

            _suppress = true;
            _window.WindowState = WindowState.Normal;
            _suppress = false;
        }

        public void Detach()
        {
            _window.PropertyChanged -= OnWindowPropertyChanged;
        }
    }
}
