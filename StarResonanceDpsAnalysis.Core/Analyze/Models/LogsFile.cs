using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Analyze.Models
{
    public class LogsFileV3_0_0 : LogsFileBase
    {
        public PlayerInfoFileData[] PlayerInfos { get; set; } = [];
        public BattleLogFileData[] BattleLogs { get; set; } = [];
    }
}
