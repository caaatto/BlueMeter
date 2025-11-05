using System;
using System.Timers;

using BlueMeter.WinForm.Plugin.Charts;
using BlueMeter.WinForm.Plugin.DamageStatistics;

namespace BlueMeter.WinForm.Plugin
{
    /// <summary>
    /// 图表配置管理器 - 统一处理各类图表的默认设置
    /// </summary>
    public static class ChartConfigManager
    {
        // 统一的默认常量
        public const string EMPTY_TEXT = "";
        public const bool HIDE_LEGEND = false;
        public const bool SHOW_GRID = true;
        public const bool SHOW_VIEW_INFO = false;
        public const bool AUTO_SCALE_FONT = false;
        public const bool PRESERVE_VIEW = true;
        public const int REFRESH_INTERVAL = 1000;
        public const int MIN_WIDTH = 450;
        public const int MIN_HEIGHT = 150;

        public static readonly Font DefaultFont = new("微软雅黑", 10, FontStyle.Regular);

        /// <summary>
        /// 统一应用图表默认配置
        /// </summary>
        public static T ApplySettings<T>(T chart) where T : UserControl
        {
            // 通用控件设置
            chart.Dock = DockStyle.Fill;

            // 根据图表类型应用特定设置
            switch (chart)
            {
                case FlatLineChart lineChart:
                    ApplyLineChartSettings(lineChart);
                    break;
                case FlatBarChart barChart:
                    ApplyBarChartSettings(barChart);
                    break;
                case FlatPieChart pieChart:
                    ApplyPieChartSettings(pieChart);
                    break;
                case FlatScatterChart scatterChart:
                    ApplyScatterChartSettings(scatterChart);
                    break;
            }

            return chart;
        }

        private static void ApplyLineChartSettings(FlatLineChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.XAxisLabel = EMPTY_TEXT;
            chart.YAxisLabel = EMPTY_TEXT;
            chart.ShowLegend = HIDE_LEGEND;
            chart.ShowGrid = SHOW_GRID;
            chart.ShowViewInfo = SHOW_VIEW_INFO;
            chart.AutoScaleFont = AUTO_SCALE_FONT;
            chart.PreserveViewOnDataUpdate = PRESERVE_VIEW;
            chart.IsDarkTheme = !AppConfig.IsLight;
            chart.MinimumSize = new Size(MIN_WIDTH, MIN_HEIGHT);
            chart.Font = DefaultFont;
        }

        private static void ApplyBarChartSettings(FlatBarChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.IsDarkTheme = !AppConfig.IsLight;
        }

        private static void ApplyPieChartSettings(FlatPieChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.IsDarkTheme = !AppConfig.IsLight;
            chart.ShowLabels = true;
            chart.ShowPercentages = true;
        }

        private static void ApplyScatterChartSettings(FlatScatterChart chart)
        {
            chart.TitleText = EMPTY_TEXT;
            chart.XAxisLabel = EMPTY_TEXT;
            chart.YAxisLabel = EMPTY_TEXT;
            chart.ShowLegend = true;
            chart.ShowGrid = SHOW_GRID;
            chart.IsDarkTheme = !AppConfig.IsLight;
        }
    }

    /// <summary>
    /// 图表数据来源
    /// </summary>
    public enum ChartDataSource
    {
        Current = 0,   // 当前战斗（单次）
        FullRecord = 1 // 全程（会话）
    }

    /// <summary>
    /// 图表数据类型
    /// </summary>
    public enum ChartDataType
    {
        Damage = 0,      // 伤害
        Healing = 1,     // 治疗 
        TakenDamage = 2  // 承伤
    }

    /// <summary>
    /// 实时图表可视化服务
    /// </summary>
    public static class ChartVisualizationService
    {
        #region 数据存储
        // ===== 将历史按数据源分离：Current 与 FullRecord 各自一份 =====
        private static readonly Dictionary<long, List<(DateTime Time, double Dps)>> _dpsHistoryCurrent = new();
        private static readonly Dictionary<long, List<(DateTime Time, double Hps)>> _hpsHistoryCurrent = new();
        private static readonly Dictionary<long, List<(DateTime Time, double TakenDps)>> _takenDpsHistoryCurrent = new();

        private static readonly Dictionary<long, List<(DateTime Time, double Dps)>> _dpsHistoryFull = new();
        private static readonly Dictionary<long, List<(DateTime Time, double Hps)>> _hpsHistoryFull = new();
        private static readonly Dictionary<long, List<(DateTime Time, double TakenDps)>> _takenDpsHistoryFull = new();

        private static DateTime? _currentCombatStartTime;
        private static DateTime? _fullCombatStartTime;

        private static readonly List<WeakReference> _registeredCharts = new();

        private const int MAX_HISTORY_POINTS = 500;
        private const double INACTIVE_TIMEOUT_SECONDS = 2.0;

        public static bool IsCapturing { get; private set; } = false;

        // 保持旧的默认数据源（用于未显式指定来源的 API）
        public static ChartDataSource DataSource { get; private set; } = ChartDataSource.Current;

        // 频率节流，避免多个图表同时触发重复采样
        private static DateTime _lastUpdateAt = DateTime.MinValue;
        private const int MIN_UPDATE_INTERVAL_MS = 200; // 两次采样至少间隔 200ms

        // 后台采样定时器（即使没有打开任何图表也会采样）
        private static System.Timers.Timer? _samplingTimer;

        // 当前战斗“从0到>0”的边沿检测，自动切分单次会话（改为依赖战斗时钟）
        private static bool _wasInCombat = false;

        // ===== 全程即时速率计算：使用“差分”获得实时值（避免使用累计平均） =====
        private static readonly Dictionary<long, (ulong Total, DateTime Ts)> _fullLastDamage = new();
        private static readonly Dictionary<long, (ulong Total, DateTime Ts)> _fullLastHealing = new();
        private static readonly Dictionary<long, (ulong Total, DateTime Ts)> _fullLastTaken = new();
        #endregion

        #region 数据更新
        /// <summary>
        /// 切换图表数据来源（会自动清空历史，避免数据混淆）。
        /// </summary>
        public static void SetDataSource(ChartDataSource source, bool clearHistory = true)
        {
            if (DataSource == source) return;
            DataSource = source;
            if (clearHistory)
            {
                if (source == ChartDataSource.Current)
                    ClearCurrentHistory();
                else
                    ClearFullHistory();
            }
        }

        // 内部：向指定历史集合添加一个数据点（保留最大点数）
        private static void AddDataPoint<T>(Dictionary<long, List<(DateTime, T)>> history, long playerId, T value)
        {
            var now = DateTime.Now;

            if (!history.TryGetValue(playerId, out var playerHistory))
            {
                playerHistory = new List<(DateTime, T)>();
                history[playerId] = playerHistory;
            }

            // 确保数值非负
            var safeValue = value is double d ? (T)(object)Math.Max(0, d) : value;
            playerHistory.Add((now, safeValue));

            if (playerHistory.Count > MAX_HISTORY_POINTS)
                playerHistory.RemoveAt(0);
        }

        // === 当前战斗：添加数据点，并在首次出现正值时设定起始时间 ===
        public static void AddDpsDataPointCurrent(long playerId, double dps)
        {
            if (_currentCombatStartTime is null && dps > 0)
                _currentCombatStartTime = DateTime.Now;
            AddDataPoint(_dpsHistoryCurrent, playerId, dps);
        }
        public static void AddHpsDataPointCurrent(long playerId, double hps)
        {
            if (_currentCombatStartTime is null && hps > 0)
                _currentCombatStartTime = DateTime.Now;
            AddDataPoint(_hpsHistoryCurrent, playerId, hps);
        }
        public static void AddTakenDpsDataPointCurrent(long playerId, double takenDps)
        {
            if (_currentCombatStartTime is null && takenDps > 0)
                _currentCombatStartTime = DateTime.Now;
            AddDataPoint(_takenDpsHistoryCurrent, playerId, takenDps);
        }

        // === 全程：添加数据点，并在首次出现正值时设定起始时间 ===
        public static void AddDpsDataPointFull(long playerId, double dps)
        {
            if (_fullCombatStartTime is null && dps > 0)
                _fullCombatStartTime = DateTime.Now;
            AddDataPoint(_dpsHistoryFull, playerId, dps);
        }
        public static void AddHpsDataPointFull(long playerId, double hps)
        {
            if (_fullCombatStartTime is null && hps > 0)
                _fullCombatStartTime = DateTime.Now;
            AddDataPoint(_hpsHistoryFull, playerId, hps);
        }
        public static void AddTakenDpsDataPointFull(long playerId, double takenDps)
        {
            if (_fullCombatStartTime is null && takenDps > 0)
                _fullCombatStartTime = DateTime.Now;
            AddDataPoint(_takenDpsHistoryFull, playerId, takenDps);
        }

        /// <summary>
        /// 统一采样：同时更新 Current 与 FullRecord 两套历史。
        /// </summary>
        public static void UpdateAllDataPoints()
        {
            // 若未处于采样状态（例如 F9 刚执行过停止与清空），则不进行任何更新，避免清空后马上被重填
            if (!IsCapturing) return;

            var now = DateTime.Now;
            if ((now - _lastUpdateAt).TotalMilliseconds < MIN_UPDATE_INTERVAL_MS)
                return;
            _lastUpdateAt = now;

            // === Current：来自 StatisticData 的实时值 ===
            var playersCurrent = StatisticData._manager.GetPlayersWithCombatData();
            foreach (var player in playersCurrent)
                player.UpdateRealtimeStats();

            // 用“战斗时钟状态”判断新的一场，避免因为实时窗口归零而误判
            var nowInCombat = StatisticData._manager.IsInCombat;
            if (!_wasInCombat && nowInCombat)
            {
                // 新战斗开始：清理单次历史，时间基与主界面时钟对齐
                ClearCurrentHistory();
                _currentCombatStartTime = DateTime.Now - StatisticData._manager.GetCombatDuration();
            }

            foreach (var player in playersCurrent)
            {
                AddDpsDataPointCurrent(player.Uid, player.DamageStats.RealtimeValue);
                AddHpsDataPointCurrent(player.Uid, player.HealingStats.RealtimeValue);
                AddTakenDpsDataPointCurrent(player.Uid, player.TakenStats.RealtimeValue);
            }
            _wasInCombat = nowInCombat;

            // === FullRecord：改为记录“实时变化率”（差分），而不是累计平均或总值 ===
            // 获取当前累计总量
            var totals = FullRecord.GetPlayersWithTotals(includeZero: true);
            foreach (var p in totals)
            {
                // Damage -> 实时DPS（差分）
                if (_fullLastDamage.TryGetValue(p.Uid, out var lastDmg))
                {
                    var dt = (now - lastDmg.Ts).TotalSeconds;
                    if (dt > 0)
                    {
                        var delta = (long)p.TotalDamage - (long)lastDmg.Total;
                        var dps = delta > 0 ? delta / dt : 0.0;
                        AddDpsDataPointFull(p.Uid, Math.Round(dps, 2, MidpointRounding.AwayFromZero));
                    }
                    _fullLastDamage[p.Uid] = ((ulong)Math.Max(0, (long)p.TotalDamage), now);
                }
                else
                {
                    _fullLastDamage[p.Uid] = (p.TotalDamage, now);
                }

                // Healing -> 实时HPS（差分）
                if (_fullLastHealing.TryGetValue(p.Uid, out var lastHeal))
                {
                    var dt = (now - lastHeal.Ts).TotalSeconds;
                    if (dt > 0)
                    {
                        var delta = (long)p.TotalHealing - (long)lastHeal.Total;
                        var hps = delta > 0 ? delta / dt : 0.0;
                        AddHpsDataPointFull(p.Uid, Math.Round(hps, 2, MidpointRounding.AwayFromZero));
                    }
                    _fullLastHealing[p.Uid] = ((ulong)Math.Max(0, (long)p.TotalHealing), now);
                }
                else
                {
                    _fullLastHealing[p.Uid] = (p.TotalHealing, now);
                }

                // Taken -> 实时承伤每秒（差分）
                if (_fullLastTaken.TryGetValue(p.Uid, out var lastTaken))
                {
                    var dt = (now - lastTaken.Ts).TotalSeconds;
                    if (dt > 0)
                    {
                        var delta = (long)p.TakenDamage - (long)lastTaken.Total;
                        var tps = delta > 0 ? delta / dt : 0.0;
                        AddTakenDpsDataPointFull(p.Uid, Math.Round(tps, 2, MidpointRounding.AwayFromZero));
                    }
                    _fullLastTaken[p.Uid] = ((ulong)Math.Max(0, (long)p.TakenDamage), now);
                }
                else
                {
                    _fullLastTaken[p.Uid] = (p.TakenDamage, now);
                }
            }

            CheckAndAddZeroValues();
        }

        private static void CheckAndAddZeroValues()
        {
            HashSet<long> activeCurrent = StatisticData._manager.GetPlayersWithCombatData().Select(p => p.Uid).ToHashSet();
            HashSet<long> activeFull = FullRecord.GetPlayersWithTotals(includeZero: false).Select(p => p.Uid).ToHashSet();

            var now = DateTime.Now;

            // 为非活跃玩家也补 0 值（防止长时间停留在上一数值）
            CheckHistoryForZeroValues(_dpsHistoryCurrent, activeCurrent, now, (id, _) => AddDpsDataPointCurrent(id, 0));
            CheckHistoryForZeroValues(_hpsHistoryCurrent, activeCurrent, now, (id, _) => AddHpsDataPointCurrent(id, 0));
            CheckHistoryForZeroValues(_takenDpsHistoryCurrent, activeCurrent, now, (id, _) => AddTakenDpsDataPointCurrent(id, 0));

            CheckHistoryForZeroValues(_dpsHistoryFull, activeFull, now, (id, _) => AddDpsDataPointFull(id, 0));
            CheckHistoryForZeroValues(_hpsHistoryFull, activeFull, now, (id, _) => AddHpsDataPointFull(id, 0));
            CheckHistoryForZeroValues(_takenDpsHistoryFull, activeFull, now, (id, _) => AddTakenDpsDataPointFull(id, 0));
        }

        private static void CheckHistoryForZeroValues<T>(Dictionary<long, List<(DateTime Time, T Value)>> history,
            HashSet<long> activePlayerIds, DateTime now, Action<long, T> addZeroValue)
            where T : struct, IComparable<T>
        {
            var zero = default(T);
            foreach (var playerId in history.Keys.ToList())
            {
                if (activePlayerIds.Contains(playerId)) continue;

                var playerHistory = history[playerId];
                if (playerHistory.Count > 0)
                {
                    var lastRecord = playerHistory.Last();
                    var timeSinceLastRecord = (now - lastRecord.Time).TotalSeconds;

                    if (timeSinceLastRecord > INACTIVE_TIMEOUT_SECONDS && lastRecord.Value.CompareTo(zero) > 0)
                        addZeroValue(playerId, zero);
                }
            }
        }

        public static void ClearAllHistory()
        {
            ClearCurrentHistory();
            ClearFullHistory();
        }

        public static void ClearCurrentHistory()
        {
            _dpsHistoryCurrent.Clear();
            _hpsHistoryCurrent.Clear();
            _takenDpsHistoryCurrent.Clear();
            _currentCombatStartTime = null;
        }

        public static void ClearFullHistory()
        {
            _dpsHistoryFull.Clear();
            _hpsHistoryFull.Clear();
            _takenDpsHistoryFull.Clear();
            _fullCombatStartTime = null;

            // 同步清空差分快照，避免跨会话造成瞬时高值
            _fullLastDamage.Clear();
            _fullLastHealing.Clear();
            _fullLastTaken.Clear();
        }

        public static void OnCombatEnd()
        {
            // 在战斗结束时压入 0 值，以便曲线自然回落
            foreach (var playerId in _dpsHistoryCurrent.Keys.ToList())
            {
                var history = _dpsHistoryCurrent[playerId];
                if (history.Count > 0 && history.Last().Dps > 0)
                    AddDpsDataPointCurrent(playerId, 0);
            }
            foreach (var playerId in _hpsHistoryCurrent.Keys.ToList())
            {
                var history = _hpsHistoryCurrent[playerId];
                if (history.Count > 0 && history.Last().Hps > 0)
                    AddHpsDataPointCurrent(playerId, 0);
            }
            foreach (var playerId in _takenDpsHistoryCurrent.Keys.ToList())
            {
                var history = _takenDpsHistoryCurrent[playerId];
                if (history.Count > 0 && history.Last().TakenDps > 0)
                    AddTakenDpsDataPointCurrent(playerId, 0);
            }

            // 全程也补 0 值，便于图形衔接
            foreach (var playerId in _dpsHistoryFull.Keys.ToList())
            {
                var history = _dpsHistoryFull[playerId];
                if (history.Count > 0 && history.Last().Dps > 0)
                    AddDpsDataPointFull(playerId, 0);
            }
            foreach (var playerId in _hpsHistoryFull.Keys.ToList())
            {
                var history = _hpsHistoryFull[playerId];
                if (history.Count > 0 && history.Last().Hps > 0)
                    AddHpsDataPointFull(playerId, 0);
            }
            foreach (var playerId in _takenDpsHistoryFull.Keys.ToList())
            {
                var history = _takenDpsHistoryFull[playerId];
                if (history.Count > 0 && history.Last().TakenDps > 0)
                    AddTakenDpsDataPointFull(playerId, 0);
            }
        }
        #endregion

        #region 图表创建
        /// <summary>
        /// 通用创建方法
        /// </summary>
        /// <typeparam name="T">图表控件类型：继承自 UserControl</typeparam>
        /// <param name="size">图表的初始大小</param>
        /// <param name="customConfig">可选：自定义配置回调，可修改图表控件的各种参数</param>
        /// <returns>已创建并应用默认配置的图表实例</returns>
        private static T CreateChart<T>(Size size, Action<T>? customConfig = null) where T : UserControl, new()
        {
            var chart = new T { Size = size };
            ChartConfigManager.ApplySettings(chart); // 应用统一的图表配置
            customConfig?.Invoke(chart); // 执行自定义配置
            return chart;
        }

        /// <summary>
        /// 创建 DPS 趋势折线图（默认使用全局 DataSource）
        /// </summary>
        public static FlatLineChart CreateDpsTrendChart(int width = 800, int height = 400, long? specificPlayerId = null)
        {
            var chart = CreateChart<FlatLineChart>(new Size(width, height));

            RegisterChart(chart); // 注册图表以便统一管理

            if (IsCapturing) // 若当前在捕获数据，则开启自动刷新
                chart.StartAutoRefresh(ChartConfigManager.REFRESH_INTERVAL);

            RefreshDpsTrendChart(chart, specificPlayerId); // 载入初始数据
            return chart;
        }

        /// <summary>
        /// 为指定数据源创建 DPS 曲线图（Current / FullRecord）。
        /// </summary>
        public static FlatLineChart CreateDpsTrendChartForSource(ChartDataSource source, int width = 800, int height = 400, long? specificPlayerId = null)
        {
            var chart = CreateChart<FlatLineChart>(new Size(width, height));
            RegisterChart(chart);
            if (IsCapturing) chart.StartAutoRefresh(ChartConfigManager.REFRESH_INTERVAL);
            RefreshDpsTrendChart(chart, specificPlayerId, ChartDataType.Damage, source);
            return chart;
        }

        /// <summary>
        /// ??????????????????FlatPieChart??
        /// </summary>
        public static FlatPieChart CreateSkillDamagePieChart(long playerId, int width = 400, int height = 400)
        {
            var chart = CreateChart<FlatPieChart>(new Size(width, height));
            RefreshSkillDamagePieChart(chart, playerId); // 初始刷新
            return chart;
        }

        /// <summary>
        /// 创建队伍 DPS 条形图（FlatBarChart）
        /// </summary>
        public static FlatBarChart CreateTeamDpsBarChart(int width = 600, int height = 400)
        {
            var chart = CreateChart<FlatBarChart>(new Size(width, height));
            RefreshTeamDpsBarChart(chart); // 初始刷新
            return chart;
        }

        /// <summary>
                /// 创建 DPS 散点图（FlatScatterChart）
/// </summary>
        public static FlatScatterChart CreateDpsRadarChart(int width = 400, int height = 400)
        {
            var chart = CreateChart<FlatScatterChart>(new Size(width, height));
            RefreshDpsRadarChart(chart); // 初始刷新
            return chart;
        }

        /// <summary>
        /// 创建伤害类型堆叠条形图（FlatBarChart）
        /// </summary>
        public static FlatBarChart CreateDamageTypeStackedChart(int width = 600, int height = 400)
        {
            var chart = CreateChart<FlatBarChart>(new Size(width, height));
            RefreshDamageTypeStackedChart(chart); // 初始刷新
            return chart;
        }

        #endregion

        #region 图表刷新
        /// <summary>
        /// 刷新 DPS 趋势图数据，支持单人/多人以及不同数据类型（默认使用全局 DataSource）
        /// </summary>
        public static void RefreshDpsTrendChart(FlatLineChart chart, long? specificPlayerId = null, ChartDataType dataType = ChartDataType.Damage)
            => RefreshDpsTrendChart(chart, specificPlayerId, dataType, DataSource);

        /// <summary>
        /// 按指定数据源刷新曲线（Current/FullRecord）。
        /// </summary>
        public static void RefreshDpsTrendChart(FlatLineChart chart, long? specificPlayerId, ChartDataType dataType, ChartDataSource source)
        {
            // 记录图表状态
            var timeScale = chart.GetTimeScale();
            var viewOffset = chart.GetViewOffset();
            var hadData = chart.HasData();

            chart.ClearSeries();

            // 选择对应历史
            Dictionary<long, List<(DateTime Time, double Value)>> historyData;
            DateTime? startTs;
            if (source == ChartDataSource.FullRecord)
            {
                historyData = dataType switch
                {
                    ChartDataType.Healing => _hpsHistoryFull.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.Hps)).ToList()),
                    ChartDataType.TakenDamage => _takenDpsHistoryFull.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.TakenDps)).ToList()),
                    _ => _dpsHistoryFull.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.Dps)).ToList()),
                };
                startTs = _fullCombatStartTime;
            }
            else
            {
                historyData = dataType switch
                {
                    ChartDataType.Healing => _hpsHistoryCurrent.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.Hps)).ToList()),
                    ChartDataType.TakenDamage => _takenDpsHistoryCurrent.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.TakenDps)).ToList()),
                    _ => _dpsHistoryCurrent.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(item => (item.Time, (double)item.Dps)).ToList()),
                };
                // 优先用“主界面战斗时长”推导出的开始时间，保证与单次计时一致
                startTs = (_currentCombatStartTime != null) ? _currentCombatStartTime : DateTime.Now - StatisticData._manager.GetCombatDuration();
            }

            // 若没有任何历史或起始时间未知（例如清空后），直接返回（保持“暂无数据”空态）
            if (historyData.Count == 0 || startTs == null)
            {
                chart.Invalidate();
                return;
            }

            var startTime = startTs.Value;

            if (specificPlayerId.HasValue)
            {
                RefreshSinglePlayerChart(chart, historyData, specificPlayerId.Value, startTime);
            }
            else
            {
                RefreshMultiPlayerChart(chart, historyData, startTime);
            }

            // 恢复视图
            if (hadData && chart.HasUserInteracted())
            {
                chart.SetTimeScale(timeScale);
                chart.SetViewOffset(viewOffset);
            }
        }

        private static void RefreshSinglePlayerChart(FlatLineChart chart, Dictionary<long, List<(DateTime Time, double Value)>> historyData,
            long playerId, DateTime startTime)
        {
            if (historyData.TryGetValue(playerId, out var playerHistory) && playerHistory.Count > 0)
            {
                var points = ConvertToPoints(playerHistory, startTime);
                if (points.Count > 0)
                {
                    // 确保从 0 秒开始：首个点若大于 0s，则补一个 (0,0)
                    if (points[0].X > 0f)
                    {
                        points.Insert(0, new PointF(0f, 0f));
                    }
                    chart.AddSeries("", points);
                }
            }
        }

        private static void RefreshMultiPlayerChart(FlatLineChart chart, Dictionary<long, List<(DateTime Time, double Value)>> historyData,
            DateTime startTime)
        {
            foreach (var (playerId, history) in historyData.OrderBy(x => x.Key))
            {
                if (history.Count == 0) continue;

                var points = ConvertToPoints(history, startTime);
                if (points.Count > 0)
                    chart.AddSeries("", points);
            }
        }

        private static List<PointF> ConvertToPoints(List<(DateTime Time, double Value)> history, DateTime startTime)
        {
            return history.Select(h => new PointF(
                (float)(h.Time - startTime).TotalSeconds,
                (float)h.Value
            )).ToList();
        }

        public static void RefreshSkillDamagePieChart(FlatPieChart chart, long playerId, ChartDataType dataType = ChartDataType.Damage)
        {
            chart.ClearData();

            try
            {
                // 根据数据类型获取相应的技能数据
                var skillData = dataType switch
                {
                    ChartDataType.Healing => StatisticData._manager.GetPlayerSkillSummaries(playerId, topN: 8, orderByTotalDesc: true, Core.SkillType.Heal),
                    ChartDataType.TakenDamage => StatisticData._manager.GetPlayerTakenDamageSummaries(playerId, topN: 8, orderByTotalDesc: true),
                    _ => StatisticData._manager.GetPlayerSkillSummaries(playerId, topN: 8, orderByTotalDesc: true, Core.SkillType.Damage)
                };

                if (skillData.Count == 0) return;

                var pieData = skillData.Select(s => (
                    Label: $"{s.SkillName}: {Common.FormatWithEnglishUnits(s.Total)}",
                    Value: (double)s.Total
                )).ToList();

                chart.SetData(pieData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刷新技能伤害饼图时出错: {ex.Message}");
            }
        }

        public static void RefreshTeamDpsBarChart(FlatBarChart chart)
        {
            chart.ClearData();
            var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
            if (players.Count == 0) return;

            var barData = players
                .OrderByDescending(p => p.GetTotalDps())
                .Select(p => (Label: "", Value: p.GetTotalDps()))
                .ToList();

            chart.SetData(barData);
        }

        public static void RefreshDpsRadarChart(FlatScatterChart chart)
        {
            chart.ClearSeries();
            var players = StatisticData._manager.GetPlayersWithCombatData().Take(5).ToList();
            if (players.Count == 0) return;

            foreach (var player in players)
            {
                var totalDps = player.GetTotalDps();
                var critRate = player.DamageStats.GetCritRate();
                var points = new List<PointF> { new((float)critRate, (float)totalDps) };
                chart.AddSeries("", points);
            }
        }

        public static void RefreshDamageTypeStackedChart(FlatBarChart chart)
        {
            chart.ClearData();
            var players = StatisticData._manager.GetPlayersWithCombatData()
                .OrderByDescending(p => p.DamageStats.Total)
                .Take(6)
                .ToList();

            if (players.Count == 0) return;

            var barData = players.Select(p => (Label: "", Value: (double)p.DamageStats.Total)).ToList();
            chart.SetData(barData);
        }
        #endregion

        #region 图表管理
        public static void RegisterChart(FlatLineChart chart)
        {
            lock (_registeredCharts)
            {
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
                _registeredCharts.Add(new WeakReference(chart));
            }
        }

        public static void StopAllChartsAutoRefresh()
        {
            IsCapturing = false;
            ExecuteOnRegisteredCharts(chart => chart.StopAutoRefresh());

            try { _samplingTimer?.Stop(); } catch { }
            try { _samplingTimer?.Dispose(); } catch { }
            _samplingTimer = null;
        }

        public static void StartAllChartsAutoRefresh(int intervalMs = 1000)
        {
            IsCapturing = true;
            ExecuteOnRegisteredCharts(chart => chart.StartAutoRefresh(intervalMs));

            // 启动后台采样（即使暂未打开任何图表也会积累数据）
            if (_samplingTimer == null)
            {
                _samplingTimer = new System.Timers.Timer(Math.Max(200, intervalMs));
                _samplingTimer.AutoReset = true;
                _samplingTimer.Elapsed += (_, __) =>
                {
                    try { UpdateAllDataPoints(); }
                    catch (Exception ex) { Console.WriteLine($"Chart sampling error: {ex.Message}"); }
                };
            }
            _samplingTimer.Interval = Math.Max(200, intervalMs);
            _samplingTimer.Start();

            // 同步一次“战斗状态”，避免刚启动时误判
            _wasInCombat = StatisticData._manager.IsInCombat;
        }

        public static void FullResetAllCharts()
        {
            ClearAllHistory();
            ExecuteOnRegisteredCharts(chart =>
            {
                try
                {
                    // 停止刷新并彻底清空曲线与视图
                    chart.StopAutoRefresh();
                    chart.ClearSeries();
                    chart.FullReset();
                    chart.Invalidate(); // 立即重绘空态
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FullReset chart error: {ex.Message}");
                }
            });
        }

        private static void ExecuteOnRegisteredCharts(Action<FlatLineChart> action)
        {
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try { action(chart); }
                        catch (Exception ex) { Console.WriteLine($"图表管理执行出错: {ex.Message}"); }
                    }
                }
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }
        #endregion

        #region 其它工具
        public static bool HasDataToVisualize() =>
            StatisticData._manager.GetPlayersWithCombatData().Any();

        public static double GetCombatDurationSeconds(ChartDataSource source = ChartDataSource.Current) =>
            (source == ChartDataSource.Current ? _currentCombatStartTime : _fullCombatStartTime)?.Let(start => (DateTime.Now - start).TotalSeconds) ?? 0;

        public static int GetDpsHistoryPointCount() =>
            _dpsHistoryCurrent.Sum(kvp => kvp.Value.Count) + _dpsHistoryFull.Sum(kvp => kvp.Value.Count);
        #endregion
    }

    /// <summary>
    /// 扩展工具方法
    /// </summary>
    public static class Extensions
    {
        public static TResult Let<T, TResult>(this T obj, Func<T, TResult> func) => func(obj);
    }
}