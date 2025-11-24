using System;
using System.Linq;
using System.Threading.Tasks;
using BlueMeter.Core.Data.Database;
using BlueMeter.Core.Data.Models;
using BlueMeter.WPF.Data;

namespace BlueMeter.Core.Data;

/// <summary>
/// Extension methods for integrating DataStorage with database persistence
/// </summary>
public static class DataStorageExtensions
{
    private static EncounterService? _encounterService;
    private static IDataStorage? _dataStorage;
    private static object? _chartDataService; // IChartDataService from WPF
    private static bool _isInitialized;
    private static DateTime _lastSaveTime = DateTime.MinValue;
    private static readonly TimeSpan MinSaveDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Initialize database integration with DataStorage
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        IDataStorage? dataStorage = null,
        string? databasePath = null,
        object? chartDataService = null,
        bool autoCleanup = true,
        int maxEncounters = 20,
        double maxSizeMB = 100)
    {
        if (_isInitialized) return;

        databasePath ??= DatabaseInitializer.GetDefaultDatabasePath();
        _dataStorage = dataStorage;
        _chartDataService = chartDataService; // Store reference to chart service

        // Initialize database with cleanup settings
        await DatabaseInitializer.InitializeAsync(databasePath, autoCleanup, maxEncounters, maxSizeMB);

        // Create encounter service
        var contextFactory = DatabaseInitializer.CreateContextFactory(databasePath);
        _encounterService = new EncounterService(contextFactory);

        // Subscribe to DataStorage events
        if (_dataStorage != null)
        {
            // Use IDataStorage instance (DataStorageV2)
            _dataStorage.NewSectionCreated += OnNewSectionCreated;
            _dataStorage.ServerConnectionStateChanged += OnServerConnectionStateChanged;
            _dataStorage.PlayerInfoUpdated += OnPlayerInfoUpdated;
        }
        else
        {
            // Fallback to static DataStorage
            DataStorage.NewSectionCreated += OnNewSectionCreated;
            DataStorage.ServerConnectionStateChanged += OnServerConnectionStateChanged;
            DataStorage.PlayerInfoUpdated += OnPlayerInfoUpdated;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Get the encounter service instance
    /// </summary>
    public static EncounterService? GetEncounterService() => _encounterService;

    /// <summary>
    /// Start a new encounter manually
    /// </summary>
    public static async Task StartNewEncounterAsync()
    {
        if (_encounterService == null) return;

        await _encounterService.StartEncounterAsync();
    }

    /// <summary>
    /// Save current encounter to database
    /// </summary>
    public static async Task SaveCurrentEncounterAsync()
    {
        if (_encounterService == null || !_encounterService.IsEncounterActive) return;

        // Avoid saving too frequently
        if (DateTime.Now - _lastSaveTime < MinSaveDuration) return;

        try
        {
            Dictionary<long, PlayerInfo> playerInfos;
            Dictionary<long, DpsData> dpsData;

            if (_dataStorage != null)
            {
                // Use IDataStorage instance
                playerInfos = _dataStorage.ReadOnlyPlayerInfoDatas.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                dpsData = _dataStorage.ReadOnlySectionedDpsDatas.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else
            {
                // Fallback to static DataStorage
                playerInfos = DataStorage.ReadOnlyPlayerInfoDatas.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                dpsData = DataStorage.ReadOnlySectionedDpsDatas.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            // Get chart history snapshots (using reflection to avoid circular dependency)
            Dictionary<long, List<Database.ChartDataPoint>>? dpsHistory = null;
            Dictionary<long, List<Database.ChartDataPoint>>? hpsHistory = null;

            if (_chartDataService != null)
            {
                try
                {
                    var serviceType = _chartDataService.GetType();
                    var getDpsMethod = serviceType.GetMethod("GetDpsHistorySnapshot");
                    var getHpsMethod = serviceType.GetMethod("GetHpsHistorySnapshot");

                    if (getDpsMethod != null && getHpsMethod != null)
                    {
                        // Get snapshots from WPF service (returns Dictionary<long, List<WPF.ChartDataPoint>>)
                        var wpfDpsHistory = getDpsMethod.Invoke(_chartDataService, null) as dynamic;
                        var wpfHpsHistory = getHpsMethod.Invoke(_chartDataService, null) as dynamic;

                        // Convert WPF ChartDataPoints to Core ChartDataPoints
                        dpsHistory = ConvertChartHistory(wpfDpsHistory);
                        hpsHistory = ConvertChartHistory(wpfHpsHistory);

                        // DEBUG: Log chart history capture
                        Console.WriteLine($"[DataStorageExtensions] Chart history captured:");
                        Console.WriteLine($"  - DPS History: {dpsHistory?.Count ?? 0} players, {dpsHistory?.Sum(kvp => kvp.Value.Count) ?? 0} total points");
                        Console.WriteLine($"  - HPS History: {hpsHistory?.Count ?? 0} players, {hpsHistory?.Sum(kvp => kvp.Value.Count) ?? 0} total points");
                    }
                    else
                    {
                        Console.WriteLine("[DataStorageExtensions] WARNING: Chart snapshot methods not found!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DataStorageExtensions] ERROR getting chart history: {ex.Message}");
                    Console.WriteLine($"  Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine("[DataStorageExtensions] WARNING: ChartDataService is null, no chart data will be saved!");
            }

            await _encounterService.SavePlayerStatsAsync(playerInfos, dpsData, dpsHistory, hpsHistory);

            _lastSaveTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving encounter: {ex.Message}");
        }
    }

    /// <summary>
    /// Convert chart history from WPF ChartDataPoint to Core ChartDataPoint
    /// </summary>
    private static Dictionary<long, List<Database.ChartDataPoint>>? ConvertChartHistory(dynamic? wpfHistory)
    {
        if (wpfHistory == null) return null;

        var result = new Dictionary<long, List<Database.ChartDataPoint>>();

        foreach (var kvp in wpfHistory)
        {
            long playerId = kvp.Key;
            var points = new List<Database.ChartDataPoint>();

            foreach (var wpfPoint in kvp.Value)
            {
                // Access properties via dynamic
                DateTime timestamp = wpfPoint.Timestamp;
                double value = wpfPoint.Value;
                points.Add(new Database.ChartDataPoint(timestamp, value));
            }

            result[playerId] = points;
        }

        return result;
    }

    /// <summary>
    /// End current encounter and save to database
    /// </summary>
    public static async Task EndCurrentEncounterAsync(long durationMs, string? bossName = null, long? bossUuid = null)
    {
        if (_encounterService == null) return;

        // Final save before ending
        await SaveCurrentEncounterAsync();

        await _encounterService.EndCurrentEncounterAsync(durationMs, 0, bossName, bossUuid);
    }

    /// <summary>
    /// Get recent encounters for history
    /// </summary>
    public static async Task<System.Collections.Generic.List<EncounterSummary>> GetRecentEncountersAsync(int count = 50)
    {
        if (_encounterService == null) return new System.Collections.Generic.List<EncounterSummary>();

        return await _encounterService.GetRecentEncountersAsync(count);
    }

    /// <summary>
    /// Load encounter from database
    /// </summary>
    public static async Task<EncounterData?> LoadEncounterAsync(string encounterId)
    {
        if (_encounterService == null) return null;

        return await _encounterService.LoadEncounterAsync(encounterId);
    }

    /// <summary>
    /// Get cached player info from database to fix "Unknown" players
    /// </summary>
    public static async Task<PlayerInfo?> GetCachedPlayerInfoAsync(long uid)
    {
        if (_encounterService == null) return null;

        return await _encounterService.GetCachedPlayerInfoAsync(uid);
    }

    /// <summary>
    /// Cleanup old encounters from database
    /// </summary>
    public static async Task CleanupOldEncountersAsync(int keepCount = 20)
    {
        if (_encounterService == null) return;

        await _encounterService.CleanupOldEncountersAsync(keepCount);
    }

    /// <summary>
    /// Delete all encounters from database
    /// </summary>
    public static async Task<int> DeleteAllEncountersAsync()
    {
        if (_encounterService == null) return 0;

        return await _encounterService.DeleteAllEncountersAsync();
    }

    // Event handlers

    private static async void OnNewSectionCreated()
    {
        try
        {
            // Boss fights now handle their own encounter lifecycle in DataStorageV2
            // This event handler is kept for potential future use (e.g., periodic saves)

            // Optionally save current encounter state
            if (_encounterService != null && _encounterService.IsEncounterActive)
            {
                await SaveCurrentEncounterAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling new section: {ex.Message}");
        }
    }

    private static async void OnServerConnectionStateChanged(bool isConnected)
    {
        try
        {
            // Boss fights now handle their own encounter lifecycle in DataStorageV2
            // Only clean up on disconnect

            if (!isConnected)
            {
                // End encounter when server disconnects (if any active)
                if (_encounterService != null && _encounterService.IsEncounterActive)
                {
                    await SaveCurrentEncounterAsync();
                    await _encounterService.EndCurrentEncounterAsync(0);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling server connection change: {ex.Message}");
        }
    }

    private static async void OnPlayerInfoUpdated(PlayerInfo playerInfo)
    {
        try
        {
            // Update player cache in database
            if (_encounterService != null)
            {
                await _encounterService.UpdatePlayerCacheAsync(playerInfo);

                // Periodically save encounter data
                await SaveCurrentEncounterAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating player info in database: {ex.Message}");
        }
    }

    /// <summary>
    /// Preload player cache from database to reduce "Unknown" players
    /// </summary>
    public static async Task PreloadPlayerCacheAsync()
    {
        if (_encounterService == null) return;

        try
        {
            using var context = DatabaseInitializer.CreateContextFactory(DatabaseInitializer.GetDefaultDatabasePath())();
            var repository = new EncounterRepository(context);
            var players = await repository.GetAllPlayersAsync();

            int loadedCount = 0;
            foreach (var player in players)
            {
                // Only preload players with names (skip NPCs and incomplete data)
                if (!string.IsNullOrEmpty(player.Name) && player.Name != "Unknown" && !player.IsNpc)
                {
                    var playerInfo = new PlayerInfo
                    {
                        UID = player.UID,
                        Name = player.Name,
                        ProfessionID = player.ProfessionID,
                        SubProfessionName = player.SubProfessionName,
                        Spec = player.Spec,
                        CombatPower = player.CombatPower,
                        Level = player.Level,
                        RankLevel = player.RankLevel,
                        Critical = player.Critical,
                        Lucky = player.Lucky,
                        MaxHP = player.MaxHP
                    };

                    // Add to DataStorage without triggering events
                    DataStorage.ReadOnlyPlayerInfoDatas.GetType()
                        .GetField("_dictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                        .GetValue(DataStorage.ReadOnlyPlayerInfoDatas);

                    // Use reflection to access private PlayerInfoDatas dictionary
                    var playerInfoDatasField = typeof(DataStorage).GetField("PlayerInfoDatas",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                    if (playerInfoDatasField != null)
                    {
                        var playerInfoDatas = playerInfoDatasField.GetValue(null) as Dictionary<long, PlayerInfo>;
                        if (playerInfoDatas != null && !playerInfoDatas.ContainsKey(player.UID))
                        {
                            playerInfoDatas[player.UID] = playerInfo;
                            loadedCount++;
                        }
                    }
                }
            }

            Console.WriteLine($"Preloaded {loadedCount} players from database cache");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preloading player cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleanup database integration
    /// </summary>
    public static void Shutdown()
    {
        if (!_isInitialized) return;

        // Unsubscribe from events
        if (_dataStorage != null)
        {
            _dataStorage.NewSectionCreated -= OnNewSectionCreated;
            _dataStorage.ServerConnectionStateChanged -= OnServerConnectionStateChanged;
            _dataStorage.PlayerInfoUpdated -= OnPlayerInfoUpdated;
        }
        else
        {
            DataStorage.NewSectionCreated -= OnNewSectionCreated;
            DataStorage.ServerConnectionStateChanged -= OnServerConnectionStateChanged;
            DataStorage.PlayerInfoUpdated -= OnPlayerInfoUpdated;
        }

        _encounterService = null;
        _dataStorage = null;
        _isInitialized = false;
    }
}
