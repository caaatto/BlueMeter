using System.Diagnostics;
using BlueMeter.WPF.Themes.SystemThemes;

namespace BlueMeter.WPF.Themes;

public class ApplicationThemeManager
{
    // internal const string LibraryNamespace = "BlueMeter.WPF";
    internal const string LibraryNamespace = "BlueMeter.WPF";

    // include assembly name and component segment so pack URIs resolve in designer
    internal const string ThemesDictionaryPath = "pack://application:,,,/" + LibraryNamespace + ";component/Themes/";
    private readonly ResourceDictionaryManager _appDictionaries = new(LibraryNamespace);
    private ApplicationTheme _cachedApplicationTheme = ApplicationTheme.Unknown;

    /// <summary>
    ///     Event triggered when the application's theme is changed.
    /// </summary>
    public event ThemeChangedEvent? Changed;

    /// <summary>
    ///     Changes the current application theme.
    /// </summary>
    /// <param name="applicationTheme">Theme to set.</param>
    public void Apply(
        ApplicationTheme applicationTheme
    )
    {
        var themeDictionaryName = applicationTheme switch
        {
            ApplicationTheme.Dark => "Dark",
            ApplicationTheme.Light => "Light",
            _ => throw new ArgumentOutOfRangeException(nameof(applicationTheme), applicationTheme, null)
        };

        var isUpdated = _appDictionaries.UpdateDictionary(
            "theme",
            new Uri(ThemesDictionaryPath + themeDictionaryName + ".xaml", UriKind.Absolute)
        );

#if DEBUG
        Debug.WriteLine(
            $"INFO | {typeof(ApplicationThemeManager)} tries to update theme to {themeDictionaryName} ({applicationTheme}): {isUpdated}",
            nameof(ApplicationThemeManager)
        );
#endif
        if (!isUpdated)
        {
            return;
        }

        _cachedApplicationTheme = applicationTheme;

        Changed?.Invoke(applicationTheme);
    }

    public void ApplySystemTheme()
    {
        SystemThemeManager.UpdateSystemThemeCache();

        var systemTheme = GetSystemTheme();

        var themeToSet = ApplicationTheme.Light;

        if (systemTheme is SystemTheme.Dark)
        {
            themeToSet = ApplicationTheme.Dark;
        }

        Apply(themeToSet);
    }

    /// <summary>
    ///     Gets currently set application theme.
    /// </summary>
    /// <returns><see cref="ApplicationTheme.Unknown" /> if something goes wrong.</returns>
    public ApplicationTheme GetAppTheme()
    {
        if (_cachedApplicationTheme == ApplicationTheme.Unknown)
        {
            FetchApplicationTheme();
        }

        return _cachedApplicationTheme;
    }

    /// <summary>
    ///     Tries to guess the currently set application theme.
    /// </summary>
    private void FetchApplicationTheme()
    {
        var themeDictionary = _appDictionaries.GetDictionary("theme");

        if (themeDictionary == null)
        {
            return;
        }

        var themeUri = themeDictionary.Source.ToString().Trim().ToLower();

        if (themeUri.Contains("light"))
        {
            _cachedApplicationTheme = ApplicationTheme.Light;
        }

        if (themeUri.Contains("dark"))
        {
            _cachedApplicationTheme = ApplicationTheme.Dark;
        }
    }

    /// <summary>
    /// Gets currently set system theme.
    /// </summary>
    /// <returns><see cref="SystemTheme.Unknown"/> if something goes wrong.</returns>
    public SystemTheme GetSystemTheme()
    {
        return SystemThemeManager.GetCachedSystemTheme();
    }
}