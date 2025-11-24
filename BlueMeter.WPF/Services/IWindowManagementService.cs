using BlueMeter.WPF.Views;
using BlueMeter.WPF.Views.Checklist;

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
    ChecklistWindow ChecklistWindow { get; }
    ChartsWindow ChartsWindow { get; }
    MainView MainView { get; }
}