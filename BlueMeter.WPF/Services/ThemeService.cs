using System.ComponentModel;
using System.Runtime.CompilerServices;
using BlueMeter.WPF.Config;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.WPF.Services;

/// <summary>
/// Service to manage theme-related properties like app name based on current theme.
/// Integrates with AppConfig to provide reactive theme updates.
/// </summary>
public class ThemeService : ObservableObject
{
    private string _appName = "BlueMeter";
    private string _themeColor = "#1690F8";

    public string AppName
    {
        get => _appName;
        set => SetProperty(ref _appName, value);
    }

    public string ThemeColor
    {
        get => _themeColor;
        set
        {
            if (SetProperty(ref _themeColor, value))
            {
                // Update app name when theme changes
                AppName = ThemeDefinitions.GetAppName(value);
            }
        }
    }

    /// <summary>
    /// Initialize theme service with current config
    /// </summary>
    public void Initialize(string currentThemeColor)
    {
        _themeColor = currentThemeColor;
        _appName = ThemeDefinitions.GetAppName(currentThemeColor);
        OnPropertyChanged(nameof(ThemeColor));
        OnPropertyChanged(nameof(AppName));
    }
}
