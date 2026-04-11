using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using BlueMeter.Config;
using BlueMeter.Extensions;
using BlueMeter.Localization;
using BlueMeter.Logging;
using BlueMeter.Plugins;
using BlueMeter.Services;
using BlueMeter.Styles.Classes;
using BlueMeter.Themes;
using BlueMeter.ViewModels;
using BlueMeter.Views;
using BlueMeter.Views.Checklist;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SharpPcap;

namespace BlueMeter;

public partial class App : Application
{
    private static ILogger<App>? _logger;
    private static IObservable<LogEvent>? _logStream;

    /// <summary>
    /// Generic host that owns DI, configuration, and logging for the whole app.
    /// Built in <see cref="OnFrameworkInitializationCompleted"/> after Avalonia is ready.
    /// </summary>
    public static IHost? Host { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var configRoot = BuildConfiguration();
        _logStream = ConfigureLogging(configRoot);

        Host = CreateHostBuilder(configRoot).Build();
        _logger = Host.Services.GetRequiredService<ILogger<App>>();

        _logger.LogInformation(LogEvents.AppStarting, "Application starting");

        // Populate the per-class profession icon resources. Must run before the main
        // window is constructed so ClassesToIconConverter can resolve the lookups.
        ClassIconResources.Register(this);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Host.Services.GetRequiredService<MainView>();
            desktop.Exit += OnDesktopExit;
        }

        // Start hotkey service after main window is created so the registered
        // window handle is available for RegisterHotKey. Same ordering as WPF.
        try
        {
            var hotkeyService = Host.Services.GetRequiredService<IGlobalHotkeyService>();
            hotkeyService.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GlobalHotkeyService.Start failed");
        }

        // Start chart data sampling for live DPS/HPS plots.
        try
        {
            var chartDataService = Host.Services.GetRequiredService<IChartDataService>();
            chartDataService.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChartDataService.Start failed");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _logger?.LogInformation(LogEvents.AppExiting, "Application exiting");

        try
        {
            Host?.Dispose();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        // User config path in AppData (persists across updates)
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BlueMeter");
        var userConfigPath = Path.Combine(appDataPath, "config.json");

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddJsonFile(userConfigPath, optional: true, reloadOnChange: true)
            .Build();
    }

    private static IObservable<LogEvent>? ConfigureLogging(IConfiguration configRoot)
    {
        IObservable<LogEvent>? streamRef = null;
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configRoot)
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Observers(obs => streamRef = obs)
            .CreateLogger();
        return streamRef;
    }

    private static IHostBuilder CreateHostBuilder(IConfiguration configRoot)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder => builder.AddConfiguration(configRoot))
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                // ----- Configuration / packet pipeline / theming -----
                services.AddJsonConfiguration();
                services.Configure<AppConfig>(context.Configuration.GetSection("Config"));

                services.AddPacketAnalyzer();
                services.AddWindowManagementService();
                services.AddMessageDialogService();
                services.AddChecklistServices();
                services.AddModuleSolverServices();
                services.AddGlobalHotkeyService();

                // ----- Reflection-based VM/View registration -----
                // Mirrors the WPF App.xaml.cs convention: every concrete class in
                // BlueMeter.ViewModels.* ending in "ViewModel" and every concrete
                // class in BlueMeter.Views.* ending in "View" is registered as
                // transient (with per-type overrides for the DPS statistics pair).
                RegisterViewModels(services);
                RegisterViews(services);

                // ----- Manually-registered windows (suffix is "Window", not "View") -----
                services.AddTransient<ChartsWindow>();
                services.AddTransient<ReplayWindow>();
                services.AddTransient<ChartTestWindow>();
                services.AddTransient<ChecklistWindow>();
                // MainView is the application root and must be a singleton so the
                // tray-restore plumbing keeps pointing at the same instance.
                services.AddSingleton<MainView>();

                // ----- Singleton services (mirror WPF App.xaml.cs) -----
                services.AddSingleton<DebugFunctions>();
                services.AddSingleton(CaptureDeviceList.Instance);
                services.AddSingleton<IApplicationControlService, ApplicationControlService>();
                services.AddSingleton<IDeviceManagementService, DeviceManagementService>();
                services.AddSingleton<IConfigManager, ConfigManager>();
                services.AddSingleton<IMousePenetrationService, MousePenetrationService>();
                services.AddSingleton<ITopmostService, TopmostService>();
                services.AddSingleton<IPluginManager, PluginManager>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddSingleton<ApplicationThemeManager>();
                services.AddSingleton<ThemeService>();
                services.AddHttpClient(); // Required for UpdateChecker
                services.AddSingleton<IUpdateChecker, UpdateChecker>();
                services.AddSingleton<IChartDataService, ChartDataService>();
                services.AddSingleton<ISoundPlayerService, SoundPlayerService>();
                services.AddSingleton<IQueueAlertManager, QueueAlertManager>();
                services.AddSingleton<IQueuePopUIDetector, QueuePopUIDetector>();

                // Plugin registrations (DpsPlugin/ModuleSolverPlugin/WorldBossPlugin)
                // land in Phase 11 once the plugin assemblies are ported. Until then,
                // IPluginManager resolves with an empty IEnumerable<IPlugin> and the
                // main window opens with no plugin tabs.

                // ----- Localization -----
                services.AddSingleton(new LocalizationConfiguration
                {
                    LocalizationDirectory = Path.Combine(AppContext.BaseDirectory, "Data"),
                });
                services.AddSingleton<LocalizationManager>();

                // ----- Avalonia plumbing -----
                services.AddSingleton(_ => Dispatcher.UIThread);

                if (_logStream != null)
                {
                    services.AddSingleton<IObservable<LogEvent>>(_logStream);
                }
            })
            .ConfigureLogging(lb => lb.ClearProviders());
    }

    /// <summary>
    /// Per-type lifetime overrides for the reflection-based registration loop.
    /// Mirrors the WPF original: <see cref="DpsStatisticsViewModel"/> is shared
    /// across all <see cref="DpsStatisticsView"/> instances (one transient view
    /// per plugin tab, one shared VM holding the live DPS rows).
    /// </summary>
    private static readonly Dictionary<Type, ServiceLifetime> LifeTimeOverrides = new()
    {
        { typeof(DpsStatisticsViewModel), ServiceLifetime.Singleton },
        { typeof(DpsStatisticsView), ServiceLifetime.Transient },
    };

    private static void RegisterViewModels(IServiceCollection services)
    {
        RegisterTypes(services, "BlueMeter.ViewModels", "ViewModel");
    }

    private static void RegisterViews(IServiceCollection services)
    {
        RegisterTypes(services, "BlueMeter.Views", "View");
    }

    private static void RegisterTypes(
        IServiceCollection services,
        string @namespace,
        string suffix)
    {
        var types = typeof(App).Assembly
            .GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsClass: true } &&
                t.Namespace != null &&
                t.Namespace.StartsWith(@namespace, StringComparison.Ordinal) &&
                t.Name.EndsWith(suffix, StringComparison.Ordinal));

        foreach (var type in types)
        {
            var lifetime = LifeTimeOverrides.TryGetValue(type, out var overrideLifetime)
                ? overrideLifetime
                : ServiceLifetime.Transient;

            services.Add(new ServiceDescriptor(type, type, lifetime));
        }
    }
}
