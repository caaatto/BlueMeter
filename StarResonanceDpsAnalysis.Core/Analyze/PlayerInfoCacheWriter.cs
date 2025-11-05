using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using StarResonanceDpsAnalysis.Core.Analyze.Models;

namespace StarResonanceDpsAnalysis.Core.Analyze
{
    public class PlayerInfoCacheWriter
    {
        private PlayerInfoCacheFileV3_0_0 PlayerInfoCacheFile { get; set; }

        private PlayerInfoCacheWriter(IEnumerable<PlayerInfoFileData> playerInfos)
            : this(new PlayerInfoCacheFileV3_0_0()
            {
                FileVersion = PlayerInfoCacheFileVersion.V3_0_0,
                PlayerInfos = [.. playerInfos]
            })
        {
        }

        private PlayerInfoCacheWriter(PlayerInfoCacheFileV3_0_0 playerInfoCacheFile)
        {
            PlayerInfoCacheFile = playerInfoCacheFile;
        }

        private void WriteToFile()
        {
            // 修改此函数时, 请注意同时修改 PlayerInfoCacheReader

            var filePath = Path.Combine(Environment.CurrentDirectory, "PlayerInfoCache.dat");

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
            using var bw = new BsonDataWriter(fs);
            var serializer = new JsonSerializer();
            serializer.Serialize(bw, PlayerInfoCacheFile);
        }

        public async Task WriteToFileAsync()
        {
            await Task.Run(WriteToFile);
        }

        public static void WriteToFile(PlayerInfoCacheFileV3_0_0 playerInfoCacheFile)
        {
            var writer = new PlayerInfoCacheWriter(playerInfoCacheFile);
            writer.WriteToFile();
        }

        public static void WriteToFile(IEnumerable<PlayerInfoFileData> playerInfos)
        {
            var writer = new PlayerInfoCacheWriter(playerInfos);
            writer.WriteToFile();
        }

        public static async Task WriteToFileAsync(PlayerInfoCacheFileV3_0_0 playerInfoCacheFile)
        {
            await Task.Run(() => WriteToFile(playerInfoCacheFile));
        }

        public static async Task WriteToFileAsync(IEnumerable<PlayerInfoFileData> playerInfoCacheFile)
        {
            await Task.Run(() => WriteToFile(playerInfoCacheFile));
        }
    }
}
