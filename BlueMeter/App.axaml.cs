using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using BlueMeter.Logging;
using BlueMeter.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Host.Services.GetRequiredService<MainWindow>();
            desktop.Exit += OnDesktopExit;
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
            .ConfigureServices((_, services) =>
            {
                // Phase 2 smoke-test wiring: just enough to resolve MainWindow.
                // AppConfig, services, view models etc. land in Phase 3+.
                services.AddSingleton<MainWindow>();
                services.AddSingleton(_ => Dispatcher.UIThread);

                if (_logStream != null)
                {
                    services.AddSingleton<IObservable<LogEvent>>(_logStream);
                }
            })
            .ConfigureLogging(lb => lb.ClearProviders());
    }
}
