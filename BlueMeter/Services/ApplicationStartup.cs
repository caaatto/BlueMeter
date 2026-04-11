using BlueMeter.Config;
using BlueMeter.Core.Analyze;
using BlueMeter.Core.Data;
using BlueMeter.Localization;
using BlueMeter.Logging;
using BlueMeter.Models;
using BlueMeter.Services.Checklist;
using BlueMeter.WPF.Data; // IDataStorage lives under this legacy namespace inside BlueMeter.Core
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

/// <summary>
/// Centralized startup/shutdown plumbing. Wires up the things that need to run
/// once when the application boots — packet analyzer start, network adapter
/// selection, database init, checklist load, queue alert wiring — and the
/// matching teardown.
///
/// Port notes (WPF -> Avalonia):
///   - Pure namespace rewrite. The Avalonia counterparts (`IConfigManager`,
///     `IDeviceManagementService`, `IGlobalHotkeyService`, `IPacketAnalyzer`,
///     `IDataStorage`, `LocalizationManager`, `IChecklistService`,
///     `IChartDataService`, `IQueueAlertManager`, `IQueuePopUIDetector`) keep
///     identical method shapes.
///   - <c>WpfLogEvents</c> → <see cref="LogEvents"/>.
///   - <c>(ChecklistService)checklistService</c> cast still required because
///     <see cref="IChecklistService"/> doesn't expose <c>InitializeAsync</c> on
///     the interface — it lives on the concrete class.
///   - Invocation point moved out of <c>App.Main</c> (which doesn't exist for
///     Avalonia's classic-desktop lifetime) into
///     <c>App.OnFrameworkInitializationCompleted</c>. Shutdown is invoked from
///     <c>desktop.Exit</c> alongside the host disposal.
/// </summary>
public sealed class ApplicationStartup(
    ILogger<ApplicationStartup> logger,
    IConfigManager configManager,
    IDeviceManagementService deviceManagementService,
    IGlobalHotkeyService hotkeyService,
    IPacketAnalyzer packetAnalyzer,
    IDataStorage dataStorage,
    LocalizationManager localization,
    IChecklistService checklistService,
    IChartDataService chartDataService,
    IQueueAlertManager queueAlertManager,
    IQueuePopUIDetector queuePopUIDetector) : IApplicationStartup
{
    public async Task InitializeAsync()
    {
        try
        {
            logger.LogInformation(LogEvents.StartupInit, "Startup initialization started");

            localization.Initialize(configManager.CurrentConfig.Language);

            await TryFindBestNetworkAdapter();

            dataStorage.LoadPlayerInfoFromFile();

            // Initialize database for encounter history
            try
            {
                await DataStorageExtensions.InitializeDatabaseAsync(
                    dataStorage,
                    chartDataService: chartDataService,
                    autoCleanup: configManager.CurrentConfig.AutoDatabaseCleanup,
                    maxEncounters: configManager.CurrentConfig.MaxEncountersToKeep,
                    maxSizeMB: configManager.CurrentConfig.MaxDatabaseSizeMB);
                logger.LogInformation(LogEvents.StartupInit, "Database initialized successfully");

                // Preload player cache from database to reduce "Unknown" players
                await DataStorageExtensions.PreloadPlayerCacheAsync();
                logger.LogInformation(LogEvents.StartupInit, "Player cache preloaded from database");
            }
            catch (Exception dbEx)
            {
                logger.LogWarning(dbEx, "Database initialization failed, continuing without database features");
            }

            // Initialize checklist service
            try
            {
                await ((ChecklistService)checklistService).InitializeAsync();
                logger.LogInformation(LogEvents.StartupInit, "Checklist service initialized successfully");
            }
            catch (Exception checklistEx)
            {
                logger.LogWarning(checklistEx, "Checklist initialization failed, continuing without checklist features");
            }

            // Initialize queue alert manager
            try
            {
                if (dataStorage is DataStorageV2 dataStorageV2)
                {
                    queueAlertManager.Initialize(dataStorageV2);
                    logger.LogInformation(LogEvents.StartupInit, "Queue alert manager initialized successfully");
                }
            }
            catch (Exception queueEx)
            {
                logger.LogWarning(queueEx, "Queue alert manager initialization failed, continuing without queue alerts");
            }

            // Start queue pop UI detector (only if enabled in settings)
            try
            {
                if (configManager.CurrentConfig.QueuePopSoundEnabled)
                {
                    queuePopUIDetector.Start();
                    logger.LogInformation(LogEvents.StartupInit, "Queue pop UI detector started successfully");
                }
                else
                {
                    logger.LogInformation(LogEvents.StartupInit, "Queue pop UI detector disabled in settings");
                }
            }
            catch (Exception uiDetectorEx)
            {
                logger.LogWarning(uiDetectorEx, "Queue pop UI detector startup failed, continuing without UI detection");
            }

            packetAnalyzer.Start();
            logger.LogInformation(LogEvents.StartupInit, "Startup initialization completed");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Startup initialization encountered an issue");
            throw;
        }
    }

    private async Task TryFindBestNetworkAdapter()
    {
        var adapters = await deviceManagementService.GetNetworkAdaptersAsync();
        NetworkAdapterInfo? target = null;
        var pref = configManager.CurrentConfig.PreferredNetworkAdapter;
        if (pref != null)
        {
            var match = adapters.FirstOrDefault(a => a.name == pref.Name);
            if (!match.Equals(default((string name, string description))))
            {
                target = new NetworkAdapterInfo(match.name, match.description);
            }
        }

        // If preferred not found, try automatic selection via routing
        target ??= await deviceManagementService.GetAutoSelectedNetworkAdapterAsync();

        target ??= adapters.Count > 0
            ? new NetworkAdapterInfo(adapters[0].name, adapters[0].description)
            : null;

        if (target != null)
        {
            logger.LogInformation(LogEvents.StartupAdapter, "Activating adapter: {Name}", target.Name);
            deviceManagementService.SetActiveNetworkAdapter(target);
            configManager.CurrentConfig.PreferredNetworkAdapter = target;
            _ = configManager.SaveAsync();
        }
        else
        {
            logger.LogWarning(LogEvents.StartupAdapter, "No adapters available for activation");
        }
    }

    public void Shutdown()
    {
        try
        {
            logger.LogInformation(LogEvents.Shutdown, "Application shutdown");
            deviceManagementService.StopActiveCapture();
            packetAnalyzer.Stop();
            hotkeyService.Stop();
            queuePopUIDetector.Stop();
            dataStorage.SavePlayerInfoToFile();

            DataStorageExtensions.Shutdown();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Shutdown encountered an issue");
        }
    }
}
