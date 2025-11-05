using System.Drawing.Drawing2D;

namespace StarResonanceDpsAnalysis.WinForm.Plugin.Charts
{
    /// <summary>
    /// 扁平化饼图控件
    /// </summary>
    public class FlatPieChart : UserControl
    {
        #region 字段和属性

        private readonly List<PieChartData> _data = new();
        private bool _isDarkTheme = false;
        private string _titleText = "";
        private bool _showLabels = true;
        private bool _showPercentages = true;

        // 现代化扁平配色
        private readonly Color[] _colors = {
            Color.FromArgb(255, 107, 107),  // 红
            Color.FromArgb(78, 205, 196),   // 青
            Color.FromArgb(69, 183, 209),   // 蓝
            Color.FromArgb(150, 206, 180),  // 绿
            Color.FromArgb(255, 234, 167),  // 黄
            Color.FromArgb(221, 160, 221),  // 紫
            Color.FromArgb(152, 216, 200),  // 薄荷
            Color.FromArgb(247, 220, 111),  // 金
            Color.FromArgb(187, 143, 206),  // 淡紫
            Color.FromArgb(133, 193, 233)   // 天蓝
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

        public bool ShowLabels
        {
            get => _showLabels;
            set
            {
                _showLabels = value;
                Invalidate();
            }
        }

        public bool ShowPercentages
        {
            get => _showPercentages;
            set
            {
                _showPercentages = value;
                Invalidate();
            }
        }

        #endregion

        #region 构造函数

        public FlatPieChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            ApplyTheme();
        }

        #endregion

        #region 数据管理

        public void SetData(List<(string Label, double Value)> data)
        {
            _data.Clear();

            var total = data.Sum(d => d.Value);
            if (total <= 0) return;

            for (int i = 0; i < data.Count; i++)
            {
                var percentage = data[i].Value / total * 100;
                _data.Add(new PieChartData
                {
                    Label = data[i].Label,
                    Value = data[i].Value,
                    Percentage = percentage,
                    Color = _colors[i % _colors.Length]
                });
            }

            Invalidate();
        }

        public void ClearData()
        {
            _data.Clear();
            Invalidate();
        }

        #endregion

        #region 主题设置

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

        #region 绘制方法

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 清除背景
            g.Clear(BackColor);

            if (_data.Count == 0)
            {
                DrawNoDataMessage(g);
                return;
            }

            // 绘制标题（如果有）
            DrawTitle(g);

            // 计算饼图区域 - 去掉标题高度，增大饼图占比
            var titleHeight = string.IsNullOrEmpty(_titleText) ? 0 : 30; // 减少标题高度
            var margin = 10; // 减少边距
            var pieSize = Math.Min(Width - margin * 2, Height - titleHeight - margin * 2);
            var pieRect = new Rectangle(
                (Width - pieSize) / 2,
                titleHeight + (Height - titleHeight - pieSize) / 2,
                pieSize,
                pieSize
            );

            // 绘制饼图
            DrawPieSlices(g, pieRect);

            // 绘制标签
            if (_showLabels)
            {
                DrawLabels(g, pieRect);
            }
        }

        private void DrawNoDataMessage(Graphics g)
        {
            var message = "暂无数据";
            var font = new Font("Microsoft YaHei", 12, FontStyle.Regular);
            var brush = new SolidBrush(_isDarkTheme ? Color.Gray : Color.DarkGray);

            var size = g.MeasureString(message, font);
            var x = (Width - size.Width) / 2;
            var y = (Height - size.Height) / 2;

            g.DrawString(message, font, brush, x, y);

            font.Dispose();
            brush.Dispose();
        }

        private void DrawTitle(Graphics g)
        {
            if (string.IsNullOrEmpty(_titleText)) return;

            using var font = new Font("Microsoft YaHei", 14, FontStyle.Bold);
            using var brush = new SolidBrush(ForeColor);

            var size = g.MeasureString(_titleText, font);
            var x = (Width - size.Width) / 2;
            var y = 10;

            g.DrawString(_titleText, font, brush, x, y);
        }

        private void DrawPieSlices(Graphics g, Rectangle pieRect)
        {
            float startAngle = 0;

            foreach (var data in _data)
            {
                var sweepAngle = (float)(data.Percentage * 360 / 100);

                // 绘制饼片 - 扁平化设计（无边框）
                using var brush = new SolidBrush(data.Color);
                g.FillPie(brush, pieRect, startAngle, sweepAngle);

                startAngle += sweepAngle;
            }
        }

        private void DrawLabels(Graphics g, Rectangle pieRect)
        {
            // 使用更小的字体适应紧凑布局
            using var font = new Font("Microsoft YaHei", 7, FontStyle.Regular); // 从9减少到7
            using var brush = new SolidBrush(ForeColor);

            float startAngle = 0;
            var centerX = pieRect.X + pieRect.Width / 2f;
            var centerY = pieRect.Y + pieRect.Height / 2f;
            var radius = pieRect.Width / 2f;

            foreach (var data in _data)
            {
                var sweepAngle = (float)(data.Percentage * 360 / 100);
                var labelAngle = startAngle + sweepAngle / 2;

                // 调整标签位置，更靠近饼图中心
                var labelRadius = radius * 0.75f; // 从0.7f增加到0.75f，稍微外移
                var labelX = centerX + labelRadius * (float)Math.Cos(labelAngle * Math.PI / 180);
                var labelY = centerY + labelRadius * (float)Math.Sin(labelAngle * Math.PI / 180);

                // 生成标签文本 - 简化文本以减少拥挤
                var labelText = "";
                if (_showLabels && _showPercentages && data.Percentage >= 5.0) // 只显示占比大于5%的标签
                {
                    // 简化标签格式，技能名太长时截断
                    var skillName = data.Label.Length > 6 ? data.Label.Substring(0, 6) + ".." : data.Label;
                    labelText = $"{skillName}\n{data.Percentage:F1}%";
                }
                else if (_showPercentages && data.Percentage >= 3.0) // 小占比只显示百分比
                {
                    labelText = $"{data.Percentage:F1}%";
                }

                if (!string.IsNullOrEmpty(labelText))
                {
                    var textSize = g.MeasureString(labelText, font);
                    var textX = labelX - textSize.Width / 2;
                    var textY = labelY - textSize.Height / 2;

                    // 调整半透明背景，使其更轻量
                    var bgColor = _isDarkTheme ? Color.FromArgb(150, 0, 0, 0) : Color.FromArgb(150, 255, 255, 255);
                    using var bgBrush = new SolidBrush(bgColor);
                    g.FillRectangle(bgBrush, textX - 1, textY - 1, textSize.Width + 2, textSize.Height + 2);

                    // 绘制文本
                    g.DrawString(labelText, font, brush, textX, textY);
                }

                startAngle += sweepAngle;
            }
        }

        #endregion
    }

    /// <summary>
    /// 饼图数据项
    /// </summary>
    public class PieChartData
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public double Percentage { get; set; }
        public Color Color { get; set; }
    }
}