using System;
using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Behaviors;

public static class WindowSnapBehavior
{
    public static readonly DependencyProperty PreventSnapMaximizeProperty = DependencyProperty.RegisterAttached(
        "PreventSnapMaximize",
        typeof(bool),
        typeof(WindowSnapBehavior),
        new PropertyMetadata(false, OnPreventSnapMaximizeChanged));

    private static readonly DependencyProperty PreventSnapHandlerProperty = DependencyProperty.RegisterAttached(
        "PreventSnapHandler",
        typeof(PreventSnapHandler),
        typeof(WindowSnapBehavior),
        new PropertyMetadata(null));

    public static bool GetPreventSnapMaximize(Window window)
    {
        return (bool)window.GetValue(PreventSnapMaximizeProperty);
    }

    public static void SetPreventSnapMaximize(Window window, bool value)
    {
        window.SetValue(PreventSnapMaximizeProperty, value);
    }

    private static void OnPreventSnapMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
        {
            return;
        }

        if (Equals(e.NewValue, e.OldValue))
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            if (window.GetValue(PreventSnapHandlerProperty) is PreventSnapHandler)
            {
                return;
            }

            var handler = new PreventSnapHandler(window);
            window.SetValue(PreventSnapHandlerProperty, handler);
        }
        else
        {
            if (window.GetValue(PreventSnapHandlerProperty) is PreventSnapHandler handler)
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
            _window.StateChanged += WindowOnStateChanged;
        }

        private void WindowOnStateChanged(object? sender, EventArgs e)
        {
            if (_suppress) return;
            if (_window.WindowState != WindowState.Maximized) return;

            _suppress = true;
            _window.WindowState = WindowState.Normal;
            _suppress = false;
        }

        public void Detach()
        {
            _window.StateChanged -= WindowOnStateChanged;
        }
    }
}
