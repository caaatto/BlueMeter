using Avalonia;
using Avalonia.Styling;

namespace BlueMeter.Themes;

/// <summary>
/// Avalonia replacement for the WPF <c>ApplicationThemeManager</c>. The original
/// swapped <c>ResourceDictionary</c>s in <c>Application.Current.Resources.MergedDictionaries</c>
/// to switch between dark and light themes; Avalonia exposes the same capability
/// natively through <see cref="Application.RequestedThemeVariant"/> + the
/// <c>ThemeDictionaries</c> already wired in <c>App.axaml</c> (Phase 8). This class
/// keeps the WPF API surface (<see cref="GetAppTheme"/>, <see cref="Apply"/>,
/// <see cref="Changed"/>) so view models can be ported without behavioral changes.
/// </summary>
public class ApplicationThemeManager
{
    /// <summary>
    /// Event triggered when the application's theme is changed.
    /// </summary>
    public event ThemeChangedEvent? Changed;

    /// <summary>
    /// Changes the current application theme.
    /// </summary>
    /// <param name="applicationTheme">Theme to set.</param>
    public void Apply(ApplicationTheme applicationTheme)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var variant = applicationTheme switch
        {
            ApplicationTheme.Dark => ThemeVariant.Dark,
            ApplicationTheme.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };

        if (app.RequestedThemeVariant == variant)
        {
            return;
        }

        app.RequestedThemeVariant = variant;
        Changed?.Invoke(applicationTheme);
    }

    /// <summary>
    /// Applies the OS-level system theme. Avalonia surfaces this via
    /// <see cref="ThemeVariant.Default"/>, which automatically follows the system
    /// preference when no explicit variant is requested.
    /// </summary>
    public void ApplySystemTheme()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.RequestedThemeVariant = ThemeVariant.Default;
        Changed?.Invoke(GetAppTheme());
    }

    /// <summary>
    /// Gets the currently set application theme.
    /// </summary>
    /// <returns><see cref="ApplicationTheme.Unknown"/> if no theme can be resolved.</returns>
    public ApplicationTheme GetAppTheme()
    {
        var app = Application.Current;
        if (app is null)
        {
            return ApplicationTheme.Unknown;
        }

        var variant = app.ActualThemeVariant;
        if (variant == ThemeVariant.Dark)
        {
            return ApplicationTheme.Dark;
        }
        if (variant == ThemeVariant.Light)
        {
            return ApplicationTheme.Light;
        }
        return ApplicationTheme.Unknown;
    }
}
