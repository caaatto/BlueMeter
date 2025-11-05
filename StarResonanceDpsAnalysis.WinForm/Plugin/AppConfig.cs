using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using AntdUI;
using SharpPcap;
using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.WinForm.Control.GDI;
using StarResonanceDpsAnalysis.WinForm.Core;

namespace StarResonanceDpsAnalysis.WinForm.Plugin
{
    /// <summary>
    /// # 分类：类概览
    /// 全局配置类（AppConfig）
    /// - 负责从 INI 文件（config.ini）读取/写入配置项。
    /// - 采用“延迟加载 + 静态缓存”的方式读取属性：首次访问从文件读取并缓存，之后直接从内存返回，性能更高。
    /// - 提供了一些 UI/按键/主题 等常用配置项，以及默认值回退策略。
    /// - 通过 Win32 API（GetPrivateProfileString / WritePrivateProfileString）访问 INI 文件。
    /// 
    /// # 使用建议
    /// - 修改配置：直接设置对应静态属性（会同步写回 INI）。
    /// - 读取配置：直接访问静态属性（第一次读会从 INI 初始化）。
    /// - 若应用目录不可写：写入函数会失败（API 返回 false），此处未抛异常；若需要强校验，可在外层检查。
    /// </summary>
    public class AppConfig
    {
        public static int ProgressBarHeight
        {
            get
            {
                int height = 50;
                switch (Config.Dpi)
                {
                    case 1F:
                        height = 35;
                        break;
                    case 1.25F:
                        height = 45;
                        break;
                    case 1.5F:
                        height = 45;
                        break;
                    case 1.75F:
                        height = 45;
                        break;
                    case 2F:
                        height = 45;
                        break;

                }

                return height;
            }
        }
        public static Size ProgressBarImageSize = new Size(25, 25);
        public static RenderContent.ContentOffset ProgressBarImage
        {
            get
            {
                int x = 64;
                if (Config.Dpi == 1F)
                {
                    x = 55;
                }

                return new RenderContent.ContentOffset { X = x, Y = 0 };
            }
        }
        public static RenderContent.ContentOffset ProgressBarNmae = new RenderContent.ContentOffset { X = 88, Y = 1 };
        public static RenderContent.ContentOffset ProgressBarHarm
        {
            get
            {
                int x = ProgressBarProportion.X - 50;
                switch (Config.Dpi)
                {
                    case 1F:
                        x = -35;
                        break;
                    case 1.25F:
                        x = -40;
                        break;
                    case 1.5F:
                        x = ProgressBarProportion.X - 45;
                        break;
                    case 1.75F:
                        x = ProgressBarProportion.X - 55;
                        break;
                    case 2:
                        x = ProgressBarProportion.X - 60;
                        break;
                }

                return new RenderContent.ContentOffset { X = x, Y = 0 };
            }
        }
        public static RenderContent.ContentOffset ProgressBarProportion = new RenderContent.ContentOffset { X = -6, Y = 0 };
        #region 字体
        /// <summary>
        /// 进度条字体
        /// </summary>
        public static Font ProgressBarFont
        {
            get => HandledAssets.HarmonyOS_Sans(9);
        }

        /// <summary>
        /// 内容文本2
        /// </summary>
        public static Font DigitalFont
        {

            get => HandledAssets.HarmonyOS_Sans(9);
        }

        /// <summary>
        /// SAO字体小
        /// </summary>
        public static Font SaoFont
        {
            get => HandledAssets.SAOWelcomeTT(10);
        }


        /// <summary>
        /// 标题SAO
        /// </summary>
        public static Font TitleFont
        {
            get => HandledAssets.SAOWelcomeTT(12);
        }

        /// <summary>
        /// 标题文本
        /// </summary>
        public static Font HeaderFont
        {
            get => HandledAssets.AliMaMaShuHeiTi(10);
        }

        public static Font BoldHarmonyFont
        {
            get => HandledAssets.HarmonyOS_Sans_Bold(9);
        }

        /// <summary>
        /// 内容字体
        /// </summary>
        public static Font ContentFont
        {
            get => HandledAssets.HarmonyOS_Sans(9);
        }

        #endregion

        /// <summary>
        /// # 分类：Win32 API 声明（读取 INI）
        /// 从 INI 文件读取指定 [section] 下的 [key] 的值。
        /// - section/key：区段名/键名（区分大小写依文件系统/实现而定，一般不区分）
        /// - defaultValue：未找到键时返回的默认值
        /// - retVal/size：接收缓冲区及长度（当前实现固定 64 字符缓存）
        /// - filePath：INI 文件路径
        /// 返回值：复制到缓冲区的字符长度
        /// 注意：若真实值超出缓冲区长度，将被截断；本类 GetValue 已固定 64 缓冲区，如需更长文本请扩大容量。
        /// </summary>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// # 分类：Win32 API 声明（写入 INI）
        /// 向 INI 文件写入指定 [section] 下的 [key]=value。
        /// - section/key/value：区段名/键名/要写入的值
        /// - filePath：INI 文件路径
        /// 返回值：true 表示成功，false 表示失败（例如路径不可写）
        /// </summary>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

        /// <summary>
        /// # 分类：路径与静态缓存
        /// 配置文件完整路径：应用当前目录下的 config.ini。
        /// </summary>
        private static string FilePath { get; } = $"{Environment.CurrentDirectory}\\config.ini";

        public static string MonsterNames = $"{Environment.CurrentDirectory}\\monster_names.json";

        /// <summary>
        /// 网卡名称
        /// </summary>
        private static string? _networkCardName = null;

        // 窗体透明度（0~100），null 表示未设置
        private static int? _transparency = null;

        // 是否使用浅色模式（true 浅色 / false 深色），null 表示未设置
        private static bool? _isLight = null;

        // 启动时的窗口状态（位置和大小），null 表示未设置
        private static Rectangle? _startUpState = null;

        // 鼠标穿透的快捷键（例如 Ctrl+Shift+...），null 表示未设置
        private static Keys? _mouseThroughKey = null;

        // 控制窗体透明度的快捷键，null 表示未设置
        private static Keys? _formTransparencyKey = null;

        // 显示/隐藏窗口的快捷键，null 表示未设置
        private static Keys? _windowToggleKey = null;

        // 清空当前数据的快捷键，null 表示未设置
        private static Keys? _clearDataKey = null;

        private static int? _damageDisplayType = null;

        /// <summary>
        /// DPS伤害类型显示
        /// </summary>
        public static int DamageDisplayType
        {
            get
            {
                if (_damageDisplayType == null)
                {
                    var value = GetValue("SetUp", "DamageDisplayType", "0").ToInt();
                    _damageDisplayType = value;
                }

                return _damageDisplayType.Value;
            }
            set
            {
                SetValue("SetUp", "DamageDisplayType", value.ToString());
                _damageDisplayType = value;
            }

        }
        /// <summary>
        /// # 分类：外观/用户信息/业务参数（立即可用的公开字段）
        /// - DpsColor：进度条或 DPS 相关 UI 颜色
        /// - NickName：用户昵称（默认“未设置昵称”）
        /// - Uid：用户 UID，默认 0
        /// - CombatTimeClearDelaySeconds：战斗计时清除延迟秒数（默认 5 秒）
        ///   用于 UI 或统计在战斗结束后延迟清除，避免过快闪烁。
        /// </summary>
        public static Color DpsColor = Color.FromArgb(0x22, 0x97, 0xF4);//进度条颜色

        private static string? _nickName = null;
        private static string? _profession = null;
        private static long? _uid = null;//用户UID
        private static int? _combatPower = null;//战斗力
        public static int? _combatTimeClearDelaySeconds;//战斗计时清除延迟
        public static bool? _clearAllDataWhenSwitch;//是否过图清空记录
        public static Color colorText = Color.Black;//文字颜色

        /// <summary>
        /// 是否过图清全程记录
        /// </summary>
        public static bool ClearAllDataWhenSwitch
        {
            get
            {
                if (_clearAllDataWhenSwitch == null)
                {
                    var value = GetValue("SetUp", "ClearAllDataWhenSwitch", "1").ToInt();
                    _clearAllDataWhenSwitch = value == 1;
                }
                return _clearAllDataWhenSwitch.Value;
            }
            set
            {
                SetValue("SetUp", "ClearAllDataWhenSwitch", value ? "1" : "0");
                _clearAllDataWhenSwitch = value;
            }
        }

        public static int CombatTimeClearDelaySeconds
        {
            get
            {
                if (_combatTimeClearDelaySeconds == null)
                {
                    var value = GetValue("SetUp", "CombatTimeClearDelaySeconds", "5").ToInt();
                    _combatTimeClearDelaySeconds = value;
                }
                return _combatTimeClearDelaySeconds.Value;
            }
            set
            {
                SetValue("SetUp", "CombatTimeClearDelaySeconds", value.ToString());
                _combatTimeClearDelaySeconds = value;
            }
        }

        /// <summary>
        /// 昵称
        /// </summary>
        public static string NickName
        {
            get
            {
                if (_nickName == null)
                {
                    var value = GetValue("SetUp", "NickName", string.Empty);
                    _nickName = value;
                }
                return _nickName;
            }
            set
            {
                SetValue("SetUp", "NickName", value);
                _nickName = value;

            }
        }
        /// <summary>
        /// 职业
        /// </summary>
        public static string Profession
        {
            get
            {
                if (_profession == null)
                {
                    var value = GetValue("SetUp", "Profession", string.Empty);
                    _profession = value;
                }
                return _profession;
            }
            set
            {
                SetValue("SetUp", "Profession", value);
                _profession = value;
            }
        }
        /// <summary>
        /// UID
        /// </summary>
        public static long Uid
        {
            get
            {
                if (!_uid.HasValue)
                {
                    var raw = GetValue("SetUp", "Uid", "0");
                    if (!long.TryParse(raw, out var pared)) pared = 0L;

                    _uid = pared;
                }
                return _uid!.Value;
            }
            set
            {
                SetValue("SetUp", "Uid", value.ToString());
                _uid = value;
            }
        }
        /// <summary>
        /// 战力
        /// </summary>
        public static int CombatPower
        {
            get
            {
                // 修复：首次访问时 _combatPower 为 null，不能与 0 比较后直接取 Value
                if (!_combatPower.HasValue)
                {
                    var value = GetValue("SetUp", "CombatPower", "0").ToInt();
                    _combatPower = value;
                }
                return _combatPower.Value;
            }
            set
            {
                SetValue("SetUp", "CombatPower", value.ToString());
                _combatPower = value;
            }
        }

        /// <summary>
        /// 网卡名称
        /// </summary>
        public static string NetworkCardName 
        {
            get 
            {
                if (_networkCardName == null)
                {
                    var value = GetValue("SetUp", "NetworkCardName", string.Empty);
                    _networkCardName = value;
                }

                return _networkCardName;
            }
            set 
            {
                SetValue("SetUp", "NetworkCardName", value);
                _networkCardName = value;
            }
        }

        public static int GetNetworkCardIndex(CaptureDeviceList devices) 
        {
            var deviceIndexData = devices
                    .Select((Data, Index) => (Data, Index))
                    .FirstOrDefault(e => e.Data.Description == NetworkCardName);

            return deviceIndexData.Data == null ? -1 : deviceIndexData.Index;
        }

        /// <summary>
        /// # 分类：透明度（0-100）
        /// UI 总体透明度百分比，默认 100（不透明）。
        /// 注意：此处未强制约束范围；调用方应确保 0-100 合法范围，或在使用处进行钳制。
        /// </summary>
        public static int Transparency
        {
            get
            {
                if (_transparency == null)
                {
                    var value = GetValue("SetUp", "Transparency", "100").ToInt();
                    _transparency = value;
                }
                return _transparency.Value;
            }
            set
            {
                SetValue("SetUp", "Transparency", value.ToString());
                _transparency = value;
            }
        }

        /// <summary>
        /// # 分类：主题模式（浅色/深色）
        /// IsLight = true 表示浅色主题，false 表示深色主题。
        /// 读取时默认值 "1"（即浅色）。
        /// </summary>
        public static bool IsLight
        {
            get
            {
                if (_isLight == null)
                {
                    var value = GetValue("SetUp", "IsLight", "1");
                    _isLight = value == "1";
                }
                return _isLight.Value;
            }
            set
            {
                SetValue("SetUp", "IsLight", value ? "1" : "0");
                _isLight = value;
            }
        }

        /// <summary>
        /// 启动位置
        /// </summary>
        public static Rectangle? StartUpState
        {
            get
            {
                if (_startUpState == null)
                {
                    var psb = Screen.PrimaryScreen?.Bounds;//630, 530 \ 420, 350
                    var valueStr = GetValue("SetUp", "StartUpState", string.Empty);
                    var valueList = valueStr.Split(',').Select(e => e.ToInt(-1)).ToList();
                    if (valueList.Count == 4 && valueList[0] >= 0 && valueList[1] >= 0 && valueList[2] >= 0 && valueList[3] >= 0)
                    {
                        _startUpState = new Rectangle(valueList[0], valueList[1], valueList[2], valueList[3]);
                    }
                }

                return _startUpState;
            }
            set
            {
                if (value != null)
                {
                    SetValue("SetUp", "StartUpState", $"{value.Value.X},{value.Value.Y},{value.Value.Width},{value.Value.Height}");
                }
                else
                {
                    SetValue("SetUp", "StartUpState", string.Empty);
                }
                _startUpState = value;
            }
        }

        /// <summary>
        /// # 分类：热键 - 鼠标穿透
        /// 默认 F6。读取到的数值会校验是否在 Keys 枚举内，不合法则回退到默认。
        /// 用途：在 UI 悬浮窗等场景切换是否“可点击/可穿透”。
        /// </summary>
        public static Keys? MouseThroughKey
        {
            get
            {
                if (_mouseThroughKey == null)
                {
                    var value = GetValue("SetKey", "MouseThroughKey", ((int)Keys.F6).ToString()).ToInt();
                    _mouseThroughKey = value.IntToKeys(Keys.F6);
                }
                return _mouseThroughKey.Value;
            }
            set
            {
                SetValue("SetKey", "MouseThroughKey", ((int?)value).ToString() ?? string.Empty);
                _mouseThroughKey = value;
            }
        }

        /// <summary>
        /// # 分类：热键 - 窗体透明切换
        /// 默认 F7。可用于快速切换不同透明度或显示模式。
        /// </summary>
        public static Keys FormTransparencyKey
        {
            get
            {
                if (_formTransparencyKey == null)
                {
                    var value = GetValue("SetKey", "FormTransparencyKey", ((int)Keys.F7).ToString()).ToInt();
                    _formTransparencyKey = value.IntToKeys(Keys.F7);
                }
                return _formTransparencyKey.Value;
            }
            set
            {
                SetValue("SetKey", "FormTransparencyKey", ((int)value).ToString());
                _formTransparencyKey = value;
            }
        }

        /// <summary>
        /// # 分类：热键 - 显示/隐藏窗口开关
        /// 默认 F8。常用于主界面或悬浮层的一键显示/隐藏。
        /// </summary>
        public static Keys WindowToggleKey
        {
            get
            {
                if (_windowToggleKey == null)
                {
                    var value = GetValue("SetKey", "WindowToggleKey", ((int)Keys.F8).ToString()).ToInt();
                    _windowToggleKey = value.IntToKeys(Keys.F8);
                }
                return _windowToggleKey.Value;
            }
            set
            {
                SetValue("SetKey", "WindowToggleKey", ((int)value).ToString());
                _windowToggleKey = value;
            }
        }

        /// <summary>
        /// # 分类：热键 - 清空当前数据
        /// 默认 F9。通常用于清空当前战斗/会话的即时统计。
        /// </summary>
        public static Keys? ClearDataKey
        {
            get
            {
                if (_clearDataKey == null)
                {
                    var value = GetValue("SetKey", "ClearDataKey", ((int)Keys.F9).ToString()).ToInt();
                    _clearDataKey = value.IntToKeys(Keys.F9);
                }
                return _clearDataKey.Value;
            }
            set
            {
                SetValue("SetKey", "ClearDataKey", ((int?)value).ToString() ?? string.Empty);
                _clearDataKey = value;
            }
        }

        /// <summary>
        /// # 分类：工具 - 配置文件存在性检查
        /// 返回当前应用目录下的 config.ini 是否存在。
        /// 可用于首次启动时决定是否走“初始化向导/创建默认配置”流程。
        /// </summary>
        public static bool GetConfigExists()
        {
            return File.Exists(FilePath);
        }

        /// <summary>
        /// # 分类：读取配置（包装）
        /// 从 INI 配置文件中读取指定 Section 和 Key 的值。
        /// - 若键不存在，返回 def（默认值）。
        /// - 通过固定 64 的缓冲区读取，能覆盖常见短文本配置；若你后续需要存长文本，请适当放大容量。
        /// - 不会抛出异常；若路径无效或权限不足，Windows API 会写入 def 到缓冲区。
        /// </summary>
        /// <param name="section">配置节名称（Section）</param>
        /// <param name="key">键名称（Key）</param>
        /// <param name="def">如果找不到该键时返回的默认值</param>
        /// <returns>读取到的值（字符串）</returns>
        public static string GetValue(string section, string key, string def)
        {
            // 用于存储读取结果的缓冲区（当前 64 字符）
            var buffer = new StringBuilder(64);
            // 调用 WinAPI GetPrivateProfileString 读取 INI 文件内容；失败时 API 会将 def 填入 buffer
            _ = GetPrivateProfileString(section, key, def, buffer, buffer.Capacity, FilePath);

            return buffer.ToString();
        }

        /// <summary>
        /// # 分类：写入配置（包装）
        /// 将指定的值写入到 INI 配置文件的指定 Section 和 Key 中。
        /// - 写入失败通常源自路径不存在、磁盘只读或权限不足；本函数不抛异常，必要时请在外部做返回值检查或日志记录。
        /// - 成功写入后，对应静态属性的“缓存值”也会在 setter 中同步更新，避免读到旧值。
        /// </summary>
        /// <param name="section">配置节名称（Section）</param>
        /// <param name="key">键名称（Key）</param>
        /// <param name="value">要写入的值</param>
        public static void SetValue(string section, string key, string value)
        {
            // 调用 WinAPI WritePrivateProfileString 写入 INI 文件
            WritePrivateProfileString(section, key, value, FilePath);
        }

    }
}
