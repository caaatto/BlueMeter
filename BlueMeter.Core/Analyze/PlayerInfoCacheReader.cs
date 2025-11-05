using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using BlueMeter.Core.Analyze.Exceptions;
using BlueMeter.Core.Analyze.Models;

namespace BlueMeter.Core.Analyze
{
    public static class PlayerInfoCacheReader
    {
        private static readonly JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new PrivateSetterContractResolver()
        });

        public static PlayerInfoCacheFileV3_0_0 ReadFile()
        {
            var baseData = ReadFileBase(out var fs);
            return baseData.FileVersion switch
            {
                // 后续新版本写兼容层到 Versions 目录下, 并更新此函数的返回值到最新版本的 PlayerInfoCacheFile

                PlayerInfoCacheFileVersion.V3_0_0 => ReadFileV3_0_0(fs),
                _ => throw new NotSupportedException("File version not supported.")
            };
        }

        private static PlayerInfoCacheFileBase ReadFileBase(out FileStream fs)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "PlayerInfoCache.dat");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            using var bdr = new BsonDataReader(fs) { CloseInput = false };

            return jsonSerializer.Deserialize<PlayerInfoCacheFileBase>(bdr) ?? throw new Exception("Data deserialize failed.");
        }

        private static PlayerInfoCacheFileV3_0_0 ReadFileV3_0_0(FileStream fs)
        {
            using (fs)
            {
                fs.Seek(0, SeekOrigin.Begin);

                using var bdr = new BsonDataReader(fs);
                var data = jsonSerializer.Deserialize<PlayerInfoCacheFileV3_0_0>(bdr) ?? throw new Exception("Data deserialize failed.");

                foreach (var playerInfo in data.PlayerInfos)
                {
                    if (!playerInfo.TestHash())
                    {
                        throw new DataTamperedException("Data has been tampered.");
                    }
                }

                return data;
            }
        }
    }
}
