using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueMeter.WPF.Models.Checklist;

/// <summary>
/// Repräsentiert ein Profil/Charakter mit eigener Task-Liste
/// </summary>
public partial class ChecklistProfile : ObservableObject
{
    /// <summary>
    /// Eindeutige Profil-ID
    /// </summary>
    [ObservableProperty]
    private string _profileId = Guid.NewGuid().ToString();

    /// <summary>
    /// Name des Profils/Charakters
    /// </summary>
    [ObservableProperty]
    private string _profileName = "Default Profile";

    /// <summary>
    /// Liste der Daily Tasks
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChecklistTask> _dailyTasks = [];

    /// <summary>
    /// Liste der Weekly Tasks
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChecklistTask> _weeklyTasks = [];

    /// <summary>
    /// Zeitstempel des letzten Daily-Resets
    /// </summary>
    [ObservableProperty]
    private DateTime _lastDailyReset = DateTime.MinValue;

    /// <summary>
    /// Zeitstempel des letzten Weekly-Resets
    /// </summary>
    [ObservableProperty]
    private DateTime _lastWeeklyReset = DateTime.MinValue;

    /// <summary>
    /// Erstellt ein neues Profil mit Standard-Tasks
    /// </summary>
    public static ChecklistProfile CreateDefault(string profileName = "Default")
    {
        var profile = new ChecklistProfile
        {
            ProfileName = profileName
        };

        // Füge Blue Protocol Daily Tasks hinzu
        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "Guild Check-In & Cargo",
            Category = TaskCategory.Social,
            Type = TaskType.Daily,
            IsIncremental = false
        });

        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "Clear Unstable Space Dungeon",
            Category = TaskCategory.Dungeon,
            Type = TaskType.Daily,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 2
        });

        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "Bureau Commissions | Can skip up to 2 days",
            Category = TaskCategory.Dailies,
            Type = TaskType.Daily,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 3
        });

        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "Homestead Commissions | Can skip up to 2 days",
            Category = TaskCategory.Dailies,
            Type = TaskType.Daily,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 3
        });

        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "World Boss Keys | Can skip up to 2 days",
            Category = TaskCategory.Raid,
            Type = TaskType.Daily,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 2
        });

        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "Elite Boss Keys | Can skip up to 2 days",
            Category = TaskCategory.Raid,
            Type = TaskType.Daily,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 2
        });

        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "Life Skill Focus | Can skip up to 4 days",
            Category = TaskCategory.Crafting,
            Type = TaskType.Daily,
            IsIncremental = false
        });

        profile.DailyTasks.Add(new ChecklistTask
        {
            Label = "Season Pass Activity (Earn 500 Activity Merits)",
            Category = TaskCategory.Dailies,
            Type = TaskType.Daily,
            IsIncremental = false
        });

        // Füge Standard Weekly Tasks hinzu

        // Social & Guild Activities
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Pioneer Awards (Pioneer NPC in town)",
            Category = TaskCategory.Social,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Reclaim Hub (If you missed a Daily/Weekly)",
            Category = TaskCategory.Dailies,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Guild Activity Rewards (Reach 7000/7000 Points)",
            Category = TaskCategory.Social,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Guild Hunt (Available on Fridays, Saturdays, Sundays)",
            Category = TaskCategory.Social,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Guild Dance (Available on Friday)",
            Category = TaskCategory.Social,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        // Raids & Boss Content
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "World Boss Crusade (Earn 1200 Points)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Fight the Bane Lord (Random Dungeon Encounter)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 5
        });

        // Dungeons
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Dungeons (Normal/Hard) | Clear for Reforge Stones",
            Category = TaskCategory.Dungeon,
            Type = TaskType.Weekly,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 20
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Dungeons (Master 1-5) | Clear for Reforge Stones",
            Category = TaskCategory.Dungeon,
            Type = TaskType.Weekly,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 20
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Dungeons (Master 6-20) | Clear for Reforge Stones | Available: 24th Nov.",
            Category = TaskCategory.Dungeon,
            Type = TaskType.Weekly,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 20
        });

        // Stores & Exchange
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Gear Exchange Stores (Buy Luno Pouches, Alloy Shards & Reforge Stones)",
            Category = TaskCategory.Dailies,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Honor Store (Earn 10000 Honor Points)",
            Category = TaskCategory.PvP,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Friendship Store (Earn 2000 Friendship Points)",
            Category = TaskCategory.Social,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Reputation Store (Buy Will Wish Coin, \"Revive\" Candy & Healing Aromatic Lv.1)",
            Category = TaskCategory.Dailies,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Guild Store (Buy Focus Potions, Supply Chests & Burl Shards)",
            Category = TaskCategory.Social,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Event Store (If available)",
            Category = TaskCategory.Dailies,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        // Life Skills & Vaults
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Life Skill Exchange Quests",
            Category = TaskCategory.Crafting,
            Type = TaskType.Weekly,
            IsIncremental = true,
            CurrentProgress = 0,
            MaxProgress = 12
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Stimen Vaults (Resets every 2 weeks) | Check Timer",
            Category = TaskCategory.Dungeon,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        // Ice Dragon Raids
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Ice Dragon Raid - Easy (12710+ Ability Score)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Ice Dragon Raid - Hard (16140+ Ability Score)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Ice Dragon Raid - Nightmare (22300+ Ability Score) | Available: 24th Nov. 2025",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        // Bone Dragon Raids
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Bone Dragon Raid - Easy (15210+ Ability Score)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Bone Dragon Raid - Hard (19040+ Ability Score)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Bone Dragon Raid - Nightmare (24180+ Ability Score) | Available: 24th Nov. 2025",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        // Light Dragon Raids
        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Light Dragon Raid - Easy (16140+ Ability Score)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Light Dragon Raid - Hard (20670+ Ability Score)",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        profile.WeeklyTasks.Add(new ChecklistTask
        {
            Label = "Light Dragon Raid - Nightmare (27790+ Ability Score) | Available: 24th Nov. 2025",
            Category = TaskCategory.Raid,
            Type = TaskType.Weekly,
            IsIncremental = false
        });

        return profile;
    }
}
