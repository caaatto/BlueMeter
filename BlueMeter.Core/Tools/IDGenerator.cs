using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Tools
{
    public static class IDGenerator
    {
        public static long StartTicks { get; } = DateTime.UtcNow.Ticks;

        private static long PrevTicks { get; set; } = 0;
        private static int Increment { get; set; } = 0;
        private static Stopwatch Timer { get; } = Stopwatch.StartNew();

        public static (long id, long timeTicks) Next() 
        {
            var ticks = Timer.ElapsedTicks;
            try
            {
                Increment = ticks == PrevTicks ? Increment + 1 : 0;

                return (ticks * 100 + Increment, StartTicks + ticks);
            }
            finally
            {
                PrevTicks = ticks;
            }
        } 
    }
}
