using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Extends.System
{
    public static class Func
    {
        /// <summary>
        /// 在函数集合中, 返回第一个返回了符合条件的返回值
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="getters">依次执行的函数</param>
        /// <param name="comparator">是否符合条件的判断函数</param>
        /// <param name="def">所有都不符合的默认值</param>
        /// <returns>函数的返回值</returns>
        /// <remarks>
        /// 函数会依次在 <paramref name="getters"/> 数组中选择函数执行后取得返回值, 
        /// 并交给 <paramref name="comparator"/> 判断这个返回值是否符合条件,
        /// 如果 <paramref name="comparator"/> 判定为 <c>True</c> 则返回这个返回值,
        /// 否则依次向后执行, 直到所有函数都被执行, 依然没有符合条件的返回值, 则返回 <paramref name="def"/>
        /// </remarks>
        public static T? GetFirstValidResult<T>(this ICollection<Func<T>> getters, Func<T, bool> comparator, T? def = default)
        {
            foreach (var getter in getters)
            {
                var value = getter();
                if (comparator(value)) return value;
            }

            return def;
        }

        /// <summary>
        /// 在函数集合中, 选择头一个符合条件的返回值返回
        /// </summary>
        /// <typeparam name="T">函数 1参 类型</typeparam>
        /// <typeparam name="T2">返回值类型</typeparam>
        /// <param name="getters">依次执行的函数</param>
        /// <param name="param">执行的函数所需的参数</param>
        /// <param name="comparator">是否符合条件的判断函数</param>
        /// <param name="def">所有都不符合的默认值</param>
        /// <returns>函数的返回值</returns>
        /// <remarks>
        /// 函数会依次在 <paramref name="getters"/> 数组中选择函数执行后取得返回值, 
        /// 并交给 <paramref name="comparator"/> 判断这个返回值是否符合条件,
        /// 如果 <paramref name="comparator"/> 判定为 <c>True</c> 则返回这个返回值,
        /// 否则依次向后执行, 直到所有函数都被执行, 依然没有符合条件的返回值, 则返回 <paramref name="def"/>
        /// </remarks>
        public static T2? GetFirstValidResult<T, T2>(this ICollection<Func<T, T2>> getters, T param, Func<T2, bool> comparator, T2? def = default)
        {
            foreach (var getter in getters)
            {
                var value = getter(param);
                if (comparator(value)) return value;
            }

            return def;
        }
    }
}
