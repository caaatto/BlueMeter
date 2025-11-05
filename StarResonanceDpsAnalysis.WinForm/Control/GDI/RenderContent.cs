using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.WinForm.Control.GDI
{

    public class RenderContent
    {
        private static readonly Font _defaultFont = SystemFonts.DefaultFont;
        /// <summary>
        /// 内容类型
        /// </summary>
        public ContentType Type { get; set; } = ContentType.Text;
        /// <summary>
        /// 内容对齐方式
        /// </summary>
        public ContentAlign Align { get; set; } = ContentAlign.MiddleLeft;
        /// <summary>
        /// 偏移量, 相对于 Align 后的位置进行偏移
        /// </summary>
        /// <remarks>
        /// 无论 Align 如何设置, Offset 始终 - 为左, + 为右
        /// </remarks>
        public ContentOffset Offset { get; set; } = new ContentOffset { X = 0, Y = 0 };

        /// <summary>
        /// 文本内容
        /// </summary>
        /// <remarks>
        /// Type 为 ContentType.Text 时有效
        /// </remarks>
        public string? Text { get; set; }
        /// <summary>
        /// 文本颜色
        /// </summary>
        /// <remarks>
        /// AutoTextColor 为 true 时, 此属性无效
        /// </remarks>
        public Color ForeColor { get; set; } = Color.Black;
        /// <summary>
        /// 文本字体
        /// </summary>
        public Font Font { get; set; } = _defaultFont;

        /// <summary>
        /// 图片内容
        /// </summary>
        /// <remarks>
        /// Type 为 ContentType.Image 时有效
        /// </remarks>
        public Image? Image { get; set; }
        /// <summary>
        /// 将要绘制的大小
        /// </summary>
        public Size ImageRenderSize { get; set; } = new Size(0, 0);


        public enum ContentType
        {
            /// <summary>
            /// 用于标记当前项为序号项, 组件使用者请勿使用该枚举
            /// </summary>
            [EditorBrowsable(EditorBrowsableState.Never)]
            Order = -1,
            /// <summary>
            /// 文字项
            /// </summary>
            Text = 0,
            /// <summary>
            /// 图片项
            /// </summary>
            Image = 1,
        }
        public enum Direction
        {
            Left = 1,
            Center = 2,
            Right = 4,

            Top = 8,
            Middle = 16,
            Bottom = 32,
        }
        public enum ContentAlign
        {
            /// <summary>
            /// ↖
            /// </summary>
            TopLeft = Direction.Left | Direction.Top,
            /// <summary>
            /// ↑
            /// </summary>
            TopCenter = Direction.Center | Direction.Top,
            /// <summary>
            /// ↗
            /// </summary>
            TopRight = Direction.Right | Direction.Top,
            /// <summary>
            /// ←
            /// </summary>
            MiddleLeft = Direction.Left | Direction.Middle,
            /// <summary>
            /// ○
            /// </summary>
            MiddleCenter = Direction.Center | Direction.Middle,
            /// <summary>
            /// →
            /// </summary>
            MiddleRight = Direction.Right | Direction.Middle,
            /// <summary>
            /// ↙
            /// </summary>
            BottomLeft = Direction.Left | Direction.Bottom,
            /// <summary>
            /// ↓
            /// </summary>
            BottomCenter = Direction.Center | Direction.Bottom,
            /// <summary>
            /// ↘
            /// </summary>
            BottomRight = Direction.Right | Direction.Bottom,
        }
        public struct ContentOffset
        {
            /// <summary>
            /// 左右偏移量, 无论 ContentAlign 如何定义, X 永远 - 为左, + 为右
            /// </summary>
            public int X { get; set; }
            /// <summary>
            /// 上下偏移量, 无论 ContentAlign 如何定义, Y 永远 - 为上, + 为下
            /// </summary>
            public int Y { get; set; }
        }
    }
}
