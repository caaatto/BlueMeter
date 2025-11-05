using System.Windows;
using BlueMeter.WPF.Interop;

namespace BlueMeter.WPF.Themes.Functions;

/// <summary>
/// Facilitates the management of the window background.
/// </summary>
/// <example>
/// <code lang="csharp">
/// WindowBackgroundManager.UpdateBackground(
///     observedWindow.RootVisual,
///     currentApplicationTheme,
///     observedWindow.Backdrop,
///     observedWindow.ForceBackgroundReplace
/// );
/// </code>
/// </example>
public static class WindowBackgroundManager
{
    /// <summary>
    /// Tries to apply dark theme to <see cref="Window"/>.
    /// </summary>
    public static void ApplyDarkThemeToWindow(Window? window)
    {
        if (window is null)
        {
            return;
        }

        if (window.IsLoaded)
        {
            _ = UnsafeNativeMethods.ApplyWindowDarkMode(window);
        }

        window.Loaded += (sender, _) => UnsafeNativeMethods.ApplyWindowDarkMode(sender as Window);
    }

    /// <summary>
    /// Tries to remove dark theme from <see cref="Window"/>.
    /// </summary>
    public static void RemoveDarkThemeFromWindow(Window? window)
    {
        if (window is null)
        {
            return;
        }

        if (window.IsLoaded)
        {
            _ = UnsafeNativeMethods.RemoveWindowDarkMode(window);
        }

        window.Loaded += (sender, _) => UnsafeNativeMethods.RemoveWindowDarkMode(sender as Window);
    }

    /// <summary>
    /// Forces change to application background. Required if custom background effect was previously applied.
    /// </summary>
    public static void UpdateBackground(Window? window,
        ApplicationTheme applicationTheme,
        bool forceBackground)
    {
        if (window is null)
        {
            return;
        }

        if (applicationTheme is ApplicationTheme.Dark)
        {
            ApplyDarkThemeToWindow(window);
        }
        else
        {
            RemoveDarkThemeFromWindow(window);
        }

        foreach (var subWindow in window.OwnedWindows)
        {
            if (subWindow is Window windowSubWindow)
            {
                if (applicationTheme is ApplicationTheme.Dark)
                {
                    ApplyDarkThemeToWindow(windowSubWindow);
                }
                else
                {
                    RemoveDarkThemeFromWindow(windowSubWindow);
                }
            }
        }
    }
}