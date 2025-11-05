using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using StarResonanceDpsAnalysis.WinForm.Control.GDI;

namespace StarResonanceDpsAnalysis.WinForm.Control
{
    public partial class SortedProgressBarList : UserControl
    {
        private readonly Dictionary<long, ProgressBarData> _dataDict = [];
        private int _progressBarHeight = 20;
        private Padding _progressBarPadding = new(3, 3, 3, 3);
        private RenderContent.ContentAlign _orderAlign = RenderContent.ContentAlign.MiddleLeft;
        private RenderContent.ContentOffset _orderOffset = new() { X = 0, Y = 0 };
        private List<Image>? _orderImages = null;
        private RenderContent.ContentAlign _orderImageAlign = RenderContent.ContentAlign.MiddleLeft;
        private RenderContent.ContentOffset _orderImageOffset = new() { X = 0, Y = 0 };
        private Size _orderImageRenderSize = new(0, 0);
        private Func<int, string>? _orderCallback = null;
        private Color _orderColor = Color.Black;
        private Font _orderFont = SystemFonts.DefaultFont;
        private float _scrollOffsetY = 0;
        private int _scrollBarWidth = 8;
        private int _scrollBarPadding = 1;
        private bool _isMouseDownInScrollBar = false;
        private Point? _prevMousePoint = null;
        private RectangleF _scrollBarRect = new(0, 0, 0, 0);

        /// <summary>
        /// 数据源
        /// </summary>
        public List<ProgressBarData>? Data
        {
            get => [.. _dataDict.Values];
            set
            {
                lock (_lock)
                {
                    // 清除所有项目
                    if (value == null || value.Count == 0)
                    {
                        _dataDict.Clear();

                        Invalidate();

                        return;
                    }

                    // 移除不存在的项
                    foreach (var key in _dataDict.Keys.ToList())
                    {
                        if (!value.Exists(e => e.ID == key))
                            _dataDict.Remove(key);
                    }

                    // 更新或新增项（后者覆盖同 ID）
                    foreach (var item in value)
                    {
                        if (item == null || item.ID < 0) continue;
                        _dataDict[item.ID] = item;
                    }

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 进度条高度
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条高度")]
        [DefaultValue(20)]
        public int ProgressBarHeight
        {
            get => _progressBarHeight;
            set => _progressBarHeight = value;
        }

        /// <summary>
        /// 进度条内边距
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条内边距")]
        [DefaultValue(typeof(Padding), "3,3,3,3")]
        public Padding ProgressBarPadding
        {
            get => _progressBarPadding;
            set => _progressBarPadding = value;
        }

        /// <summary>
        /// 排序序号对齐模式
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("排序序号对齐模式")]
        [DefaultValue(RenderContent.ContentAlign.MiddleLeft)]
        public RenderContent.ContentAlign OrderAlign
        {
            get => _orderAlign;
            set => _orderAlign = value;
        }

        /// <summary>
        /// 排序序号偏移量
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("排序序号偏移量")]
        public RenderContent.ContentOffset OrderOffset
        {
            get => _orderOffset;
            set => _orderOffset = value;
        }

        /// <summary>
        /// 排序中根据序号显示的图片列表
        /// </summary>
        public List<Image>? OrderImages
        {
            get => _orderImages;
            set => _orderImages = value;
        }

        /// <summary>
        /// 排序序号图片对齐模式
        /// </summary>
        public RenderContent.ContentAlign OrderImageAlign
        {
            get => _orderImageAlign;
            set => _orderImageAlign = value;
        }

        /// <summary>
        /// 排序序号图片偏移量
        /// </summary>
        public RenderContent.ContentOffset OrderImageOffset
        {
            get => _orderImageOffset;
            set => _orderImageOffset = value;
        }

        /// <summary>
        /// 排序序号图片渲染大小
        /// </summary>
        public Size OrderImageRenderSize
        {
            get => _orderImageRenderSize;
            set => _orderImageRenderSize = value;
        }

        /// <summary>
        /// 序号文字重排回调
        /// </summary>
        /// <remarks>
        /// 会传递给函数一个从 1 开始的 int 序号, 
        /// 将其转为所需类型的 string 后返回即可;
        /// 如果函数为 null, 则不会显示序号
        /// </remarks>
        public Func<int, string>? OrderCallback
        {
            private get => _orderCallback;
            set => _orderCallback = value;
        }

        /// <summary>
        /// 序号文字颜色
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("序号文字颜色")]
        public Color OrderColor
        {
            get => _orderColor;
            set => _orderColor = value;
        }

        /// <summary>
        /// 序号文字字体
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("序号文字字体")]
        public Font OrderFont
        {
            get => _orderFont;
            set => _orderFont = value;
        }

        /// <summary>
        /// 滚动偏移量 Y
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("滚动偏移量 Y")]
        [DefaultValue(0)]
        public float ScrollOffsetY
        {
            get => _scrollOffsetY;
            set
            {
                value = Math.Max(0, Math.Min(_infoBuffer.Count * ProgressBarHeight - Height, value));

                if (_scrollOffsetY != value)
                {
                    _scrollOffsetY = value;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 滚动条宽度
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("滚动条宽度")]
        [DefaultValue(6)]
        public int ScrollBarWidth
        {
            get => _scrollBarWidth;
            set
            {
                if (_scrollBarWidth != value)
                {
                    _scrollBarWidth = value;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 滚动条内边距
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("滚动条内边距")]
        [DefaultValue(1)]
        public int ScrollBarPadding
        {
            get => _scrollBarPadding;
            set
            {
                if (_scrollBarPadding != value)
                {
                    _scrollBarPadding = value;

                    Invalidate();
                }
            }
        }

        public SortedProgressBarList()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Redraw(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            ScrollOffsetY -= e.Delta;

            Invalidate();
        }

        private void SortedProgressBarList_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) > 0)
            {
                _prevMousePoint = e.Location;

                // 判定鼠标着陆点在 ScrollBar 范围之内
                _isMouseDownInScrollBar = e.Location.X > _scrollBarRect.X && e.Location.X < _scrollBarRect.X + _scrollBarRect.Width
                    && e.Location.Y > _scrollBarRect.Y && e.Location.Y < _scrollBarRect.Y + _scrollBarRect.Height;
            }
            else
            {
                _prevMousePoint = null;
                _isMouseDownInScrollBar = false;
            }
        }

        private void SortedProgressBarList_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (_prevMousePoint == null || !_isMouseDownInScrollBar)
                {
                    return;
                }

                var offsetY = e.Location.Y - _prevMousePoint.Value.Y;

                ScrollOffsetY += offsetY * (1f * _infoBuffer.Count * ProgressBarHeight / Height);
            }
            finally
            {
                _prevMousePoint = (e.Button & MouseButtons.Left) > 0
                    ? e.Location
                    : null;
            }
        }

        private void SortedProgressBarList_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == 0)
            {
                _prevMousePoint = null;
                _isMouseDownInScrollBar = false;
            }
        }
    }
}
