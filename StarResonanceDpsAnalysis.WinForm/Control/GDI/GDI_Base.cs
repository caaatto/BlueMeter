using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;

namespace StarResonanceDpsAnalysis.WinForm.Control.GDI
{
    public static class GDI_Base
    {
        public static void RenderRoundedCornerRectangle(Graphics g, RectangleF rect, Padding padding, int radius, Brush brush)
        {
            var diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(rect.Width, rect.Height)));
            var cornerRect = new RectangleF(0, 0, diameter, diameter);

            using var path = new GraphicsPath();
            // 左上角
            cornerRect.X = rect.X + padding.Left;
            cornerRect.Y = rect.Y + padding.Top;
            path.AddArc(cornerRect, 180, 90);
            // 右上角
            cornerRect.X = rect.X + padding.Left + (rect.Width - diameter);
            path.AddArc(cornerRect, 270, 90);
            // 右下角
            cornerRect.Y = rect.Y + padding.Bottom + (rect.Height - diameter);
            path.AddArc(cornerRect, 0, 90);
            // 左下角
            cornerRect.X = rect.X + padding.Left;
            path.AddArc(cornerRect, 90, 90);
            // 闭合图形
            path.CloseFigure();

            g.InterpolationMode = InterpolationMode.Default;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.None;

            g.FillPath(brush, path);
        }
    }
}
