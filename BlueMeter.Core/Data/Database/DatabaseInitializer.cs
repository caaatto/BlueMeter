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
    public static async Task InitializeAsync(string databasePath)
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

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('PlayerEncounterStats') WHERE name='TotalHits'";
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

            if (!exists)
            {
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

                Console.WriteLine("Successfully applied aggregate statistics migration to PlayerEncounterStats table");
            }

            // Migration: Add chart history JSON columns to PlayerEncounterStats
            // Check if DpsHistoryJson column exists
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('PlayerEncounterStats') WHERE name='DpsHistoryJson'";
            var historyColumnsExist = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

            if (!historyColumnsExist)
            {
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
                }

                Console.WriteLine("Successfully applied chart history migration to PlayerEncounterStats table");
            }
        }
        catch (Exception ex)
        {
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
