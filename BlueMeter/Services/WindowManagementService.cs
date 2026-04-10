using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using BlueMeter.Logging;
using BlueMeter.ViewModels;
using BlueMeter.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

/// <summary>
/// Avalonia implementation of <see cref="IWindowManagementService"/>.
///
/// The WPF original exposed Window-typed properties that lazily resolved from DI and
/// cached per-type; the Avalonia contract is method-based (Phase 5 redesign). This
/// class keeps the same lazy singleton pattern under the hood but surfaces it through
/// discrete Show/Minimize methods so view models stay UI-agnostic.
///
/// Unported views (SettingsView, ModuleSolveView, ChecklistWindow, MainView) are
/// currently stubbed with a warning log. Those methods become real once Phase 10
/// final-batch view ports land.
/// </summary>
public class WindowManagementService(IServiceProvider provider, ILogger<WindowManagementService> logger) : IWindowManagementService
{
    private DpsStatisticsView? _dpsStatisticsView;
    private AboutView? _aboutView;
    private DamageReferenceView? _damageReferenceView;
    private BossTrackerView? _bossTrackerView;
    private ChartsWindow? _chartsWindow;
    private SkillBreakdownView? _skillBreakdownView;

    /// <summary>
    /// The app's main window, obtained via the classic desktop lifetime. May be null
    /// before Phase 10 wires <see cref="MainView"/> as the real main window — in that
    /// case ownership assignment is skipped and tool windows still open standalone.
    /// </summary>
    private Window? MainWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    // ===== Singleton-cached ported windows =====

    private DpsStatisticsView GetDpsStatisticsView() =>
        GetOrCreate(() => _dpsStatisticsView, v => _dpsStatisticsView = v);

    private AboutView GetAboutView() =>
        GetOrCreate(() => _aboutView, v => _aboutView = v);

    private DamageReferenceView GetDamageReferenceView() =>
        GetOrCreate(() => _damageReferenceView, v => _damageReferenceView = v);

    private BossTrackerView GetBossTrackerView() =>
        GetOrCreate(() => _bossTrackerView, v => _bossTrackerView = v);

    private ChartsWindow GetChartsWindow() =>
        GetOrCreate(() => _chartsWindow, v => _chartsWindow = v);

    private SkillBreakdownView GetSkillBreakdownView() =>
        GetOrCreate(() => _skillBreakdownView, v => _skillBreakdownView = v);

    // ===== IWindowManagementService =====

    public void MinimizeDpsStatisticsWindow()
    {
        var view = GetDpsStatisticsView();
        view.WindowState = WindowState.Minimized;
    }

    public void ShowDpsStatistics() => ShowOrActivate(GetDpsStatisticsView());

    public void ShowSettings()
    {
        // SettingsView has not been ported yet (deferred Phase 10 final batch).
        logger.LogWarning("ShowSettings called but SettingsView is not yet ported");
    }

    public void ShowSettingsAndHighlightUidField()
    {
        // Same deferred view; once SettingsView lands, this should show it and
        // invoke a helper to scroll/highlight the ManualPlayerUid field.
        logger.LogWarning("ShowSettingsAndHighlightUidField called but SettingsView is not yet ported");
    }

    public void ShowAbout() => ShowOrActivate(GetAboutView());

    public void ShowDamageReference() => ShowOrActivate(GetDamageReferenceView());

    public void ShowModuleSolve()
    {
        logger.LogWarning("ShowModuleSolve called but ModuleSolveView is not yet ported");
    }

    public void ShowBossTracker() => ShowOrActivate(GetBossTrackerView());

    public void ShowChecklist()
    {
        logger.LogWarning("ShowChecklist called but ChecklistWindow is not yet ported");
    }

    public void ShowCharts() => ShowOrActivate(GetChartsWindow());

    public void ShowChartsForFocusedPlayer(long playerUid)
    {
        var window = GetChartsWindow();
        if (window.DataContext is ChartsWindowViewModel vm)
        {
            vm.SetFocusedPlayer(playerUid);
        }
        ShowOrActivate(window);
    }

    public async Task LoadHistoricalEncounterInChartsAsync(string encounterId)
    {
        var window = GetChartsWindow();
        ShowOrActivate(window);
        if (window.DataContext is ChartsWindowViewModel vm)
        {
            await vm.LoadHistoricalEncounterAsync(encounterId);
        }
        else
        {
            logger.LogWarning(
                "LoadHistoricalEncounterInChartsAsync: ChartsWindow DataContext is not ChartsWindowViewModel");
        }
    }

    public void ShowEncounterHistory(EncounterHistoryViewModel viewModel)
    {
        // EncounterHistory is transient — each call opens a new window hosting the
        // supplied VM. The caller constructs the VM (see DpsStatisticsViewModel.OpenEncounterHistory).
        var view = new EncounterHistoryView(viewModel);
        logger.LogDebug(LogEvents.WindowCreated, "Window created: {Window}", nameof(EncounterHistoryView));
        view.Closed += (_, _) =>
            logger.LogDebug(LogEvents.WindowClosed, "Window closed: {Window}", nameof(EncounterHistoryView));

        var main = MainWindow;
        if (main is not null && view != main)
        {
            view.Show(main);
        }
        else
        {
            view.Show();
        }
    }

    // ===== Helpers =====

    /// <summary>
    /// Lazy resolve a singleton window from DI, cache it, and null the cache on close.
    /// </summary>
    private T GetOrCreate<T>(Func<T?> get, Action<T?> set) where T : Window
    {
        var cached = get();
        if (cached is not null)
        {
            return cached;
        }

        var view = provider.GetRequiredService<T>();
        logger.LogDebug(LogEvents.WindowCreated, "Window created: {Window}", typeof(T).Name);
        set(view);

        view.Closed += (_, _) =>
        {
            if (ReferenceEquals(get(), view))
            {
                set(null);
            }
            logger.LogDebug(LogEvents.WindowClosed, "Window closed: {Window}", typeof(T).Name);
        };

        return view;
    }

    /// <summary>
    /// Show the window (attached to the main window as parent if available) and bring
    /// it to the foreground. Re-shows with the main-window parent on first display;
    /// subsequent calls only un-minimize and activate.
    /// </summary>
    private void ShowOrActivate(Window view)
    {
        if (view.IsVisible)
        {
            if (view.WindowState == WindowState.Minimized)
            {
                view.WindowState = WindowState.Normal;
            }
            view.Activate();
            return;
        }

        var main = MainWindow;
        if (main is not null && view != main)
        {
            view.Show(main);
        }
        else
        {
            view.Show();
        }
    }
}

public static class WindowManagementServiceExtensions
{
    public static IServiceCollection AddWindowManagementService(this IServiceCollection services)
    {
        services.AddSingleton<IWindowManagementService, WindowManagementService>();
        return services;
    }
}
