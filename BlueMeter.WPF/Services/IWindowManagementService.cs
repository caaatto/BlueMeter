using BlueMeter.WPF.Views;

namespace BlueMeter.WPF.Services;

public interface IWindowManagementService
{
    DpsStatisticsView DpsStatisticsView { get; }
    SettingsView SettingsView { get; }
    SkillBreakdownView SkillBreakdownView { get; }
    AboutView AboutView { get; }
    DamageReferenceView DamageReferenceView { get; }
    ModuleSolveView ModuleSolveView { get; }
    BossTrackerView BossTrackerView { get; }
    MainView MainView { get; }
}