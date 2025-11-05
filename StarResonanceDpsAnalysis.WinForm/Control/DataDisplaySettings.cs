using AntdUI;
using StarResonanceDpsAnalysis.WinForm.Plugin;

namespace StarResonanceDpsAnalysis.WinForm.Control
{
    public partial class DataDisplaySettings : UserControl
    {
        private readonly BorderlessForm _parentForm;
        private System.Windows.Forms.Timer? _refreshDelayTimer;
        private bool _isUpdatingCheckboxes = false; // 防止递归更新

        public DataDisplaySettings(BorderlessForm borderlessForm)
        {
            InitializeComponent();
            _parentForm = borderlessForm;

            // 初始化延迟刷新定时器 - 增加延迟时间减少卡顿
            _refreshDelayTimer = new System.Windows.Forms.Timer
            {
                Interval = 500 // 增加到500ms延迟，进一步减少频繁刷新
            };
            _refreshDelayTimer.Tick += RefreshDelayTimer_Tick;
        }

        private void DataDisplaySettings_Load(object sender, EventArgs e)
        {
            // 优化FlowPanel的滑动显示性能
            OptimizeFlowPanelDisplay();

            InitializeOptimizedLayout();
        }

        /// <summary>
        /// 优化FlowPanel显示性能，减少滑动时的显示问题
        /// </summary>
        private void OptimizeFlowPanelDisplay()
        {
            try
            {
                // 启用双缓冲减少闪烁
                typeof(System.Windows.Forms.Panel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, flowPanel1, new object[] { true });

                // 通过反射设置优化属性
                var setStyleMethod = typeof(System.Windows.Forms.Control).GetMethod("SetStyle",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (setStyleMethod != null)
                {
                    setStyleMethod.Invoke(flowPanel1, new object[] { ControlStyles.OptimizedDoubleBuffer, true });
                    setStyleMethod.Invoke(flowPanel1, new object[] { ControlStyles.AllPaintingInWmPaint, true });
                    setStyleMethod.Invoke(flowPanel1, new object[] { ControlStyles.UserPaint, true });
                    setStyleMethod.Invoke(flowPanel1, new object[] { ControlStyles.ResizeRedraw, true });
                }

                Console.WriteLine("FlowPanel显示优化已启用");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FlowPanel显示优化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化优化后的布局
        /// </summary>
        private void InitializeOptimizedLayout()
        {
            // 暂停布局更新以提高性能
            flowPanel1.SuspendLayout();

            try
            {
                // 清空现有控件
                flowPanel1.Controls.Clear();

                // 设置FlowPanel的基本属性
                flowPanel1.AutoScroll = true;
                flowPanel1.Padding = new Padding(10, 10, 10, 10);

                // 步骤1：将操作按钮区域放到最上面
                AddControlButtons();

                // 步骤2：重新定义分组数据
                var groups = new Dictionary<string, string[]>
                {
                    { "⚔️ 伤害数据", new[] { "TotalDamage", "CriticalDamage", "LuckyDamage", "CritLuckyDamage", "DamageTaken" } },
                    { "💥 DPS数据", new[] { "InstantDps", "MaxInstantDps", "TotalDps", "CritRate", "LuckyRate" } },
                    { "🛡️ 治疗数据", new[] { "TotalHealingDone", "CriticalHealingDone", "LuckyHealingDone", "CritLuckyHealingDone" } },
                    { "💚 HPS数据", new[] { "InstantHps", "MaxInstantHps", "TotalHps" } }
                };

                // 步骤3：创建两列布局容器
                CreateTwoColumnLayout(groups);

                // 调试输出
                Console.WriteLine("=== 布局初始化完成 ===");
                for (int i = 0; i < flowPanel1.Controls.Count; i++)
                {
                    var control = flowPanel1.Controls[i];
                    Console.WriteLine($"控件{i}: {control.GetType().Name} - Height: {control.Height}");
                }
            }
            finally
            {
                // 恢复布局更新
                flowPanel1.ResumeLayout(true);

                // 强制刷新显示
                flowPanel1.PerformLayout();
                flowPanel1.Refresh();
            }
        }

        /// <summary>
        /// 创建两列布局
        /// </summary>
        private void CreateTwoColumnLayout(Dictionary<string, string[]> groups)
        {
            var groupList = groups.ToList();
            int groupsPerColumn = (int)Math.Ceiling(groupList.Count / 2.0);

            // 创建两列容器的主面板
            var mainContainer = new System.Windows.Forms.Panel
            {
                Width = flowPanel1.ClientSize.Width - 20,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5),
                BackColor = Color.Transparent
            };

            // 左列
            var leftColumn = new System.Windows.Forms.Panel
            {
                Width = (mainContainer.Width - 20) / 2,
                Location = new Point(0, 0),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // 右列  
            var rightColumn = new System.Windows.Forms.Panel
            {
                Width = (mainContainer.Width - 20) / 2,
                Location = new Point((mainContainer.Width - 20) / 2 + 10, 0),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            int currentY_Left = 0;
            int currentY_Right = 0;

            // 分配分组到两列
            for (int i = 0; i < groupList.Count; i++)
            {
                var group = groupList[i];
                var groupPanel = CreateCompactGroupPanel(group.Key, group.Value, leftColumn.Width - 10);

                if (i < groupsPerColumn)
                {
                    // 添加到左列
                    groupPanel.Location = new Point(0, currentY_Left);
                    leftColumn.Controls.Add(groupPanel);
                    currentY_Left += groupPanel.Height + 10;
                }
                else
                {
                    // 添加到右列
                    groupPanel.Location = new Point(0, currentY_Right);
                    rightColumn.Controls.Add(groupPanel);
                    currentY_Right += groupPanel.Height + 10;
                }
            }

            // 设置列的最终高度
            leftColumn.Height = currentY_Left;
            rightColumn.Height = currentY_Right;

            // 设置主容器高度为两列中较高的那个
            mainContainer.Height = Math.Max(currentY_Left, currentY_Right);

            mainContainer.Controls.AddRange(new System.Windows.Forms.Control[] { leftColumn, rightColumn });
            flowPanel1.Controls.Add(mainContainer);
        }

        /// <summary>
        /// 创建紧凑的分组面板 - 移除分隔线以减少卡顿
        /// </summary>
        private System.Windows.Forms.Panel CreateCompactGroupPanel(string groupTitle, string[] itemKeys, int panelWidth)
        {
            var groupContainer = new System.Windows.Forms.Panel
            {
                Width = panelWidth,
                AutoSize = true,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 0, 15) // 增加底部间距来替代分隔线的视觉分割效果
            };

            // 启用双缓冲优化滑动显示
            try
            {
                typeof(System.Windows.Forms.Panel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, groupContainer, new object[] { true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"分组面板启用双缓冲失败: {ex.Message}");
            }

            int currentY = 0;

            // 创建分组标题 - 优化显示
            var titleLabel = new System.Windows.Forms.Label
            {
                Text = groupTitle,
                Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold),
                ForeColor = AppConfig.IsLight ? Color.FromArgb(51, 51, 51) : Color.FromArgb(220, 220, 220),
                AutoSize = true,
                Location = new Point(0, currentY),
                BackColor = Color.Transparent,
                UseMnemonic = false, // 优化文本显示
                UseCompatibleTextRendering = false // 使用新的文本渲染
            };
            groupContainer.Controls.Add(titleLabel);
            currentY += titleLabel.Height + 6;

            // 创建选项区域 - 使用更紧凑的布局
            var optionsPanel = CreateCompactOptionsGrid(itemKeys, panelWidth - 15);
            optionsPanel.Location = new Point(15, currentY);
            groupContainer.Controls.Add(optionsPanel);
            currentY += optionsPanel.Height + 8;

            // 移除分隔线 - 这是导致卡顿的主要原因
            // 使用底部间距来替代分隔线的视觉效果

            groupContainer.Height = currentY;
            return groupContainer;
        }

        /// <summary>
        /// 创建紧凑的选项网格布局 - 优化滑动显示
        /// </summary>
        private System.Windows.Forms.Panel CreateCompactOptionsGrid(string[] itemKeys, int panelWidth)
        {
            var panel = new System.Windows.Forms.Panel
            {
                Width = panelWidth,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // 使用反射启用双缓冲以减少滑动时的闪烁
            try
            {
                typeof(System.Windows.Forms.Panel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, panel, new object[] { true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启用双缓冲失败: {ex.Message}");
            }

            // 对于较窄的列，优先使用单列布局，确保文本完整显示
            int columnCount = 1; // 每列一个选项，确保显示清晰
            int columnWidth = panel.Width;
            int rowHeight = 28; // 稍微减小行高
            int currentRow = 0;

            foreach (var key in itemKeys)
            {
                var setting = ColumnSettingsManager.AllSettings.FirstOrDefault(x => x.Key == key);
                if (setting == null) continue;

                // 创建复选框 - 优化显示性能
                var checkbox = new AntdUI.Checkbox
                {
                    Text = setting.Title,
                    Name = setting.Key,
                    Checked = setting.IsVisible,
                    Tag = setting.Key,
                    Size = new Size(columnWidth - 5, 24),
                    Location = new Point(0, currentRow * rowHeight),
                    Font = new Font("Microsoft YaHei UI", 8.5F),
                    BackColor = Color.Transparent
                };

                checkbox.CheckedChanged += checkbox_CheckedChanged;
                panel.Controls.Add(checkbox);
                currentRow++;
            }

            // 计算并设置面板高度
            panel.Height = Math.Max(rowHeight, currentRow * rowHeight);
            return panel;
        }

        /// <summary>
        /// 添加控制按钮（放在最上面）
        /// </summary>
        private void AddControlButtons()
        {
            var buttonContainer = new System.Windows.Forms.Panel
            {
                Width = flowPanel1.ClientSize.Width - 30,
                Height = 45,
                Margin = new Padding(0, 0, 0, 15), // 下边距，与下方内容有间隔
                BackColor = Color.Transparent
            };

            // 全选按钮
            var selectAllBtn = new AntdUI.Button
            {
                Text = "全选",
                Size = new Size(70, 32),
                Location = new Point(0, 6),
                Type = TTypeMini.Primary,
                BorderWidth = 1
            };
            selectAllBtn.Click += (s, e) => SetAllCheckboxes(true);

            // 全不选按钮
            var deselectAllBtn = new AntdUI.Button
            {
                Text = "全不选",
                Size = new Size(70, 32),
                Location = new Point(80, 6),
                Type = TTypeMini.Default,
                BorderWidth = 1
            };
            deselectAllBtn.Click += (s, e) => SetAllCheckboxes(false);

            // 默认按钮
            var defaultBtn = new AntdUI.Button
            {
                Text = "默认",
                Size = new Size(70, 32),
                Location = new Point(160, 6),
                Type = TTypeMini.Warn,
                BorderWidth = 1
            };
            defaultBtn.Click += (s, e) => ResetToDefaults();

            buttonContainer.Controls.AddRange(new System.Windows.Forms.Control[] { selectAllBtn, deselectAllBtn, defaultBtn });
            flowPanel1.Controls.Add(buttonContainer);
        }

        /// <summary>
        /// 复选框状态改变事件处理 - 高度优化，减少滑动卡顿
        /// </summary>
        private void checkbox_CheckedChanged(object sender, BoolEventArgs e)
        {
            // 防止递归更新导致的性能问题
            if (_isUpdatingCheckboxes) return;

            try
            {
                if (sender is Checkbox cb && cb.Tag is string key)
                {
                    var setting = ColumnSettingsManager.AllSettings.FirstOrDefault(x => x.Key == key);
                    if (setting != null)
                    {
                        // 立即更新内存中的设置，但延迟刷新UI
                        setting.IsVisible = cb.Checked;

                        // 异步保存设置，避免阻塞UI线程
                        Task.Run(() =>
                        {
                            try
                            {
                                AppConfig.SetValue("TableSet", cb.Name, cb.Checked.ToString());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"保存配置异步处理出错: {ex.Message}");
                            }
                        });
                    }

                    // 使用延迟刷新，避免频繁调用造成卡顿
                    if (_refreshDelayTimer != null)
                    {
                        _refreshDelayTimer.Stop();
                        _refreshDelayTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"复选框状态改变处理出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 延迟刷新定时器回调
        /// </summary>
        private void RefreshDelayTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                _refreshDelayTimer?.Stop();

                // 在UI线程中延迟执行刷新
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        ColumnSettingsManager.RefreshTableAction?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"表格刷新出错: {ex.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"延迟刷新出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置所有复选框状态 - 优化批量操作
        /// </summary>
        private void SetAllCheckboxes(bool isChecked)
        {
            try
            {
                _isUpdatingCheckboxes = true; // 开始批量更新，防止单个事件触发

                // 先停止定时器，避免中间状态的刷新
                _refreshDelayTimer?.Stop();

                TraverseAndSetCheckboxes(flowPanel1, isChecked);

                // 批量更新完成后，触发一次刷新
                _refreshDelayTimer?.Start();
            }
            finally
            {
                _isUpdatingCheckboxes = false; // 恢复正常事件处理
            }
        }

        /// <summary>
        /// 遍历并设置复选框状态 - 优化性能
        /// </summary>
        private void TraverseAndSetCheckboxes(System.Windows.Forms.Control parent, bool isChecked)
        {
            foreach (System.Windows.Forms.Control control in parent.Controls)
            {
                if (control is Checkbox checkbox)
                {
                    // 批量操作时直接设置，不触发单个事件处理
                    checkbox.Checked = isChecked;

                    // 直接更新设置，不通过事件
                    if (checkbox.Tag is string key)
                    {
                        var setting = ColumnSettingsManager.AllSettings.FirstOrDefault(x => x.Key == key);
                        if (setting != null)
                        {
                            setting.IsVisible = isChecked;
                            // 异步保存
                            Task.Run(() => AppConfig.SetValue("TableSet", checkbox.Name, isChecked.ToString()));
                        }
                    }
                }
                else if (control.HasChildren)
                {
                    TraverseAndSetCheckboxes(control, isChecked);
                }
            }
        }

        /// <summary>
        /// 重置为默认设置 - 优化批量操作
        /// </summary>
        private void ResetToDefaults()
        {
            try
            {
                _isUpdatingCheckboxes = true; // 开始批量更新
                _refreshDelayTimer?.Stop(); // 停止定时器

                // 定义默认显示的重要列
                var defaultColumns = new HashSet<string>
                {
                    // 伤害数据（优先级最高）
                    "TotalDamage",      // 总伤害
                    "DamageTaken",      // 承伤
                    "CriticalDamage",   // 纯暴击
                    
                    // DPS数据
                    "TotalDps",         // DPS
                    "CritRate",         // 暴击率
                    "LuckyRate",        // 幸运率
                    
                    // 治疗数据
                    "TotalHealingDone", // 总治疗
                    
                    // HPS数据
                    "TotalHps"          // HPS
                };

                TraverseAndResetCheckboxes(flowPanel1, defaultColumns);

                // 批量操作完成后触发刷新
                _refreshDelayTimer?.Start();
            }
            finally
            {
                _isUpdatingCheckboxes = false; // 恢复正常处理
            }
        }

        /// <summary>
        /// 遍历并重置复选框为默认状态 - 优化性能
        /// </summary>
        private void TraverseAndResetCheckboxes(System.Windows.Forms.Control parent, HashSet<string> defaultColumns)
        {
            foreach (System.Windows.Forms.Control control in parent.Controls)
            {
                if (control is Checkbox checkbox && checkbox.Tag is string key)
                {
                    bool shouldBeChecked = defaultColumns.Contains(key);
                    checkbox.Checked = shouldBeChecked;

                    // 直接更新设置
                    var setting = ColumnSettingsManager.AllSettings.FirstOrDefault(x => x.Key == key);
                    if (setting != null)
                    {
                        setting.IsVisible = shouldBeChecked;
                        // 异步保存
                        Task.Run(() => AppConfig.SetValue("TableSet", checkbox.Name, shouldBeChecked.ToString()));
                    }
                }
                else if (control.HasChildren)
                {
                    TraverseAndResetCheckboxes(control, defaultColumns);
                }
            }
        }

        private void flowPanel1_Click(object sender, EventArgs e)
        {
            // 保留原有的点击事件处理
        }

        /// <summary>
        /// 清理延迟刷新定时器资源
        /// </summary>
        public void CleanupResources()
        {
            try
            {
                // 释放延迟刷新定时器
                _refreshDelayTimer?.Stop();
                _refreshDelayTimer?.Dispose();
                _refreshDelayTimer = null;
                Console.WriteLine("数据显示设置资源已清理");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理资源时出错: {ex.Message}");
            }
        }
    }
}
