using System.Drawing.Drawing2D;

namespace StarResonanceDpsAnalysis.WinForm.Plugin.Charts
{
    /// <summary>
    /// 扁平化散点图控件
    /// </summary>
    public class FlatScatterChart : UserControl
    {
        #region 字段和属性

        private readonly List<ScatterChartSeries> _series = new();
        private bool _isDarkTheme = false;
        private string _titleText = "";
        private string _xAxisLabel = "";
        private string _yAxisLabel = "";
        private bool _showLegend = true;
        private bool _showGrid = true;

        // 边距设置
        private const int PaddingLeft = 60;
        private const int PaddingRight = 20;
        private const int PaddingTop = 40;
        private const int PaddingBottom = 80;

        // 颜色主题
        private readonly Color[] _colors = {
            Color.FromArgb(255, 99, 132),   // 红
            Color.FromArgb(54, 162, 235),   // 蓝
            Color.FromArgb(255, 206, 86),   // 黄
            Color.FromArgb(75, 192, 192),   // 青
            Color.FromArgb(153, 102, 255),  // 紫
            Color.FromArgb(255, 159, 64),   // 橙
            Color.FromArgb(199, 199, 199),  // 灰
            Color.FromArgb(83, 102, 255),   // 靛青
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

        #endregion

        #region 构造函数

        public FlatScatterChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            ApplyTheme();
        }

        #endregion

        #region 数据管理

        public void AddSeries(string name, List<PointF> points)
        {
            var series = new ScatterChartSeries
            {
                Name = name,
                Points = new List<PointF>(points),
                Color = _colors[_series.Count % _colors.Length],
                MarkerSize = 8
            };

            _series.Add(series);
            Invalidate();
        }

        public void ClearSeries()
        {
            _series.Clear();
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

            if (_series.Count == 0 || !_series.Any(s => s.Points.Count > 0))
            {
                DrawNoDataMessage(g);
                return;
            }

            // 计算数据范围
            var dataRange = CalculateDataRange();
            if (dataRange.IsEmpty) return;

            // 计算绘图区域
            var chartRect = new Rectangle(PaddingLeft, PaddingTop,
                                        Width - PaddingLeft - PaddingRight,
                                        Height - PaddingTop - PaddingBottom);

            // 绘制网格
            if (_showGrid)
            {
                DrawGrid(g, chartRect, dataRange);
            }

            // 绘制轴
            DrawAxes(g, chartRect, dataRange);

            // 绘制散点
            DrawScatterPoints(g, chartRect, dataRange);

            // 绘制标题
            DrawTitle(g);

            // 绘制图例
            if (_showLegend && _series.Count > 0)
            {
                DrawLegend(g);
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

        private RectangleF CalculateDataRange()
        {
            var allPoints = _series.SelectMany(s => s.Points);
            if (!allPoints.Any()) return RectangleF.Empty;

            var minX = 0f; // X轴从0开始
            var maxX = allPoints.Max(p => p.X);
            var minY = 0f; // Y轴从0开始
            var maxY = allPoints.Max(p => p.Y);

            // 添加一些边距
            var rangeX = maxX - minX;
            var rangeY = maxY - minY;

            if (rangeX == 0) rangeX = 100; // 默认X轴范围
            if (rangeY == 0) rangeY = 1000; // 默认Y轴范围

            return new RectangleF(minX, minY, rangeX * 1.1f, rangeY * 1.1f);
        }

        private void DrawGrid(Graphics g, Rectangle chartRect, RectangleF dataRange)
        {
            var gridColor = _isDarkTheme ? Color.FromArgb(64, 64, 64) : Color.FromArgb(230, 230, 230);
            using var gridPen = new Pen(gridColor, 1);

            // 绘制垂直网格线
            for (int i = 0; i <= 10; i++)
            {
                var x = chartRect.X + (float)chartRect.Width * i / 10;
                g.DrawLine(gridPen, x, chartRect.Y, x, chartRect.Bottom);
            }

            // 绘制水平网格线
            for (int i = 0; i <= 10; i++)
            {
                var y = chartRect.Y + (float)chartRect.Height * i / 10;
                g.DrawLine(gridPen, chartRect.X, y, chartRect.Right, y);
            }
        }

        private void DrawAxes(Graphics g, Rectangle chartRect, RectangleF dataRange)
        {
            var axisColor = _isDarkTheme ? Color.FromArgb(128, 128, 128) : Color.FromArgb(180, 180, 180);
            using var axisPen = new Pen(axisColor, 1);
            using var textBrush = new SolidBrush(ForeColor);
            using var font = new Font("Microsoft YaHei", 9);

            // 绘制X轴
            g.DrawLine(axisPen, chartRect.X, chartRect.Bottom, chartRect.Right, chartRect.Bottom);

            // 绘制Y轴
            g.DrawLine(axisPen, chartRect.X, chartRect.Y, chartRect.X, chartRect.Bottom);

            // X轴标签
            for (int i = 0; i <= 10; i++)
            {
                var x = chartRect.X + (float)chartRect.Width * i / 10;
                var value = dataRange.X + dataRange.Width * i / 10;
                var text = $"{value:F0}";

                var size = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, x - size.Width / 2, chartRect.Bottom + 5);
            }

            // Y轴标签
            for (int i = 0; i <= 10; i++)
            {
                var y = chartRect.Bottom - (float)chartRect.Height * i / 10;
                var value = dataRange.Y + dataRange.Height * i / 10;
                var text = Common.FormatWithEnglishUnits(value);

                var size = g.MeasureString(text, font);
                g.DrawString(text, font, textBrush, chartRect.X - size.Width - 5, y - size.Height / 2);
            }

            // 轴标签
            if (!string.IsNullOrEmpty(_xAxisLabel))
            {
                var size = g.MeasureString(_xAxisLabel, font);
                var x = chartRect.X + (chartRect.Width - size.Width) / 2;
                var y = chartRect.Bottom + 35;
                g.DrawString(_xAxisLabel, font, textBrush, x, y);
            }

            if (!string.IsNullOrEmpty(_yAxisLabel))
            {
                var size = g.MeasureString(_yAxisLabel, font);
                using var matrix = new Matrix();
                matrix.RotateAt(-90, new PointF(15, chartRect.Y + (chartRect.Height + size.Width) / 2));
                g.Transform = matrix;
                g.DrawString(_yAxisLabel, font, textBrush, 15, chartRect.Y + (chartRect.Height + size.Width) / 2);
                g.ResetTransform();
            }
        }

        private void DrawScatterPoints(Graphics g, Rectangle chartRect, RectangleF dataRange)
        {
            foreach (var series in _series)
            {
                using var brush = new SolidBrush(series.Color);

                foreach (var point in series.Points)
                {
                    var x = chartRect.X + (point.X - dataRange.X) / dataRange.Width * chartRect.Width;
                    var y = chartRect.Bottom - (point.Y - dataRange.Y) / dataRange.Height * chartRect.Height;

                    var markerRect = new RectangleF(
                        x - series.MarkerSize / 2f,
                        y - series.MarkerSize / 2f,
                        series.MarkerSize,
                        series.MarkerSize
                    );

                    // 绘制圆形标记点
                    g.FillEllipse(brush, markerRect);
                }
            }
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

        private void DrawLegend(Graphics g)
        {
            using var font = new Font("Microsoft YaHei", 9);
            using var textBrush = new SolidBrush(ForeColor);

            var legendHeight = _series.Count * 20 + 10;
            var legendWidth = _series.Max(s => (int)g.MeasureString(s.Name, font).Width) + 30;
            var legendX = Width - legendWidth - 10;
            var legendY = PaddingTop + 10;

            // 绘制图例背景
            var legendBg = _isDarkTheme ? Color.FromArgb(40, 40, 40) : Color.FromArgb(250, 250, 250);
            using var bgBrush = new SolidBrush(legendBg);
            g.FillRectangle(bgBrush, legendX - 5, legendY - 5, legendWidth, legendHeight);

            // 绘制图例项
            for (int i = 0; i < _series.Count; i++)
            {
                var series = _series[i];
                var y = legendY + i * 20;

                // 绘制颜色标记（圆形）
                using var colorBrush = new SolidBrush(series.Color);
                g.FillEllipse(colorBrush, legendX, y + 4, 12, 12);

                // 绘制文本
                g.DrawString(series.Name, font, textBrush, legendX + 18, y);
            }
        }

        #endregion
    }

    /// <summary>
    /// 散点图数据系列
    /// </summary>
    public class ScatterChartSeries
    {
        public string Name { get; set; } = "";
        public List<PointF> Points { get; set; } = new();
        public Color Color { get; set; }
        public int MarkerSize { get; set; } = 8;
    }
}