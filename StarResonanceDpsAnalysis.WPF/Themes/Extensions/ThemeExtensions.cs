using Microsoft.Extensions.DependencyInjection;
using StarResonanceDpsAnalysis.WPF.Themes.SystemThemes;

// ReSharper disable once CheckNamespace
namespace StarResonanceDpsAnalysis.WPF.Themes;

public static class ThemeExtensions
{
    public static void AddThemes(this IServiceCollection services)
    {
        services.AddSingleton<ApplicationThemeManager>();
        services.AddSingleton<ResourceDictionaryManager>();
        services.AddSingleton<SystemThemeWatcher>();
    }
}