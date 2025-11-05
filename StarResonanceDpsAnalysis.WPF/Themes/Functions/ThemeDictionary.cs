using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Themes;

/// <summary>
/// Themes dictionary, this class is inspired by WPF-UI
/// </summary>
public class ThemesDictionary : ResourceDictionary
{
    public ThemesDictionary()
    {
        SetSourceBasedOnSelectedTheme(ApplicationTheme.Light);
    }

    /// <summary>
    ///     Sets the default application theme.
    /// </summary>
    public ApplicationTheme Theme
    {
        set => SetSourceBasedOnSelectedTheme(value);
    }

    private void SetSourceBasedOnSelectedTheme(ApplicationTheme? selectedApplicationTheme)
    {
        var themeName = selectedApplicationTheme switch
        {
            ApplicationTheme.Dark => "Dark",
            _ => "Light"
        };

        Source = new Uri($"{ApplicationThemeManager.ThemesDictionaryPath}{themeName}.xaml", UriKind.Absolute);
    }
}