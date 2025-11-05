using System;
using System.Collections.Generic;

namespace BlueMeter.WinForm.Core.Module
{
    public enum ModuleType : int
    {
        BASIC_ATTACK = 5500101,
        HIGH_PERFORMANCE_ATTACK = 5500102,
        EXCELLENT_ATTACK = 5500103,
        BASIC_HEALING = 5500201,
        HIGH_PERFORMANCE_HEALING = 5500202,
        EXCELLENT_HEALING = 5500203,
        BASIC_PROTECTION = 5500301,
        HIGH_PERFORMANCE_PROTECTION = 5500302,
        EXCELLENT_PROTECTION = 5500303,
    }

    public enum ModuleAttrType : int
    {
        // 基础/通用属性
        STRENGTH_BOOST = 1110,
        AGILITY_BOOST = 1111,
        INTELLIGENCE_BOOST = 1112,
        SPECIAL_ATTACK_DAMAGE = 1113,
        ELITE_STRIKE = 1114,
        SPECIAL_HEALING_BOOST = 1205,
        EXPERT_HEALING_BOOST = 1206,
        MAGIC_RESISTANCE = 1307,
        PHYSICAL_RESISTANCE = 1308,
        CASTING_FOCUS = 1407,
        ATTACK_SPEED_FOCUS = 1408,
        CRITICAL_FOCUS = 1409,
        LUCK_FOCUS = 1410,

        // 特殊（EXTREME）属性
        EXTREME_DAMAGE_STACK = 2104,
        EXTREME_FLEXIBLE_MOVEMENT = 2105,
        EXTREME_LIFE_CONVERGENCE = 2204,
        EXTREME_EMERGENCY_MEASURES = 2205,
        EXTREME_DESPERATE_GUARDIAN = 2304,
        EXTREME_LIFE_FLUCTUATION = 2404,
        EXTREME_LIFE_DRAIN = 2405,
        EXTREME_TEAM_CRIT = 2406,
    }

    public enum ModuleCategory
    {
        ATTACK,
        GUARDIAN,
        SUPPORT,
        ALL
    }

    /// <summary>模组信息（与 Python 对齐：Uuid 为 string）</summary>
    public class ModuleInfo
    {
        public string? Name { get; set; }
        public int ConfigId { get; set; }
        public long Uuid { get; set; }   // Python 是字符串，这里也保持一致
        public int Quality { get; set; }
        public List<ModulePart> Parts { get; set; } = new();
    }

    public class ModulePart
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    public static class ModuleMaps
    {
        // 模组名称映射（与 Python MODULE_NAMES 一致）
        public static readonly Dictionary<int, string> MODULE_NAMES = new()
        {
            { (int)ModuleType.BASIC_ATTACK, "基础攻击" },
            { (int)ModuleType.HIGH_PERFORMANCE_ATTACK, "高性能攻击" },
            { (int)ModuleType.EXCELLENT_ATTACK, "卓越攻击" },
            { (int)ModuleType.BASIC_HEALING, "基础治疗" },
            { (int)ModuleType.HIGH_PERFORMANCE_HEALING, "高性能治疗" },
            { (int)ModuleType.EXCELLENT_HEALING, "卓越辅助" }, // 注意：Python里是“卓越辅助”
            { (int)ModuleType.BASIC_PROTECTION, "基础防护" },
            { (int)ModuleType.HIGH_PERFORMANCE_PROTECTION, "高性能守护" },
            { (int)ModuleType.EXCELLENT_PROTECTION, "卓越守护" },
        };

        // 模组属性名称映射（与 Python MODULE_ATTR_NAMES 一致）
        public static readonly Dictionary<int, string> MODULE_ATTR_NAMES = new()
        {
            { (int)ModuleAttrType.STRENGTH_BOOST, "力量加持" },
            { (int)ModuleAttrType.AGILITY_BOOST, "敏捷加持" },
            { (int)ModuleAttrType.INTELLIGENCE_BOOST, "智力加持" },
            { (int)ModuleAttrType.SPECIAL_ATTACK_DAMAGE, "特攻伤害" },
            { (int)ModuleAttrType.ELITE_STRIKE, "精英打击" },
            { (int)ModuleAttrType.SPECIAL_HEALING_BOOST, "特攻治疗加持" },
            { (int)ModuleAttrType.EXPERT_HEALING_BOOST, "专精治疗加持" },
            { (int)ModuleAttrType.CASTING_FOCUS, "施法专注" },
            { (int)ModuleAttrType.ATTACK_SPEED_FOCUS, "攻速专注" },
            { (int)ModuleAttrType.CRITICAL_FOCUS, "暴击专注" },
            { (int)ModuleAttrType.LUCK_FOCUS, "幸运专注" },
            { (int)ModuleAttrType.MAGIC_RESISTANCE, "抵御魔法" },
            { (int)ModuleAttrType.PHYSICAL_RESISTANCE, "抵御物理" },

            // EXTREME（特殊）属性中文名
            { (int)ModuleAttrType.EXTREME_DAMAGE_STACK, "极-伤害叠加" },
            { (int)ModuleAttrType.EXTREME_FLEXIBLE_MOVEMENT, "极-灵活身法" },
            { (int)ModuleAttrType.EXTREME_LIFE_CONVERGENCE, "极-生命凝聚" },
            { (int)ModuleAttrType.EXTREME_EMERGENCY_MEASURES, "极-急救措施" },
            { (int)ModuleAttrType.EXTREME_LIFE_FLUCTUATION, "极-生命波动" },
            { (int)ModuleAttrType.EXTREME_LIFE_DRAIN, "极-生命汲取" },
            { (int)ModuleAttrType.EXTREME_TEAM_CRIT, "极-全队幸暴" },
            { (int)ModuleAttrType.EXTREME_DESPERATE_GUARDIAN, "极-绝境守护" },
        };

        // 模组类型到分类的映射（与 Python MODULE_CATEGORY_MAP 一致）
        public static readonly Dictionary<int, ModuleCategory> MODULE_CATEGORY_MAP = new()
        {
            { (int)ModuleType.BASIC_ATTACK, ModuleCategory.ATTACK },
            { (int)ModuleType.HIGH_PERFORMANCE_ATTACK, ModuleCategory.ATTACK },
            { (int)ModuleType.EXCELLENT_ATTACK, ModuleCategory.ATTACK },
            { (int)ModuleType.BASIC_PROTECTION, ModuleCategory.GUARDIAN },
            { (int)ModuleType.HIGH_PERFORMANCE_PROTECTION, ModuleCategory.GUARDIAN },
            { (int)ModuleType.EXCELLENT_PROTECTION, ModuleCategory.GUARDIAN },
            { (int)ModuleType.BASIC_HEALING, ModuleCategory.SUPPORT },
            { (int)ModuleType.HIGH_PERFORMANCE_HEALING, ModuleCategory.SUPPORT },
            { (int)ModuleType.EXCELLENT_HEALING, ModuleCategory.SUPPORT },
        };

        // 分类中文名（补齐 ALL）
        public static readonly Dictionary<ModuleCategory, string> MODULE_CATEGORY_NAMES = new()
        {
            { ModuleCategory.ATTACK, "攻击" },
            { ModuleCategory.GUARDIAN, "守护" },
            { ModuleCategory.SUPPORT, "辅助" },
            { ModuleCategory.ALL, "全部" },
        };

        // 属性阈值和效果等级（与 Python ATTR_THRESHOLDS 一致）
        public static readonly int[] ATTR_THRESHOLDS = new[] { 1, 4, 8, 12, 16, 20 };

        // 基础词条战力映射（与 Python BASIC_ATTR_POWER_MAP 一致）
        public static readonly Dictionary<int, int> BASIC_ATTR_POWER_MAP = new()
        {
            {1, 7}, {2, 14}, {3, 29}, {4, 44}, {5, 167}, {6, 254},
        };

        // 特殊词条战力映射（与 Python SPECIAL_ATTR_POWER_MAP 一致）
        public static readonly Dictionary<int, int> SPECIAL_ATTR_POWER_MAP = new()
        {
            {1, 14}, {2, 29}, {3, 59}, {4, 89}, {5, 298}, {6, 448},
        };

        // 模组总属性值战力映射（与 Python TOTAL_ATTR_POWER_MAP 一致）
        public static readonly Dictionary<int, int> TOTAL_ATTR_POWER_MAP = new()
        {
            {0,0},{1,5},{2,11},{3,17},{4,23},{5,29},{6,34},{7,40},{8,46},
            {18,104},{19,110},{20,116},{21,122},{22,128},{23,133},{24,139},{25,145},
            {26,151},{27,157},{28,163},{29,168},{30,174},{31,180},{32,186},{33,192},
            {34,198},{35,203},{36,209},{37,215},{38,221},{39,227},{40,233},{41,238},
            {42,244},{43,250},{44,256},{45,262},{46,267},{47,273},{48,279},{49,285},
            {50,291},{51,297},{52,302},{53,308},{54,314},{55,320},{56,326},{57,332},
            {58,337},{59,343},{60,349},{61,355},{62,361},{63,366},{64,372},{65,378},
            {66,384},{67,390},{68,396},{69,401},{70,407},{71,413},{72,419},{73,425},
            {74,431},{75,436},{76,442},{77,448},{78,454},{79,460},{80,466},{81,471},
            {82,477},{83,483},{84,489},{85,495},{86,500},{87,506},{88,512},{89,518},
            {90,524},{91,530},{92,535},{93,541},{94,547},{95,553},{96,559},{97,565},
            {98,570},{99,576},{100,582},{101,588},{102,594},{103,599},{104,605},{105,611},
            {106,617},{113,658},{114,664},{115,669},{116,675},{117,681},{118,687},{119,693},{120,699},
        };

        // 基础词条ID集合（与 Python BASIC_ATTR_IDS 一致）
        public static readonly HashSet<int> BASIC_ATTR_IDS = new()
        {
            (int)ModuleAttrType.STRENGTH_BOOST,
            (int)ModuleAttrType.AGILITY_BOOST,
            (int)ModuleAttrType.INTELLIGENCE_BOOST,
            (int)ModuleAttrType.SPECIAL_ATTACK_DAMAGE,
            (int)ModuleAttrType.ELITE_STRIKE,
            (int)ModuleAttrType.SPECIAL_HEALING_BOOST,
            (int)ModuleAttrType.EXPERT_HEALING_BOOST,
            (int)ModuleAttrType.CASTING_FOCUS,
            (int)ModuleAttrType.ATTACK_SPEED_FOCUS,
            (int)ModuleAttrType.CRITICAL_FOCUS,
            (int)ModuleAttrType.LUCK_FOCUS,
            (int)ModuleAttrType.MAGIC_RESISTANCE,
            (int)ModuleAttrType.PHYSICAL_RESISTANCE,
        };

        // 特殊词条ID集合（与 Python SPECIAL_ATTR_IDS 一致）
        public static readonly HashSet<int> SPECIAL_ATTR_IDS = new()
        {
            (int)ModuleAttrType.EXTREME_DAMAGE_STACK,
            (int)ModuleAttrType.EXTREME_FLEXIBLE_MOVEMENT,
            (int)ModuleAttrType.EXTREME_LIFE_CONVERGENCE,
            (int)ModuleAttrType.EXTREME_EMERGENCY_MEASURES,
            (int)ModuleAttrType.EXTREME_LIFE_FLUCTUATION,
            (int)ModuleAttrType.EXTREME_LIFE_DRAIN,
            (int)ModuleAttrType.EXTREME_TEAM_CRIT,
            (int)ModuleAttrType.EXTREME_DESPERATE_GUARDIAN,
        };

        // 属性名称到类型的映射（与 Python ATTR_NAME_TYPE_MAP 一致）
        // "basic" / "special"
        public static readonly Dictionary<string, string> ATTR_NAME_TYPE_MAP = new()
        {
            { "力量加持", "basic" },
            { "敏捷加持", "basic" },
            { "智力加持", "basic" },
            { "特攻伤害", "basic" },
            { "精英打击", "basic" },
            { "特攻治疗加持", "basic" },
            { "专精治疗加持", "basic" },
            { "施法专注", "basic" },
            { "攻速专注", "basic" },
            { "暴击专注", "basic" },
            { "幸运专注", "basic" },
            { "抵御魔法", "basic" },
            { "抵御物理", "basic" },
            { "极-伤害叠加", "special" },
            { "极-灵活身法", "special" },
            { "极-生命凝聚", "special" },
            { "极-急救措施", "special" },
            { "极-生命波动", "special" },
            { "极-生命汲取", "special" },
            { "极-全队幸暴", "special" },
            { "极-绝境守护", "special" },
        };
    }
}
