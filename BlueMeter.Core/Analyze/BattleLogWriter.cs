using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using BlueMeter.Core.Analyze.Models;
using BlueMeter.Core.Data.Models;

namespace BlueMeter.Core.Analyze
{
    public class BattleLogWriter
    {
        private string SavePath { get; set; }
        private LogsFileV3_0_0 LogsFile { get; set; }

        private BattleLogWriter(string path, IEnumerable<PlayerInfoFileData> playerInfos, IEnumerable<BattleLogFileData> battleLogs)
            : this(path, new()
            {
                FileVersion = LogsFileVersion.V3_0_0,
                PlayerInfos = [.. playerInfos],
                BattleLogs = [.. battleLogs]
            })
        {
        }

        private BattleLogWriter(string path, LogsFileV3_0_0 logsFile)
        {
            SavePath = path;
            LogsFile = logsFile;
        }

        private void WriteToFile()
        {
            // 修改此函数时, 请注意同时修改 BattleLogReader

            if (LogsFile.BattleLogs.Length == 0)
            {
                throw new InvalidOperationException("BattleLogs can not be empty.");
            }

            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }

            var playerName = LogsFile.PlayerInfos.Length == 1 ? LogsFile.PlayerInfos[0].Name : "Logs";
            var fromTimeStr = new DateTime(LogsFile.BattleLogs[0].TimeTicks, DateTimeKind.Utc);
            var endTimeStr = new DateTime(LogsFile.BattleLogs[^1].TimeTicks, DateTimeKind.Utc);
            var fileName = $"{playerName}_{fromTimeStr:yyyy_MM_dd_HH_mm_ss}_{endTimeStr:yyyy_MM_dd_HH_mm_ss}.srlogs";

            var filePath = Path.Combine(SavePath, fileName);

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
            using var bw = new BsonDataWriter(fs);
            var serializer = new JsonSerializer();
            serializer.Serialize(bw, LogsFile);
        }

        public async Task WriteToFileAsync()
        {
            await Task.Run(WriteToFile);
        }

        public static void WriteToFile(string path, LogsFileV3_0_0 logsFile)
        {
            var writer = new BattleLogWriter(path, logsFile);
            writer.WriteToFile();
        }

        public static void WriteToFile(string path, IEnumerable<PlayerInfoFileData> playerInfos, IEnumerable<BattleLogFileData> battleLogs)
        {
            var writer = new BattleLogWriter(path, playerInfos, battleLogs);
            writer.WriteToFile();
        }

        public static async Task WriteToFileAsync(string path, LogsFileV3_0_0 logsFile)
        {
            await Task.Run(() => WriteToFile(path, logsFile));
        }

        public static async Task WriteToFileAsync(string path, IEnumerable<PlayerInfoFileData> playerInfos, IEnumerable<BattleLogFileData> battleLogs)
        {
            await Task.Run(() => WriteToFile(path, playerInfos, battleLogs));
        }
    }
}
