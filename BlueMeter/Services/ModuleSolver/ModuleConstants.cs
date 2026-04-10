using BlueMeter.Services.ModuleSolver.Models;

namespace BlueMeter.Services.ModuleSolver;

/// <summary>
/// Constants and mappings for module types and attributes
/// </summary>
public static class ModuleConstants
{
    // Module Type IDs
    public const int BASIC_ATTACK = 5500101;
    public const int HIGH_PERFORMANCE_ATTACK = 5500102;
    public const int EXCELLENT_ATTACK = 5500103;
    public const int EXCELLENT_ATTACK_PREFERRED = 5500104;
    public const int BASIC_HEALING = 5500201;
    public const int HIGH_PERFORMANCE_HEALING = 5500202;
    public const int EXCELLENT_HEALING = 5500203;
    public const int EXCELLENT_HEALING_PREFERRED = 5500204;
    public const int BASIC_PROTECTION = 5500301;
    public const int HIGH_PERFORMANCE_PROTECTION = 5500302;
    public const int EXCELLENT_PROTECTION = 5500303;
    public const int EXCELLENT_PROTECTION_PREFERRED = 5500304;

    // Attribute Type IDs
    public const int STRENGTH_BOOST = 1110;
    public const int AGILITY_BOOST = 1111;
    public const int INTELLIGENCE_BOOST = 1112;
    public const int SPECIAL_ATTACK_DAMAGE = 1113;
    public const int ELITE_STRIKE = 1114;
    public const int SPECIAL_HEALING_BOOST = 1205;
    public const int EXPERT_HEALING_BOOST = 1206;
    public const int CASTING_FOCUS = 1407;
    public const int ATTACK_SPEED_FOCUS = 1408;
    public const int CRITICAL_FOCUS = 1409;
    public const int LUCK_FOCUS = 1410;
    public const int MAGIC_RESISTANCE = 1307;
    public const int PHYSICAL_RESISTANCE = 1308;
    public const int EXTREME_DAMAGE_STACK = 2104;
    public const int EXTREME_FLEXIBLE_MOVEMENT = 2105;
    public const int EXTREME_LIFE_CONVERGENCE = 2204;
    public const int EXTREME_EMERGENCY_MEASURES = 2205;
    public const int EXTREME_LIFE_FLUCTUATION = 2404;
    public const int EXTREME_LIFE_DRAIN = 2405;
    public const int EXTREME_TEAM_CRIT = 2406;
    public const int EXTREME_DESPERATE_GUARDIAN = 2304;

    /// <summary>
    /// Module names (English)
    /// </summary>
    public static readonly Dictionary<int, string> ModuleNames = new()
    {
        { BASIC_ATTACK, "Basic Attack Module" },
        { HIGH_PERFORMANCE_ATTACK, "Advanced Attack Module" },
        { EXCELLENT_ATTACK, "Excellent Attack Module" },
        { EXCELLENT_ATTACK_PREFERRED, "Excellent Attack Module (Preferred)" },
        { BASIC_HEALING, "Basic Support Module" },
        { HIGH_PERFORMANCE_HEALING, "Advanced Support Module" },
        { EXCELLENT_HEALING, "Excellent Support Module" },
        { EXCELLENT_HEALING_PREFERRED, "Excellent Support Module (Preferred)" },
        { BASIC_PROTECTION, "Basic Guard Module" },
        { HIGH_PERFORMANCE_PROTECTION, "Advanced Guard Module" },
        { EXCELLENT_PROTECTION, "Excellent Guard Module" },
        { EXCELLENT_PROTECTION_PREFERRED, "Excellent Guard Module (Preferred)" }
    };

    /// <summary>
    /// Attribute names (English)
    /// </summary>
    public static readonly Dictionary<int, string> AttributeNames = new()
    {
        { STRENGTH_BOOST, "Strength Boost" },
        { AGILITY_BOOST, "Agility Boost" },
        { INTELLIGENCE_BOOST, "Intellect Boost" },
        { SPECIAL_ATTACK_DAMAGE, "Special Attack" },
        { ELITE_STRIKE, "Elite Strike" },
        { SPECIAL_HEALING_BOOST, "Healing Boost" },
        { EXPERT_HEALING_BOOST, "Healing Enhance" },
        { CASTING_FOCUS, "Cast Focus" },
        { ATTACK_SPEED_FOCUS, "Attack SPD" },
        { CRITICAL_FOCUS, "Crit Focus" },
        { LUCK_FOCUS, "Luck Focus" },
        { MAGIC_RESISTANCE, "Resistance" },
        { PHYSICAL_RESISTANCE, "Armor" },
        { EXTREME_DAMAGE_STACK, "DMG Stack" },
        { EXTREME_FLEXIBLE_MOVEMENT, "Agile" },
        { EXTREME_LIFE_CONVERGENCE, "Life Condense" },
        { EXTREME_EMERGENCY_MEASURES, "First Aid" },
        { EXTREME_LIFE_FLUCTUATION, "Life Wave" },
        { EXTREME_LIFE_DRAIN, "Life Steal" },
        { EXTREME_TEAM_CRIT, "Team Luck&Crit" },
        { EXTREME_DESPERATE_GUARDIAN, "Final Protection" }
    };

    /// <summary>
    /// Module type to category mapping
    /// </summary>
    public static readonly Dictionary<int, ModuleCategory> ModuleCategoryMap = new()
    {
        { BASIC_ATTACK, ModuleCategory.Attack },
        { HIGH_PERFORMANCE_ATTACK, ModuleCategory.Attack },
        { EXCELLENT_ATTACK, ModuleCategory.Attack },
        { EXCELLENT_ATTACK_PREFERRED, ModuleCategory.Attack },
        { BASIC_PROTECTION, ModuleCategory.Defense },
        { HIGH_PERFORMANCE_PROTECTION, ModuleCategory.Defense },
        { EXCELLENT_PROTECTION, ModuleCategory.Defense },
        { EXCELLENT_PROTECTION_PREFERRED, ModuleCategory.Defense },
        { BASIC_HEALING, ModuleCategory.Support },
        { HIGH_PERFORMANCE_HEALING, ModuleCategory.Support },
        { EXCELLENT_HEALING, ModuleCategory.Support },
        { EXCELLENT_HEALING_PREFERRED, ModuleCategory.Support }
    };

    /// <summary>
    /// Get module name by config ID
    /// </summary>
    public static string GetModuleName(int configId)
    {
        return ModuleNames.TryGetValue(configId, out var name) ? name : $"Unknown Module ({configId})";
    }

    /// <summary>
    /// Get attribute name by attribute ID
    /// </summary>
    public static string GetAttributeName(int attrId)
    {
        return AttributeNames.TryGetValue(attrId, out var name) ? name : $"Unknown Attr ({attrId})";
    }

    /// <summary>
    /// Get module category by config ID
    /// </summary>
    public static ModuleCategory GetModuleCategory(int configId)
    {
        return ModuleCategoryMap.TryGetValue(configId, out var category) ? category : ModuleCategory.All;
    }

    /// <summary>
    /// Get all unique attribute names for UI dropdowns
    /// </summary>
    public static List<string> GetAllAttributeNames()
    {
        return new List<string>(AttributeNames.Values);
    }
}
