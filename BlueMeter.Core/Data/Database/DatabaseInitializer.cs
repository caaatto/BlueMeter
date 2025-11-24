using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BlueMeter.Core.Data.Database;

/// <summary>
/// Initializes and migrates the BlueMeter SQLite database
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Initialize database and run migrations
    /// </summary>
    public static async Task InitializeAsync(string databasePath, bool autoCleanup = true, int maxEncounters = 20, double maxSizeMB = 100)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = CreateOptions(databasePath);

        using var context = new BlueMeterDbContext(options);

        // Create database if it doesn't exist and apply migrations
        await context.Database.EnsureCreatedAsync();

        // Apply manual migrations for existing databases
        await ApplyManualMigrationsAsync(context);

        // Deactivate any active encounters from previous session
        var repository = new EncounterRepository(context);
        await repository.DeactivateAllEncountersAsync();

        // Automatic cleanup if enabled
        if (autoCleanup)
        {
            await AutoCleanupDatabaseAsync(databasePath, context, maxEncounters, maxSizeMB);
        }
    }

    /// <summary>
    /// Automatically clean up old encounters based on size and count limits
    /// </summary>
    private static async Task AutoCleanupDatabaseAsync(string databasePath, BlueMeterDbContext context, int maxEncounters, double maxSizeMB)
    {
        try
        {
            var sizeMB = GetDatabaseSizeMB(databasePath);
            DebugLogger.Log($"[DatabaseInitializer] Database size: {sizeMB:F2} MB (limit: {maxSizeMB} MB)");

            // Check if cleanup is needed
            if (sizeMB > maxSizeMB)
            {
                DebugLogger.Log($"[DatabaseInitializer] ⚠️ Database size exceeds limit! Starting automatic cleanup...");
                Console.WriteLine($"Database size ({sizeMB:F2} MB) exceeds limit ({maxSizeMB} MB). Starting cleanup...");

                var repository = new EncounterRepository(context);

                // Get current encounter count
                var currentCount = await repository.GetEncounterCountAsync();
                DebugLogger.Log($"[DatabaseInitializer] Current encounters: {currentCount}");

                // Clean up old encounters
                var deletedCount = await repository.CleanupOldEncountersAsync(maxEncounters);
                DebugLogger.Log($"[DatabaseInitializer] ✅ Deleted {deletedCount} old encounters, keeping most recent {maxEncounters}");
                Console.WriteLine($"Deleted {deletedCount} old encounters, keeping most recent {maxEncounters}");

                // Get new size after cleanup
                var newSizeMB = GetDatabaseSizeMB(databasePath);
                var savedMB = sizeMB - newSizeMB;
                DebugLogger.Log($"[DatabaseInitializer] Database size after cleanup: {newSizeMB:F2} MB (saved {savedMB:F2} MB)");
                Console.WriteLine($"Database cleaned up: {newSizeMB:F2} MB (freed {savedMB:F2} MB)");

                // Vacuum database to reclaim space
                DebugLogger.Log($"[DatabaseInitializer] Running VACUUM to reclaim disk space...");
                await VacuumDatabaseAsync(context);

                var finalSizeMB = GetDatabaseSizeMB(databasePath);
                var totalSavedMB = sizeMB - finalSizeMB;
                DebugLogger.Log($"[DatabaseInitializer] ✅ Final database size: {finalSizeMB:F2} MB (total saved: {totalSavedMB:F2} MB)");
                Console.WriteLine($"Cleanup complete! Final size: {finalSizeMB:F2} MB (saved {totalSavedMB:F2} MB total)");
            }
            else
            {
                DebugLogger.Log($"[DatabaseInitializer] ✓ Database size is within limits, no cleanup needed");
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[DatabaseInitializer] ❌ ERROR during auto-cleanup: {ex.Message}");
            Console.WriteLine($"Error during auto-cleanup: {ex.Message}");
            // Don't throw - cleanup is optional
        }
    }

    /// <summary>
    /// Vacuum database to reclaim disk space after deletions
    /// </summary>
    private static async Task VacuumDatabaseAsync(BlueMeterDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;

            if (!wasOpen)
            {
                await connection.OpenAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "VACUUM";
            await command.ExecuteNonQueryAsync();

            DebugLogger.Log($"[DatabaseInitializer] VACUUM completed successfully");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[DatabaseInitializer] WARNING: VACUUM failed: {ex.Message}");
            // Don't throw - vacuum is optional optimization
        }
    }

    /// <summary>
    /// Apply manual schema migrations for existing databases
    /// </summary>
    private static async Task ApplyManualMigrationsAsync(BlueMeterDbContext context)
    {
        try
        {
            // Migration: Add aggregate statistics columns to PlayerEncounterStats
            // Check if TotalHits column exists
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            DebugLogger.Log("[DatabaseInitializer] Checking for required database migrations...");

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('PlayerEncounterStats') WHERE name='TotalHits'";
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

            if (!exists)
            {
                DebugLogger.Log("[DatabaseInitializer] TotalHits column missing, applying aggregate statistics migration...");

                // Add aggregate statistics columns
                var alterCommands = new[]
                {
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN TotalHits INTEGER NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN TotalCrits INTEGER NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN TotalLuckyHits INTEGER NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN AvgDamagePerHit REAL NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN CritRate REAL NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN LuckyRate REAL NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN DPS REAL NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN HPS REAL NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN HighestCrit INTEGER NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN MinDamage INTEGER NOT NULL DEFAULT 0",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN MaxDamage INTEGER NOT NULL DEFAULT 0"
                };

                foreach (var alterCommand in alterCommands)
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = alterCommand;
                    await alterCmd.ExecuteNonQueryAsync();
                }

                DebugLogger.Log("[DatabaseInitializer] ✅ Successfully applied aggregate statistics migration to PlayerEncounterStats table");
                Console.WriteLine("Successfully applied aggregate statistics migration to PlayerEncounterStats table");
            }
            else
            {
                DebugLogger.Log("[DatabaseInitializer] ✓ Aggregate statistics columns already exist");
            }

            // Migration: Add chart history JSON columns to PlayerEncounterStats
            // Check if DpsHistoryJson column exists
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('PlayerEncounterStats') WHERE name='DpsHistoryJson'";
            var historyColumnsExist = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

            if (!historyColumnsExist)
            {
                DebugLogger.Log("[DatabaseInitializer] DpsHistoryJson column missing, applying chart history migration...");

                // Add chart history columns
                var historyAlterCommands = new[]
                {
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN DpsHistoryJson TEXT",
                    "ALTER TABLE PlayerEncounterStats ADD COLUMN HpsHistoryJson TEXT"
                };

                foreach (var alterCommand in historyAlterCommands)
                {
                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = alterCommand;
                    await alterCmd.ExecuteNonQueryAsync();
                    DebugLogger.Log($"[DatabaseInitializer]   Executed: {alterCommand}");
                }

                DebugLogger.Log("[DatabaseInitializer] ✅ Successfully applied chart history migration to PlayerEncounterStats table");
                Console.WriteLine("Successfully applied chart history migration to PlayerEncounterStats table");
            }
            else
            {
                DebugLogger.Log("[DatabaseInitializer] ✓ Chart history columns already exist");
            }

            DebugLogger.Log("[DatabaseInitializer] All migrations checked and applied successfully");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[DatabaseInitializer] ❌ ERROR applying manual migrations: {ex.Message}");
            DebugLogger.Log($"[DatabaseInitializer] Stack trace: {ex.StackTrace}");
            Console.WriteLine($"Error applying manual migrations: {ex.Message}");
            // Continue anyway - new databases won't need these migrations
        }
    }

    /// <summary>
    /// Create DbContext options for the given database path
    /// </summary>
    public static DbContextOptions<BlueMeterDbContext> CreateOptions(string databasePath)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlueMeterDbContext>();
        optionsBuilder.UseSqlite($"Data Source={databasePath}");

        // Enable sensitive data logging in debug builds
        #if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        #endif

        return optionsBuilder.Options;
    }

    /// <summary>
    /// Create a DbContext factory function
    /// </summary>
    public static Func<BlueMeterDbContext> CreateContextFactory(string databasePath)
    {
        var options = CreateOptions(databasePath);
        return () => new BlueMeterDbContext(options);
    }

    /// <summary>
    /// Get default database path
    /// </summary>
    public static string GetDefaultDatabasePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var blueMeterPath = Path.Combine(appDataPath, "BlueMeter");

        if (!Directory.Exists(blueMeterPath))
        {
            Directory.CreateDirectory(blueMeterPath);
        }

        return Path.Combine(blueMeterPath, "BlueMeter.db");
    }

    /// <summary>
    /// Backup database to specified path
    /// </summary>
    public static async Task BackupDatabaseAsync(string sourcePath, string backupPath)
    {
        if (!File.Exists(sourcePath))
            return;

        var directory = Path.GetDirectoryName(backupPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await Task.Run(() => File.Copy(sourcePath, backupPath, true));
    }

    /// <summary>
    /// Get database file size in MB
    /// </summary>
    public static double GetDatabaseSizeMB(string databasePath)
    {
        if (!File.Exists(databasePath))
            return 0;

        var fileInfo = new FileInfo(databasePath);
        return fileInfo.Length / (1024.0 * 1024.0);
    }
}
