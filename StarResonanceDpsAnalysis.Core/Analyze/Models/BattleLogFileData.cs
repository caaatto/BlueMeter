using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using StarResonanceDpsAnalysis.Core.Data.Models;
using StarResonanceDpsAnalysis.Core.Extends.System;

namespace StarResonanceDpsAnalysis.Core.Analyze.Models
{
    public struct BattleLogFileData()
    {
        /// <summary>
        /// 包ID
        /// </summary>
        [JsonProperty("p")]
        public long PacketID { get; internal set; }
        /// <summary>
        /// 时间戳 (Ticks)
        /// </summary>
        [JsonProperty("t")]
        public long TimeTicks { get; internal set; }
        /// <summary>
        /// 技能ID
        /// </summary>
        [JsonProperty("s")]
        public long SkillID { get; internal set; }
        /// <summary>
        /// 释放对象UUID (发出者)
        /// </summary>
        [JsonProperty("a")]
        public long AttackerUuid { get; internal set; }
        /// <summary>
        /// 目标对象UUID (目标者)
        /// </summary>
        [JsonProperty("tu")]
        public long TargetUuid { get; internal set; }
        /// <summary>
        /// 具体数值 (伤害)
        /// </summary>
        [JsonProperty("v")]
        public long Value { get; internal set; }
        /// <summary>
        /// 数值元素类型
        /// </summary>
        [JsonProperty("ve")]
        public int ValueElementType { get; internal set; }
        /// <summary>
        /// 伤害来源类型
        /// </summary>
        [JsonProperty("d")]
        public int DamageSourceType { get; internal set; }

        /// <summary>
        /// 释放对象 (发出者) 是否为玩家
        /// </summary>
        [JsonProperty("iap")]
        public bool IsAttackerPlayer { get; internal set; }
        /// <summary>
        /// 目标对象 (目标者) 是否为玩家
        /// </summary>
        [JsonProperty("itp")]
        public bool IsTargetPlayer { get; internal set; }
        /// <summary>
        /// 具体数值是否为幸运一击
        /// </summary>
        [JsonProperty("il")]
        public bool IsLucky { get; internal set; }
        /// <summary>
        /// 具体数值是否为暴击
        /// </summary>
        [JsonProperty("ic")]
        public bool IsCritical { get; internal set; }
        /// <summary>
        /// 具体数值是否为治疗
        /// </summary>
        [JsonProperty("ih")]
        public bool IsHeal { get; internal set; }
        /// <summary>
        /// 具体数值是否为闪避
        /// </summary>
        [JsonProperty("im")]
        public bool IsMiss { get; internal set; }
        /// <summary>
        /// 目标对象 (目标者) 是否阵亡
        /// </summary>
        [JsonProperty("id")]
        public bool IsDead { get; internal set; }
        [JsonProperty("h")]
        public byte[] Hash { get; internal set; } = [];

        private static byte[] CreateMD5(BattleLogFileData d) =>
            MD5.HashData($"""
                {d.PacketID}_{d.TimeTicks}_{d.SkillID}_{d.AttackerUuid}_{d.TargetUuid}_{d.Value}_{d.ValueElementType}_{d.DamageSourceType}_{d.IsAttackerPlayer}_{d.IsTargetPlayer}_{d.IsLucky}_{d.IsCritical}_{d.IsHeal}_{d.IsMiss}_{d.IsDead}
                """.GetBytes());

        public bool TestHash() => TestHash(this);
        public static bool TestHash(BattleLogFileData data) => data.Hash.SequenceEqual(CreateMD5(data));

        public static implicit operator BattleLogFileData(BattleLog b)
        {
            var tmp = new BattleLogFileData()
            {
                PacketID = b.PacketID,
                TimeTicks = b.TimeTicks,
                SkillID = b.SkillID,
                AttackerUuid = b.AttackerUuid,
                TargetUuid = b.TargetUuid,
                Value = b.Value,
                ValueElementType = b.ValueElementType,
                DamageSourceType = b.DamageSourceType,
                IsAttackerPlayer = b.IsAttackerPlayer,
                IsTargetPlayer = b.IsTargetPlayer,
                IsLucky = b.IsLucky,
                IsCritical = b.IsCritical,
                IsHeal = b.IsHeal,
                IsMiss = b.IsMiss,
                IsDead = b.IsDead,
            };
            tmp.Hash = CreateMD5(tmp);
            return tmp;
        }
    }
}
