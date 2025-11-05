using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Extends.System
{
    public static class NumberExtends
    {
        private static string[] CompactSuffixes { get; } = [string.Empty, "K", "M", "B", "T", "Q"];
        private static string[] ChineseUnits { get; } = [string.Empty, "万", "亿", "兆", "吉"];

        /// <summary>
        /// 转换为英文简写 (K / M / B / T ...)
        /// </summary>
        /// <param name="number">原始数值</param>
        /// <param name="digits">保留小数位数</param>
        /// <returns>如 1200 -> "1.2K"</returns>
        public static string ToCompactString<T>(this T number, int digits = 2) where T : INumber<T>
        {
            var value = double.CreateChecked(number);

            if (value < 1000) return value.ToString($"F{digits}");

            var index = 0;
            while (value >= 1000 && index < CompactSuffixes.Length - 1)
            {
                value /= 1000d;
                index++;
            }

            return $"{value.ToString($"F{digits}")}{CompactSuffixes[index]}";
        }

        /// <summary>
        /// 转换为中文数字单位 (万 / 亿 / 万亿 / 千万亿 ...)
        /// </summary>
        /// <param name="number">原始数值</param>
        /// <param name="digits">保留小数位数</param>
        /// <returns>如 120000 -> "12万"</returns>
        public static string ToChineseUnitString<T>(this T number, int digits = 2) where T : INumber<T>
        {
            var value = double.CreateChecked(number);

            if (value < 10000) return value.ToString($"F{digits}");

            var index = 0;
            while (value >= 10000 && index < ChineseUnits.Length - 1)
            {
                value /= 10000d;
                index++;
            }

            return value.ToString($"F{digits}") + ChineseUnits[index];
        }
    }
}
