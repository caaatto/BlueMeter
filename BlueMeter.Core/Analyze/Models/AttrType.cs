using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Analyze.Models
{
    /// <summary>
    /// 玩家属性类型定义
    /// 用于 SyncNearEntities 消息中的属性ID解析
    /// </summary>
    public enum AttrType
    {
        /// <summary>名称（玩家/单位名，字符串）</summary>
        AttrName = 0x01,                    // name
        AttrId = 0x0A, // 整数（实体/怪物 ID，可映射名字）
        /// <summary>职业 ID（职业枚举/配置映射用）</summary>
        AttrProfessionId = 0xDC,            // profession id

        /// <summary>战力（Fight Point/Combat Power，整数）</summary>
        AttrFightPoint = 0x272E,            // fight power

        /// <summary>等级（Level）</summary>
        AttrLevel = 0x2710,                 // level

        /// <summary>阶位/段位（Rank Level，具体含义按游戏定义）</summary>
        AttrRankLevel = 0x274C,             // rank level

        /// <summary>暴击率（单位由上游决定，常见万分比或千分比）</summary>
        AttrCri = 0x2B66,                   // crit rate

        /// <summary>幸运率（单位由上游决定，常见万分比或千分比）</summary>
        AttrLucky = 0x2B7A,                 // lucky rate

        /// <summary>当前生命值（HP）</summary>
        AttrHp = 0x2C2E,                    // hp

        /// <summary>最大生命值（Max HP）</summary>
        AttrMaxHp = 0x2C38,                 // max hp

        /// <summary>
        /// 元素标识位（元素相关的位标志/掩码，如冰/雷/火等；具体 bit 含义按配置表解析）
        /// </summary>
        AttrElementFlag = 0x646D6C,         // element flags (bitmask)

        /// <summary>
        /// 减抗/易伤等级（Reduction Level，表示受到某类减抗效果的等级）
        /// </summary>
        AttrReductionLevel = 0x64696D,      // reduction/vulnerability level

        /// <summary>
        /// 减抗/易伤效果 ID（用于区分来源或具体效果条目）
        /// </summary>
        AttrReduntionId = 0x6F6C65,         // reduction effect id

        /// <summary>
        /// 能量标识位（Energy Flag/Charge 状态，通常为位标志；具体定义以协议/配置为准）
        /// </summary>
        AttrEnergyFlag = 0x543CD3C6         // energy flags (bitmask)
    }
}
