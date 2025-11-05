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
    private static bool _isInitialized;
    private static DateTime _lastSaveTime = DateTime.MinValue;
    private static readonly TimeSpan MinSaveDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Initialize database integration with DataStorage
    /// </summary>
    public static async Task InitializeDatabaseAsync(IDataStorage? dataStorage = null, string? databasePath = null)
    {
        if (_isInitialized) return;

        databasePath ??= DatabaseInitializer.GetDefaultDatabasePath();
        _dataStorage = dataStorage;

        // Initialize database
        await DatabaseInitializer.InitializeAsync(databasePath);

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

            await _encounterService.SavePlayerStatsAsync(playerInfos, dpsData);

            _lastSaveTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving encounter: {ex.Message}");
        }
    }

    /// <summary>
    /// End current encounter and save to database
    /// </summary>
    public static async Task EndCurrentEncounterAsync(long durationMs)
    {
        if (_encounterService == null) return;

        // Final save before ending
        await SaveCurrentEncounterAsync();

        await _encounterService.EndCurrentEncounterAsync(durationMs);
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
    public static async Task CleanupOldEncountersAsync(int keepCount = 100)
    {
        if (_encounterService == null) return;

        await _encounterService.CleanupOldEncountersAsync(keepCount);
    }

    // Event handlers

    private static async void OnNewSectionCreated()
    {
        try
        {
            // End previous encounter if active
            if (_encounterService != null && _encounterService.IsEncounterActive)
            {
                var durationMs = DataStorage.SectionTimeout.TotalMilliseconds;
                await EndCurrentEncounterAsync((long)durationMs);
            }

            // Start new encounter
            await StartNewEncounterAsync();
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
            if (isConnected)
            {
                // Start new encounter when server connects
                await StartNewEncounterAsync();
            }
            else
            {
                // End encounter when server disconnects
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
