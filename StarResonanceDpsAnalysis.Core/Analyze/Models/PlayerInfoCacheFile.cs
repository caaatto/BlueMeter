using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Analyze.Models
{
    public class PlayerInfoCacheFileV3_0_0 : PlayerInfoCacheFileBase
    {
        public PlayerInfoFileData[] PlayerInfos { get; set; } = [];
    }
}
