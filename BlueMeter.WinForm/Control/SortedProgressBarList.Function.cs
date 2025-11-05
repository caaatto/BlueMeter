using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlueMeter.WinForm.Control.GDI;

using static BlueMeter.WinForm.Control.GDI.RenderContent;

namespace BlueMeter.WinForm.Control
{
    public partial class SortedProgressBarList
    {
        private readonly object _lock = new();
        private readonly Dictionary<long, GDI_ProgressBar> _gdiProgressBarDict = [];
        private readonly DrawInfo _drawInfo = new();
        private readonly ProgressBarPrivateData _progressBarPrivateData = new();
        private readonly List<RenderContent> _renderContentBuffer = [];

        private readonly SolidBrush _scrollBarBrush = new(Color.FromArgb(0xB2, 0xB2, 0xB2));

        private List<SortingInfo> _infoBuffer = new(128);

        public void Redraw(PaintEventArgs e, bool needResort = true)
        {
            if (Width == 0 || Height == 0) return;

            lock (_lock)
            {
                var g = e.Graphics;

                g.Clear(BackColor);

                if (needResort) Resort();

                // 绘制进度条内容
                foreach (var data in _infoBuffer)
                {
                    var outOfHeightIndex = (Height / ProgressBarHeight) + 1;
                    var top = (float)data.Order * ProgressBarHeight - ScrollOffsetY;

                    _progressBarPrivateData.OrderAlign = OrderAlign;
                    _progressBarPrivateData.OrderColor = OrderColor;
                    _progressBarPrivateData.OrderFont = OrderFont;
                    _progressBarPrivateData.OrderOffset = OrderOffset;
                    _progressBarPrivateData.OrderImageAlign = OrderImageAlign;
                    _progressBarPrivateData.OrderImageOffset = OrderImageOffset;
                    _progressBarPrivateData.OrderString = OrderCallback?.Invoke(data.Order + 1);

                    _progressBarPrivateData.OrderImage = OrderImages == null || data.Order >= OrderImages.Count
                        ? null
                        : OrderImages[data.Order];

                    _progressBarPrivateData.OrderImageRenderSize =
                        _progressBarPrivateData.OrderImage != null && (OrderImageRenderSize.Width == 0 || OrderImageRenderSize.Height == 0)
                        ? _progressBarPrivateData.OrderImage.Size
                        : OrderImageRenderSize;

                    _progressBarPrivateData.Top = top;
                    _progressBarPrivateData.Opacity = 255;

                    if (top < -ProgressBarHeight || top > Height)
                    {
                        continue;
                    }

                    DrawProgressBar(g, data.Data, _progressBarPrivateData);
                }

                // 绘制滚动条
                var totalHeight = Padding.Top + Padding.Bottom + _infoBuffer.Count * ProgressBarHeight;
                var scrollBarSizePersent = Math.Min(1f * Height / totalHeight, 1);
                var offsetYPersent = (1f - scrollBarSizePersent) * ScrollOffsetY / (totalHeight - Height);

                _scrollBarRect.X = Width - ScrollBarWidth - ScrollBarPadding;
                _scrollBarRect.Y = Height * offsetYPersent;
                _scrollBarRect.Width = ScrollBarWidth - ScrollBarPadding * 2;
                _scrollBarRect.Height = Height * scrollBarSizePersent;

                GDI_Base.RenderRoundedCornerRectangle(
                    g,
                    _scrollBarRect,
                    new Padding(ScrollBarPadding),
                    99,
                    _scrollBarBrush);
            }
        }

        private void Resort()
        {
            // 移除不存在的渲染项
            _infoBuffer.RemoveAll(info => !_dataDict.ContainsKey(info.ID));

            foreach (var data in _dataDict)
            {
                // 添加新增数据
                if (!ContainsId(_infoBuffer, data.Key))
                {
                    _infoBuffer.Add(new SortingInfo
                    {
                        ID = data.Key,
                        Order = -1,
                        Data = data.Value
                    });
                }
            }

            _infoBuffer.Sort((a, b) => b.Data.ProgressBarValue.CompareTo(a.Data.ProgressBarValue));

            // 重新排序, 并更新 Order
            for (var i = 0; i < _infoBuffer.Count; ++i) 
            {
                var info = _infoBuffer[i];
                info.Order = i;
                info.Data = _dataDict[info.ID];
                _infoBuffer[i] = info;
            }
        }

        private bool ContainsId(List<SortingInfo> list, long id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id) return true;
            }
            return false;
        }

        private void DrawProgressBar(Graphics g, ProgressBarData data, ProgressBarPrivateData privateData)
        {
            var flag = _gdiProgressBarDict.TryGetValue(data.ID, out var gdiProgressBar);
            if (!flag || gdiProgressBar == null)
            {
                gdiProgressBar = new GDI_ProgressBar();

                _gdiProgressBarDict[data.ID] = gdiProgressBar;
            }

            _drawInfo.Width = Width - ScrollBarWidth;
            _drawInfo.Height = ProgressBarHeight;
            _drawInfo.BackColor = BackColor;
            _drawInfo.ProgressBarColor = Color.FromArgb(privateData.Opacity, data.ProgressBarColor);
            _drawInfo.ProgressBarValue = data.ProgressBarValue;
            _drawInfo.ProgressBarCornerRadius = data.ProgressBarCornerRadius;
            _drawInfo.Padding = data.ProgressBarPadding;
            _drawInfo.Top = privateData.Top;

            if (privateData.OrderImage == null && privateData.OrderString == null)
            {
                _drawInfo.ContentList = data.ContentList;
            }
            else
            {
                _renderContentBuffer.Clear();

                if (privateData.OrderImage != null)
                {
                    _renderContentBuffer.Add(new RenderContent
                    {
                        Type = ContentType.Image,
                        Align = privateData.OrderImageAlign,
                        Offset = privateData.OrderImageOffset,
                        Image = privateData.OrderImage,
                        ImageRenderSize = privateData.OrderImageRenderSize
                    });
                }

                if (!string.IsNullOrEmpty(privateData.OrderString))
                {
                    _renderContentBuffer.Add(new RenderContent
                    {
                        Type = ContentType.Text,
                        Align = privateData.OrderAlign,
                        Offset = privateData.OrderOffset,
                        Text = privateData.OrderString,
                        ForeColor = privateData.OrderColor,
                        Font = privateData.OrderFont,
                    });
                }

                if (data.ContentList != null)
                {
                    _renderContentBuffer.AddRange(data.ContentList);
                }

                _drawInfo.ContentList = _renderContentBuffer;
            }



            gdiProgressBar.Draw(g, _drawInfo);
        }

        private class ProgressBarPrivateData
        {
            public float Top { get; set; }
            public byte Opacity { get; set; }
            public ContentAlign OrderAlign { get; set; } = ContentAlign.MiddleLeft;
            public ContentOffset OrderOffset { get; set; } = new ContentOffset { X = 0, Y = 0 };
            public string? OrderString { get; set; }
            public Color OrderColor { get; set; } = Color.Black;
            public Font OrderFont { get; set; } = SystemFonts.DefaultFont;
            public Image? OrderImage { get; set; } = null;
            public ContentAlign OrderImageAlign { get; set; } = ContentAlign.MiddleLeft;
            public ContentOffset OrderImageOffset { get; set; } = new ContentOffset { X = 0, Y = 0 };
            public Size OrderImageRenderSize { get; set; } = new Size(0, 0);
        }

        private struct SortingInfo
        {
            public long ID { get; set; }
            public int Order { get; set; }
            public ProgressBarData Data { get; set; }
        }
    }

    public class ProgressBarData
    {
        public long ID { get; set; }
        public double ProgressBarValue { get; set; }
        public Color ProgressBarColor { get; set; } = Color.FromArgb(0x56, 0x9C, 0xD6);
        public int ProgressBarCornerRadius { get; set; }
        public List<RenderContent>? ContentList { get; set; }
        public Padding ProgressBarPadding { get; set; } = new(3, 3, 3, 3);
    }
}
