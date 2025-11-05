using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using BlueMeter.WinForm.Control.GDI;

namespace BlueMeter.WinForm.Control
{
    public partial class TextProgressBar : UserControl
    {
        private double _progressBarValue = 0.0d;
        private Color _progressBarColor = Color.FromArgb(0x56, 0x9C, 0xD6);
        private int _progressBarCornerRadius = 3;
        private List<RenderContent> _contentList = [];

        /// <summary>
        /// 进度条进度
        /// </summary>
        /// <remarks>
        /// 0.0d ~ 1.0d, 会自动限制在这个范围内
        /// </remarks>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条进度\r\n0.0d ~ 1.0d, 会自动限制在这个范围内")]
        [DefaultValue(1.0d)]
        public double ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                if (_progressBarValue != value)
                {
                    _progressBarValue = Math.Max(0.0d, Math.Min(1.0d, value));

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 进度条颜色
        /// </summary>
        /// <remarks>
        /// 默认色是从 VS public 关键字上复制的...
        /// </remarks>
        [Browsable(true)]
        [Category("外观")]
        [Description("进度条颜色")]
        public Color ProgressBarColor
        {
            get => _progressBarColor;
            set
            {
                if (_progressBarColor != value)
                {
                    _progressBarColor = value;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        [Browsable(true)]
        [Category("外观")]
        [Description("圆角半径")]
        [DefaultValue(0)]
        public int ProgressBarCornerRadius
        {
            get => _progressBarCornerRadius;
            set
            {
                // 此处不进行多余合法性判断, 绘制时会根据情况自适应
                if (_progressBarCornerRadius != value)
                {
                    _progressBarCornerRadius = value;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// 渲染内容列表
        /// </summary>
        public List<RenderContent> ContentList
        {
            get => _contentList;
            set
            {
                _contentList = value;

                Invalidate();
            }
        }

        public TextProgressBar()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawTextProgressBarControl(e);
        }
    }
}
