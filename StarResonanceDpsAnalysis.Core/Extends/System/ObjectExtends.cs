using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Extends.System
{
    public static class ObjectExtends
    {
        #region ToInt()

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIntEx(this object? obj)
        {
            return Convert.ToInt32(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this object? obj, int def = 0)
        {
            if (TryToInt(obj, out int result))
            {
                return result;
            }

            return def;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToInt(this object? obj, out int result)
        {
            try
            {
                result = ToIntEx(obj);

                return true;
            }
            catch
            {
                result = 0;

                return false;
            }
        }

        #endregion
    }
}
