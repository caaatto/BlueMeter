using BlueMeter.WPF.Services.Checklist;
using BlueMeter.WPF.ViewModels.Checklist;
using BlueMeter.WPF.Views.Checklist;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.WPF.Extensions;

/// <summary>
/// Extension-Methoden für Checklist-Service-Registrierung
/// </summary>
public static class ChecklistServiceExtensions
{
    /// <summary>
    /// Registriert alle Checklist-Services im DI-Container
    /// </summary>
    public static IServiceCollection AddChecklistServices(this IServiceCollection services)
    {
        // Services als Singleton (persistent während App-Lifetime)
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<ITimerService, TimerService>();
        services.AddSingleton<IChecklistService, ChecklistService>();

        // ViewModel als Singleton (wird nur einmal erstellt)
        services.AddSingleton<ChecklistViewModel>();

        // Window als Transient (neue Instanz bei jedem Aufruf)
        services.AddTransient<ChecklistWindow>();

        return services;
    }
}
