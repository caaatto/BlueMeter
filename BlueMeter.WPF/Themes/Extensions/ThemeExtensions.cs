using Microsoft.Extensions.DependencyInjection;
using BlueMeter.WPF.Themes.SystemThemes;

// ReSharper disable once CheckNamespace
namespace BlueMeter.WPF.Themes;

public static class ThemeExtensions
{
    public static void AddThemes(this IServiceCollection services)
    {
        services.AddSingleton<ApplicationThemeManager>();
        services.AddSingleton<ResourceDictionaryManager>();
        services.AddSingleton<SystemThemeWatcher>();
    }
}