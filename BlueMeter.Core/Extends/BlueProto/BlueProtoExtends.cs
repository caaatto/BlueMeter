using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Extends.BlueProto
{
    public static class BlueProtoExtends
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUuidPlayerRaw(this long uuidRaw)
        {
            // UUID低16位标识玩家
            return (uuidRaw & 0xFFFFL) == 640L;
        }
    }
}
