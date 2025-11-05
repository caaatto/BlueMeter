using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlueMeter.WinForm.Effects.Enum;

namespace BlueMeter.WinForm.Effects
{
    /// <summary>
    /// Cubic Bezier 曲线计算类
    /// </summary>
    /// <remarks>
    /// 用作绘制渲染时, 请一定注意复用实例, 否则会导致性能问题。
    /// </remarks>
    public class CubicBezier
    {
        private Dictionary<Quality, int> QualityConfig { get; } = new()
        {
            { Quality.VeryLow, 5 },
            { Quality.Low, 7 },
            { Quality.Medium, 13 },
            { Quality.High, 25 },
            { Quality.VeryHigh, 49 },
            { Quality.Extreme, 499 },
            { Quality.AlmostAccurate, 2499 },
        };

        public Quality Quality { get; init; }

        private PointF[] PreCalcedPoints { get; init; }

        public CubicBezier(float p1x, float p1y, float p2x, float p2y, Quality quality)
            : this(new PointF(p1x, p1y), new PointF(p2x, p2y), quality)
        { }

        public CubicBezier(PointF p1, PointF p2, Quality quality) : this(new PointF(0, 0), p1, p2, new PointF(1, 1), quality)
        { }

        public CubicBezier(float sx, float sy, float p1x, float p1y, float p2x, float p2y, float ex, float ey, Quality quality)
            : this(new PointF(sx, sy), new PointF(p1x, p1y), new PointF(p2x, p2y), new PointF(ex, ey), quality)
        { }

        public CubicBezier(PointF s, PointF p1, PointF p2, PointF e, Quality quality = Quality.Low)
        {
            Quality = quality;

            var flag = QualityConfig.TryGetValue(quality, out var points);
            if (!flag)
            {
                throw new ArgumentException($"Unsupported CalcQuality: {quality}", nameof(quality));
            }

            PreCalcedPoints = new PointF[points + 2];

            InitPoints(s, p1, p2, e, points);
        }

        private void InitPoints(PointF s, PointF p1, PointF p2, PointF e, int points)
        {
            PreCalcedPoints[0] = s;
            PreCalcedPoints[^1] = e;

            var steps = points + 1;
            for (var i = 1; i < steps; ++i)
            {
                var p = GetBezierPointF(s, p1, p2, e, (float)i / steps);
                PreCalcedPoints[i] = p;
            }
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        private static PointF LerpPointF(PointF a, PointF b, float t) => new(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
        private static PointF GetBezierPointF(PointF s, PointF p1, PointF p2, PointF e, float t)
        {
            var lp11 = LerpPointF(s, p1, t);
            var lp12 = LerpPointF(p1, p2, t);
            var lp13 = LerpPointF(p2, e, t);

            var lp21 = LerpPointF(lp11, lp12, t);
            var lp22 = LerpPointF(lp12, lp13, t);

            var result = LerpPointF(lp21, lp22, t);

            return result;
        }


        public float GetProximateBezierValue(float persent)
        {
            if (persent <= PreCalcedPoints[0].X) return PreCalcedPoints[0].X;
            if (persent >= PreCalcedPoints[^1].X) return PreCalcedPoints[^1].X;

            // 二分查找
            var left = 0;
            var right = PreCalcedPoints.Length - 1;
            while (left <= right)
            {
                int mid = (left + right) / 2;

                if (PreCalcedPoints[mid].X == persent) return PreCalcedPoints[mid].X;

                if (PreCalcedPoints[mid].X < persent)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            // left 是第一个大于 persent 的位置
            // right 是最后一个小于 persent 的位置
            var higher = PreCalcedPoints[Math.Min(left, PreCalcedPoints.Length - 1)];
            var lower = PreCalcedPoints[Math.Max(right, 0)];

            // 线性插值
            return Lerp(lower.Y, higher.Y, (persent - lower.X) / (higher.X - lower.X));
        }
    }
}
