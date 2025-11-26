using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SharpPcap;
using BlueMeter.WPF.Config;
using BlueMeter.WPF.Extensions;
using BlueMeter.WPF.Localization;
using BlueMeter.WPF.Logging;
using BlueMeter.WPF.Plugins;
using BlueMeter.WPF.Plugins.BuiltIn;
using BlueMeter.WPF.Plugins.Interfaces;
using BlueMeter.WPF.Services;
using BlueMeter.WPF.Themes;
using BlueMeter.WPF.ViewModels;
using BlueMeter.WPF.Views;

namespace BlueMeter.WPF;

public partial class App : Application
{
    private static ILogger<App>? _logger;
    private static IObservable<LogEvent>? _logStream; // exposed for UI subscription

    public static IHost? Host { get; private set; }

    [STAThread]
    private static void Main(string[] args)
    {
        var configRoot = BuildConfiguration();
        _logStream = ConfigureLogging(configRoot);

        Host = CreateHostBuilder(args, configRoot).Build();
        _logger = Host.Services.GetRequiredService<ILogger<App>>();

        _logger.LogInformation(WpfLogEvents.AppStarting, "Application starting");

        // Initialize DeepL translator for skill name translation
        DpsStatisticsSubViewModel.InitializeTranslator("77ff22ea-b6d6-414c-b08c-1b950c9f8eca:fx");
        _logger.LogInformation("DeepL translator initialized for skill name translation");

        var app = new App();
        app.InitializeComponent();

        // Check if Npcap is installed before starting the app
        if (!Services.NpcapChecker.IsNpcapInstalled())
        {
            _logger.LogError("Npcap is not installed on this system");
            Services.NpcapChecker.ShowNpcapRequiredDialog();
            Log.CloseAndFlush();
            Environment.Exit(1);
            return;
        }

        // Centralized application startup (localization, adapter, analyzer)
        var appStartup = Host.Services.GetRequiredService<IApplicationStartup>();
        appStartup.InitializeAsync().GetAwaiter().GetResult();

        app.MainWindow = Host.Services.GetRequiredService<MainView>();
        app.MainWindow.Show();

        // Start hotkey service on UI thread after main window is shown
        var hotkeyService = Host.Services.GetRequiredService<IGlobalHotkeyService>();
        hotkeyService.Start();

        // Start chart data service for real-time DPS/HPS sampling
        var chartDataService = Host.Services.GetRequiredService<IChartDataService>();
        chartDataService.Start();

        app.Run();

        // Centralized shutdown
        try
        {
            appStartup.Shutdown();
        }
        catch
        {
            // ignored
        }

        _logger.LogInformation(WpfLogEvents.AppExiting, "Application exiting");
        Log.CloseAndFlush();
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
            .AddJsonFile("appsettings.json", false, true)  // Default settings
            .AddJsonFile("appsettings.Development.json", true, true)
            .AddJsonFile(userConfigPath, optional: true, reloadOnChange: true)  // User settings (override defaults)
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

    private static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configRoot)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => { builder.AddConfiguration(configRoot); })
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                services.AddJsonConfiguration();
                services.Configure<AppConfig>(context.Configuration.GetSection("Config"));

                RegisterViewModels(services);
                RegisterViews(services);

                services.AddPacketAnalyzer();
                services.AddThemes();
                services.AddWindowManagementService();
                services.AddMessageDialogService();
                services.AddChecklistServices();

                // Charts Window registration (not auto-registered due to "Window" suffix)
                services.AddTransient<ChartsWindowViewModel>();
                services.AddTransient<ChartsWindow>();

                // Chart Views and ViewModels (Phase 4-6)
                services.AddTransient<DpsTrendChartViewModel>();
                services.AddTransient<DpsTrendChartView>();
                services.AddTransient<SkillBreakdownChartViewModel>();
                services.AddTransient<SkillBreakdownChartView>();

                services.AddSingleton<DebugFunctions>();
                services.AddSingleton(CaptureDeviceList.Instance);
                services.AddSingleton<IApplicationControlService, ApplicationControlService>();
                services.AddSingleton<IDeviceManagementService, DeviceManagementService>();
                services.AddSingleton<IApplicationStartup, ApplicationStartup>();
                services.AddSingleton<IConfigManager, ConfigManger>();
                services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
                services.AddSingleton<IMousePenetrationService, MousePenetrationService>();
                services.AddSingleton<ITopmostService, TopmostService>();
                services.AddSingleton<IPluginManager, PluginManager>();
                services.AddSingleton<IPlugin, DpsPlugin>();
                services.AddSingleton<IPlugin, ModuleSolverPlugin>();
                services.AddSingleton<IPlugin, WorldBossPlugin>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddSingleton<ThemeService>();
                services.AddHttpClient(); // Required for UpdateChecker
                services.AddSingleton<IUpdateChecker, UpdateChecker>();
                services.AddSingleton<IChartDataService, ChartDataService>(); // Chart data sampling service
                services.AddSingleton<ISoundPlayerService, SoundPlayerService>(); // Sound player for queue pop alerts
                services.AddSingleton<IQueueAlertManager, QueueAlertManager>(); // Queue pop alert manager
                services.AddSingleton<IQueuePopUIDetector, QueuePopUIDetector>(); // UI Automation queue pop detector

                if (_logStream != null) services.AddSingleton<IObservable<LogEvent>>(_logStream);

                services.AddSingleton(_ => Current.Dispatcher);

                // Localization manager singleton
                services.AddSingleton<LocalizationConfiguration>(new LocalizationConfiguration
                {
                    LocalizationDirectory = Path.Combine(AppContext.BaseDirectory, "Data")
                });
                services.AddSingleton<LocalizationManager>();
            })
            .ConfigureLogging(lb => lb.ClearProviders());
    }

    static readonly Dictionary<Type, ServiceLifetime> LifeTimeOverrides = new()
    {
        { typeof(DpsStatisticsViewModel), ServiceLifetime.Singleton },
        { typeof(DpsStatisticsView), ServiceLifetime.Transient }
    };

    private static void RegisterViewModels(IServiceCollection services)
    {
        RegisterTypes(services, "BlueMeter.WPF.ViewModels", "ViewModel");
    }

    private static void RegisterViews(IServiceCollection services)
    {
        RegisterTypes(services, "BlueMeter.WPF.Views", "View");
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
