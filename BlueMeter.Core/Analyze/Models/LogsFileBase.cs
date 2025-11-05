using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Analyze.Models
{
    public class LogsFileBase
    {
        public required LogsFileVersion FileVersion { get; set; }
    }
}
