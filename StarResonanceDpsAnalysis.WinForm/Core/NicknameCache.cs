using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using Timer = System.Threading.Timer;

namespace StarResonanceDpsAnalysis.WinForm.Core
{


    public sealed class UserProfile
    {
        public long Uid { get; set; }
        public string Nickname { get; set; } = "";
        public string Profession { get; set; } = "";
        public long Power { get; set; }
    }

    /// <summary>
    /// 本地内存缓存 + 去抖动快照持久化（JSON 原子替换）
    /// 读/写全部走内存；合并写入降低 IO 频率；线程安全。
    /// </summary>
    public sealed class UserLocalCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, UserProfile> _map =
            new(StringComparer.Ordinal);

        private readonly string _filePath;
        private readonly string _tmpPath;
        private readonly object _flushLock = new();  // 磁盘写锁
        private readonly Timer _flushTimer;
        private volatile bool _dirty = false;

        // 可调参数
        private readonly TimeSpan _flushDelay;
        private readonly JsonSerializerOptions _jsonOpt = new()
        {
            WriteIndented = false
        };

        public UserLocalCache(string? dir = null, string fileName = "user_cache.json",
                              int flushDelayMs = 1000)
        {
            // 优先使用 EXE 目录；若无写权限（如装在 Program Files），自动回退到 LocalAppData
            var root = dir ?? GetWritableRoot();
            Directory.CreateDirectory(root);

            _filePath = Path.Combine(root, fileName);
            _tmpPath = _filePath + ".tmp";
            _flushDelay = TimeSpan.FromMilliseconds(Math.Max(100, flushDelayMs));

            // 不存在就创建一个空的 "{}"
            EnsureStoreFileExists();

            // 读入历史快照
            LoadSnapshotIfExists();

            // 定时器：需要时才启用（Change 动态设定）
            _flushTimer = new Timer(_ => FlushIfDirty(), null,
                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private static string GetWritableRoot()
        {
            // 1) EXE 目录
            var exeDir = AppContext.BaseDirectory;

            try
            {
                // 探测写权限
                var test = Path.Combine(exeDir, ".write_test");
                File.WriteAllText(test, "ok");
                TryDeleteQuiet(test);
                return exeDir; // 可写就用 EXE 目录
            }
            catch
            {
                // 2) 回退到 LocalAppData\StarResonanceDps
                var fallback = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "StarResonanceDps");
                return fallback;
            }
        }

        private void EnsureStoreFileExists()
        {
            if (!File.Exists(_filePath))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                    File.WriteAllText(_filePath, "{}"); // 初始空字典
                }
                catch
                {
                    // 忽略创建失败；后续写入时仍会尝试
                }
            }
        }

        // ------- 对外API -------

        public bool Upsert(UserProfile profile)
        {
            // 智能写：只有确实有变化才更新并调度落盘
            return UpsertIfChanged(profile);
        }


        public int BulkUpsert(IEnumerable<UserProfile> profiles)
        {
            if (profiles == null) return 0;
            int n = 0;
            foreach (var p in profiles)
                if (UpsertIfChanged(p)) n++;
            return n;
        }

        public UserProfile? Get(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid)) return null;
            return _map.TryGetValue(uid, out var v) ? Clone(v) : null;
        }

        public bool Remove(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid)) return false;
            var ok = _map.TryRemove(uid, out _);
            if (ok) ScheduleFlush();
            return ok;
        }

        public List<UserProfile> GetAll()
        {
            var list = new List<UserProfile>(_map.Count);
            foreach (var kv in _map)
                list.Add(Clone(kv.Value));
            return list;
        }

        /// <summary>手动立刻刷盘（可在退出前调用）。</summary>
        public void FlushNow()
        {
            FlushIfDirty(force: true);
        }

        public void Dispose()
        {
            try { FlushNow(); } catch { /* 忽略退出时异常 */ }
            _flushTimer.Dispose();
        }

        // ------- 内部实现 -------

        private void LoadSnapshotIfExists()
        {
            if (!File.Exists(_filePath)) return;

            try
            {
                var json = File.ReadAllText(_filePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, UserProfile>>(json, _jsonOpt);
                if (dict != null)
                {
                    foreach (var kv in dict)
                        if (!string.IsNullOrWhiteSpace(kv.Key) && kv.Value != null)
                            _map[kv.Key] = kv.Value;
                }
            }
            catch
            {
                // 读取损坏时忽略（可按需记录日志）
            }
        }

        private void ScheduleFlush()
        {
            _dirty = true;
            _flushTimer.Change(_flushDelay, Timeout.InfiniteTimeSpan);
        }

        private void FlushIfDirty(bool force = false)
        {
            if (!_dirty && !force) return;
            lock (_flushLock)
            {
                if (!_dirty && !force) return;
                try
                {
                    // 序列化为 { uid: profile, ... } 的字典，体积更小
                    var dict = new Dictionary<string, UserProfile>(_map.Count, StringComparer.Ordinal);
                    foreach (var kv in _map) dict[kv.Key] = kv.Value;

                    var json = JsonSerializer.Serialize(dict, _jsonOpt);

                    // 原子替换：先写 tmp，再 Replace/Move
                    File.WriteAllText(_tmpPath, json);
                    // 优先用 File.Replace（保留备份），不支持则回退 Move
                    try
                    {
                        var bak = _filePath + ".bak";
                        if (File.Exists(_filePath))
                            File.Replace(_tmpPath, _filePath, bak, ignoreMetadataErrors: true);
                        else
                            File.Move(_tmpPath, _filePath);
                        // 备份可按需删除
                        if (File.Exists(bak)) TryDeleteQuiet(bak);
                    }
                    catch
                    {
                        // 某些文件系统不支持 Replace，退回 Move 覆盖
                        if (File.Exists(_filePath)) TryDeleteQuiet(_filePath);
                        File.Move(_tmpPath, _filePath);
                    }

                    _dirty = false;
                }
                catch
                {
                    // 写入异常：下次定时器会再次尝试（保持 _dirty = true）
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// 仅当(昵称/职业/战力)任一发生变化时才更新并调度落盘；否则不做任何事。
        /// 返回值：是否发生了更新（写入/新增）
        /// </summary>
        public bool UpsertIfChanged(UserProfile incoming, bool caseInsensitiveName = false, bool trimName = true)
        {
            // 1) 校验：Uid 为 0 视为无效；ulong 不可为 null
            if (incoming is null || incoming.Uid == 0L)
                return false;

            var key = incoming.Uid.ToString(); // 仍用 string 作为字典键，便于 JSON 序列化

            var cmp = caseInsensitiveName ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            static string Norm(string? s, bool trim) => trim ? (s ?? string.Empty).Trim() : (s ?? string.Empty);

            bool changed = false;

            _map.AddOrUpdate(
                key,
                // 不存在：直接新增
                addValueFactory: _ =>
                {
                    changed = true;
                    return new UserProfile
                    {
                        Uid = incoming.Uid,
                        Nickname = Norm(incoming.Nickname, trimName),
                        Profession = incoming.Profession,
                        Power = incoming.Power
                    };
                },
                // 已存在：逐字段比对，只在不同才替换
                updateValueFactory: (_, exist) =>
                {
                    var newName = Norm(incoming.Nickname, trimName);
                    bool needUpdate =
                           !string.Equals(exist.Nickname ?? "", newName, cmp)
                        || !string.Equals(exist.Profession ?? "", incoming.Profession ?? "", StringComparison.Ordinal) // 职业区分大小写；需要可改成 OrdinalIgnoreCase
                        || exist.Power != incoming.Power;

                    if (!needUpdate)
                        return exist; // 完全一致：不更新、不落盘

                    changed = true;
                    return new UserProfile
                    {
                        Uid = exist.Uid,             // 保留原 Uid（与 key 一致）
                        Nickname = newName,
                        Profession = incoming.Profession,
                        Power = incoming.Power
                    };
                });

            if (changed) ScheduleFlush(); // 只有真的发生变化才调度去抖动保存
            return changed;
        }


        private static void TryDeleteQuiet(string path)
        {
            try { File.Delete(path); } catch { }
        }

        private static UserProfile Clone(UserProfile p) => new()
        {
            Uid = p.Uid,
            Nickname = p.Nickname,
            Profession = p.Profession,
            Power = p.Power
        };
    }
}
