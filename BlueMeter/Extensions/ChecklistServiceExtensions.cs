using BlueMeter.Services.Checklist;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.Extensions;

/// <summary>
/// Extension-Methoden für Checklist-Service-Registrierung.
///
/// The WPF version also registered <c>ChecklistViewModel</c> and
/// <c>ChecklistWindow</c> here; both are deferred until Phase 5 view-model +
/// Phase 10 view ports land. When those arrive, add them back alongside the
/// three service registrations below.
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

        return services;
    }
}
