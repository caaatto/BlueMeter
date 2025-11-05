using AntdUI;
using BlueMeter.WinForm.Plugin;
using BlueMeter.WinForm.Plugin.Charts;
using BlueMeter.WinForm.Plugin.DamageStatistics;

using SystemPanel = System.Windows.Forms.Panel;

namespace BlueMeter.WinForm.Forms
{
    /// <summary>
    /// 实时图表窗口 - 使用扁平化自定义图表控件，自动加载所有图表
    /// </summary>
    public partial class RealtimeChartsForm : BorderlessForm
    {
        private Tabs _tabControl;
        private FlatLineChart _dpsTrendChart;
        private FlatPieChart _skillPieChart;
        private FlatBarChart _teamDpsChart;
        private FlatScatterChart _multiDimensionChart;
        private FlatBarChart _damageTypeChart;
        private Dropdown _playerSelector;

        // 控制按钮
        private AntdUI.Button _refreshButton;
        private AntdUI.Button _closeButton;
        private AntdUI.Button _autoRefreshToggle;

        // 自动刷新相关
        private System.Windows.Forms.Timer _autoRefreshTimer;
        private bool _autoRefreshEnabled = false;

        // 窗体拖动相关
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private SystemPanel _draggablePanel;

        public RealtimeChartsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);

            Text = "实时图表可视化";
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;

            // 设置标准字体
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);

            InitializeControls();
            InitializeAutoRefreshTimer();

            // 应用当前主题
            RefreshChartsTheme();

            // 自动加载所有图表
            LoadAllCharts();

            // 默认启用自动刷新
            EnableAutoRefreshByDefault();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // RealtimeChartsForm
            // 
            ClientSize = new Size(1000, 700);
            Name = "RealtimeChartsForm";
            Load += RealtimeChartsForm_Load;
            ResumeLayout(false);
        }

        private void InitializeControls()
        {
            // 创建控制按钮面板（可拖动）
            _draggablePanel = new SystemPanel
            {
                Height = 50,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 5, 10, 5),
                Cursor = Cursors.SizeAll // 显示可移动光标
            };

            // 为拖动面板添加鼠标事件
            _draggablePanel.MouseDown += DraggablePanel_MouseDown;
            _draggablePanel.MouseMove += DraggablePanel_MouseMove;
            _draggablePanel.MouseUp += DraggablePanel_MouseUp;

            _refreshButton = new AntdUI.Button
            {
                Text = "手动刷新",
                Type = TTypeMini.Primary,
                Size = new Size(80, 35),
                Location = new Point(10, 8),
                Font = Font
            };
            _refreshButton.Click += RefreshButton_Click;

            _autoRefreshToggle = new AntdUI.Button
            {
                Text = "自动刷新: 开", // 默认显示为开启状态
                Type = TTypeMini.Primary, // 默认使用Primary样式
                Size = new Size(100, 35),
                Location = new Point(100, 8),
                Font = Font
            };
            _autoRefreshToggle.Click += AutoRefreshToggle_Click;

            _closeButton = new AntdUI.Button
            {
                Text = "关闭",
                Type = TTypeMini.Default,
                Size = new Size(60, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(_draggablePanel.Width - 70, 8),
                Font = Font
            };
            _closeButton.Click += CloseButton_Click;

            _draggablePanel.Controls.Add(_refreshButton);
            _draggablePanel.Controls.Add(_autoRefreshToggle);
            _draggablePanel.Controls.Add(_closeButton);

            // 创建选项卡控件
            _tabControl = new Tabs
            {
                Dock = DockStyle.Fill,
                Font = Font
            };

            // 添加TabPage - 纯文本标题
            _tabControl.Pages.Add(new AntdUI.TabPage
            {
                Text = "DPS趋势图",
                Font = Font
            });
            _tabControl.Pages.Add(new AntdUI.TabPage
            {
                Text = "技能占比图",
                Font = Font
            });
            _tabControl.Pages.Add(new AntdUI.TabPage
            {
                Text = "团队DPS对比",
                Font = Font
            });
            _tabControl.Pages.Add(new AntdUI.TabPage
            {
                Text = "多维度对比",
                Font = Font
            });
            _tabControl.Pages.Add(new AntdUI.TabPage
            {
                Text = "伤害分布图",
                Font = Font
            });

            // 准备各页面容器
            for (int i = 0; i < 5; i++)
            {
                var panel = new SystemPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = AppConfig.IsLight ? Color.White : Color.FromArgb(31, 31, 31)
                };
                _tabControl.Pages[i].Controls.Add(panel);
            }

            // 为技能占比图页面添加玩家选择器
            var skillChartPage = _tabControl.Pages[1];
            var skillChartPanel = skillChartPage.Controls[0] as SystemPanel;

            var playerSelectorPanel = new SystemPanel
            {
                Height = 50,
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            var playerLabel = new AntdUI.Label
            {
                Text = "选择玩家：",
                Location = new Point(10, 15),
                AutoSize = true,
                Font = Font
            };

            _playerSelector = new Dropdown
            {
                Location = new Point(90, 10),
                Size = new Size(200, 30),
                Font = Font
            };
            _playerSelector.SelectedValueChanged += PlayerSelector_SelectedValueChanged;

            playerSelectorPanel.Controls.Add(playerLabel);
            playerSelectorPanel.Controls.Add(_playerSelector);
            skillChartPanel.Controls.Add(playerSelectorPanel);

            Controls.Add(_tabControl);
            Controls.Add(_draggablePanel);
        }

        #region 窗体拖动事件处理

        private void DraggablePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
                _draggablePanel.Cursor = Cursors.Hand;
            }
        }

        private void DraggablePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.Button == MouseButtons.Left)
            {
                // 计算移动距离
                var deltaX = e.Location.X - _dragStartPoint.X;
                var deltaY = e.Location.Y - _dragStartPoint.Y;

                // 移动窗体
                this.Location = new Point(this.Location.X + deltaX, this.Location.Y + deltaY);
            }
        }

        private void DraggablePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                _draggablePanel.Cursor = Cursors.SizeAll;
            }
        }

        #endregion

        /// <summary>
        /// 默认启用自动刷新
        /// </summary>
        private void EnableAutoRefreshByDefault()
        {
            _autoRefreshEnabled = true;
            _autoRefreshTimer.Enabled = true;
            _autoRefreshToggle.Text = "自动刷新: 开";
            _autoRefreshToggle.Type = TTypeMini.Primary;
        }

        /// <summary>
        /// 自动加载所有图表
        /// </summary>
        private void LoadAllCharts()
        {
            try
            {
                // 加载DPS趋势图（移除滑块控制）
                var dpsTrendPanel = _tabControl.Pages[0].Controls[0] as SystemPanel;
                _dpsTrendChart = ChartVisualizationService.CreateDpsTrendChart();
                dpsTrendPanel.Controls.Add(_dpsTrendChart);

                // 加载技能占比图
                var skillChartPanel = _tabControl.Pages[1].Controls[0] as SystemPanel;
                UpdatePlayerSelector();
                var selectedPlayer = _playerSelector.SelectedValue as PlayerSelectorItem;
                var playerId = selectedPlayer?.Uid ?? 0;
                _skillPieChart = ChartVisualizationService.CreateSkillDamagePieChart(playerId);
                skillChartPanel.Controls.Add(_skillPieChart);

                // 加载团队DPS对比图
                var teamDpsPanel = _tabControl.Pages[2].Controls[0] as SystemPanel;
                _teamDpsChart = ChartVisualizationService.CreateTeamDpsBarChart();
                teamDpsPanel.Controls.Add(_teamDpsChart);

                // 加载多维度对比图
                var multiDimensionPanel = _tabControl.Pages[3].Controls[0] as SystemPanel;
                _multiDimensionChart = ChartVisualizationService.CreateDpsRadarChart();
                multiDimensionPanel.Controls.Add(_multiDimensionChart);

                // 加载伤害分布图
                var damageTypePanel = _tabControl.Pages[4].Controls[0] as SystemPanel;
                _damageTypeChart = ChartVisualizationService.CreateDamageTypeStackedChart();
                damageTypePanel.Controls.Add(_damageTypeChart);

                // 初始刷新所有图表数据
                RefreshAllCharts();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载图表时出错: {ex.Message}");
                MessageBox.Show($"加载图表时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeAutoRefreshTimer()
        {
            _autoRefreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 100, // 0.1秒 (100毫秒) 高频刷新
                Enabled = false
            };
            _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
        }

        #region 事件处理

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshAllCharts();

            // 显示刷新状态
            _refreshButton.Text = "刷新中...";
            _refreshButton.Enabled = false;

            var resetTimer = new System.Windows.Forms.Timer { Interval = 300 };
            resetTimer.Tick += (s, args) =>
            {
                _refreshButton.Text = "手动刷新";
                _refreshButton.Enabled = true;
                resetTimer.Stop();
                resetTimer.Dispose();
            };
            resetTimer.Start();
        }

        private void AutoRefreshToggle_Click(object sender, EventArgs e)
        {
            _autoRefreshEnabled = !_autoRefreshEnabled;
            _autoRefreshTimer.Enabled = _autoRefreshEnabled;

            _autoRefreshToggle.Text = $"自动刷新: {(_autoRefreshEnabled ? "开" : "关")}";
            _autoRefreshToggle.Type = _autoRefreshEnabled ? TTypeMini.Primary : TTypeMini.Default;
        }

        private void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshAllCharts();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PlayerSelector_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            if (_playerSelector.SelectedValue is PlayerSelectorItem item && _skillPieChart != null)
            {
                ChartVisualizationService.RefreshSkillDamagePieChart(_skillPieChart, item.Uid);
            }
        }

        #endregion

        private void RefreshAllCharts()
        {
            try
            {
                // 更新数据点
                ChartVisualizationService.UpdateAllDataPoints();

                // 刷新所有图表，避免用户记录丢失
                if (_dpsTrendChart != null)
                {
                    ChartVisualizationService.RefreshDpsTrendChart(_dpsTrendChart, null, ChartDataType.Damage);
                    _dpsTrendChart.ReloadPersistentData(); // 重新加载数据防止丢失
                }

                if (_skillPieChart != null)
                {
                    var selectedPlayer = _playerSelector.SelectedValue as PlayerSelectorItem;
                    ChartVisualizationService.RefreshSkillDamagePieChart(_skillPieChart, selectedPlayer?.Uid ?? 0);
                }

                if (_teamDpsChart != null)
                    ChartVisualizationService.RefreshTeamDpsBarChart(_teamDpsChart);

                if (_multiDimensionChart != null)
                    ChartVisualizationService.RefreshDpsRadarChart(_multiDimensionChart);

                if (_damageTypeChart != null)
                    ChartVisualizationService.RefreshDamageTypeStackedChart(_damageTypeChart);

                // 更新玩家选择器
                UpdatePlayerSelector();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刷新图表时出错: {ex.Message}");
            }
        }

        private void UpdatePlayerSelector()
        {
            var players = StatisticData._manager.GetPlayersWithCombatData().ToList();

            // 保存当前选择
            var currentSelection = _playerSelector.SelectedValue as PlayerSelectorItem;

            _playerSelector.Items.Clear();

            foreach (var player in players)
            {
                var displayName = string.IsNullOrEmpty(player.Nickname) ? $"玩家{player.Uid}" : player.Nickname;
                var item = new PlayerSelectorItem { Uid = player.Uid, DisplayName = displayName };
                _playerSelector.Items.Add(item);

                // 恢复选择或默认选择第一个
                if ((currentSelection != null && currentSelection.Uid == player.Uid) ||
                    (currentSelection == null && _playerSelector.Items.Count == 1))
                {
                    _playerSelector.SelectedValue = item;
                }
            }
        }

        /// <summary>
        /// 刷新图表主题
        /// </summary>
        public void RefreshChartsTheme()
        {
            var isDark = !AppConfig.IsLight;

            // 设置窗口主题
            FormGui.SetColorMode(this, AppConfig.IsLight);

            // 更新所有图表主题
            if (_dpsTrendChart != null)
                _dpsTrendChart.IsDarkTheme = isDark;

            if (_skillPieChart != null)
                _skillPieChart.IsDarkTheme = isDark;

            if (_teamDpsChart != null)
                _teamDpsChart.IsDarkTheme = isDark;

            if (_multiDimensionChart != null)
                _multiDimensionChart.IsDarkTheme = isDark;

            if (_damageTypeChart != null)
                _damageTypeChart.IsDarkTheme = isDark;
        }

        /// <summary>
        /// 清空所有图表数据
        /// </summary>
        public void ClearAllChartData()
        {
            _dpsTrendChart?.ClearSeries();
            _skillPieChart?.ClearData();
            _teamDpsChart?.ClearData();
            _multiDimensionChart?.ClearSeries();
            _damageTypeChart?.ClearData();
            _playerSelector?.Items.Clear();
        }

        /// <summary>
        /// 手动刷新所有图表
        /// </summary>
        public void ManualRefreshCharts()
        {
            RefreshAllCharts();
        }

        /// <summary>
        /// 设置自动刷新间隔
        /// </summary>
        public void SetAutoRefreshInterval(int milliseconds)
        {
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Interval = Math.Max(50, milliseconds); // 最小50毫秒，支持更高频率
            }
        }

        /// <summary>
        /// 获取当前自动刷新状态
        /// </summary>
        public bool IsAutoRefreshEnabled => _autoRefreshEnabled;

        /// <summary>
        /// 获取当前刷新间隔
        /// </summary>
        public int GetRefreshInterval => _autoRefreshTimer?.Interval ?? 100;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _autoRefreshTimer?.Stop();
            _autoRefreshTimer?.Dispose();
            base.OnFormClosed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 窗口加载后自动刷新一次图表
            if (_dpsTrendChart != null)
            {
                RefreshAllCharts();
            }
        }

        private void RealtimeChartsForm_Load(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// 玩家选择器项
    /// </summary>
    public class PlayerSelectorItem
    {
        public long Uid { get; set; }
        public string DisplayName { get; set; } = "";

        public override string ToString()
        {
            return DisplayName;
        }
    }
}