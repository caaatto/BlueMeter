using System.Drawing.Drawing2D;

namespace StarResonanceDpsAnalysis.WinForm.Plugin.Charts
{
    /// <summary>
    /// 扁平化条形图控件
    /// </summary>
    public class FlatBarChart : UserControl
    {
        #region 字段和属性

        private readonly List<BarChartData> _data = new();
        private bool _isDarkTheme = false;
        private string _titleText = "";
        private string _xAxisLabel = "";
        private string _yAxisLabel = "";
        private bool _showLegend = true;

        // 边距设置 - 减少边距以增大图表占比
        private const int PaddingLeft = 35;   // 从60减少到35
        private const int PaddingRight = 15;  // 从20减少到15
        private const int PaddingTop = 25;    // 从10增加到25，以提供更多空间给条形上方的文本标签
        private const int PaddingBottom = 50; // 从100减少到50

        // 现代化配色
        private readonly Color[] _colors = {
            Color.FromArgb(74, 144, 226),   // 蓝
            Color.FromArgb(126, 211, 33),   // 绿
            Color.FromArgb(245, 166, 35),   // 橙
            Color.FromArgb(208, 2, 27),     // 红
            Color.FromArgb(144, 19, 254),   // 紫
            Color.FromArgb(80, 227, 194),   // 青
            Color.FromArgb(184, 233, 134),  // 浅绿
            Color.FromArgb(75, 213, 238),   // 天蓝
            Color.FromArgb(248, 231, 28),   // 黄
            Color.FromArgb(189, 16, 224)    // 品红
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

        #endregion

        #region 构造函数

        public FlatBarChart()
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

            for (int i = 0; i < data.Count; i++)
            {
                _data.Add(new BarChartData
                {
                    Label = data[i].Label,
                    Value = data[i].Value,
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

            // 计算最大值
            var maxValue = _data.Max(d => d.Value);
            if (maxValue <= 0) return;

            // 计算图表区域
            var chartRect = new Rectangle(PaddingLeft, PaddingTop,
                                        Width - PaddingLeft - PaddingRight,
                                        Height - PaddingTop - PaddingBottom);

            // 绘制网格
            DrawGrid(g, chartRect, maxValue);

            // 绘制坐标轴
            DrawAxes(g, chartRect, maxValue);

            // 绘制柱状条
            DrawBars(g, chartRect, maxValue);

            // 绘制标题（如果有）
            DrawTitle(g);
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

        private void DrawGrid(Graphics g, Rectangle chartRect, double maxValue)
        {
            var gridColor = _isDarkTheme ? Color.FromArgb(64, 64, 64) : Color.FromArgb(230, 230, 230);
            using var gridPen = new Pen(gridColor, 1);

            // 绘制水平网格线 - 减少网格线数量
            for (int i = 0; i <= 5; i++) // 从10条减少到5条
            {
                var y = chartRect.Y + (float)chartRect.Height * i / 5;
                g.DrawLine(gridPen, chartRect.X, y, chartRect.Right, y);
            }
        }

        private void DrawAxes(Graphics g, Rectangle chartRect, double maxValue)
        {
            var axisColor = _isDarkTheme ? Color.FromArgb(128, 128, 128) : Color.FromArgb(180, 180, 180);
            using var axisPen = new Pen(axisColor, 1);
            using var textBrush = new SolidBrush(ForeColor);
            using var font = new Font("Microsoft YaHei", 7); // 从9减少到7

            // 绘制X轴
            g.DrawLine(axisPen, chartRect.X, chartRect.Bottom, chartRect.Right, chartRect.Bottom);

            // 绘制Y轴
            g.DrawLine(axisPen, chartRect.X, chartRect.Y, chartRect.X, chartRect.Bottom);

            // X轴标签（类型标签）
            var barWidth = (float)chartRect.Width / _data.Count;
            for (int i = 0; i < _data.Count; i++)
            {
                var x = chartRect.X + barWidth * (i + 0.5f);
                var text = _data[i].Label;

                var size = g.MeasureString(text, font);

                // 简化标签显示，直接水平显示而不旋转
                var textX = x - size.Width / 2;
                var textY = chartRect.Bottom + 5; // 减少间距

                g.DrawString(text, font, textBrush, textX, textY);
            }

            // Y轴标签 - 简化显示
            for (int i = 0; i <= 5; i++) // 从10个刻度减少到5个
            {
                var y = chartRect.Bottom - (float)chartRect.Height * i / 5;
                var value = maxValue * i / 5;
                var text = $"{value:F0}%"; // 直接显示百分比，简化格式

                var size = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, chartRect.X - size.Width - 3, y - size.Height / 2);
            }

            // 轴标签（如果有）- 使用更小字体
            if (!string.IsNullOrEmpty(_xAxisLabel))
            {
                var size = g.MeasureString(_xAxisLabel, font);
                var x = chartRect.X + (chartRect.Width - size.Width) / 2;
                var y = chartRect.Bottom + 35; // 调整位置
                g.DrawString(_xAxisLabel, font, textBrush, x, y);
            }

            if (!string.IsNullOrEmpty(_yAxisLabel))
            {
                var size = g.MeasureString(_yAxisLabel, font);
                using var matrix = new Matrix();
                matrix.RotateAt(-90, new PointF(10, chartRect.Y + (chartRect.Height + size.Width) / 2));
                g.Transform = matrix;
                g.DrawString(_yAxisLabel, font, textBrush, 10, chartRect.Y + (chartRect.Height + size.Width) / 2);
                g.ResetTransform();
            }
        }

        private void DrawBars(Graphics g, Rectangle chartRect, double maxValue)
        {
            var barWidth = (float)chartRect.Width / _data.Count * 0.85f; // 增加条形宽度从0.8f到0.85f
            var barSpacing = (float)chartRect.Width / _data.Count * 0.075f; // 减少间距

            for (int i = 0; i < _data.Count; i++)
            {
                var data = _data[i];
                var barHeight = (float)(data.Value / maxValue * chartRect.Height);

                var x = chartRect.X + i * (barWidth + barSpacing * 2) + barSpacing;
                var y = chartRect.Bottom - barHeight;

                var barRect = new RectangleF(x, y, barWidth, barHeight);

                // 绘制条形 - 扁平化无边框设计
                using var brush = new SolidBrush(data.Color);
                g.FillRectangle(brush, barRect);

                // 绘制数值标签 - 智能调整标签位置
                if (barHeight > 15) // 只有足够高的条形才显示标签
                {
                    var valueText = $"{data.Value:F1}%"; // 简化数值格式显示
                    using var font = new Font("Microsoft YaHei", 6, FontStyle.Regular); // 从8减少到6
                    using var textBrush = new SolidBrush(ForeColor);

                    var textSize = g.MeasureString(valueText, font);
                    var textX = x + (barWidth - textSize.Width) / 2;

                    // 智能选择标签位置：优先放在条形上方，否则放在条形内部
                    var textAboveY = y - textSize.Height - 2; // 条形上方位置
                    var textInsideY = y + 2; // 条形内部上端位置

                    // 检查上方位置是否有足够空间
                    var textY = (textAboveY >= chartRect.Y) ? textAboveY : textInsideY;

                    // 确保标签在图表区域内
                    if (textY + textSize.Height <= chartRect.Bottom && textY >= chartRect.Y)
                    {
                        // 根据标签位置选择合适的文本颜色
                        Color textColor = ForeColor;
                        if (textY == textInsideY) // 如果标签在条形内部
                        {
                            // 使用与背景对比的颜色
                            textColor = GetContrastColor(data.Color);
                        }

                        using var contrastBrush = new SolidBrush(textColor);
                        g.DrawString(valueText, font, contrastBrush, textX, textY);
                    }
                }
            }
        }

        /// <summary>
        /// 根据背景色获取对比色（黑色或白色）
        /// </summary>
        private Color GetContrastColor(Color backgroundColor)
        {
            // 计算RGB亮度值
            var brightness = (backgroundColor.R * 0.299 + backgroundColor.G * 0.587 + backgroundColor.B * 0.114);

            // 根据亮度选择黑色或白色作为对比色
            return brightness > 128 ? Color.Black : Color.White;
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

        #endregion
    }

    /// <summary>
    /// 条形图数据项
    /// </summary>
    public class BarChartData
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public Color Color { get; set; }
    }
}