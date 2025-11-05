using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Extends.System
{
    public static class UInt64Extends
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ShiftRight16(this ulong value)
        {
            return value >> 16;
        }
    }
}
