using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Data.Models
{
    public class SkillData
    {
        /// <summary>
        /// 技能ID
        /// </summary>
        public long SkillId { get; internal set; }
        /// <summary>
        /// 总数值 (DPS/HPS)
        /// </summary>
        public long TotalValue { get; internal set; }
        /// <summary>
        /// 技能使用次数
        /// </summary>
        public int UseTimes { get; internal set; }
        /// <summary>
        /// 暴击次数
        /// </summary>
        public int CritTimes { get; internal set; }
        /// <summary>
        /// 幸运一击次数
        /// </summary>
        public int LuckyTimes { get; internal set; }
        /// <summary>
        /// 最小伤害值
        /// </summary>
        public long MinDamage { get; internal set; } = long.MaxValue;
        /// <summary>
        /// 最大伤害值
        /// </summary>
        public long MaxDamage { get; internal set; } = 0;
        /// <summary>
        /// 最高暴击伤害值
        /// </summary>
        public long HighestCrit { get; internal set; } = 0;
    }
}
