using System.Drawing.Drawing2D;

namespace StarResonanceDpsAnalysis.WinForm.Plugin.Charts
{
    /// <summary>
    /// 扁平化线性图控件 - 支持拖动、缩放和实时刷新功能
    /// </summary>
    public class FlatLineChart : UserControl
    {
        #region 字段和属性

        private readonly List<LineChartSeries> _series = new();
        private bool _isDarkTheme = false;
        private string _titleText = "";
        private string _xAxisLabel = "";
        private string _yAxisLabel = "";
        private bool _showLegend = true;
        private bool _showGrid = true;
        private bool _showViewInfo = false;
        private bool _autoScaleFont = true; // 新增：控制字体自适应

        // 边距设置 - 调整边距使图表左对齐，保持较窄的宽度
        private const int PaddingLeft = 60;    // 恢复到正常的左边距
        private int _paddingRight = 160;       // 改为实例变量，支持动态调整
        private const int PaddingTop = 35;     // 保持顶部边距
        private const int PaddingBottom = 45;  // 保持底部边距

        // 网格线配置
        private int _verticalGridLines = 5;    // 垂直网格线数量（默认6条线，0-5）

        // 字体大小设置（基础大小，会根据图表大小调整）
        private const float BaseTitleFontSize = 12f;    // 减小标题字体从14到12
        private const float BaseAxisLabelFontSize = 8f;  // 减少轴标签字体从9到8
        private const float BaseAxisValueFontSize = 7f;  // 减少轴数值字体从8到7
        private const float BaseLegendFontSize = 7f;     // 减小图例字体从8到7
        private const float BaseNoDataFontSize = 9f;     // 减小无数据提示字体从10到9

        // 缩放和视图相关
        private float _timeScale = 1.0f;
        private float _viewOffset = 0.0f;
        private float _currentTimeSeconds = 0.0f;

        // 数据持久化
        private readonly Dictionary<string, List<PointF>> _persistentData = new();

        // 鼠标交互相关
        private Point _lastMousePosition;
        private bool _isPanning = false;
        private ToolTip _tooltip;
        private bool _showTooltip = false;
        private string _tooltipText = "";

        // 实时刷新相关
        private System.Windows.Forms.Timer _refreshTimer;
        private bool _autoRefreshEnabled = false;
        private int _refreshInterval = 1000;
        private Action _refreshCallback;

        // 视图保持相关
        private bool _preserveViewOnDataUpdate = true; // 新增：控制数据更新时是否保持视图
        private DateTime _lastUserInteraction = DateTime.MinValue; // 新增：记录最后用户交互时间
        private const double UserInteractionCooldownMs = double.MaxValue; // 修改：永不过期的用户交互保护时间
        private bool _hasUserInteracted = false; // 新增：标记用户是否有过交互

        // 自适应字体相关
        private float _fontScaleFactor = 1.0f;
        private const float MinFontSize = 6f;
        private const float MaxFontSize = 24f;
        private const int BaseFontSize = 8; // 基础字体大小
        private const int BaseWidth = 400; // 基础宽度
        private const int BaseHeight = 200; // 基础高度

        // 颜色配置
        private readonly Color[] _colors = {
            Color.FromArgb(255, 99, 132),   // 红
            Color.FromArgb(54, 162, 235),   // 蓝
            Color.FromArgb(255, 206, 86),   // 黄
            Color.FromArgb(75, 192, 192),   // 青
            Color.FromArgb(153, 102, 255),  // 紫
            Color.FromArgb(255, 159, 64),   // 橙
            Color.FromArgb(199, 199, 199),  // 灰
            Color.FromArgb(83, 102, 255),   // 靛蓝
            Color.FromArgb(255, 99, 255),   // 品红
            Color.FromArgb(99, 255, 132),   // 绿
        };

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                _isDarkTheme = value;
                ApplyTheme();
                Invalidate();
            }
        }

        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value;
                Invalidate();
            }
        }

        public string XAxisLabel
        {
            get => _xAxisLabel;
            set
            {
                _xAxisLabel = value;
                Invalidate();
            }
        }

        public string YAxisLabel
        {
            get => _yAxisLabel;
            set
            {
                _yAxisLabel = value;
                Invalidate();
            }
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set
            {
                _showLegend = value;
                Invalidate();
            }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
                Invalidate();
            }
        }

        public bool ShowViewInfo
        {
            get => _showViewInfo;
            set
            {
                _showViewInfo = value;
                Invalidate();
            }
        }

        public bool AutoScaleFont
        {
            get => _autoScaleFont;
            set
            {
                _autoScaleFont = value;
                Invalidate();
            }
        }

        public bool AutoRefreshEnabled
        {
            get => _autoRefreshEnabled;
            set
            {
                _autoRefreshEnabled = value;
                if (_refreshTimer != null)
                {
                    _refreshTimer.Enabled = value;
                }
            }
        }

        public int RefreshInterval
        {
            get => _refreshInterval;
            set
            {
                _refreshInterval = Math.Max(100, value);
                if (_refreshTimer != null)
                {
                    _refreshTimer.Interval = _refreshInterval;
                }
            }
        }

        public bool PreserveViewOnDataUpdate
        {
            get => _preserveViewOnDataUpdate;
            set
            {
                _preserveViewOnDataUpdate = value;
            }
        }

        /// <summary>
        /// 获取当前时间缩放
        /// </summary>
        public float GetTimeScale()
        {
            return _timeScale;
        }

        /// <summary>
        /// 获取当前视图偏移
        /// </summary>
        public float GetViewOffset()
        {
            return _viewOffset;
        }

        /// <summary>
        /// 检查图表是否有数据
        /// </summary>
        public bool HasData()
        {
            return _series.Count > 0 && _series.Any(s => s.Points.Count > 0);
        }

        /// <summary>
        /// 检查用户是否有过交互
        /// </summary>
        public bool HasUserInteracted()
        {
            return _hasUserInteracted;
        }

        /// <summary>
        /// 设置右侧内边距
        /// </summary>
        public void SetPaddingRight(int paddingRight)
        {
            _paddingRight = Math.Max(10, paddingRight); // 最小值为10
            Invalidate();
        }

        /// <summary>
        /// 获取当前右侧内边距
        /// </summary>
        public int GetPaddingRight()
        {
            return _paddingRight;
        }

        /// <summary>
        /// 设置垂直网格线数量
        /// </summary>
        public void SetVerticalGridLines(int lineCount)
        {
            _verticalGridLines = Math.Max(3, Math.Min(10, lineCount)); // 修改：限制范围3-10之间，从20改为10
            Invalidate();
        }

        /// <summary>
        /// 获取当前垂直网格线数量
        /// </summary>
        public int GetVerticalGridLines()
        {
            return _verticalGridLines;
        }

        #endregion

        #region 构造函数

        public FlatLineChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable | ControlStyles.UserMouse, true);

            // 初始化工具提示
            _tooltip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 100,
                ReshowDelay = 500,
                ShowAlways = true,
                IsBalloon = true
            };

            // 初始化实时刷新定时器
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = _refreshInterval,
                Enabled = false
            };
            _refreshTimer.Tick += RefreshTimer_Tick;

            ApplyTheme();

            // 注册鼠标事件
            MouseMove += OnChartMouseMove;
            MouseWheel += OnChartMouseWheel;
            MouseDown += OnChartMouseDown;
            MouseUp += OnChartMouseUp;
            MouseEnter += OnChartMouseEnter;
            KeyDown += OnChartKeyDown;

            // 允许控件接收焦点以处理键盘事件
            TabStop = true;
        }

        #endregion

        #region 实时刷新方法

        /// <summary>
        /// 设置刷新回调函数
        /// </summary>
        public void SetRefreshCallback(Action callback)
        {
            _refreshCallback = callback;
        }

        /// <summary>
        /// 启动实时刷新
        /// </summary>
        public void StartAutoRefresh(int intervalMs = 1000)
        {
            RefreshInterval = intervalMs;
            AutoRefreshEnabled = true;
        }

        /// <summary>
        /// 停止实时刷新
        /// </summary>
        public void StopAutoRefresh()
        {
            AutoRefreshEnabled = false;
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // 如果用户正在拖动，跳过此次刷新以避免中断操作
                if (_isPanning)
                {
                    return;
                }

                // 如果停止抓包了，完全保持用户的视图状态，永不回弹
                if (!ChartVisualizationService.IsCapturing)
                {
                    // 停止抓包后，只执行数据刷新回调，但完全保持视图状态
                    _refreshCallback?.Invoke();
                    Invalidate();
                    return;
                }

                // 保存当前的视图状态 - 永远保持用户设置的视图
                var currentTimeScale = _timeScale;
                var currentViewOffset = _viewOffset;

                _refreshCallback?.Invoke();

                // 如果启用了视图保持功能，永远恢复用户的设置（移除时间限制）
                if (_preserveViewOnDataUpdate)
                {
                    _timeScale = currentTimeScale;
                    _viewOffset = currentViewOffset;
                    ClampViewOffset(); // 重新约束偏移量以确保有效性
                }

                Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"图表自动刷新时出错: {ex.Message}");
            }
        }

        #endregion

        #region 字体自适应方法

        /// <summary>
        /// 根据图表大小计算自适应字体大小
        /// </summary>
        private float CalculateScaledFontSize(float baseFontSize)
        {
            if (!_autoScaleFont) return baseFontSize;

            // 基于图表宽度和高度计算缩放因子
            var baseWidth = 500f;  // 提高基准宽度从400到500
            var baseHeight = 200f; // 保持基准高度200

            var widthScale = Width / baseWidth;
            var heightScale = Height / baseHeight;

            // 取较小的缩放因子，避免字体过大
            var scale = Math.Min(widthScale, heightScale);

            // 更保守的缩放范围，避免文字过大
            scale = Math.Max(0.7f, Math.Min(1.4f, scale)); // 调整范围从0.6-1.8到0.7-1.4

            return baseFontSize * scale;
        }

        /// <summary>
        /// 根据区域大小计算字体大小（用于轴标签等需要更精细控制的区域）
        /// </summary>
        private float CalculateScaledFontSizeForArea(float baseFontSize, float areaWidth, float areaHeight)
        {
            if (!_autoScaleFont) return baseFontSize;

            // 根据可用区域计算合适的字体大小
            var baseAreaWidth = 300f;  // 提高基准宽度从200到300
            var baseAreaHeight = 120f; // 提高基准高度从100到120

            var widthScale = areaWidth / baseAreaWidth;
            var heightScale = areaHeight / baseAreaHeight;

            var scale = Math.Min(widthScale, heightScale);

            // 更保守的缩放范围，避免文字过大或过小
            scale = Math.Max(0.8f, Math.Min(1.2f, scale)); // 调整范围从0.7-1.5到0.8-1.2

            return baseFontSize * scale;
        }

        /// <summary>
        /// 创建自适应字体
        /// </summary>
        private Font CreateScaledFont(string fontFamily, float baseFontSize, FontStyle style = FontStyle.Regular)
        {
            var scaledSize = CalculateScaledFontSize(baseFontSize);
            // 更严格的字体大小限制，避免文字过大
            scaledSize = Math.Max(6f, Math.Min(16f, scaledSize)); // 将最大值从24降到16
            return new Font(fontFamily, scaledSize, style);
        }

        /// <summary>
        /// 创建区域自适应字体
        /// </summary>
        private Font CreateScaledFontForArea(string fontFamily, float baseFontSize, float areaWidth, float areaHeight, FontStyle style = FontStyle.Regular)
        {
            var scaledSize = CalculateScaledFontSizeForArea(baseFontSize, areaWidth, areaHeight);
            scaledSize = Math.Max(6f, Math.Min(14f, scaledSize)); // 将最大值从20降到14
            return new Font(fontFamily, scaledSize, style);
        }

        #endregion

        #region 数据管理

        public void AddSeries(string name, List<PointF> points)
        {
            // 保存当前的视图状态
            var currentTimeScale = _timeScale;
            var currentViewOffset = _viewOffset;
            // 如果停止抓包了，总是保持当前视图
            var shouldPreserveView = _series.Count > 0 || !ChartVisualizationService.IsCapturing;

            _persistentData[name] = new List<PointF>(points);

            var series = new LineChartSeries
            {
                Name = name,
                Points = new List<PointF>(points),
                Color = _colors[_series.Count % _colors.Length],
                LineWidth = 3.5f
            };

            _series.Add(series);

            if (points.Count > 0)
            {
                _currentTimeSeconds = Math.Max(_currentTimeSeconds, points.Max(p => p.X));
            }

            // 如果应该保持视图，则恢复之前的缩放和偏移
            if (shouldPreserveView)
            {
                _timeScale = currentTimeScale;
                _viewOffset = currentViewOffset;
                // 停止抓包时不限制视图偏移
                if (ChartVisualizationService.IsCapturing)
                {
                    ClampViewOffset();
                }
            }

            Invalidate();
        }

        public void ClearSeries()
        {
            _series.Clear();
            // 只有在明确清空时才重置视图，而且要检查是否有用户交互
            if (!_hasUserInteracted)
            {
                ResetViewToDefault();
            }
            Invalidate();
        }

        public void UpdateSeries(string name, List<PointF> points)
        {
            // 保存当前的视图状态
            var currentTimeScale = _timeScale;
            var currentViewOffset = _viewOffset;

            _persistentData[name] = new List<PointF>(points);

            var series = _series.FirstOrDefault(s => s.Name == name);
            if (series != null)
            {
                series.Points = new List<PointF>(points);

                if (points.Count > 0)
                {
                    _currentTimeSeconds = Math.Max(_currentTimeSeconds, points.Max(p => p.X));
                }

                // 如果停止抓包了，完全保持用户的视图状态
                if (!ChartVisualizationService.IsCapturing)
                {
                    _timeScale = currentTimeScale;
                    _viewOffset = currentViewOffset;
                    // 停止抓包时不调用ClampViewOffset
                }
                else
                {
                    // 正在抓包时，恢复用户的视图状态，避免更新数据时重置视图
                    _timeScale = currentTimeScale;
                    _viewOffset = currentViewOffset;
                    ClampViewOffset();
                }

                Invalidate();
            }
        }

        public void ReloadPersistentData()
        {
            _series.Clear();
            int colorIndex = 0;

            foreach (var kvp in _persistentData)
            {
                var series = new LineChartSeries
                {
                    Name = kvp.Key,
                    Points = new List<PointF>(kvp.Value),
                    Color = _colors[colorIndex % _colors.Length],
                    LineWidth = 3.5f
                };
                _series.Add(series);
                colorIndex++;
            }

            Invalidate();
        }

        #endregion

        #region 视图控制

        public void SetTimeScale(float scale)
        {
            var oldScale = _timeScale;
            _timeScale = Math.Max(0.1f, Math.Min(10.0f, scale));

            // 获取缩放前后的视图宽度
            var oldViewWidth = GetViewTimeRange(oldScale);
            var newViewWidth = GetViewTimeRange(_timeScale);

            // 计算当前视图的中心点（用户正在查看的位置）
            var currentViewCenter = _viewOffset + oldViewWidth / 2;

            // 以当前视图中心为基准调整偏移量，保持用户当前查看的位置
            _viewOffset = currentViewCenter - newViewWidth / 2;

            // 只有在抓包状态时才限制视图偏移
            if (ChartVisualizationService.IsCapturing)
            {
                ClampViewOffset();
            }

            Invalidate();
        }

        public void SetViewOffset(float offset)
        {
            _viewOffset = offset;
            // 只有在抓包状态时才限制视图偏移
            if (ChartVisualizationService.IsCapturing)
            {
                ClampViewOffset();
            }
            Invalidate();
        }

        public void ResetViewToDefault()
        {
            _timeScale = 1.0f;
            // 修改默认视图偏移，使其从0秒开始显示5秒范围
            _viewOffset = Math.Max(0, _currentTimeSeconds - 5);
            ClampViewOffset();
            Invalidate();
        }

        public void ResetZoomAndPan()
        {
            ResetViewToDefault();
        }

        private void ClampViewOffset()
        {
            // 如果停止抓包了，完全禁用视图偏移限制，用户可以自由拖动到任何位置
            if (!ChartVisualizationService.IsCapturing)
            {
                // 停止抓包后不限制视图偏移，用户可以拖动到任何时间点
                return;
            }

            var viewWidth = GetViewTimeRange(_timeScale);
            var maxOffset = _currentTimeSeconds - viewWidth;
            var minOffset = Math.Max(0, _currentTimeSeconds - 300);

            _viewOffset = Math.Max(minOffset, Math.Min(maxOffset, _viewOffset));
        }

        private float GetViewTimeRange(float scale)
        {
            // 修改默认时间范围从10秒改为5秒
            return 30.0f / scale;
        }

        #endregion

        #region 主题管理

        private void ApplyTheme()
        {
            if (_isDarkTheme)
            {
                BackColor = Color.FromArgb(31, 31, 31);
                ForeColor = Color.White;
            }
            else
            {
                BackColor = Color.White;
                ForeColor = Color.Black;
            }
        }

        #endregion

        #region 鼠标事件处理

        private void OnChartMouseEnter(object sender, EventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }
        }

        private void OnChartMouseMove(object sender, MouseEventArgs e)
        {
            var chartRect = new Rectangle(PaddingLeft, PaddingTop,
                                        Width - PaddingLeft - _paddingRight,
                                        Height - PaddingTop - PaddingBottom);

            if (chartRect.Contains(e.Location))
            {
                if (_isPanning && e.Button == MouseButtons.Left)
                {
                    _lastUserInteraction = DateTime.Now; // 记录用户交互时间
                    _hasUserInteracted = true; // 标记用户有交互

                    var deltaX = e.X - _lastMousePosition.X;
                    var timeRange = GetViewTimeRange(_timeScale);
                    var timeDelta = -deltaX * timeRange / chartRect.Width;

                    SetViewOffset(_viewOffset + timeDelta);
                    _lastMousePosition = e.Location;
                    return;
                }

                if (!_isPanning)
                {
                    FindNearestDataPoint(e.Location, chartRect);
                }
            }
            else
            {
                HideTooltip();
            }

            _lastMousePosition = e.Location;
        }

        private void OnChartMouseWheel(object sender, MouseEventArgs e)
        {
            if (!Focused)
            {
                Focus();
            }

            var shouldZoom = (ModifierKeys & Keys.Control) == Keys.Control || !_isPanning;

            if (shouldZoom)
            {
                _lastUserInteraction = DateTime.Now; // 记录用户交互时间
                _hasUserInteracted = true; // 标记用户有交互

                var chartRect = new Rectangle(PaddingLeft, PaddingTop,
                                            Width - PaddingLeft - _paddingRight,
                                            Height - PaddingTop - PaddingBottom);

                if (chartRect.Contains(e.Location))
                {
                    var scaleDelta = e.Delta > 0 ? 1.1f : 0.9f;
                    var newScale = _timeScale * scaleDelta;

                    if (newScale >= 0.1f && newScale <= 20.0f)
                    {
                        SetTimeScale(newScale);
                    }
                }
            }
        }

        private void OnChartMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!Focused)
                {
                    Focus();
                }

                var chartRect = new Rectangle(PaddingLeft, PaddingTop,
                                            Width - PaddingLeft - _paddingRight,
                                            Height - PaddingTop - PaddingBottom);

                if (chartRect.Contains(e.Location))
                {
                    _lastUserInteraction = DateTime.Now; // 记录用户交互时间
                    _hasUserInteracted = true; // 标记用户有交互
                    _isPanning = true;
                    _lastMousePosition = e.Location;
                    Cursor = Cursors.Hand;
                    HideTooltip();
                }
            }
        }

        private void OnChartMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isPanning)
            {
                _isPanning = false;
                Cursor = Cursors.Default;

                var timer = new System.Windows.Forms.Timer { Interval = 100 };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void OnChartKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R)
            {
                ResetViewToDefault();
                e.Handled = true;
            }
        }

        #endregion

        #region 数据点查找和提示

        private void FindNearestDataPoint(Point mouseLocation, Rectangle chartRect)
        {
            if (_series.Count == 0) return;

            var viewRange = CalculateViewRange();
            if (viewRange.IsEmpty) return;

            var minDistance = double.MaxValue;
            string bestTooltip = "";
            var found = false;

            foreach (var series in _series)
            {
                if (series.Points.Count == 0) continue;

                foreach (var point in series.Points)
                {
                    if (point.X < viewRange.X || point.X > viewRange.X + viewRange.Width)
                        continue;

                    var screenX = chartRect.X + ((point.X - viewRange.X) / viewRange.Width) * chartRect.Width;
                    var screenY = chartRect.Bottom - (point.Y - viewRange.Y) / viewRange.Height * chartRect.Height;

                    var distance = Math.Sqrt(Math.Pow(mouseLocation.X - screenX, 2) + Math.Pow(mouseLocation.Y - screenY, 2));

                    if (distance < 15 && distance < minDistance)
                    {
                        minDistance = distance;
                        var timeText = FormatTimeLabel(point.X);
                        var dpsText = Common.FormatWithEnglishUnits(point.Y);
                        bestTooltip = $"{series.Name}\n时间: {timeText}\nDPS: {dpsText}";
                        found = true;
                    }
                }
            }

            if (found)
            {
                ShowTooltip(bestTooltip, mouseLocation);
            }
            else
            {
                HideTooltip();
            }
        }

        private void ShowTooltip(string text, Point location)
        {
            if (_tooltipText != text)
            {
                _tooltipText = text;
                _showTooltip = true;
                _tooltip.Show(text, this, location.X + 10, location.Y - 30, 3000);
            }
        }

        private void HideTooltip()
        {
            if (_showTooltip)
            {
                _showTooltip = false;
                _tooltip.Hide(this);
            }
        }

        #endregion

        #region 重写方法以确保焦点处理

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.R)
            {
                ResetViewToDefault();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.R)
                return true;
            return base.IsInputKey(keyData);
        }

        #endregion

        #region 绘制方法

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.Clear(BackColor);

            if (_series.Count == 0)
            {
                DrawNoDataMessage(g);
                return;
            }

            var viewRange = CalculateViewRange();
            if (viewRange.IsEmpty) return;

            var chartRect = new Rectangle(PaddingLeft, PaddingTop,
                                        Width - PaddingLeft - _paddingRight,
                                        Height - PaddingTop - PaddingBottom);

            if (_showGrid)
            {
                DrawGrid(g, chartRect, viewRange);
            }

            DrawAxes(g, chartRect, viewRange);

            var clipRect = new Rectangle(chartRect.X - 1, chartRect.Y - 1,
                                        chartRect.Width + 2, chartRect.Height + 2);
            g.SetClip(clipRect);

            DrawDataLines(g, chartRect, viewRange);
            g.ResetClip();

            DrawTitle(g);

            if (_showViewInfo)
            {
                DrawViewInfo(g);
            }

            if (_showLegend && _series.Count > 0)
            {
                DrawLegend(g);
            }
        }

        private void DrawNoDataMessage(Graphics g)
        {
            var message = "暂无数据\n\n使用方法：\n? Ctrl + 鼠标滚轮：缩放时间轴\n? 左键拖动：平移视图\n? R键：重置视图\n? 鼠标悬停：查看数据";
            using var font = CreateScaledFont("Microsoft YaHei", BaseNoDataFontSize, FontStyle.Regular);
            using var brush = new SolidBrush(_isDarkTheme ? Color.Gray : Color.DarkGray);

            var size = g.MeasureString(message, font);
            var x = (Width - size.Width) / 2;
            var y = (Height - size.Height) / 2;

            g.DrawString(message, font, brush, x, y);
        }

        private RectangleF CalculateViewRange()
        {
            if (_series.Count == 0) return RectangleF.Empty;

            var allPoints = _series.SelectMany(s => s.Points);
            if (!allPoints.Any()) return RectangleF.Empty;

            var minY = 0f;
            var maxY = allPoints.Max(p => p.Y);
            var rangeY = maxY - minY;
            if (rangeY == 0) rangeY = 1;

            var viewWidth = GetViewTimeRange(_timeScale);
            var viewMinX = _viewOffset;

            return new RectangleF(
                viewMinX,
                minY,
                viewWidth,
                rangeY * 1.15f
            );
        }

        private void DrawGrid(Graphics g, Rectangle chartRect, RectangleF viewRange)
        {
            var gridColor = _isDarkTheme ? Color.FromArgb(64, 64, 64) : Color.FromArgb(230, 230, 230);
            using var gridPen = new Pen(gridColor, 1);

            // 垂直网格线 - 根据动态调整的_verticalGridLines数量绘制
            // _verticalGridLines表示线条数量，实际标签点数量是_verticalGridLines + 1
            for (int i = 0; i <= _verticalGridLines; i++)
            {
                var x = chartRect.X + (float)chartRect.Width * i / _verticalGridLines;
                g.DrawLine(gridPen, x, chartRect.Y, x, chartRect.Bottom);
            }

            // 水平网格线 - 保持固定的6条线(0-5)
            for (int i = 0; i <= 5; i++)
            {
                var y = chartRect.Y + (float)chartRect.Height * i / 5;
                g.DrawLine(gridPen, chartRect.X, y, chartRect.Right, y);
            }
        }

        private void DrawAxes(Graphics g, Rectangle chartRect, RectangleF viewRange)
        {
            var axisColor = _isDarkTheme ? Color.FromArgb(128, 128, 128) : Color.FromArgb(180, 180, 180);
            using var axisPen = new Pen(axisColor, 1);
            using var textBrush = new SolidBrush(ForeColor);

            // 为轴标签使用基于图表区域的字体大小
            using var font = CreateScaledFontForArea("Microsoft YaHei", BaseAxisValueFontSize, chartRect.Width, chartRect.Height);

            g.DrawLine(axisPen, chartRect.X, chartRect.Bottom, chartRect.Right, chartRect.Bottom);
            g.DrawLine(axisPen, chartRect.X, chartRect.Y, chartRect.X, chartRect.Bottom);

            // X轴时间标签 - 根据动态调整的_verticalGridLines数量绘制标签
            // _verticalGridLines条线对应_verticalGridLines + 1个标签点
            for (int i = 0; i <= _verticalGridLines; i++)
            {
                var x = chartRect.X + (float)chartRect.Width * i / _verticalGridLines;
                var timeValue = viewRange.X + viewRange.Width * i / _verticalGridLines;
                var text = FormatTimeLabel(timeValue);

                var size = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, x - size.Width / 2, chartRect.Bottom + 8);
            }

            // Y轴数值标签 - 调整为6个标签
            for (int i = 0; i <= 5; i++) // 从4改为5，6个标签(0-5)
            {
                var y = chartRect.Bottom - (float)chartRect.Height * i / 5; // 分母从4改为5
                var value = viewRange.Y + viewRange.Height * i / 5; // 分母从4改为5
                var text = Common.FormatWithEnglishUnits(value);

                var size = g.MeasureString(text, font);
                var labelX = Math.Max(5, chartRect.X - size.Width - 8);
                g.DrawString(text, font, textBrush, labelX, y - size.Height / 2);
            }

            if (!string.IsNullOrEmpty(_xAxisLabel))
            {
                using var axisFont = CreateScaledFont("Microsoft YaHei", BaseAxisLabelFontSize);
                var size = g.MeasureString(_xAxisLabel, axisFont);
                var x = chartRect.X + (chartRect.Width - size.Width) / 2;
                var y = chartRect.Bottom + Math.Max(20, 45 * CalculateScaledFontSize(BaseAxisLabelFontSize) / BaseAxisLabelFontSize);
                g.DrawString(_xAxisLabel, axisFont, textBrush, x, y);
            }

            if (!string.IsNullOrEmpty(_yAxisLabel))
            {
                using var axisFont = CreateScaledFont("Microsoft YaHei", BaseAxisLabelFontSize);
                var size = g.MeasureString(_yAxisLabel, axisFont);
                using var matrix = new Matrix();
                matrix.RotateAt(-90, new PointF(20, chartRect.Y + (chartRect.Height + size.Width) / 2));
                g.Transform = matrix;
                g.DrawString(_yAxisLabel, axisFont, textBrush, 20, chartRect.Y + (chartRect.Height + size.Width) / 2);
                g.ResetTransform();
            }
        }

        private string FormatTimeLabel(float seconds)
        {
            if (seconds < 60)
            {
                return $"{seconds:F0}s";
            }
            else
            {
                var minutes = (int)(seconds / 60);
                var remainingSeconds = (int)(seconds % 60);
                return $"{minutes}m{remainingSeconds:D2}s";
            }
        }

        private void DrawDataLines(Graphics g, Rectangle chartRect, RectangleF viewRange)
        {
            foreach (var series in _series)
            {
                if (series.Points.Count < 2) continue;

                using var pen = new Pen(series.Color, series.LineWidth);
                pen.LineJoin = LineJoin.Round;
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                var visiblePoints = series.Points
                    .Where(p => p.X >= viewRange.X && p.X <= viewRange.X + viewRange.Width)
                    .Select(p =>
                    {
                        var screenX = chartRect.X + ((p.X - viewRange.X) / viewRange.Width) * chartRect.Width;
                        var screenY = chartRect.Bottom - (p.Y - viewRange.Y) / viewRange.Height * chartRect.Height;

                        screenX = Math.Max(chartRect.X, Math.Min(chartRect.Right, screenX));
                        screenY = Math.Max(chartRect.Y, Math.Min(chartRect.Bottom, screenY));

                        return new PointF(screenX, screenY);
                    }).ToArray();

                if (visiblePoints.Length < 2) continue;

                try
                {
                    if (visiblePoints.Length >= 3)
                    {
                        g.DrawCurve(pen, visiblePoints, 0.6f);
                    }
                    else
                    {
                        g.DrawLines(pen, visiblePoints);
                    }
                }
                catch
                {
                    for (int i = 0; i < visiblePoints.Length - 1; i++)
                    {
                        try
                        {
                            g.DrawLine(pen, visiblePoints[i], visiblePoints[i + 1]);
                        }
                        catch { }
                    }
                }
            }
        }

        private void DrawTitle(Graphics g)
        {
            if (string.IsNullOrEmpty(_titleText)) return;

            using var font = CreateScaledFont("Microsoft YaHei", BaseTitleFontSize, FontStyle.Bold);
            using var brush = new SolidBrush(ForeColor);

            var size = g.MeasureString(_titleText, font);
            var x = (Width - size.Width) / 2;
            var y = 15;

            g.DrawString(_titleText, font, brush, x, y);
        }

        private void DrawViewInfo(Graphics g)
        {
            var info = $"缩放: {_timeScale:F1}x | 当前时间: {FormatTimeLabel(_currentTimeSeconds)}";

            using var font = CreateScaledFont("Microsoft YaHei", BaseAxisValueFontSize);
            using var brush = new SolidBrush(_isDarkTheme ? Color.LightGray : Color.DarkGray);

            var size = g.MeasureString(info, font);
            g.DrawString(info, font, brush, Width - size.Width - 10, Height - size.Height - 5);
        }

        private void DrawLegend(Graphics g)
        {
            using var font = CreateScaledFont("Microsoft YaHei", BaseLegendFontSize);
            using var textBrush = new SolidBrush(ForeColor);

            // 根据字体大小调整图例项的间距
            var scaledItemHeight = (int)(18 * CalculateScaledFontSize(BaseLegendFontSize) / BaseLegendFontSize);
            var legendHeight = _series.Count * scaledItemHeight + 10;
            var maxTextWidth = _series.Max(s => (int)g.MeasureString(s.Name, font).Width);
            var legendWidth = maxTextWidth + 35;
            var legendX = Width - legendWidth - 15;
            var legendY = PaddingTop + 15;

            var legendBg = _isDarkTheme ? Color.FromArgb(50, 50, 50) : Color.FromArgb(245, 245, 245);
            using var bgBrush = new SolidBrush(legendBg);
            using var borderPen = new Pen(_isDarkTheme ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 200, 200), 1);

            var legendRect = new Rectangle(legendX - 8, legendY - 8, legendWidth + 6, legendHeight + 6);
            g.FillRectangle(bgBrush, legendRect);
            g.DrawRectangle(borderPen, legendRect);

            for (int i = 0; i < _series.Count; i++)
            {
                var series = _series[i];
                var y = legendY + i * scaledItemHeight;

                // 根据字体大小调整线条粗细
                var lineWidth = Math.Max(2f, 3f * CalculateScaledFontSize(BaseLegendFontSize) / BaseLegendFontSize);
                using var colorPen = new Pen(series.Color, lineWidth);
                g.DrawLine(colorPen, legendX, y + scaledItemHeight / 2, legendX + 20, y + scaledItemHeight / 2);
                g.DrawString(series.Name, font, textBrush, legendX + 25, y + 2);
            }
        }

        #endregion

        #region 资源清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tooltip?.Dispose();
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// 完全重置图表状态（用于清空数据时）
        /// </summary>
        public void FullReset()
        {
            // 清空所有数据
            _series.Clear();
            _persistentData.Clear();

            // 重置所有状态变量
            _timeScale = 1.0f;
            _viewOffset = 0.0f;
            _currentTimeSeconds = 0.0f;
            _hasUserInteracted = false;
            _lastUserInteraction = DateTime.MinValue;

            // 停止所有定时器（但不重置定时器对象）
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                // 不要设置为null，保持定时器对象可用
                AutoRefreshEnabled = false;
            }

            // 强制重绘
            Invalidate();
        }
    }

    /// <summary>
    /// 线性图数据系列
    /// </summary>
    public class LineChartSeries
    {
        public string Name { get; set; } = "";
        public List<PointF> Points { get; set; } = new();
        public Color Color { get; set; }
        public float LineWidth { get; set; } = 3.5f;
    }
}