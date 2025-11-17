using System.Drawing;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using BlueMeter.Core.Models;
using BlueMeter.WPF.Models;
using KeyBinding = BlueMeter.WPF.Models.KeyBinding;

namespace BlueMeter.WPF.Config;

/// <summary>
/// 应用配置类
/// 集成了配置管理器功能，支持INI文件持久化和属性变更通知
/// </summary>
public partial class AppConfig : ObservableObject
{
    /// <summary>
    /// 昵称
    /// </summary>
    [ObservableProperty]
    private string _nickname = string.Empty;

    [ObservableProperty]
    private ModifierKeys _testModifier = ModifierKeys.None;

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
    /// 鼠标穿透开关（WPF）
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
    private KeyBinding _mouseThroughShortcut = new(Key.F6, ModifierKeys.None);

    /// <summary>
    /// 置顶切换快捷键
    /// </summary>
    [ObservableProperty]
    private KeyBinding _topmostShortcut = new(Key.F7, ModifierKeys.None);

    /// <summary>
    /// 清空数据快捷键数据
    /// </summary>
    [ObservableProperty]
    private KeyBinding _clearDataShortcut = new(Key.F9, ModifierKeys.None);

    /// <summary>
    /// 当前窗口是否置顶
    /// </summary>
    [ObservableProperty]
    private bool _topmostEnabled;

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

    public AppConfig Clone()
    {
        // TODO: Add unittest
        var json = JsonConvert.SerializeObject(this);
        return JsonConvert.DeserializeObject<AppConfig>(json)!;
    }
}
