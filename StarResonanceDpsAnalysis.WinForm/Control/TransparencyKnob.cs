using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace StarResonanceDpsAnalysis.WinForm.Control
{
    /// <summary>
    /// 透明度旋钮控件
    /// </summary>
    public partial class TransparencyKnob : UserControl
    {
        private int _value = 100;
        private int _minimum = 10;
        private int _maximum = 100;
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private double _lastValidAngle = 45; // 记录上次有效角度，初始化为最大值对应角度
        private Color _knobColor = Color.FromArgb(34, 151, 244);
        private Color _trackColor = Color.FromArgb(220, 220, 220);
        private Color _textColor = Color.FromArgb(160, 160, 160);
        private Color _highlightColor = Color.FromArgb(100, 34, 151, 244);
        private Color _indicatorColor = Color.FromArgb(180, 50, 50);
        private Color _outerIndicatorColor = Color.FromArgb(255, 50, 150, 255);
        private Color _startMarkerColor = Color.FromArgb(180, 180, 180);
        private Color _endMarkerColor = Color.FromArgb(120, 120, 120);
        private Color _centerColor = Color.FromArgb(200, 200, 200); // 中心纯色
        private Color _centerBorderColor = Color.FromArgb(160, 160, 160); // 中心边框颜色
        private float _textSize = 6.3f;
        private int _textOffsetY = 15;
        private bool _isDarkMode = false;
        private bool _isHovering = false;

        public event EventHandler<int> ValueChanged;

        /// <summary>
        /// 指示点颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("旋钮指示点的颜色")]
        [DefaultValue(typeof(Color), "180, 50, 50")]
        public Color IndicatorColor
        {
            get => _indicatorColor;
            set
            {
                if (_indicatorColor != value)
                {
                    _indicatorColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 外圈指示线条颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("外圈位置指示线条的颜色")]
        [DefaultValue(typeof(Color), "50, 150, 255")]
        public Color OuterIndicatorColor
        {
            get => _outerIndicatorColor;
            set
            {
                if (_outerIndicatorColor != value)
                {
                    _outerIndicatorColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 起始点标识颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("起始点标识的颜色")]
        [DefaultValue(typeof(Color), "180, 180, 180")]
        public Color StartMarkerColor
        {
            get => _startMarkerColor;
            set
            {
                if (_startMarkerColor != value)
                {
                    _startMarkerColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 结束点标识颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("结束点标识的颜色")]
        [DefaultValue(typeof(Color), "120, 120, 120")]
        public Color EndMarkerColor
        {
            get => _endMarkerColor;
            set
            {
                if (_endMarkerColor != value)
                {
                    _endMarkerColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 中心颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("旋钮中心的纯色填充颜色")]
        [DefaultValue(typeof(Color), "200, 200, 200")]
        public Color CenterColor
        {
            get => _centerColor;
            set
            {
                if (_centerColor != value)
                {
                    _centerColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 中心边框颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("旋钮中心边框的颜色")]
        [DefaultValue(typeof(Color), "160, 160, 160")]
        public Color CenterBorderColor
        {
            get => _centerBorderColor;
            set
            {
                if (_centerBorderColor != value)
                {
                    _centerBorderColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 文字颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("显示文字的颜色")]
        [DefaultValue(typeof(Color), "160, 160, 160")]
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 文字大小
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("显示文字的字体大小")]
        [DefaultValue(6.3f)]
        public float TextSize
        {
            get => _textSize;
            set
            {
                if (_textSize != value && value > 0)
                {
                    _textSize = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 文字垂直偏移
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("文字相对于旋钮的垂直偏移量")]
        [DefaultValue(15)]
        public int TextOffsetY
        {
            get => _textOffsetY;
            set
            {
                if (_textOffsetY != value)
                {
                    _textOffsetY = value;
                    Invalidate();
                }
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                if (value < _minimum) value = _minimum;
                if (value > _maximum) value = _maximum;
                if (_value != value)
                {
                    _value = value;
                    Invalidate();
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                if (_value < _minimum) Value = _minimum;
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                if (_value > _maximum) Value = _maximum;
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                UpdateColors();
                Invalidate();
            }
        }

        public TransparencyKnob()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(160, 140);
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (_isDarkMode)
            {
                _knobColor = Color.FromArgb(34, 151, 244);
                _trackColor = Color.FromArgb(80, 80, 80);
                _highlightColor = Color.FromArgb(150, 34, 151, 244);
                BackColor = Color.Transparent;
            }
            else
            {
                _knobColor = Color.FromArgb(34, 151, 244);
                _trackColor = Color.FromArgb(220, 220, 220);
                _highlightColor = Color.FromArgb(100, 34, 151, 244);
                BackColor = Color.Transparent;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            var center = new Point(rect.Width / 2, rect.Height / 2 - 10);
            var radius = Math.Min(rect.Width - 40, rect.Height - 40) / 2 - 15;

            // 计算当前值对应的角度 (从-225度到45度，总共270度)
            var angle = -225 + (270.0 * (_value - _minimum) / (_maximum - _minimum));
            var angleRad = angle * Math.PI / 180;

            // 绘制起始和结束点标识
            DrawStartEndMarkers(g, center, radius);

            // 绘制外圆环阴影
            if (!_isDarkMode)
            {
                var shadowRect = new Rectangle(center.X - radius - 2, center.Y - radius - 2, (radius + 2) * 2, (radius + 2) * 2);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.FillEllipse(shadowBrush, shadowRect);
                }
            }

            // 绘制外圆环 (轨道) - 从左下到右下的270度弧
            var trackRect = new Rectangle(center.X - radius, center.Y - radius, radius * 2, radius * 2);
            using (var pen = new Pen(_trackColor, 6))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, trackRect, -225, 270);
            }

            // 绘制主进度弧 - 使用更明显的颜色
            using (var pen = new Pen(_knobColor, 6))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                if (angle > -225)
                    g.DrawArc(pen, trackRect, -225, (float)(angle + 225));
            }

            // 绘制外圈位置指示线条 - 单层设计
            DrawOuterPositionIndicator(g, center, radius, angle);

            // 绘制简洁的中心设计
            DrawCenterDesign(g, center);

            // 绘制旋钮指示点（更靠近边缘，使用自定义颜色）
            DrawKnobIndicator(g, center, radius, angleRad);

            // 绘制文字，使用用户自定义的属性
            var labelText = "透明度";
            var valueText = _value.ToString();
            var combinedText = $"{labelText} {valueText}";

            using (var font = new Font("HarmonyOS Sans SC", _textSize))
            using (var brush = new SolidBrush(_textColor))
            {
                var textSize = g.MeasureString(combinedText, font);
                var textRect = new PointF(center.X - textSize.Width / 2, center.Y + radius + _textOffsetY);
                g.DrawString(combinedText, font, brush, textRect);
            }

            // 绘制刻度标记
            DrawScaleMarks(g, center, radius);
        }

        private void DrawCenterDesign(Graphics g, Point center)
        {
            var centerRadius = 25;
            var centerRect = new Rectangle(center.X - centerRadius, center.Y - centerRadius, centerRadius * 2, centerRadius * 2);

            // 中心圆阴影
            if (!_isDarkMode)
            {
                var shadowCenterRect = new Rectangle(center.X - centerRadius - 2, center.Y - centerRadius - 2, (centerRadius + 2) * 2, (centerRadius + 2) * 2);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    g.FillEllipse(shadowBrush, shadowCenterRect);
                }
            }

            // 绘制纯色背景，替换原有的渐变效果
            using (var centerBrush = new SolidBrush(_centerColor))
            {
                g.FillEllipse(centerBrush, centerRect);
            }

            // 中心圆边框 - 使用用户自定义颜色
            using (var pen = new Pen(_centerBorderColor, 2))
            {
                g.DrawEllipse(pen, centerRect);
            }

            // 移除所有渐变效果、高光效果、几何图案和装饰点 - 保持纯净的中心设计
        }

        private void DrawStartEndMarkers(Graphics g, Point center, int radius)
        {
            // 计算左下角起始点和右下角结束点位置
            var startAngleRad = -225 * Math.PI / 180; // 左下角
            var endAngleRad = 45 * Math.PI / 180;     // 右下角

            var markerRadius = radius + 20;

            // 起始点位置
            var startX = center.X + markerRadius * Math.Cos(startAngleRad);
            var startY = center.Y + markerRadius * Math.Sin(startAngleRad);

            // 结束点位置  
            var endX = center.X + markerRadius * Math.Cos(endAngleRad);
            var endY = center.Y + markerRadius * Math.Sin(endAngleRad);

            // 绘制起始点符号（小点）- 使用用户自定义颜色
            using (var brush = new SolidBrush(_startMarkerColor))
            {
                g.FillEllipse(brush, (float)startX - 3, (float)startY - 3, 6, 6);
            }

            // 绘制结束点符号（稍大的点）- 使用用户自定义颜色
            using (var brush = new SolidBrush(_endMarkerColor))
            {
                g.FillEllipse(brush, (float)endX - 4, (float)endY - 4, 8, 8);
            }
        }

        private void DrawKnobIndicator(Graphics g, Point center, int radius, double angleRad)
        {
            // 调整指示点位置：从 radius + 5 改为 radius + 6
            var indicatorRadius = radius + 6; // 稍微往外移动一点
            var indicatorX = center.X + indicatorRadius * Math.Cos(angleRad);
            var indicatorY = center.Y + indicatorRadius * Math.Sin(angleRad);

            // 指示点阴影
            if (!_isDarkMode)
            {
                using (var shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                {
                    var shadowRect = new Rectangle((int)indicatorX - 2, (int)indicatorY - 2, 5, 5);
                    g.FillEllipse(shadowBrush, shadowRect);
                }
            }

            // 直接绘制指示点主体（使用用户自定义颜色）
            using (var brush = new SolidBrush(_indicatorColor))
            {
                var indicatorRect = new Rectangle((int)indicatorX - 2, (int)indicatorY - 2, 4, 4);
                g.FillEllipse(brush, indicatorRect);
            }

            // 在指示点内部添加一个小的高亮点以增强立体感
            using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255))) // 白色高光
            {
                var highlightRect = new Rectangle((int)indicatorX, (int)indicatorY, 1, 1);
                g.FillEllipse(brush, highlightRect);
            }
        }

        private void DrawScaleMarks(Graphics g, Point center, int radius)
        {
            using (var pen = new Pen(Color.FromArgb(80, _textColor.R, _textColor.G, _textColor.B), 1.5f))
            {
                // 在主要位置绘制刻度标记
                for (int i = 0; i <= 4; i++)
                {
                    var angle = -225 + (270.0 * i / 4);
                    var angleRad = angle * Math.PI / 180;
                    var markLength = 10;

                    var outerX = center.X + (radius - 6) * Math.Cos(angleRad);
                    var outerY = center.Y + (radius - 6) * Math.Sin(angleRad);
                    var innerX = center.X + (radius - 6 - markLength) * Math.Cos(angleRad);
                    var innerY = center.Y + (radius - 6 - markLength) * Math.Sin(angleRad);

                    g.DrawLine(pen, (float)innerX, (float)innerY, (float)outerX, (float)outerY);
                }
            }
        }

        private void DrawOuterPositionIndicator(Graphics g, Point center, int radius, double currentAngle)
        {
            // 只有当前角度大于起始角度时才绘制外圈位置指示线条
            if (currentAngle <= -225) return;

            var outerRadius = radius + 12; // 适中的距离
            var startAngle = -225;
            var sweepAngle = currentAngle - startAngle;

            if (sweepAngle > 0)
            {
                var outerRect = new Rectangle(center.X - outerRadius, center.Y - outerRadius, outerRadius * 2, outerRadius * 2);

                // 简单单层外圈指示线条 - 线条不要太宽
                using (var pen = new Pen(_outerIndicatorColor, 4))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, outerRect, (float)startAngle, (float)sweepAngle);
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;
            // 移除Invalidate()，不需要重绘悬停效果
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;
            // 移除Invalidate()，不需要重绘悬停效果
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _lastMousePosition = e.Location;

                // 根据当前值初始化_lastValidAngle
                _lastValidAngle = -225 + (270.0 * (_value - _minimum) / (_maximum - _minimum));

                UpdateValueFromMouse(e.Location);
                Capture = true;
                Invalidate(); // 只在拖拽时重绘
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                UpdateValueFromMouse(e.Location);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                Capture = false;
                Invalidate(); // 只在拖拽结束时重绘
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            var delta = e.Delta > 0 ? 5 : -5;
            Value = _value + delta;
        }

        private void UpdateValueFromMouse(Point mousePos)
        {
            var center = new Point(Width / 2, Height / 2 - 10);
            var dx = mousePos.X - center.X;
            var dy = mousePos.Y - center.Y;

            // 避免除零错误和处理中心点
            if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1) return;

            var angle = Math.Atan2(dy, dx) * 180 / Math.PI;

            // 将角度转换为0-360范围
            if (angle < 0) angle += 360;

            // 完全重新设计角度映射逻辑 - 修复跳跃问题
            double normalizedAngle;

            // 旋钮有效范围：-225° 到 +45° (共270°)
            // 对应鼠标位置：左下角(-225°) 到 右下角(45°)

            if (angle >= 0 && angle <= 45)
            {
                // 右下区域：0°-45° -> 直接对应旋钮的0°-45°(高值区域)
                normalizedAngle = angle;
            }
            else if (angle > 45 && angle < 135)
            {
                // 右侧禁用区域：智能选择边界，防止跳跃
                if (_isDragging)
                {
                    // 拖拽时，选择更接近上次位置的边际
                    var distTo45 = Math.Min(Math.Abs(angle - 45), Math.Abs(angle - (45 + 360)));
                    var distTo225 = Math.Min(Math.Abs(angle + 225), Math.Abs(angle - (225 - 360)));
                    normalizedAngle = distTo45 < distTo225 ? 45 : -225;
                }
                else
                {
                    // 非拖拽时保持最大值
                    normalizedAngle = 45;
                }
            }
            else if (angle >= 135 && angle <= 225)
            {
                // 上方到左侧区域：对应旋钮的-225°(最小值)
                normalizedAngle = -225;
            }
            else if (angle > 225 && angle < 315)
            {
                // 左下方区域：225°-315° -> 线性映射到-225°到-45°
                // 这个区域对应从最小值到中等值的过渡
                var mappedRange = (angle - 225) / 90.0; // 0.0 to 1.0
                normalizedAngle = -225 + mappedRange * 180; // -225° to -45°
            }
            else // angle >= 315 && angle < 360
            {
                // 下方区域：315°-360° -> 线性映射到-45°到0°
                // 这个区域对应从中等值到高值的过渡
                var mappedRange = (angle - 315) / 45.0; // 0.0 to 1.0  
                normalizedAngle = -45 + mappedRange * 45; // -45° to 0°
            }

            // 拖拽时的跳跃检测和保护
            if (_isDragging)
            {
                var angleDiff = Math.Abs(normalizedAngle - _lastValidAngle);
                // 如果角度变化太大(超过180度)，可能是跳跃，使用渐进调整
                if (angleDiff > 180)
                {
                    var direction = normalizedAngle > _lastValidAngle ? 1 : -1;
                    normalizedAngle = _lastValidAngle + direction * Math.Min(angleDiff, 30);
                }
            }

            // 确保角度在有效范围内
            normalizedAngle = Math.Max(-225, Math.Min(45, normalizedAngle));

            // 更新上次有效角度
            _lastValidAngle = normalizedAngle;

            // 计算对应的数值
            var progress = (normalizedAngle + 225) / 270.0; // 0.0 to 1.0
            var newValue = _minimum + (int)Math.Round(progress * (_maximum - _minimum));

            // 确保数值在有效范围内
            newValue = Math.Max(_minimum, Math.Min(_maximum, newValue));

            // 添加调试输出，帮助分析问题
#if DEBUG
            Console.WriteLine($"Mouse: ({mousePos.X}, {mousePos.Y}), Angle: {angle:F1}°, Normalized: {normalizedAngle:F1}°, Progress: {progress:F2}, Value: {newValue}");
#endif

            Value = newValue;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TransparencyKnob
            // 
            this.Name = "TransparencyKnob";
            this.Size = new System.Drawing.Size(160, 140);
            this.ResumeLayout(false);
        }
    }
}