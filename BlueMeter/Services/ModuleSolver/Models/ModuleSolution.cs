namespace BlueMeter.Services.ModuleSolver.Models;

/// <summary>
/// Represents a solution (combination) of modules with a score
/// </summary>
public class ModuleSolution
{
    /// <summary>
    /// List of modules in this solution
    /// </summary>
    public List<ModuleInfo> Modules { get; set; } = new();

    /// <summary>
    /// Overall score for this combination
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Breakdown of attribute totals (attribute name -> total value)
    /// </summary>
    public Dictionary<string, int> AttributeBreakdown { get; set; } = new();

    /// <summary>
    /// Calculate attribute breakdown from modules
    /// </summary>
    public void CalculateAttributeBreakdown()
    {
        AttributeBreakdown.Clear();

        foreach (var module in Modules)
        {
            foreach (var part in module.Parts)
            {
                if (!AttributeBreakdown.ContainsKey(part.Name))
                {
                    AttributeBreakdown[part.Name] = 0;
                }
                AttributeBreakdown[part.Name] += part.Value;
            }
        }
    }

    /// <summary>
    /// Get total combat power (sum of all attribute values)
    /// </summary>
    public int GetTotalCombatPower()
    {
        return AttributeBreakdown.Values.Sum();
    }

    public override string ToString()
    {
        return $"Score: {Score:F2}, Modules: {Modules.Count}, Total CP: {GetTotalCombatPower()}";
    }
}
