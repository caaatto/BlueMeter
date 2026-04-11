using System.Drawing;
using Avalonia.Input;
using BlueMeter.Core.Models;
using BlueMeter.Models;
using BlueMeter.Services.Theming;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using KeyBinding = BlueMeter.Models.KeyBinding;

namespace BlueMeter.Config;

/// <summary>
/// Application configuration. POCO loaded via the .NET Options pattern from
/// <c>appsettings.json</c> + <c>%APPDATA%\BlueMeter\config.json</c>, and persisted
/// back through <see cref="IConfigManager"/>.
/// </summary>
public partial class AppConfig : ObservableObject
{
    [ObservableProperty]
    private string _nickname = string.Empty;

    /// <summary>
    /// 职业
    /// </summary>
    [ObservableProperty]
    private Classes _classes;

    /// <summary>
    /// 用户UID
    /// </summary>
    [ObservableProperty]
    private long _uid;

    /// <summary>
    /// DPS伤害类型显示
    /// </summary>
    [ObservableProperty]
    private NumberDisplayMode _damageDisplayType;

    /// <summary>
    /// 战斗力
    /// </summary>
    [ObservableProperty]
    private int _combatPower;

    /// <summary>
    /// 战斗计时清除延迟（秒）
    /// </summary>
    [ObservableProperty]
    private int _combatTimeClearDelay = 5;

    /// <summary>
    /// 是否过图清空全程记录
    /// </summary>
    [ObservableProperty]
    private bool _clearLogAfterTeleport;

    /// <summary>
    /// 不透明度（0-100）, 默认100, 0为全透明
    /// </summary>
    [ObservableProperty]
    private double _opacity = 100;

    /// <summary>
    /// 鼠标穿透开关
    /// </summary>
    [ObservableProperty]
    private bool _mouseThroughEnabled;

    /// <summary>
    /// 是否使用浅色模式
    /// </summary>
    [ObservableProperty]
    private string _theme = "Light";

    /// <summary>
    /// Panel Farbmodus (Light = Weiß, Dark = Dunkelgrau)
    /// </summary>
    [ObservableProperty]
    private string _panelColorMode = "Dark";

    /// <summary>
    /// 主题颜色（窗口背景色）
    /// </summary>
    [ObservableProperty]
    private string? _themeColor = "#0047AB";

    /// <summary>
    /// Enable automatic holiday themes (Christmas, Halloween, etc.)
    /// </summary>
    [ObservableProperty]
    private bool _enableHolidayThemes = false;

    /// <summary>
    /// 背景图片路径
    /// </summary>
    [ObservableProperty]
    private string? _backgroundImagePath;

    /// <summary>
    /// 背景图片专用模式（仅显示背景，面板透明）
    /// </summary>
    [ObservableProperty]
    private bool _backgroundOnlyMode = false;

    /// <summary>
    /// 在DPS统计窗口中显示背景图片
    /// </summary>
    [ObservableProperty]
    private bool _showBackgroundInDpsMeter = false;

    /// <summary>
    /// 当前界面语言（如 zh-CN、en-US、auto）
    /// </summary>
    [ObservableProperty]
    private Language _language = Language.Auto;

    /// <summary>
    /// 启动时的窗口状态
    /// </summary>
    [ObservableProperty]
    private Rectangle? _startUpState;

    /// <summary>
    /// 首选网络适配器
    /// </summary>
    [ObservableProperty]
    private NetworkAdapterInfo? _preferredNetworkAdapter;

    /// <summary>
    /// 鼠标穿透快捷键数据
    /// </summary>
    [ObservableProperty]
    private KeyBinding _mouseThroughShortcut = new(Key.F6, KeyModifiers.None);

    /// <summary>
    /// 置顶切换快捷键
    /// </summary>
    [ObservableProperty]
    private KeyBinding _topmostShortcut = new(Key.F7, KeyModifiers.None);

    /// <summary>
    /// 清空数据快捷键数据
    /// </summary>
    [ObservableProperty]
    private KeyBinding _clearDataShortcut = new(Key.F9, KeyModifiers.None);

    /// <summary>
    /// 当前窗口是否置顶
    /// </summary>
    [ObservableProperty]
    private bool _topmostEnabled;

    /// <summary>
    /// Enable or disable all global hotkeys
    /// </summary>
    [ObservableProperty]
    private bool _globalHotkeysEnabled = true;

    /// <summary>
    /// Save DPS window position and size on application exit
    /// </summary>
    [ObservableProperty]
    private bool _saveDpsWindowPosition = true;

    [ObservableProperty]
    private double? _dpsWindowLeft;

    [ObservableProperty]
    private double? _dpsWindowTop;

    [ObservableProperty]
    private double? _dpsWindowWidth;

    [ObservableProperty]
    private double? _dpsWindowHeight;

    [ObservableProperty]
    private bool _debugEnabled = false;

    /// <summary>
    /// Plugin AutoStart-Status (Dictionary: Plugin Name -> AutoStart enabled)
    /// </summary>
    [ObservableProperty]
    private Dictionary<string, bool> _pluginAutoStartStates = [];

    /// <summary>
    /// Record all encounters regardless of duration
    /// </summary>
    [ObservableProperty]
    private bool _recordAllEncounters = true;

    /// <summary>
    /// Ignore encounters shorter than 1 minute
    /// </summary>
    [ObservableProperty]
    private bool _ignoreEncountersUnder1Min = false;

    /// <summary>
    /// Ignore encounters shorter than 2 minutes
    /// </summary>
    [ObservableProperty]
    private bool _ignoreEncountersUnder2Min = false;

    /// <summary>
    /// Minimum encounter duration in seconds for recording (custom value)
    /// </summary>
    [ObservableProperty]
    private int _minEncounterDuration = 0;

    /// <summary>
    /// Training mode type: None, Personal, Faction, or Extreme
    /// </summary>
    [ObservableProperty]
    private TrainingMode _trainingMode = TrainingMode.None;

    /// <summary>
    /// Manual Player UID for Solo Training mode filtering (0 = auto-detect)
    /// </summary>
    [ObservableProperty]
    private long _manualPlayerUid = 0;

    // ===== Database Cleanup Settings =====

    [ObservableProperty]
    private bool _autoDatabaseCleanup = true;

    [ObservableProperty]
    private int _maxEncountersToKeep = 20;

    [ObservableProperty]
    private double _maxDatabaseSizeMB = 100;

    // ===== Queue Pop Alert Settings =====

    [ObservableProperty]
    private bool _queuePopSoundEnabled = true;

    [ObservableProperty]
    private QueuePopSound _queuePopSound = QueuePopSound.Harp;

    /// <summary>
    /// Queue pop sound volume (0-100%)
    /// </summary>
    [ObservableProperty]
    private double _queuePopSoundVolume = 10.0;

    /// <summary>
    /// Enable queue detection logging for debugging
    /// </summary>
    [ObservableProperty]
    private bool _queueDetectionLoggingEnabled = false;

    partial void OnQueueDetectionLoggingEnabledChanged(bool value)
    {
        BlueMeter.Core.Data.DataStorageV2.EnableQueueDetectionLogging = value;
    }

    partial void OnDpsRefreshRateChanged(DpsRefreshRate value)
    {
        // Update global batch timeout when DPS refresh rate changes
        var intervalMs = value.GetIntervalMs();
        BlueMeter.Core.Data.BattleLogQueue.GlobalBatchTimeout = TimeSpan.FromMilliseconds(intervalMs);
    }

    // ===== Advanced Combat Logging Settings =====

    /// <summary>
    /// Enable advanced packet-level combat logging.
    /// When disabled (default), only aggregated stats are saved to SQLite.
    /// When enabled, detailed BSON files are created with full replay capability.
    /// </summary>
    [ObservableProperty]
    private bool _enableAdvancedCombatLogging = false;

    /// <summary>
    /// Maximum number of encounters to store in advanced mode.
    /// Oldest encounters are automatically deleted when limit is reached.
    /// Recommended: 10 (~450 MB).
    /// </summary>
    [ObservableProperty]
    private int _maxStoredEncounters = 10;

    /// <summary>
    /// Custom directory for battle logs (null = default %LocalAppData%/BlueMeter/CombatLogs)
    /// </summary>
    [ObservableProperty]
    private string? _battleLogDirectory = null;

    /// <summary>
    /// DPS Refresh Rate (how often DPS numbers update).
    /// Minimal=10fps, Low=20fps, Medium=30fps, High=60fps.
    /// </summary>
    [ObservableProperty]
    private DpsRefreshRate _dpsRefreshRate = DpsRefreshRate.Low;

    /// <summary>
    /// Effective theme color (considers holiday themes if enabled).
    /// </summary>
    public string EffectiveThemeColor => GetEffectiveThemeColor();

    /// <summary>
    /// Get the effective theme color. Always returns the user's selected color
    /// — holiday themes only add decorations, not colors.
    /// </summary>
    public string GetEffectiveThemeColor()
    {
        return ThemeColor ?? "#0047AB";
    }

    partial void OnThemeColorChanged(string? value)
    {
        OnPropertyChanged(nameof(EffectiveThemeColor));
    }

    partial void OnEnableHolidayThemesChanged(bool value)
    {
        OnPropertyChanged(nameof(EffectiveThemeColor));
        OnPropertyChanged(nameof(CurrentHolidayName));
        OnPropertyChanged(nameof(IsHolidayDecorationVisible));
    }

    /// <summary>
    /// Check if holiday decorations should be shown. Resolves
    /// <see cref="IHolidayThemeProvider"/> via <see cref="App.Host"/> at call time
    /// because <see cref="AppConfig"/> is a JSON-deserialized POCO and can't take
    /// constructor injection.
    /// </summary>
    public bool ShouldShowHolidayDecorations()
    {
        if (!EnableHolidayThemes) return false;
        return ResolveHolidayProvider()?.GetCurrentHolidayTheme() != null;
    }

    /// <summary>
    /// XAML-friendly gate for holiday decoration visibility. Combines the user
    /// toggle with the date-based check from <see cref="IHolidayThemeProvider"/>
    /// so the Christmas overlay only shows during the configured window — the
    /// raw <see cref="EnableHolidayThemes"/> flag on its own would leak the
    /// decorations into every month whenever the toggle is left on.
    /// </summary>
    public bool IsHolidayDecorationVisible => ShouldShowHolidayDecorations();

    /// <summary>
    /// Get the current holiday name if holiday themes are enabled and a holiday is active.
    /// Returns <c>null</c> if no holiday is active or holiday themes are disabled.
    /// </summary>
    public string? GetCurrentHolidayName()
    {
        if (!EnableHolidayThemes) return null;
        return ResolveHolidayProvider()?.GetCurrentHolidayName();
    }

    public string? CurrentHolidayName => GetCurrentHolidayName();

    private static IHolidayThemeProvider? ResolveHolidayProvider()
    {
        return App.Host?.Services.GetService<IHolidayThemeProvider>();
    }

    public AppConfig Clone()
    {
        // TODO: Add unittest
        var json = JsonConvert.SerializeObject(this);
        return JsonConvert.DeserializeObject<AppConfig>(json)!;
    }
}
