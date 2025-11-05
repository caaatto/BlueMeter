using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StarResonanceDpsAnalysis.WinForm.Core.Module
{
    public class ModuleCardDisplay
    {
        public class ResultCardSimpleItem : AntdUI.VirtualShadowItem
        {
            // ====== 数据 ======
            public string RankText { get; set; } = "第一名搭配";
            public int HighestLevel { get; set; } = 5;
            public string Score { get; set; } = "";
            public int? ForceCardWidthPx { get; set; }
            public int? ForceCardHeightPx { get; set; }

            public List<(string Left, string Right)> ModuleRows { get; set; } = new();
            public List<(string Name, int Value)> AttrLines { get; set; } = new();

            // ====== 外观 ======
            public Padding ContentPadding { get; set; } = new Padding(16, 16, 16, 16);
            public int LineGap { get; set; } = 6;
            public int SectionGap { get; set; } = 10;
            public int MinHeightDp { get; set; } = 160;
            public int LeftRightGap { get; set; } = 8;

            private static Color H(string hex) => ColorTranslator.FromHtml(hex);
            private static readonly Color Primary = H("#2563EB");
            private static readonly Color Muted = H("#64748B");

            public override void Paint(AntdUI.Canvas g, AntdUI.VirtualPanelArgs e)
            {
                float dpi = AntdUI.Config.Dpi;
                int padL = (int)(ContentPadding.Left * dpi);
                int padT = (int)(ContentPadding.Top * dpi);
                int padR = (int)(ContentPadding.Right * dpi);
                int padB = (int)(ContentPadding.Bottom * dpi);

                using (var path = AntdUI.Helper.RoundPath(e.Rect, e.Radius))
                {
                    g.Fill(AntdUI.Style.Db.BgContainer, path);
                    var border = Hover ? AntdUI.Style.Db.BorderColorDisable : AntdUI.Style.Db.BorderColor;
                    g.Draw(border, 1 * dpi, path);
                }

                int x = e.Rect.X + padL;
                int y = e.Rect.Y + padT;
                int w = Math.Max(1, e.Rect.Width - padL - padR);

                var sfLT = AntdUI.Helper.SF(lr: StringAlignment.Near, tb: StringAlignment.Near);

                var fontTitle = new Font(e.Panel.Font, FontStyle.Bold);
                var fontBody = e.Panel.Font ?? SystemFonts.DefaultFont;

                // 标题
                var sz = AntdUI.Helper.Size(g.MeasureString(RankText ?? string.Empty, fontTitle, w));
                g.String(RankText ?? string.Empty, fontTitle, AntdUI.Style.Db.Text,
                         new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 最高属性等级
                string levelText = $"最高属性等级 {HighestLevel}";
                sz = AntdUI.Helper.Size(g.MeasureString(levelText, fontBody, w));
                g.String(levelText, fontBody, AntdUI.Style.Db.Text,
                         new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 综合评分
                string scoreLabel = "综合评分：";
                int labelW = AntdUI.Helper.Size(g.MeasureString(scoreLabel, fontBody)).Width;
                sz = AntdUI.Helper.Size(g.MeasureString($"{scoreLabel}{Score}", fontBody, w));
                g.String(scoreLabel, fontBody, Muted, new Rectangle(x, y, w, sz.Height), sfLT);
                g.String($"{Score}", fontBody, Primary,
                         new Rectangle(x + labelW, y, Math.Max(1, w - labelW), sz.Height), sfLT);
                y += sz.Height + SectionGap;

                // 模组列表标题
                const string listTitle = "模组列表：";
                sz = AntdUI.Helper.Size(g.MeasureString(listTitle, fontBody, w));
                g.String(listTitle, fontBody, Muted, new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 模组行
                foreach (var row in ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var leftText = row.Left ?? string.Empty;
                    var rightText = $"[{row.Right ?? string.Empty}]";

                    int gapPx = (int)MathF.Round(LeftRightGap * dpi);
                    int rightW = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Width;

                    int leftMaxW = Math.Max(1, w - rightW - gapPx);

                    var leftSize = AntdUI.Helper.Size(g.MeasureString(leftText, fontBody, leftMaxW));
                    int leftActualW = Math.Min(leftSize.Width, leftMaxW);

                    var rightSize = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody, rightW));
                    int rowH = Math.Max(leftSize.Height, rightSize.Height);

                    var leftRect = new Rectangle(x, y, leftMaxW, rowH);
                    var rightRect = new Rectangle(x + leftActualW + gapPx, y, rightW, rowH);

                    g.String(leftText, fontBody, AntdUI.Style.Db.Text, leftRect, sfLT);
                    g.String(rightText, fontBody, Muted, rightRect, sfLT);

                    y += rowH + 4;
                }

                y += SectionGap;

                // ===== 总属性值（与“综合评分”同格式，同颜色），放在“属性分布”上面 =====
                const string totalAttrName = "总属性值";
                int? totalAttrVal = null;
                int totalIndex = -1;
                if (AttrLines != null)
                {
                    for (int i = 0; i < AttrLines.Count; i++)
                    {
                        if (AttrLines[i].Name == totalAttrName)
                        {
                            totalAttrVal = AttrLines[i].Value;
                            totalIndex = i;
                            break;
                        }
                    }
                }
                if (totalAttrVal.HasValue)
                {
                    string totalLabel = "总属性值：";
                    int totalLabelW = AntdUI.Helper.Size(g.MeasureString(totalLabel, fontBody)).Width;
                    string totalTextAll = $"{totalLabel}{totalAttrVal.Value}";
                    var totalSz = AntdUI.Helper.Size(g.MeasureString(totalTextAll, fontBody, w));

                    g.String(totalLabel, fontBody, Muted, new Rectangle(x, y, w, totalSz.Height), sfLT);
                    g.String($"{totalAttrVal.Value}", fontBody, Primary,
                             new Rectangle(x + totalLabelW, y, Math.Max(1, w - totalLabelW), totalSz.Height), sfLT);

                    y += totalSz.Height + LineGap;
                }

                // 属性分布标题
                const string distTitle = "属性分布：";
                sz = AntdUI.Helper.Size(g.MeasureString(distTitle, fontBody, w));
                g.String(distTitle, fontBody, Muted, new Rectangle(x, y, w, sz.Height), sfLT);
                y += sz.Height + LineGap;

                // 属性分布内容（跳过“总属性值”）
                foreach (var kv in AttrLines ?? Enumerable.Empty<(string Name, int Value)>())
                {
                    if (kv.Name == totalAttrName) continue; // ← 跳过
                    string line = $"{kv.Name}：+{kv.Value}";
                    sz = AntdUI.Helper.Size(g.MeasureString(line, fontBody, w));
                    g.String(line, fontBody, AntdUI.Style.Db.Text, new Rectangle(x, y, w, sz.Height), sfLT);
                    y += sz.Height + 4;
                }
            }

            public override Size Size(AntdUI.Canvas g, AntdUI.VirtualPanelArgs e)
            {
                float dpi = AntdUI.Config.Dpi;
                int padL = (int)MathF.Round(ContentPadding.Left * dpi);
                int padT = (int)MathF.Round(ContentPadding.Top * dpi);
                int padR = (int)MathF.Round(ContentPadding.Right * dpi);
                int padB = (int)MathF.Round(ContentPadding.Bottom * dpi);
                int gap = (int)MathF.Round(LeftRightGap * dpi);

                var baseFont = e.Panel?.Font ?? SystemFonts.DefaultFont;
                using var fontBody = new Font(baseFont, baseFont.Style);
                using var fontTitle = new Font(baseFont, FontStyle.Bold);

                int maxRowContentW = 0;
                foreach (var row in ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var leftText = row.Left ?? string.Empty;
                    var rightText = $"[{row.Right ?? string.Empty}]";

                    int leftW = AntdUI.Helper.Size(g.MeasureString(leftText, fontBody)).Width;
                    int rightW = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Width;

                    maxRowContentW = Math.Max(maxRowContentW, leftW + gap + rightW);
                }

                int minCardW = (int)(240 * dpi);
                int maxCardW = Math.Max(1, e.Rect.Width);
                int cardW = padL + maxRowContentW + padR;
                if (cardW < minCardW) cardW = minCardW;
                if (cardW > maxCardW) cardW = maxCardW;

                ModuleCardDisplay.UniformCardWidth.Collect(cardW, e.Rect.Width);
                cardW = ModuleCardDisplay.UniformCardWidth.GetOr(cardW);

                int contentW = Math.Max(1, cardW - padL - padR);
                int y = 0;

                y += AntdUI.Helper.Size(g.MeasureString(RankText ?? string.Empty, fontTitle, contentW)).Height + LineGap;
                y += AntdUI.Helper.Size(g.MeasureString($"最高属性等级 {HighestLevel}", fontBody, contentW)).Height + LineGap;
                y += AntdUI.Helper.Size(g.MeasureString($"综合评分：{Score}", fontBody, contentW)).Height + SectionGap;

                y += AntdUI.Helper.Size(g.MeasureString("模组列表：", fontBody, contentW)).Height + LineGap;

                foreach (var row in ModuleRows ?? Enumerable.Empty<(string Left, string Right)>())
                {
                    var leftText = row.Left ?? string.Empty;
                    var rightText = $"[{row.Right ?? string.Empty}]";

                    int rightW = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Width;
                    int leftMaxW = Math.Max(1, contentW - rightW - gap);

                    int hL = AntdUI.Helper.Size(g.MeasureString(leftText, fontBody, leftMaxW)).Height;
                    int hR = AntdUI.Helper.Size(g.MeasureString(rightText, fontBody)).Height;

                    y += Math.Max(hL, hR) + 4;
                }
                y += SectionGap;

                // ===== 为“总属性值”预留高度（与“综合评分”同样的测量方式）=====
                const string totalAttrName = "总属性值";
                int? totalAttrVal = null;
                if (AttrLines != null)
                {
                    foreach (var (name, val) in AttrLines)
                    {
                        if (name == totalAttrName) { totalAttrVal = val; break; }
                    }
                }
                if (totalAttrVal.HasValue)
                {
                    string totalStr = $"总属性值：{totalAttrVal.Value}";
                    y += AntdUI.Helper.Size(g.MeasureString(totalStr, fontBody, contentW)).Height + LineGap;
                }

                // 属性分布标题与内容
                y += AntdUI.Helper.Size(g.MeasureString("属性分布：", fontBody, contentW)).Height + LineGap;

                foreach (var (name, val) in AttrLines ?? Enumerable.Empty<(string Name, int Value)>())
                {
                    if (name == totalAttrName) continue; // ← 跳过“总属性值”
                    y += AntdUI.Helper.Size(g.MeasureString($"{name}：+{val}", fontBody, contentW)).Height + 4;
                }

                int minH = (int)(MinHeightDp * dpi);
                int measuredH = Math.Max(minH, y + padT + padB);

                int finalW = ForceCardWidthPx ?? cardW;
                int finalH = ForceCardHeightPx ?? measuredH;

                return new Size(finalW, finalH);
            }
        }

        public class ResultCardData
        {
            public string RankText { get; set; } = "";
            public int HighestLevel { get; set; }
            public string Score { get; set; } = "";
            public List<(string Left, string Right)> ModuleRows { get; set; } = new();
            public List<(string Name, int Value)> AttrLines { get; set; } = new();

            // ← 新版：从 ModuleOptimizer.ModuleSolution 构造
            public static ResultCardData FromSolution(ModuleOptimizer.ModuleSolution s, int rank, ISet<string>? priorityAttrs = null)
            {
                var dto = new ResultCardData
                {
                    RankText = $"第{rank}名搭配",
                    HighestLevel = ComputeHighestLevel(s.AttrBreakdown, priorityAttrs),
                    Score = s.Score.ToString("0.0"),
                    AttrLines = s.AttrBreakdown
                                    .OrderBy(k => k.Key, StringComparer.Ordinal)
                                    .Select(kv => (kv.Key, kv.Value)).ToList()
                };

                // 新增：总属性值行（排在最上面）
                dto.AttrLines.Insert(0, ("总属性值", s.TotalAttrValue));

                for (int i = 0; i < s.Modules.Count; i++)
                {
                    var m = s.Modules[i];
                    var parts = m.Parts ?? Array.Empty<ModuleOptimizer.IModulePart>();
                    var right = string.Join(" ", parts.Select(p => $"{p.Name}+{p.Value}"));
                    dto.ModuleRows.Add((m.Name ?? $"模组{i + 1}", right));
                }
                return dto;
            }





            private static int ComputeHighestLevel(
    Dictionary<string, int> attrBreakdown,
    ISet<string>? onlyIn = null)
            {
                if (attrBreakdown == null || attrBreakdown.Count == 0) return 0;

                int highest = 0;
                var thresholds = ModuleMaps.ATTR_THRESHOLDS;

                IEnumerable<KeyValuePair<string, int>> seq = attrBreakdown;
                if (onlyIn != null && onlyIn.Count > 0)
                    seq = attrBreakdown.Where(kv => onlyIn.Contains(kv.Key));

                foreach (var (_, v) in seq)
                {
                    int level = 0;
                    for (int i = 0; i < thresholds.Length; i++)
                    {
                        if (v >= thresholds[i]) level = i + 1;
                        else break;
                    }
                    if (level > highest) highest = level;
                }
                return highest;
            }
        }

        public static class ModuleResultMemory
        {
            private static readonly List<ResultCardData> _list = new();

            public static void Clear() => _list.Clear();
            public static void Set(IEnumerable<ResultCardData> items)
            {
                _list.Clear();
                if (items != null) _list.AddRange(items);
            }
            public static void Add(ResultCardData item) => _list.Add(item);
            public static List<ResultCardData> GetSnapshot() => new List<ResultCardData>(_list);

            // 方便：一次性从优化器“解”列表写入
            public static void FromSolutions(IEnumerable<ModuleOptimizer.ModuleSolution> solutions)
            {
                var priority = new HashSet<string>(BuildEliteCandidatePool.Attributes ?? new List<string>(), StringComparer.Ordinal);
                Set(solutions?.Select((s, i) => ResultCardData.FromSolution(s, i + 1,
                            priority.Count > 0 ? priority : null))
                    ?? Enumerable.Empty<ResultCardData>());
            }


            private static int ComputeHighestLevel(
    Dictionary<string, int> attrBreakdown,
    ISet<string>? onlyIn = null)
            {
                if (attrBreakdown == null || attrBreakdown.Count == 0) return 0;

                int highest = 0;
                var thresholds = ModuleMaps.ATTR_THRESHOLDS;

                IEnumerable<KeyValuePair<string, int>> seq = attrBreakdown;
                if (onlyIn != null && onlyIn.Count > 0)
                    seq = attrBreakdown.Where(kv => onlyIn.Contains(kv.Key));

                foreach (var (_, v) in seq)
                {
                    int level = 0;
                    for (int i = 0; i < thresholds.Length; i++)
                    {
                        if (v >= thresholds[i]) level = i + 1;
                        else break;
                    }
                    if (level > highest) highest = level;
                }
                return highest;
            }

        }

        public static class UniformCardHeight
        {
            private static readonly object _lock = new();
            private static int _maxH = 0;
            private static bool _frozen = false;

            public static void Reset()
            {
                lock (_lock)
                {
                    _maxH = 0;
                    _frozen = false;
                }
            }

            public static void Collect(int h)
            {
                lock (_lock)
                {
                    if (!_frozen && h > _maxH) _maxH = h;
                }
            }

            public static void Freeze()
            {
                lock (_lock) { _frozen = true; }
            }

            public static int GetOr(int measured)
            {
                lock (_lock) { return _frozen ? _maxH : measured; }
            }

            public static bool IsFrozen { get { lock (_lock) return _frozen; } }
        }

        public static class UniformCardWidth
        {
            private static readonly object _lock = new();
            private static int _maxMeasuredW = 0;
            private static int _panelW = 0;
            private static int _gutterPx = 12;
            private static int _minCardW = 240;
            private static int _maxCols = 3;
            private static bool _frozen = false;
            private static int _frozenW = 0;

            public static void Reset(int gutterPx, int minCardW, int maxCols)
            {
                lock (_lock)
                {
                    _gutterPx = Math.Max(0, gutterPx);
                    _minCardW = Math.Max(1, minCardW);
                    _maxCols = Math.Max(1, maxCols);
                    _maxMeasuredW = 0;
                    _panelW = 0;
                    _frozen = false;
                    _frozenW = 0;
                }
            }

            public static void Collect(int preferredCardW, int panelW)
            {
                lock (_lock)
                {
                    if (preferredCardW > _maxMeasuredW) _maxMeasuredW = preferredCardW;
                    if (panelW > _panelW) _panelW = panelW;
                }
            }

            public static void Freeze()
            {
                lock (_lock)
                {
                    int W = Math.Max(1, _panelW);
                    int desired = Math.Max(_minCardW, _maxMeasuredW);

                    int cols = (desired + _gutterPx) > 0
                        ? Math.Max(1, (W + _gutterPx) / (desired + _gutterPx))
                        : 1;

                    cols = Math.Min(cols, _maxCols);
                    cols = Math.Max(1, cols);

                    int unified = (W - _gutterPx * (cols - 1)) / cols;
                    unified = Math.Min(unified, W);
                    unified = Math.Max(1, unified);

                    _frozenW = unified;
                    _frozen = true;
                }
            }

            public static int GetOr(int measured)
            {
                lock (_lock) return _frozen ? _frozenW : measured;
            }

            public static bool IsFrozen { get { lock (_lock) return _frozen; } }
        }
    }
}
