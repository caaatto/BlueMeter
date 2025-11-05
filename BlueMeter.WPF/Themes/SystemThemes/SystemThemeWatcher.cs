using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using BlueMeter.WPF.Interop;
using BlueMeter.WPF.Themes.Functions;

namespace BlueMeter.WPF.Themes.SystemThemes;

/// <summary>
/// Automatically updates the application background if the system theme or color is changed.
/// <para><see cref="SystemThemeWatcher"/> settings work globally and cannot be changed for each <see cref="System.Windows.Window"/>.</para>
/// </summary>
/// <example>
/// <code lang="csharp">
/// SystemThemeWatcher.Watch(this as System.Windows.Window);
/// SystemThemeWatcher.UnWatch(this as System.Windows.Window);
/// </code>
/// <code lang="csharp">
/// SystemThemeWatcher.Watch(
///     _serviceProvider.GetRequiredService&lt;MainView&gt;()
/// );
/// </code>
/// </example>
public class SystemThemeWatcher(ApplicationThemeManager applicationThemeManager)
{
    private readonly ICollection<ObservedWindow> _observedWindows = new List<ObservedWindow>();

    /// <summary>
    /// Watches the <see cref="Window"/> and applies the background effect and theme according to the system theme.
    /// </summary>
    /// <param name="window">The window that will be updated.</param>
    /// <param name="forceBackgroundReplace">If <see langword="true"/>, bypasses the app's theme compatibility check and tries to force the change of a background effect.</param>
    public void Watch(Window? window,
        bool forceBackgroundReplace = false)
    {
        if (window is null)
        {
            return;
        }

        if (window.IsLoaded)
        {
            ObserveLoadedWindow(window, forceBackgroundReplace);
        }
        else
        {
            ObserveWindowWhenLoaded(window, forceBackgroundReplace);
        }

        if (!_observedWindows.Any())
        {
#if DEBUG
            Debug.WriteLine(
                $"INFO | {typeof(SystemThemeWatcher)} changed the app theme on initialization.",
                nameof(SystemThemeWatcher)
            );
#endif
            applicationThemeManager.ApplySystemTheme();
        }
    }

    private void ObserveLoadedWindow(Window window,
        bool forceBackgroundReplace)
    {
        IntPtr hWnd =
            (hWnd = new WindowInteropHelper(window).Handle) == IntPtr.Zero
                ? throw new InvalidOperationException("Could not get window handle.")
                : hWnd;

        if (hWnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("Window handle cannot be empty");
        }

        ObserveLoadedHandle(new ObservedWindow(hWnd, forceBackgroundReplace));
    }

    private void ObserveWindowWhenLoaded(Window window,
        bool forceBackgroundReplace)
    {
        window.Loaded += (_, _) =>
        {
            IntPtr hWnd =
                (hWnd = new WindowInteropHelper(window).Handle) == IntPtr.Zero
                    ? throw new InvalidOperationException("Could not get window handle.")
                    : hWnd;

            if (hWnd == IntPtr.Zero)
            {
                throw new InvalidOperationException("Window handle cannot be empty");
            }

            ObserveLoadedHandle(new ObservedWindow(hWnd, forceBackgroundReplace));
        };
    }

    private void ObserveLoadedHandle(ObservedWindow observedWindow)
    {
        if (!observedWindow.HasHook)
        {
#if DEBUG
            Debug.WriteLine(
                $"INFO | {observedWindow.Handle} ({observedWindow.RootVisual?.Title}) registered as watched window.",
                nameof(SystemThemeWatcher)
            );
#endif
            observedWindow.AddHook(WndProc);
            _observedWindows.Add(observedWindow);
        }
    }

    /// <summary>
    /// Unwatches the window and removes the hook to receive messages from the system.
    /// </summary>
    public void UnWatch(Window? window)
    {
        if (window is null)
        {
            return;
        }

        if (!window.IsLoaded)
        {
            throw new InvalidOperationException("You cannot unwatch a window that is not yet loaded.");
        }

        IntPtr hWnd =
            (hWnd = new WindowInteropHelper(window).Handle) == IntPtr.Zero
                ? throw new InvalidOperationException("Could not get window handle.")
                : hWnd;

        var observedWindow = _observedWindows.FirstOrDefault(x => x.Handle == hWnd);

        if (observedWindow is null)
        {
            return;
        }

        observedWindow.RemoveHook(WndProc);

        _ = _observedWindows.Remove(observedWindow);
    }

    /// <summary>
    /// Listens to system messages on the application windows.
    /// </summary>
    private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == (int)User32.WM.WININICHANGE)
        {
            UpdateObservedWindow(hWnd);
        }

        return IntPtr.Zero;
    }

    private void UpdateObservedWindow(nint hWnd)
    {
        if (!UnsafeNativeMethods.IsValidWindow(hWnd))
        {
            return;
        }

        var observedWindow = _observedWindows.FirstOrDefault(x => x.Handle == hWnd);

        if (observedWindow is null)
        {
            return;
        }

        applicationThemeManager.ApplySystemTheme();
        var currentApplicationTheme = applicationThemeManager.GetAppTheme();

#if DEBUG
        Debug.WriteLine(
            $"INFO | {observedWindow.Handle} ({observedWindow.RootVisual?.Title}) triggered the application theme change to {applicationThemeManager.GetSystemTheme()}.",
            nameof(SystemThemeWatcher)
        );
#endif

        if (observedWindow.RootVisual is not null)
        {
            WindowBackgroundManager.UpdateBackground(
                observedWindow.RootVisual,
                currentApplicationTheme,
                observedWindow.ForceBackgroundReplace
            );
        }
    }
}