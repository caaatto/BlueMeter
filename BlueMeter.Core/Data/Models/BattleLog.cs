using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Data.Models
{
    /// <summary>
    /// 战斗日志
    /// </summary>
    /// <remarks>
    /// 现阶段的字段顺序是经过设计的, 请勿随意更改
    /// 本段中, 后续的多个 bool 类型, 可以考虑合并为一个 Flag 以节省空间并提高效率, 但会降低可读性
    /// </remarks>
    public struct BattleLog
    {
        /// <summary>
        /// 包ID
        /// </summary>
        public long PacketID { get; internal set; }
        /// <summary>
        /// 时间戳 (Ticks)
        /// </summary>
        public long TimeTicks { get; internal set; }
        /// <summary>
        /// 技能ID
        /// </summary>
        public long SkillID { get; internal set; }
        /// <summary>
        /// 释放对象UUID (发出者)
        /// </summary>
        public long AttackerUuid { get; internal set; }
        /// <summary>
        /// 目标对象UUID (目标者)
        /// </summary>
        public long TargetUuid { get; internal set; }
        /// <summary>
        /// 具体数值 (伤害)
        /// </summary>
        public long Value { get; internal set; }
        /// <summary>
        /// HP damage taken (actual HP loss)
        /// </summary>
        public long HpLessenValue { get; internal set; }
        /// <summary>
        /// Shield damage absorbed (mitigation)
        /// </summary>
        public long ShieldLessenValue { get; internal set; }
        /// <summary>
        /// 数值元素类型
        /// </summary>
        public int ValueElementType { get; internal set; }
        /// <summary>
        /// 伤害来源类型
        /// </summary>
        public int DamageSourceType { get; internal set; }

        /// <summary>
        /// 释放对象 (发出者) 是否为玩家
        /// </summary>
        public bool IsAttackerPlayer { get; internal set; }
        /// <summary>
        /// 目标对象 (目标者) 是否为玩家
        /// </summary>
        public bool IsTargetPlayer { get; internal set; }
        /// <summary>
        /// 具体数值是否为幸运一击
        /// </summary>
        public bool IsLucky { get; internal set; }
        /// <summary>
        /// 具体数值是否为暴击
        /// </summary>
        public bool IsCritical { get; internal set; }
        /// <summary>
        /// 具体数值是否为治疗
        /// </summary>
        public bool IsHeal { get; internal set; }
        /// <summary>
        /// 具体数值是否为闪避
        /// </summary>
        public bool IsMiss { get; internal set; }
        /// <summary>
        /// 目标对象 (目标者) 是否阵亡
        /// </summary>
        public bool IsDead { get; internal set; }
    }
}
