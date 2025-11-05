namespace StarResonanceDpsAnalysis.Core.Configuration;

/// <summary>
/// Memory optimization configuration settings
/// </summary>
public static class MemoryConfig
{
    /// <summary>
    /// Maximum number of battle logs to keep in memory (ring buffer size)
    /// </summary>
    public static int MaxBattleLogs { get; set; } = 50000;

    /// <summary>
    /// Maximum size of player info cache
    /// </summary>
    public static int PlayerInfoCacheSize { get; set; } = 1000;

    /// <summary>
    /// Maximum size of DPS data cache
    /// </summary>
    public static int DpsDataCacheSize { get; set; } = 5000;

    /// <summary>
    /// Batch size for processing operations
    /// </summary>
    public static int ProcessingBatchSize { get; set; } = 1000;

    /// <summary>
    /// Frequency of garbage collection hints (in seconds)
    /// </summary>
    public static int GCHintFrequencySeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Maximum memory usage before triggering cleanup (in MB)
    /// </summary>
    public static long MaxMemoryUsageMB { get; set; } = 512;

    /// <summary>
    /// Enable memory profiling and logging
    /// </summary>
    public static bool EnableMemoryProfiling { get; set; } = false;

    /// <summary>
    /// Use object pooling for temporary objects
    /// </summary>
    public static bool UseObjectPooling { get; set; } = true;

    /// <summary>
    /// Enable aggressive memory optimization (may impact performance slightly)
    /// </summary>
    public static bool AggressiveMemoryOptimization { get; set; } = false;

    /// <summary>
    /// Maximum string cache size (for frequently used strings)
    /// </summary>
    public static int StringCacheSize { get; set; } = 2000;

    /// <summary>
    /// Frequency of weak reference cleanup (in seconds)
    /// </summary>
    public static int WeakReferenceCleanupSeconds { get; set; } = 30;

    /// <summary>
    /// Enable SIMD optimizations where available
    /// </summary>
    public static bool EnableSIMDOptimizations { get; set; } = true;

    /// <summary>
    /// Apply optimization settings based on available memory
    /// </summary>
    public static void AutoOptimize()
    {
        var availableMemory = GC.GetTotalMemory(false);
        var memoryMB = availableMemory / (1024 * 1024);

        if (memoryMB < 256) // Low memory system
        {
            MaxBattleLogs = 10000;
            PlayerInfoCacheSize = 500;
            DpsDataCacheSize = 1000;
            ProcessingBatchSize = 500;
            AggressiveMemoryOptimization = true;
            GCHintFrequencySeconds = 60; // More frequent GC
        }
        else if (memoryMB < 512) // Medium memory system
        {
            MaxBattleLogs = 25000;
            PlayerInfoCacheSize = 750;
            DpsDataCacheSize = 2500;
            ProcessingBatchSize = 750;
            AggressiveMemoryOptimization = false;
        }
        else // High memory system - use defaults or increase limits
        {
            MaxBattleLogs = 100000;
            PlayerInfoCacheSize = 2000;
            DpsDataCacheSize = 10000;
            ProcessingBatchSize = 2000;
            AggressiveMemoryOptimization = false;
            GCHintFrequencySeconds = 600; // Less frequent GC
        }
    }

    /// <summary>
    /// Get current memory usage statistics
    /// </summary>
    public static MemoryStats GetMemoryStats()
    {
        return new MemoryStats
        {
            TotalMemory = GC.GetTotalMemory(false),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            AvailableMemory = GC.GetTotalMemory(false)
        };
    }
}

/// <summary>
/// Memory usage statistics
/// </summary>
public struct MemoryStats
{
    public long TotalMemory;
    public int Gen0Collections;
    public int Gen1Collections;
    public int Gen2Collections;
    public long AvailableMemory;
    
    public long TotalMemoryMB => TotalMemory / (1024 * 1024);
    public long AvailableMemoryMB => AvailableMemory / (1024 * 1024);
}