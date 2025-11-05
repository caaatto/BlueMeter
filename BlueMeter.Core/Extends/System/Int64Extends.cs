using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Extends.System
{
    public static class Int64Extends
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ShiftRight16(this long value)
        {
            return value >> 16;
        }
    }
}
