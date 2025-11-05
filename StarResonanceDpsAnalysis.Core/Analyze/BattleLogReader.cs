using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using StarResonanceDpsAnalysis.Core.Analyze.Exceptions;
using StarResonanceDpsAnalysis.Core.Analyze.Models;

namespace StarResonanceDpsAnalysis.Core.Analyze
{
    public static class BattleLogReader
    {
        private static readonly JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new PrivateSetterContractResolver()
        });

        public static LogsFileV3_0_0 ReadFile(string filePath)
        {
            var baseData = ReadFileBase(filePath, out var fs);
            return baseData.FileVersion switch
            {
                // 后续新版本写兼容层到 Versions 目录下, 并更新此函数的返回值到最新版本的 LogsFile

                LogsFileVersion.V3_0_0 => ReadFileV3_0_0(fs),
                _ => throw new NotSupportedException("File version not supported.")
            };
        }

        private static LogsFileBase ReadFileBase(string filePath, out FileStream fs)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            using var bdr = new BsonDataReader(fs) { CloseInput = false };

            return jsonSerializer.Deserialize<LogsFileBase>(bdr) ?? throw new Exception("Data deserialize failed.");
        }

        private static LogsFileV3_0_0 ReadFileV3_0_0(FileStream fs)
        {
            using (fs)
            {
                fs.Seek(0, SeekOrigin.Begin);

                using var bdr = new BsonDataReader(fs);
                var data = jsonSerializer.Deserialize<LogsFileV3_0_0>(bdr) ?? throw new Exception("Data deserialize failed.");

                foreach (var playerInfo in data.PlayerInfos)
                {
                    if (!playerInfo.TestHash())
                    {
                        throw new DataTamperedException("Data has been tampered.");
                    }
                }

                foreach (var battleLog in data.BattleLogs)
                {
                    if (!battleLog.TestHash())
                    {
                        throw new DataTamperedException("Data has been tampered.");
                    }
                }

                return data;
            }
        }
    }
}
