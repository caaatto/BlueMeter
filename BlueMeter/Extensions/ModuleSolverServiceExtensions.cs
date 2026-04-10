using BlueMeter.Services.ModuleSolver;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.Extensions;

/// <summary>
/// Extension-Methoden für ModuleSolver-Service-Registrierung.
///
/// The WPF version also registered <c>ModuleSolveViewModel</c> here; it is
/// deferred until the Phase 5 view-model port lands.
/// </summary>
public static class ModuleSolverServiceExtensions
{
    /// <summary>
    /// Registriert alle ModuleSolver-Services im DI-Container
    /// </summary>
    public static IServiceCollection AddModuleSolverServices(this IServiceCollection services)
    {
        services.AddSingleton<PacketCaptureService>();
        services.AddSingleton<ModuleOptimizerService>();
        services.AddSingleton<ModulePersistenceService>();
        services.AddSingleton<ModuleOCRCaptureService>();

        return services;
    }
}
