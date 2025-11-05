using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.WPF.Plugins
{
    public class PluginState(bool isAutoStart = false, bool isRunning = false)
    {
        public bool IsAutoStart { get; internal set; } = isAutoStart;
        public bool InRunning { get; internal set; } = isRunning;
    }
}
