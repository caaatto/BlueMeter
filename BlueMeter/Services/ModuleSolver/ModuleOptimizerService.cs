using BlueMeter.Services.ModuleSolver.Models;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services.ModuleSolver;

/// <summary>
/// Service for optimizing module combinations
/// </summary>
public class ModuleOptimizerService
{
    private readonly ILogger<ModuleOptimizerService> _logger;
    private const int MAX_MODULES_PER_SOLUTION = 6; // Maximum modules that can be equipped
    private const int MAX_SOLUTIONS = 100; // Maximum number of solutions to keep

    public ModuleOptimizerService(ILogger<ModuleOptimizerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Optimize module combinations based on target attributes
    /// </summary>
    /// <param name="modules">Available modules</param>
    /// <param name="targetAttributes">Desired attributes to prioritize (can be null)</param>
    /// <param name="excludeAttributes">Attributes to avoid (can be null)</param>
    /// <param name="category">Module category filter</param>
    /// <param name="sortByLevel">If true, sort by total attribute value; if false, sort by weighted score</param>
    /// <returns>List of optimized solutions</returns>
    public List<ModuleSolution> OptimizeModules(
        List<ModuleInfo> modules,
        List<string>? targetAttributes = null,
        List<string>? excludeAttributes = null,
        ModuleCategory category = ModuleCategory.All,
        bool sortByLevel = false)
    {
        _logger.LogInformation("Starting module optimization...");
        _logger.LogInformation("Available modules: {Count}", modules.Count);
        _logger.LogInformation("Target attributes: {Attrs}", targetAttributes != null ? string.Join(", ", targetAttributes) : "None");
        _logger.LogInformation("Category filter: {Category}", category);

        // Filter modules by category
        var filteredModules = FilterByCategory(modules, category);
        _logger.LogInformation("Modules after category filter: {Count}", filteredModules.Count);

        // Filter out modules with excluded attributes
        if (excludeAttributes != null && excludeAttributes.Count > 0)
        {
            filteredModules = FilterExcludeAttributes(filteredModules, excludeAttributes);
            _logger.LogInformation("Modules after exclude filter: {Count}", filteredModules.Count);
        }

        if (filteredModules.Count == 0)
        {
            _logger.LogWarning("No modules remaining after filtering");
            return new List<ModuleSolution>();
        }

        // Generate solutions
        List<ModuleSolution> solutions;

        if (filteredModules.Count <= 50)
        {
            // For small sets, use enumeration
            _logger.LogInformation("Using enumeration strategy (small module count)");
            solutions = EnumerateAllCombinations(filteredModules, targetAttributes, sortByLevel);
        }
        else
        {
            // For larger sets, use greedy + local search
            _logger.LogInformation("Using greedy + local search strategy (large module count)");
            solutions = GreedyWithLocalSearch(filteredModules, targetAttributes, sortByLevel);
        }

        _logger.LogInformation("Generated {Count} solutions", solutions.Count);
        return solutions;
    }

    private List<ModuleInfo> FilterByCategory(List<ModuleInfo> modules, ModuleCategory category)
    {
        if (category == ModuleCategory.All)
            return modules;

        return modules.Where(m => m.Category == category).ToList();
    }

    private List<ModuleInfo> FilterExcludeAttributes(List<ModuleInfo> modules, List<string> excludeAttributes)
    {
        return modules.Where(module =>
        {
            // Exclude module if it has any of the excluded attributes
            return !module.Parts.Any(part => excludeAttributes.Contains(part.Name));
        }).ToList();
    }

    private List<ModuleSolution> EnumerateAllCombinations(
        List<ModuleInfo> modules,
        List<string>? targetAttributes,
        bool sortByLevel)
    {
        var solutions = new List<ModuleSolution>();

        // Generate combinations of 1 to MAX_MODULES_PER_SOLUTION modules
        for (int size = 1; size <= Math.Min(MAX_MODULES_PER_SOLUTION, modules.Count); size++)
        {
            var combinations = GetCombinations(modules, size);

            foreach (var combination in combinations)
            {
                var solution = CreateSolution(combination, targetAttributes, sortByLevel);
                solutions.Add(solution);

                if (solutions.Count >= MAX_SOLUTIONS * 10)
                {
                    // Prune if we have too many candidates
                    solutions = solutions.OrderByDescending(s => s.Score).Take(MAX_SOLUTIONS).ToList();
                }
            }
        }

        return solutions.OrderByDescending(s => s.Score).Take(MAX_SOLUTIONS).ToList();
    }

    private List<ModuleSolution> GreedyWithLocalSearch(
        List<ModuleInfo> modules,
        List<string>? targetAttributes,
        bool sortByLevel)
    {
        var solutions = new List<ModuleSolution>();
        var random = new Random();

        // Generate multiple greedy solutions with different starting points
        for (int attempt = 0; attempt < 20; attempt++)
        {
            var shuffled = modules.OrderBy(_ => random.Next()).ToList();
            var greedySolution = GreedySelection(shuffled, targetAttributes, sortByLevel);

            // Apply local search to improve the solution
            var improvedSolution = LocalSearch(greedySolution, modules, targetAttributes, sortByLevel);
            solutions.Add(improvedSolution);
        }

        return solutions.OrderByDescending(s => s.Score).Take(MAX_SOLUTIONS).ToList();
    }

    private ModuleSolution GreedySelection(
        List<ModuleInfo> modules,
        List<string>? targetAttributes,
        bool sortByLevel)
    {
        var selectedModules = new List<ModuleInfo>();

        // Greedily select modules that maximize score
        for (int i = 0; i < MAX_MODULES_PER_SOLUTION && i < modules.Count; i++)
        {
            ModuleInfo? bestModule = null;
            double bestScore = double.MinValue;

            foreach (var module in modules)
            {
                if (selectedModules.Contains(module))
                    continue;

                var testList = new List<ModuleInfo>(selectedModules) { module };
                var testSolution = CreateSolution(testList, targetAttributes, sortByLevel);

                if (testSolution.Score > bestScore)
                {
                    bestScore = testSolution.Score;
                    bestModule = module;
                }
            }

            if (bestModule != null)
            {
                selectedModules.Add(bestModule);
            }
        }

        return CreateSolution(selectedModules, targetAttributes, sortByLevel);
    }

    private ModuleSolution LocalSearch(
        ModuleSolution initialSolution,
        List<ModuleInfo> allModules,
        List<string>? targetAttributes,
        bool sortByLevel)
    {
        var currentSolution = initialSolution;
        var improved = true;
        int iterations = 0;
        const int maxIterations = 50;

        while (improved && iterations < maxIterations)
        {
            improved = false;
            iterations++;

            // Try swapping each module with an unused module
            for (int i = 0; i < currentSolution.Modules.Count; i++)
            {
                foreach (var newModule in allModules)
                {
                    if (currentSolution.Modules.Contains(newModule))
                        continue;

                    var testModules = new List<ModuleInfo>(currentSolution.Modules);
                    testModules[i] = newModule;

                    var testSolution = CreateSolution(testModules, targetAttributes, sortByLevel);

                    if (testSolution.Score > currentSolution.Score)
                    {
                        currentSolution = testSolution;
                        improved = true;
                        break;
                    }
                }

                if (improved) break;
            }
        }

        return currentSolution;
    }

    private ModuleSolution CreateSolution(
        List<ModuleInfo> modules,
        List<string>? targetAttributes,
        bool sortByLevel)
    {
        var solution = new ModuleSolution { Modules = modules };
        solution.CalculateAttributeBreakdown();

        if (sortByLevel)
        {
            // Sort by total attribute value
            solution.Score = solution.GetTotalCombatPower();
        }
        else
        {
            // Sort by weighted score based on target attributes
            solution.Score = CalculateWeightedScore(solution, targetAttributes);
        }

        return solution;
    }

    private double CalculateWeightedScore(ModuleSolution solution, List<string>? targetAttributes)
    {
        double score = 0;

        foreach (var (attrName, attrValue) in solution.AttributeBreakdown)
        {
            double weight = 1.0;

            // Increase weight for target attributes
            if (targetAttributes != null && targetAttributes.Contains(attrName))
            {
                weight = 3.0; // 3x weight for target attributes
            }

            score += attrValue * weight;
        }

        // Bonus for having more target attributes
        if (targetAttributes != null && targetAttributes.Count > 0)
        {
            int targetAttrCount = solution.AttributeBreakdown.Keys.Count(k => targetAttributes.Contains(k));
            score += targetAttrCount * 50; // Bonus for diversity in target attributes
        }

        return score;
    }

    private IEnumerable<List<T>> GetCombinations<T>(List<T> items, int size)
    {
        if (size == 0)
        {
            yield return new List<T>();
            yield break;
        }

        for (int i = 0; i <= items.Count - size; i++)
        {
            var item = items[i];
            var remainingItems = items.Skip(i + 1).ToList();

            foreach (var combination in GetCombinations(remainingItems, size - 1))
            {
                yield return new List<T> { item }.Concat(combination).ToList();
            }
        }
    }
}
