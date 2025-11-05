namespace StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics
{
    /// <summary>
    /// 全程记录（跨战斗，会话级）：通过在 AddDamage/AddHealing/AddTakenDamage 内部打点，实时累加
    /// - Start(): 开启记录（不会被 ClearAll 清掉）
    /// - Stop(): 关闭记录（保留数据，可随时快照）
    /// - Reset(): 手动清空本会话
    /// - TakeSnapshot(): 生成“全程快照”（含玩家聚合与技能明细）
    /// - GetTeamDps()/GetPlayerDps(): 全程秒伤
    /// </summary>
    public static class FullRecord
    {
        // # 导航 / 分类索引（仅注释，不影响代码）：
        // #   1) 通用工具与数值格式: R2()
        // #   2) Shim 只读外观（与 StatisticData 口径对齐）: Shim.StatsLike / Shim.PlayerLike / Shim.TakenOverviewLike
        // #   3) UI 视图投影: StatView / ToView() / MergeStats()
        // #   4) 对外统计查询（与 StatisticData 一致口径）: GetPlayerDamageStats/HealingStats/TakenStats
        // #   5) 会话状态与控制: IsRecording/StartedAt/EndedAt + Start/Stop/Reset/GetSessionTotalTimeSpan
        // #   6) 快照入口与历史: TakeSnapshot / SessionHistory / 内部 StopInternal/EffectiveEndTime
        // #   7) 写入点（由解码管线调用）: RecordDamage/RecordHealing/RecordTakenDamage + UpdateRealtimeDps
        // #   8) 快照 & 秒伤对外接口: GetPlayersWithTotals/GetPlayersWithTotalsArray/GetTeamDps/GetPlayerDps 等
        // #   9) 快照时间检索: GetAllPlayersDataBySnapshotTime/GetPlayerSkillsBySnapshotTime
        // #  10) 内部实现工具: SessionSeconds/GetOrCreate/Accumulate/ToSkillSummary
        // #  11) 内部数据结构: PlayerAcc / StatAcc

        // ======================================================================
        // # 分类 1：通用工具与数值格式
        // ======================================================================

        // # 通用：两位小数四舍五入（远离零）
        private static double R2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        // ======================================================================
        // # 分类 2：Shim 只读外观（对齐 StatisticData 接口口径，便于 UI 复用）
        // ======================================================================
        public static class Shim
        {
            // # —— 与 PlayerData.*Stats 口径一致的“只读统计对象” —— 
            // # 用于向上层提供只读统计视图，避免直接暴露内部累加器
            public sealed class StatsLike
            {
                public ulong Total, Normal, Critical, Lucky;
                public int CountTotal, CountNormal, CountCritical, CountLucky;
                public ulong MaxSingleHit, MinSingleHit; // Min=0 表示无记录
                public double ActiveSeconds;             // 用于计算 Dps/Hps

                public double GetAveragePerHit() => CountTotal > 0 ? R2((double)Total / CountTotal) : 0.0;
                public double GetCritRate() => CountTotal > 0 ? R2((double)CountCritical * 100.0 / CountTotal) : 0.0;
                public double GetLuckyRate() => CountTotal > 0 ? R2((double)CountLucky * 100.0 / CountTotal) : 0.0;
            }

            // # —— 与 StatisticData._manager.GetOrCreate(uid) 返回的“p”相似的外观 —— 
            // # 上层可像使用当前战斗统计那样读“全程统计”
            public sealed class PlayerLike
            {
                public StatsLike DamageStats { get; init; } = new();
                public StatsLike HealingStats { get; init; } = new();
                public StatsLike TakenStats { get; init; } = new();

                public double GetTotalDps() => DamageStats.ActiveSeconds > 0 ? R2(DamageStats.Total / DamageStats.ActiveSeconds) : 0.0;
                public double GetTotalHps() => HealingStats.ActiveSeconds > 0 ? R2(HealingStats.Total / HealingStats.ActiveSeconds) : 0.0;
            }

            // # 承伤总览（用于 UI 顶部概览）
            public sealed class TakenOverviewLike
            {
                public ulong Total { get; init; }
                public double AvgTakenPerSec { get; init; }
                public ulong MaxSingleHit { get; init; }
                public ulong MinSingleHit { get; init; }
            }

            // # 内部：将累加器转换为只读 StatsLike
            private static StatsLike From(StatAcc s)
            {
                // # 将内部累加器 StatAcc 投影为只读 StatsLike，供 UI/外部展示
                return new StatsLike
                {
                    Total = s.Total,
                    Normal = s.Normal,
                    Critical = s.Critical,
                    Lucky = s.Lucky + s.CritLucky,   // ★ 合并
                    CountTotal = s.CountTotal,
                    CountNormal = s.CountNormal,
                    CountCritical = s.CountCritical,
                    CountLucky = s.CountLucky,
                    MaxSingleHit = s.MaxSingleHit,
                    MinSingleHit = s.MinSingleHit, // 0 代表没记录
                    ActiveSeconds = s.ActiveSeconds
                };
            }

            // # 内部：聚合一组统计（常用于承伤按技能 → 玩家层合并）
            private static StatAcc MergeStats(IEnumerable<StatAcc> items)
            {
                // # 聚合多项 StatAcc：用于把逐技能合并为玩家层（承伤等）
                var acc = new StatAcc();
                ulong min = 0; bool hasMin = false;
                double maxActiveSecs = 0;

                foreach (var s in items)
                {
                    acc.Total += s.Total;
                    acc.Normal += s.Normal;
                    acc.Critical += s.Critical;
                    acc.Lucky += s.Lucky;
                    acc.CritLucky += s.CritLucky;
                    acc.HpLessen += s.HpLessen;

                    acc.CountNormal += s.CountNormal;
                    acc.CountCritical += s.CountCritical;
                    acc.CountLucky += s.CountLucky;
                    acc.CountTotal += s.CountTotal;

                    if (s.MaxSingleHit > acc.MaxSingleHit) acc.MaxSingleHit = s.MaxSingleHit;
                    if (s.MinSingleHit > 0 && (!hasMin || s.MinSingleHit < min)) { min = s.MinSingleHit; hasMin = true; }
                    if (s.ActiveSeconds > maxActiveSecs) maxActiveSecs = s.ActiveSeconds;
                }

                acc.MinSingleHit = hasMin ? min : 0;
                acc.ActiveSeconds = maxActiveSecs; // 不相加，取最大活跃时长，避免夸大分母
                return acc;
            }

            /// <summary>
            /// # 获取“只读玩家视图”：从 FullRecord 内部数据投影为与 StatisticData 类似的结构
            /// </summary>
            public static PlayerLike GetOrCreate(long uid)
            {
                // # 以 FullRecord 的内部累加为来源，返回近似 StatisticData 的“只读外观”
                lock (_sync)
                {
                    if (!_players.TryGetValue(uid, out var p))
                        return new PlayerLike();

                    // Damage / Healing 直接来自 FullRecord 的玩家聚合器
                    var dmg = From(p.Damage);
                    var heal = From(p.Healing);

                    // Taken：按技能合并（若没有按技能承伤，则用 TakenDamage + 会话秒数兜底）
                    StatAcc takenAcc;
                    if (p.TakenSkills != null && p.TakenSkills.Count > 0)
                        takenAcc = MergeStats(p.TakenSkills.Values);
                    else
                        takenAcc = new StatAcc
                        {
                            Total = p.TakenDamage,
                            ActiveSeconds = Math.Max(0.0, GetSessionTotalTimeSpan().TotalSeconds)
                        };
                    var taken = From(takenAcc);

                    return new PlayerLike
                    {
                        DamageStats = dmg,
                        HealingStats = heal,
                        TakenStats = taken
                    };
                }
            }

            /// <summary>
            /// # 承伤总览（合计、每秒均值、最大/最小单次）
            /// </summary>
            public static TakenOverviewLike GetPlayerTakenOverview(long uid)
            {
                // # 承伤总览：总量/每秒均值/单击最大最小
                var p = GetOrCreate(uid);
                var t = p.TakenStats;
                double perSec = t.ActiveSeconds > 0 ? R2(t.Total / t.ActiveSeconds) : 0.0;

                return new TakenOverviewLike
                {
                    Total = t.Total,
                    AvgTakenPerSec = perSec,
                    MaxSingleHit = t.MaxSingleHit,
                    MinSingleHit = t.MinSingleHit
                };
            }
        }

        // ======================================================================
        // # 分类 3：UI 只读统计视图（StatView 映射与合并工具）
        // ======================================================================

        // # === UI 只读统计视图 ===
        public readonly record struct StatView(
            ulong Total,
            ulong Normal,
            ulong Critical,
            ulong Lucky,
            int CountTotal,
            int CountNormal,
            int CountCritical,
            int CountLucky,
            ulong MaxSingleHit,
            ulong MinSingleHit,
            double PerSecond,      // = Total / ActiveSeconds(>0 ?)
            double AveragePerHit,  // = Total / CountTotal(>0 ?)
            double CritRate,       // %，两位小数
            double LuckyRate       // %
        );

        // # 将内部累加器 → UI 展示用视图
        private static StatView ToView(StatAcc s)
        {
            // # 将内部累加器映射为 UI 展示用视图（带每秒/均伤/暴击率/幸运率）
            int ct = s.CountTotal;
            double secs = s.ActiveSeconds > 0 ? s.ActiveSeconds : 0;
            double perSec = secs > 0 ? R2(s.Total / secs) : 0;
            double avg = ct > 0 ? R2((double)s.Total / ct) : 0;
            double crit = ct > 0 ? R2((double)s.CountCritical * 100.0 / ct) : 0.0;
            double lucky = ct > 0 ? R2((double)s.CountLucky * 100.0 / ct) : 0.0;

            ulong min = s.MinSingleHit; // StatAcc 里 Min=0 表示未赋值，直接返回 0 即可
            ulong luckyCombined = s.Lucky + s.CritLucky;   // ★ 关键：合并
            return new StatView(
                Total: s.Total,
                Normal: s.Normal,
                Critical: s.Critical,
                Lucky: luckyCombined,
                CountTotal: s.CountTotal,
                CountNormal: s.CountNormal,
                CountCritical: s.CountCritical,
                CountLucky: s.CountLucky,
                MaxSingleHit: s.MaxSingleHit,
                MinSingleHit: min,
                PerSecond: perSec,
                AveragePerHit: avg,
                CritRate: crit,
                LuckyRate: lucky
            );
        }

        // # 合并一组 StatAcc（用于 Taken：把各技能承伤合成玩家总承伤视图）
        private static StatAcc MergeStats(IEnumerable<StatAcc> items)
        {
            var acc = new StatAcc();
            ulong min = 0;
            bool hasMin = false;
            double maxActiveSecs = 0;

            foreach (var s in items)
            {
                acc.Total += s.Total;
                acc.Normal += s.Normal;
                acc.Critical += s.Critical;
                acc.Lucky += s.Lucky;
                acc.CritLucky += s.CritLucky;
                acc.HpLessen += s.HpLessen;

                acc.CountNormal += s.CountNormal;
                acc.CountCritical += s.CountCritical;
                acc.CountLucky += s.CountLucky;
                acc.CountTotal += s.CountTotal;

                if (s.MaxSingleHit > acc.MaxSingleHit) acc.MaxSingleHit = s.MaxSingleHit;
                if (s.MinSingleHit > 0 && (!hasMin || s.MinSingleHit < min)) { min = s.MinSingleHit; hasMin = true; }

                if (s.ActiveSeconds > maxActiveSecs) maxActiveSecs = s.ActiveSeconds;
            }

            acc.MinSingleHit = hasMin ? min : 0;
            acc.ActiveSeconds = maxActiveSecs; // 取最大活跃秒数，避免相加放大
            return acc;
        }

        // ======================================================================
        // # 分类 4：对外统计查询（口径对齐 StatisticData）
        // ======================================================================

        /// <summary>获取玩家全程伤害统计（UI 视图口径）。</summary>
        public static StatView GetPlayerDamageStats(long uid)
        {
            lock (_sync)
            {
                if (_players.TryGetValue(uid, out var p))
                    return ToView(p.Damage);
                return default;
            }
        }

        /// <summary>获取玩家全程治疗统计（UI 视图口径）。</summary>
        public static StatView GetPlayerHealingStats(long uid)
        {
            lock (_sync)
            {
                if (_players.TryGetValue(uid, out var p))
                    return ToView(p.Healing);
                return default;
            }
        }

        /// <summary>获取玩家全程承伤统计（UI 视图口径）。</summary>
        public static StatView GetPlayerTakenStats(long uid)
        {
            lock (_sync)
            {
                if (_players.TryGetValue(uid, out var p))
                {
                    if (p.TakenSkills.Count > 0)
                        return ToView(MergeStats(p.TakenSkills.Values));

                    // 没有逐技能承伤明细时，至少返回 Total；秒数兜底用会话时长
                    var secs = GetSessionTotalTimeSpan().TotalSeconds; // 你已实现的会话秒数API
                    var fake = new StatAcc { Total = p.TakenDamage, ActiveSeconds = secs > 0 ? secs : 0 };
                    return ToView(fake);
                }
                return default;
            }
        }

        // ======================================================================
        // # 分类 4.5：死亡统计查询（基于 TakenSkills.CountDead）
        // ======================================================================

        /// <summary>获取团队在当前会话的总死亡次数（所有玩家之和）。</summary>
        public static int GetTeamDeathCount()
        {
            int teamDeaths = 0;
            PlayerAcc[] playersSnapshot;
            lock (_sync) { playersSnapshot = _players.Values.ToArray(); }

            foreach (var p in playersSnapshot)
                foreach (var kv in p.TakenSkills)
                    teamDeaths += kv.Value.CountDead;

            return teamDeaths;
        }

        /// <summary>获取指定玩家在当前会话的死亡次数。</summary>
        public static int GetPlayerDeathCount(long uid)
        {
            lock (_sync)
            {
                if (!_players.TryGetValue(uid, out var p)) return 0;
                int deaths = 0;
                foreach (var kv in p.TakenSkills)
                    deaths += kv.Value.CountDead;
                return deaths;
            }
        }

        /// <summary>
        /// 获取“所有玩家”的死亡次数清单（默认降序）。
        /// includeZero=false 时，会过滤掉死亡数为 0 的玩家。
        /// </summary>
        public static List<(long Uid, string Nickname, int CombatPower, string Profession, string? SubProfession, int Deaths)>
            GetAllPlayerDeathCounts(bool includeZero = false)
        {
            var result = new List<(long, string, int, string, string?, int)>();
            PlayerAcc[] playersSnapshot;
            lock (_sync) { playersSnapshot = _players.Values.ToArray(); }

            foreach (var p in playersSnapshot)
            {
                int deaths = 0;
                foreach (var kv in p.TakenSkills)
                    deaths += kv.Value.CountDead;

                if (includeZero || deaths > 0)
                    result.Add((p.Uid, p.Nickname, p.CombatPower, p.Profession, p.SubProfession, deaths));
            }

            return result.OrderByDescending(x => x.Item6).ToList(); // 按 Deaths 降序
        }

        /// <summary>
        /// 获取“某玩家按技能”的死亡次数细分（降序）。
        /// 返回：SkillId, SkillName, Deaths
        /// </summary>
        public static List<(long SkillId, string SkillName, int Deaths)>
            GetPlayerDeathBreakdownBySkill(long uid)
        {
            lock (_sync)
            {
                if (!_players.TryGetValue(uid, out var p) || p.TakenSkills.Count == 0)
                    return new();

                var list = new List<(long, string, int)>(p.TakenSkills.Count);
                foreach (var kv in p.TakenSkills)
                {
                    var sid = kv.Key;
                    var s = kv.Value;
                    if (s.CountDead <= 0) continue;

                    var name = SkillBook.Get(sid).Name;
                    list.Add((sid, name, s.CountDead));
                }
                return list.OrderByDescending(x => x.Item3).ToList();

            }
        }
        // ======================================================================
        // # 分类 5：对外绑定的行结构（列表项定义）
        // ======================================================================

        // # 用于对外绑定的行结构（可按需增删字段）
        public sealed record FullPlayerTotal(
                long Uid,
                string Nickname,
                int CombatPower,
                string Profession,
                string? SubProfession,
                ulong TotalDamage,
                ulong TotalHealing,
                ulong TakenDamage,
                double Dps,   // 全程秒伤（只算伤害）
                double Hps    // 全程秒疗
            );

        // ======================================================================
        // # 分类 6：会话状态与控制（启动/停止/重置/时长）
        // ======================================================================

        // # 会话状态字段 —— 记录当前是否在录制，以及开始/结束时间点
        public static bool IsRecording { get; private set; }
        public static DateTime? StartedAt { get; private set; }
        public static DateTime? EndedAt { get; private set; }

        // # 彻底取消“事件空闲期自动停止”机制：不再跟踪 LastEventAt / 不再使用定时器
        // # 保留占位但不再使用（如需可直接删除字段与引用）
        private static readonly bool DisableIdleAutoStop = true;

        // # 持久累加存储：跨战斗的全程聚合
        private static readonly Dictionary<long, PlayerAcc> _players = new();

        // # ★ 全程快照历史（Stop 或 自动停止时都会入栈）
        private static readonly List<FullSessionSnapshot> _sessionHistory = new();
        public static IReadOnlyList<FullSessionSnapshot> SessionHistory => _sessionHistory; // 只读暴露，便于 UI 历史查看

        // # —— 新增：实时队伍 DPS（便于 UI 显示）
        public static double TeamRealtimeDps { get; private set; }     // 基于“有效会话秒数”的实时队伍DPS（只算伤害）

        // # 区域：控制（启动/停止/重置） ------------------------------------------------------
        #region 控制

        /// <summary>
        /// 启动全程记录：
        /// - 若已在记录则直接返回；
        /// - 首次启动设置 StartedAt；
        /// - EndedAt 清空以表示进行中。
        /// </summary>
        public static void Start()
        {
            if (IsRecording) return;

            IsRecording = true;
            if (StartedAt is null) StartedAt = DateTime.Now; // 记录首次启动时间
            EndedAt = null;
        }

        private static readonly object _sync = new();

        /// <summary>
        /// 手动停止全程记录（不清空数据）：
        /// - 如在录制，生成一次会话快照；
        /// - 然后清空“当前会话”累计数据（历史快照保留）；
        /// - 重置时间基，准备新会话。
        /// </summary>
        public static void Stop()
        {
            lock (_sync)
            {
                // 1) 若在录制中，先入快照（保留历史）
                if (IsRecording)
                    StopInternal(auto: false);

                // 2) 清【当前会话】累计（不动历史）
                _players.Clear();
                TeamRealtimeDps = 0;
                _npcs.Clear();          // ★ 全程 NPC 会话数据清空


                // 3) 重置时间基，准备新会话
                StartedAt = null;
                EndedAt = null;
            }
        }

        /// <summary>
        /// 清空快照
        /// </summary>
        public static void ClearSessionHistory()
        {
            lock (_sync)
            {
                _sessionHistory.Clear();
            }
        }



        /// <summary>
        /// 重置当前会话：
        /// - 如有进行中的或已有数据的会话，先保存一次快照；
        /// - 清除当前会话累计与时间基；
        /// - IsRecording 置为 true（进入新会话录制状态）。
        /// </summary>
        public static void Reset(bool preserveHistory = true)
        {

            if (AppConfig.ClearAllDataWhenSwitch && preserveHistory) return;//如果是0，则不清除数据
            lock (_sync)
            {
                // 1) 如有进行中的或已有数据的会话，先入一条快照（不影响历史）
                bool hasData = _players.Count > 0 || StartedAt != null;
                if (hasData)
                {
                    // StopInternal: 固定 EndedAt，生成快照，加入 _sessionHistory
                    StopInternal(auto: false);
                }

                // 2) 清【当前会话】累计（不动历史，除非显式要求清）
                _players.Clear();
                TeamRealtimeDps = 0;
                _npcs.Clear();          // ★

                // 3) 清时间基与录制状态
                StartedAt = DateTime.Now;   // 原来是 null
                EndedAt = null;
                IsRecording = true;

                // 4) 可选：清历史（当前保留）

            }
        }

        /// <summary>
        /// 获取当前会话总时长（TimeSpan）。
        /// - 进行中：Now - StartedAt
        /// - 已停止：EndedAt - StartedAt
        /// </summary>
        public static TimeSpan GetSessionTotalTimeSpan()
        {
            if (StartedAt is null) return TimeSpan.Zero;
            DateTime end = IsRecording ? DateTime.Now : (EndedAt ?? DateTime.Now);
            var duration = end - StartedAt.Value;
            return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        }

        /// <summary>
        /// 获取指定玩家的全程技能统计（基于当前时刻快照）。
        /// 返回三个只读列表（伤害技能/治疗技能/承伤技能）。
        /// </summary>
        public static (IReadOnlyList<SkillSummary> DamageSkills,
                       IReadOnlyList<SkillSummary> HealingSkills,
                       IReadOnlyList<SkillSummary> TakenSkills)
            GetPlayerSkills(long uid)
        {
            var snap = TakeSnapshot();
            if (snap.Players.TryGetValue(uid, out var p))
            {
                return (p.DamageSkills, p.HealingSkills, p.TakenSkills);
            }
            return (Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>());
        }

        #endregion

        // ======================================================================
        // # 分类 7：列表数据获取 / 汇总：用于 UI 绑定与展示
        // ======================================================================

        /// <summary>
        /// 获取“此刻”的全程逐玩家总量清单（默认按总伤害降序）。
        /// includeZero=false 时会过滤掉三项全为 0 的玩家。
        /// Dps/Hps 分母为各自“有效活跃秒数”（无活跃时回退会话时长）。
        /// </summary>
        public static List<FullPlayerTotal> GetPlayersWithTotals(bool includeZero = false)
        {
            var snap = TakeSnapshot();

            // 不再用 snap.Duration 作为统一分母
            var list = new List<FullPlayerTotal>(snap.Players.Count);
            foreach (var kv in snap.Players)
            {
                var p = kv.Value;

                // 各自有效分母（回退到会话时长以兜底）
                var secsDmg = p.ActiveSecondsDamage > 0 ? p.ActiveSecondsDamage : snap.Duration.TotalSeconds;
                var secsHeal = p.ActiveSecondsHealing > 0 ? p.ActiveSecondsHealing : snap.Duration.TotalSeconds;

                // includeZero 过滤逻辑保持不变
                if (!includeZero && p.TotalDamage == 0 && p.TotalHealing == 0 && p.TakenDamage == 0)
                    continue;

                list.Add(new FullPlayerTotal(
                    Uid: p.Uid,
                    Nickname: p.Nickname,
                    CombatPower: p.CombatPower,
                    SubProfession: p.SubProfession,
                    Profession: p.Profession,
                    TotalDamage: p.TotalDamage,
                    TotalHealing: p.TotalHealing,
                    TakenDamage: p.TakenDamage,
                    Dps: secsDmg > 0 ? R2(p.TotalDamage / secsDmg) : 0,
                    Hps: secsHeal > 0 ? R2(p.TotalHealing / secsHeal) : 0
                ));
            }

            return list.OrderByDescending(r => r.TotalDamage).ToList();
        }

        /// <summary>
        /// 查看全程战斗时间（HH:mm:ss，基于 Damage 有效时长最大值）
        /// 用于 UI 文本显示。
        /// </summary>
        public static string GetEffectiveDurationString()
        {
            double activeSeconds = 0;

            // 先在锁内把集合定格为数组，锁外再计算
            PlayerAcc[] playersSnapshot;
            lock (_sync)
            {
                playersSnapshot = _players.Values.ToArray();
            }

            foreach (var p in playersSnapshot)
            {
                if (p.Damage.ActiveSeconds > activeSeconds)
                    activeSeconds = p.Damage.ActiveSeconds;
            }

            var ts = TimeSpan.FromSeconds(activeSeconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        /// <summary>
        /// 便于外层直接 ToArray 绑定（示例对齐 StatisticData 用法）。
        /// </summary>
        public static FullPlayerTotal[] GetPlayersWithTotalsArray(bool includeZero = false)
            => GetPlayersWithTotals(includeZero).ToArray();

        // ======================================================================
        // # 分类 8：内部停止与有效结束时间（快照辅助）
        // ======================================================================

        // # 内部调用
        /// <summary>
        /// 内部停止封装：
        /// - auto=true 表示由空闲超时触发；
        /// - 固化 EndedAt；
        /// - 生成快照并写入历史。
        /// </summary>
        private static void StopInternal(bool auto)
        {
            IsRecording = false;
            EndedAt = DateTime.Now;

            bool hasAnyData;
            PlayerAcc[] playersSnapshot;
            lock (_sync)
            {
                playersSnapshot = _players.Values.ToArray();
            }
            hasAnyData = playersSnapshot.Any(p =>
                p.Damage.Total > 0 || p.Healing.Total > 0 || p.TakenDamage > 0);

            if (!hasAnyData) return;

            var snapshot = TakeSnapshot(); // 见下一节我们也会把 TakeSnapshot 做成快照安全
            if (snapshot.Duration.TotalSeconds >= 1 || hasAnyData)
            {
                lock (_sync) // 写历史列表也建议锁一下
                {
                    _sessionHistory.Add(snapshot);
                }
            }
        }

        // # 内部调用
        /// <summary>
        /// 获取“有效结束时间”：
        /// - 录制中：以 Now 为结束点；
        /// - 已停止：使用 EndedAt。
        /// </summary>
        private static DateTime EffectiveEndTime()
        {
            if (StartedAt is null) return DateTime.Now;
            return IsRecording ? DateTime.Now : (EndedAt ?? DateTime.Now);
        }

        // ======================================================================
        // # 分类 9：写入点（由外部解码/事件管线调用）
        // ======================================================================

        #region 内嵌写入点会调用的 API（只加一行即可）

        /// <summary>
        /// 记录伤害事件：
        /// - 聚合到玩家总伤害与对应技能；
        /// - 更新实时 DPS（基于伤害侧有效秒数）；
        /// - 忽略 0 值。
        /// </summary>
        public static void RecordDamage(
            long uid, long skillId, ulong value, bool isCrit, bool isLucky, ulong hpLessen,
            string nickname, int combatPower, string profession,
            string? damageElement = null, bool isCauseLucky = false, string? subProfession = null // ★ 新增
        )
        {
            if (!IsRecording || value == 0) return;
            var p = GetOrCreate(uid, nickname, combatPower, profession, subProfession);

            // ① 顶层聚合：带 isCauseLucky
            Accumulate(p.Damage, value, isCrit, isLucky, hpLessen, isCauseLucky);

            // ② 逐技能：带 isCauseLucky
            var s = p.DamageSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.DamageSkills[skillId] = new StatAcc());
            Accumulate(s, value, isCrit, isLucky, hpLessen, isCauseLucky);

            // ③ 可选：按元素细分（如果你传了 element）
            if (!string.IsNullOrEmpty(damageElement))
            {
                if (!p.DamageSkillsByElement.TryGetValue(skillId, out var byElem))
                    byElem = p.DamageSkillsByElement[skillId] = new Dictionary<string, StatAcc>();

                if (!byElem.TryGetValue(damageElement, out var es))
                    es = byElem[damageElement] = new StatAcc();

                Accumulate(es, value, isCrit, isLucky, hpLessen, isCauseLucky);
            }

            // —— 更新实时DPS（保持原逻辑）
            UpdateRealtimeDps(p);

        }

        /// <summary>
        /// 记录治疗事件：
        /// - 聚合到玩家总治疗与对应技能；
        /// - 更新“治疗侧”实时指标（RealtimeDpsHealing）；
        /// - 忽略 0 值。
        /// </summary>
        public static void RecordHealing(
            long uid, long skillId, ulong value, bool isCrit, bool isLucky,
            string nickname, int combatPower, string profession,
            string? damageElement = null, bool isCauseLucky = false, ulong targetUuid = 0, string? subProfession = null // ★ 新增
        )

        {
            if (!IsRecording || value == 0) return;
            var p = GetOrCreate(uid, nickname, combatPower, profession, subProfession);

            // 顶层
            Accumulate(p.Healing, value, isCrit, isLucky, 0, isCauseLucky);

            // 逐技能
            var s = p.HealingSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.HealingSkills[skillId] = new StatAcc());
            Accumulate(s, value, isCrit, isLucky, 0, isCauseLucky);

            // 可选：按元素
            if (!string.IsNullOrEmpty(damageElement))
            {
                if (!p.DamageSkillsByElement.TryGetValue(skillId, out var byElem)) // 也可以单独建 HealingSkillsByElement，看你是否要分开
                    byElem = p.DamageSkillsByElement[skillId] = new Dictionary<string, StatAcc>();
                if (!byElem.TryGetValue(damageElement, out var es))
                    es = byElem[damageElement] = new StatAcc();
                Accumulate(es, value, isCrit, isLucky, 0, isCauseLucky);
            }

            // 可选：按目标
            if (targetUuid != 0)
            {
                if (!p.HealingSkillsByTarget.TryGetValue(skillId, out var byTarget))
                    byTarget = p.HealingSkillsByTarget[skillId] = new Dictionary<ulong, StatAcc>();
                if (!byTarget.TryGetValue(targetUuid, out var ts))
                    ts = byTarget[targetUuid] = new StatAcc();
                Accumulate(ts, value, isCrit, isLucky, 0, isCauseLucky);
            }

            UpdateRealtimeDps(p);

        }

        /// <summary>
        /// 记录承伤事件：
        /// - 聚合承伤总量与逐技能承伤（hpLessen 写入）；
        /// - 不计入队伍/玩家DPS（仅用于受击统计与 UI 展示）；
        /// - 忽略 0 值。
        /// </summary>
        public static void RecordTakenDamage(
            long uid, long skillId, ulong value, bool isCrit, bool isLucky, ulong hpLessen,
            string nickname, int combatPower, string profession,
            int damageSource = 0, bool isMiss = false, bool isDead = false // ★ 新增
        )

        {
            if (!IsRecording) return; // 注意：承伤 value 可能为 0（比如被格挡/护盾），不能一刀切 return
            var p = GetOrCreate(uid, nickname, combatPower, profession);

            // 逐技能桶
            var s = p.TakenSkills.TryGetValue(skillId, out var tmp) ? tmp : (p.TakenSkills[skillId] = new StatAcc());

            // ① Miss：只计数，不入数值
            if (isMiss)
            {
                s.CountMiss++;
                return;
            }

            // ② Dead：计数 + 若有数值继续入库
            if (isDead)
                s.CountDead++;

            // hpLessen 兜底
            var lessen = hpLessen > 0 ? hpLessen : value;

            // 玩家总承伤累计真实扣血
            p.TakenDamage += lessen;

            // 有效承伤再记数值（value 可能为 0，按你协议实际情况决定是否过滤）
            if (value > 0 || lessen > 0)
            {
                Accumulate(s, value, isCrit, isLucky, lessen, false /* 因果幸运一般不用于承伤 */);
            }

            // 承伤不进队伍DPS；仅刷新玩家实时显示（保持你原逻辑）
            UpdateRealtimeDps(p, includeHealing: false);

        }

        /// <summary>
        /// 更新玩家与队伍的实时 DPS：
        /// - 玩家侧：Damage/Healing 分别按自身 ActiveSeconds 计算；
        /// - 技能侧：逐技能 RealtimeDps 按各自 ActiveSeconds；
        /// - 队伍侧：总伤害 / 全队最大“伤害活跃秒数”。
        /// </summary>
        private static void UpdateRealtimeDps(PlayerAcc p, bool includeHealing = true)
        {
            // 玩家聚合：按事件有效时长计算
            var dmgSecs = p.Damage.ActiveSeconds;
            p.RealtimeDpsDamage = dmgSecs > 0 ? R2(p.Damage.Total / dmgSecs) : 0;

            if (includeHealing)
            {
                var healSecs = p.Healing.ActiveSeconds;
                p.RealtimeDpsHealing = healSecs > 0 ? R2(p.Healing.Total / healSecs) : 0;
            }

            // 逐技能（可选：也按各自有效时长计算）
            foreach (var kv in p.DamageSkills)
            {
                var s = kv.Value;
                var secs = s.ActiveSeconds;
                s.RealtimeDps = secs > 0 ? R2(s.Total / secs) : 0;
            }
            if (includeHealing)
            {
                foreach (var kv in p.HealingSkills)
                {
                    var s = kv.Value;
                    var secs = s.ActiveSeconds;
                    s.RealtimeDps = secs > 0 ? R2(s.Total / secs) : 0;
                }
            }

            // 队伍实时DPS：用“全队有效时长（取最大）”更贴近“团队在打的时间”
            double teamActiveSecs = 0;
            foreach (var pp in _players.Values)
                teamActiveSecs = Math.Max(teamActiveSecs, pp.Damage.ActiveSeconds);

            ulong teamTotal = 0;
            foreach (var pp in _players.Values) teamTotal += pp.Damage.Total;

            TeamRealtimeDps = teamActiveSecs > 0 ? R2(teamTotal / teamActiveSecs) : 0;
        }

        #endregion


        // ======================================================================
        // # 分类 9.5：NPC 全程统计（会话级，不分技能，只按攻击者聚合）
        // ======================================================================
        #region NPC (Session-wide)

        private sealed class NpcAcc
        {
            public long NpcId { get; }
            public string Name { get; set; } = "未知NPC";
            public StatAcc Taken { get; } = new();                       // NPC 总承伤
            public Dictionary<long, StatAcc> DamageByPlayer { get; } = new(); // 玩家→该NPC 的聚合

            public NpcAcc(long id) { NpcId = id; }
        }

        private static readonly Dictionary<long, NpcAcc> _npcs = new();

        private static NpcAcc GetOrCreateNpc(long npcId, string? name = null)
        {
            if (!_npcs.TryGetValue(npcId, out var n))
            {
                n = new NpcAcc(npcId);
                _npcs[npcId] = n;
            }
            if (!string.IsNullOrWhiteSpace(name)) n.Name = name!;
            return n;
        }

        private static StatAcc GetNpcPlayerAcc(NpcAcc n, long uid)
        {
            if (!n.DamageByPlayer.TryGetValue(uid, out var s))
                s = n.DamageByPlayer[uid] = new StatAcc();
            return s;
        }

        /// <summary>
        /// 【全程】记录一条“玩家 → NPC”的伤害（不分技能）：
        /// - value/hpLessen 按你现有口径；
        /// - Miss 仅计次数（在玩家→NPC桶里），不进数值；
        /// - Dead 仅计次（NPC承伤侧 Taken.CountDead++）。
        /// </summary>
        public static void RecordNpcTakenDamage(
            long npcId,
            long attackerUid,
            ulong value,
            bool isCrit,
            bool isLucky,
            ulong hpLessen = 0,
            bool isMiss = false,
            bool isDead = false,
            string? npcName = null
        )
        {
            if (!IsRecording) return;

            var n = GetOrCreateNpc(npcId, npcName);
            var lessen = hpLessen > 0 ? hpLessen : value;

            if (isMiss)
            {
                // 只在“攻击者→该NPC”的桶里记 Miss 次数
                GetNpcPlayerAcc(n, attackerUid).CountMiss++;
                return;
            }
            if (isDead) n.Taken.CountDead++;

            // NPC 承伤聚合
            Accumulate(n.Taken, value, isCrit, isLucky, lessen, false);

            // 攻击者对该 NPC 的聚合（可计算该玩家对该NPC的专属DPS）
            var ps = GetNpcPlayerAcc(n, attackerUid);
            Accumulate(ps, value, isCrit, isLucky, lessen, false);
        }

        /// <summary>设置 NPC 名称（可选）。</summary>
        public static void SetNpcName(long npcId, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            GetOrCreateNpc(npcId).Name = name;
        }

        /// <summary>获取当前会话中出现过的 NPC 列表（ID 集）。</summary>
        public static IReadOnlyList<long> GetAllNpcIds() => _npcs.Keys.ToList();

        /// <summary>
        /// 读取 NPC 全程承伤概览：总量/每秒/最大最小单次/最后时间。
        /// PerSec 用 NPC 自身的 ActiveSeconds（由 Accumulate 推进）。
        /// </summary>
        public static (ulong Total, double PerSec, ulong MaxHit, ulong MinHit, DateTime? LastTime, string Name)
            GetNpcOverview(long npcId)
        {
            if (!_npcs.TryGetValue(npcId, out var n)) return (0, 0, 0, 0, null, "未知NPC");
            var s = n.Taken;
            var secs = s.ActiveSeconds > 0 ? s.ActiveSeconds : 0;
            var per = secs > 0 ? R2(s.Total / secs) : 0;
            var min = s.MinSingleHit; // 0=无记录
            return (s.Total, per, s.MaxSingleHit, min, s.LastAt, n.Name);
        }

        /// <summary>
        /// 对指定 NPC 的“对其伤害排名”（按总伤害降序）。
        /// 同时返回：玩家全程 DPS（GetPlayerDps）与对该NPC“专属DPS”（玩家→该NPC桶的ActiveSeconds）。
        /// </summary>
        public static List<(long Uid, string Nickname, int CombatPower, string Profession,
                           ulong DamageToNpc, double PlayerDps, double NpcOnlyDps)>
            GetNpcTopAttackers(long npcId, int topN = 20)
        {
            if (!_npcs.TryGetValue(npcId, out var n) || n.DamageByPlayer.Count == 0) return new();

            // 拍快照，避免遍历期间集合被改动
            var arr = n.DamageByPlayer.ToArray();

            // 先拿一份玩家基础信息快照
            Dictionary<long, (string Nick, int Power, string Prof)> baseInfo;
            lock (_sync)
            {
                baseInfo = _players.ToDictionary(
                    kv => kv.Key,
                    kv => (kv.Value.Nickname, kv.Value.CombatPower, kv.Value.Profession)
                );
            }

            return arr
                .OrderByDescending(kv => kv.Value.Total)
                .Take(topN)
                .Select(kv =>
                {
                    var uid = kv.Key;
                    var s = kv.Value;
                    var nick = baseInfo.TryGetValue(uid, out var bi) ? bi.Nick : "未知";
                    var power = baseInfo.TryGetValue(uid, out bi) ? bi.Power : 0;
                    var prof = baseInfo.TryGetValue(uid, out bi) ? bi.Prof : "未知";

                    var playerDps = GetPlayerDps(uid); // 全程DPS（伤害侧）
                    var npcOnlyDps = s.ActiveSeconds > 0 ? R2(s.Total / s.ActiveSeconds) : 0;

                    return (uid, nick, power, prof, s.Total, playerDps, npcOnlyDps);
                })
                .ToList();
        }

        #endregion


        // ======================================================================
        // # 分类 10：快照 & 秒伤（对外接口 / 快照产出）
        // ======================================================================

        /// <summary>
        /// 生成一次全程快照（当前时刻）：
        /// - 结束时间使用 EffectiveEndTime()；
        /// - 汇总队伍总伤害/治疗/承伤；
        /// - 逐玩家构建 SnapshotPlayer（含按伤害/治疗/承伤降序的技能汇总）。
        /// </summary>
        public static FullSessionSnapshot TakeSnapshot()
        {
            // ========= 1) 锁内：只做取值与拷贝，避免在锁内做任何 LINQ/重计算 =========
            DateTime end, start;
            PlayerAcc[] playersSnap;
            NpcAcc[] npcSnap;

            lock (_sync)
            {
                end = EffectiveEndTime();                  // 结束时刻（进行中=Now，停止后=EndedAt）
                start = StartedAt ?? end;                  // 若未启动则视为 0 时长
                playersSnap = _players.Values.ToArray();   // ★ 关键：把可变集合定格为数组
                npcSnap = _npcs.Values.ToArray();  // ★ 定格 NPC 集合
            }

            // ========= 2) 锁外：基于快照做所有 LINQ/聚合，完全不触碰 _players =========
            var duration = end - start;
            if (duration < TimeSpan.Zero) duration = TimeSpan.Zero;

            ulong teamDmg = 0, teamHeal = 0, teamTaken = 0;

            // 注意：这里是“快照字典”，跟 _players 无关
            var players = new Dictionary<long, SnapshotPlayer>(playersSnap.Length);

            foreach (var p in playersSnap)
            {
                // 顶层合计（基于快照）
                teamDmg += p.Damage.Total;
                teamHeal += p.Healing.Total;
                teamTaken += p.TakenDamage;

                // —— 逐技能：对内部字典也先定格为数组，再做 Select/OrderBy —— //
                var damageSkillsArr = p.DamageSkills.Count > 0 ? p.DamageSkills.ToArray() : Array.Empty<KeyValuePair<long, StatAcc>>();
                var healingSkillsArr = p.HealingSkills.Count > 0 ? p.HealingSkills.ToArray() : Array.Empty<KeyValuePair<long, StatAcc>>();
                var takenSkillsArr = p.TakenSkills.Count > 0 ? p.TakenSkills.ToArray() : Array.Empty<KeyValuePair<long, StatAcc>>();

                var damageSkills = damageSkillsArr
                    .Select(kv => ToSkillSummary(kv.Key, kv.Value, duration))
                    .OrderByDescending(x => x.Total)
                    .ToList();

                var healingSkills = healingSkillsArr
                    .Select(kv => ToSkillSummary(kv.Key, kv.Value, duration))
                    .OrderByDescending(x => x.Total)
                    .ToList();

                var takenSkills = takenSkillsArr
                    .Select(kv => ToSkillSummary(kv.Key, kv.Value, duration))
                    .OrderByDescending(x => x.Total)
                    .ToList();

                // —— 逐技能实时峰值（需要 p.*Skills 的 RealtimeDps；同样基于快照数组计算）——
                double dmgRealtimeMax = damageSkillsArr.Length > 0
                    ? damageSkillsArr.Select(s => s.Value.RealtimeDps).DefaultIfEmpty(0).Max()
                    : 0;

                double healRealtimeMax = healingSkillsArr.Length > 0
                    ? healingSkillsArr.Select(s => s.Value.RealtimeDps).DefaultIfEmpty(0).Max()
                    : 0;

                players[p.Uid] = new SnapshotPlayer
                {
                    Uid = p.Uid,
                    Nickname = p.Nickname,
                    CombatPower = p.CombatPower,
                    Profession = p.Profession,
                    SubProfession = p.SubProfession,

                    // 顶层聚合
                    TotalDamage = p.Damage.Total,
                    TotalHealing = p.Healing.Total,
                    TakenDamage = p.TakenDamage,

                    // 按各自“有效活跃秒数”计算的全程每秒（保持你原口径）
                    TotalDps = p.Damage.ActiveSeconds > 0 ? R2(p.Damage.Total / p.Damage.ActiveSeconds) : 0,
                    TotalHps = p.Healing.ActiveSeconds > 0 ? R2(p.Healing.Total / p.Healing.ActiveSeconds) : 0,

                    LastRecordTime = null, // 如需，可在写入路径维护最后时间
                    ActiveSecondsDamage = p.Damage.ActiveSeconds,
                    ActiveSecondsHealing = p.Healing.ActiveSeconds,

                    // 逐技能列表
                    DamageSkills = damageSkills,
                    HealingSkills = healingSkills,
                    TakenSkills = takenSkills,

                    // 实时指标（来自 FullRecord 的累计实时指标/逐技能窗口）
                    RealtimeDps = (ulong)Math.Round(p.RealtimeDpsDamage),
                    HealingRealtime = (ulong)Math.Round(p.RealtimeDpsHealing),
                    RealtimeDpsMax = (ulong)Math.Round(dmgRealtimeMax),
                    HealingRealtimeMax = (ulong)Math.Round(healRealtimeMax),

                    // —— 伤害侧细分与比率 —— 
                    CriticalDamage = p.Damage.Critical,
                    LuckyDamage = p.Damage.Lucky + p.Damage.CritLucky, // ★ 合并
                    CritLuckyDamage = p.Damage.CritLucky,
                    MaxSingleHit = p.Damage.MaxSingleHit,
                    CritRate = p.Damage.CountTotal > 0 ? R2((double)p.Damage.CountCritical * 100.0 / p.Damage.CountTotal) : 0.0,
                    LuckyRate = p.Damage.CountTotal > 0 ? R2((double)p.Damage.CountLucky * 100.0 / p.Damage.CountTotal) : 0.0,
                };
            }

            // —— NPC 会话快照 —— //
            var nps = new Dictionary<long, FullSessionNpc>(npcSnap.Length);
            foreach (var n in npcSnap)
            {
                var s = n.Taken;
                var secs = s.ActiveSeconds > 0 ? s.ActiveSeconds : 0;
                var per = secs > 0 ? R2(s.Total / secs) : 0;
                var min = s.MinSingleHit;

                // Top 攻击者（只取前 10，可调）
                var top = n.DamageByPlayer
                    .OrderByDescending(kv => kv.Value.Total)
                    .Take(10)
                    .Select(kv =>
                    {
                        var uid = kv.Key;
                        var ns = kv.Value;
                        var npcOnlyDps = ns.ActiveSeconds > 0 ? R2(ns.Total / ns.ActiveSeconds) : 0;

                        // 拿昵称：用 playersSnap 的快照映射一次
                        var p = playersSnap.FirstOrDefault(pp => pp.Uid == uid);
                        var nick = p != null ? p.Nickname : "未知";

                        return (uid, nick, ns.Total, npcOnlyDps);
                    })
                    .ToList();

                nps[n.NpcId] = new FullSessionNpc
                {
                    NpcId = n.NpcId,
                    Name = n.Name,
                    TotalTaken = s.Total,
                    TakenPerSec = per,
                    MaxSingleHit = s.MaxSingleHit,
                    MinSingleHit = min,
                    TopAttackers = top
                };
            }
            // ========= 3) 组装快照对象（仅使用本地快照数据） =========
            return new FullSessionSnapshot
            {
                StartedAt = start,
                EndedAt = end,
                Duration = duration,
                TeamTotalDamage = teamDmg,
                TeamTotalHealing = teamHeal,
                TeamTotalTakenDamage = teamTaken,
                Players = players,
                Npcs = nps              // ★ 新增

            };
        }


        /// <summary>
        /// 获取队伍当前全程 DPS（只计算伤害）。
        /// - 取全队最大 Damage.ActiveSeconds 为分母；
        /// - 汇总总伤害为分子。
        /// </summary>
        public static double GetTeamDps()
        {
            lock (_sync)
            {
                double teamActiveSecs = 0;
                foreach (var p in _players.Values)
                    if (p.Damage.ActiveSeconds > teamActiveSecs)
                        teamActiveSecs = p.Damage.ActiveSeconds;

                if (teamActiveSecs <= 0) return 0.0;

                ulong total = 0;
                foreach (var p in _players.Values) total += p.Damage.Total;

                return R2(total / teamActiveSecs);
            }
        }

        /// <summary>
        /// 获取指定玩家当前全程 DPS（只计算伤害）。
        /// - 分母：整个会话秒数（SessionSeconds）；
        /// - 若尚未开始或分母为 0，返回 0。
        /// </summary>
        public static double GetPlayerDps(long uid)
        {
            var secs = SessionSeconds();
            if (secs <= 0) return 0;
            return _players.TryGetValue(uid, out var p) ? R2(p.Damage.Total / secs) : 0;
        }

        // using StarResonanceDpsAnalysis.Plugin.DamageStatistics; // 确保命名空间可见

        public static (IReadOnlyList<SkillSummary> DamageSkills,
                      IReadOnlyList<SkillSummary> HealingSkills,
                      IReadOnlyList<SkillSummary> TakenSkills)
        GetPlayerSkillsBySnapshotTimeEx(DateTime snapshotStartTime, long uid, double toleranceSeconds = 2.0)
        {
            // —— 先查【全程历史】——
            var session = SessionHistory?.FirstOrDefault(s =>
                s.StartedAt == snapshotStartTime ||
                Math.Abs((s.StartedAt - snapshotStartTime).TotalSeconds) <= toleranceSeconds);

            if (session != null && session.Players != null &&
                session.Players.TryGetValue(uid, out var sp1))
            {
                return (sp1.DamageSkills ?? new List<SkillSummary>(),
                        sp1.HealingSkills ?? new List<SkillSummary>(),
                        sp1.TakenSkills ?? new List<SkillSummary>());
            }

            // —— 再查【单场历史】——
            var battles = StatisticData._manager.History;
            var battle = battles?.FirstOrDefault(s =>
                s.StartedAt == snapshotStartTime ||
                Math.Abs((s.StartedAt - snapshotStartTime).TotalSeconds) <= toleranceSeconds);

            if (battle != null && battle.Players != null &&
                battle.Players.TryGetValue(uid, out var sp2))
            {
                return (sp2.DamageSkills ?? new List<SkillSummary>(),
                        sp2.HealingSkills ?? new List<SkillSummary>(),
                        sp2.TakenSkills ?? new List<SkillSummary>());
            }

            // —— 都没找到：返回空 —— 
            return (Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>());
        }



        // ======================================================================
        // # 分类 11：快照时间检索（历史查询）
        // ======================================================================

        #region 查询（按快照时间检索）
        /// <summary>
        /// 按快照的开始时间获取该快照中所有玩家数据。
        /// - 如果找不到对应快照，返回 null。
        /// </summary>
        public static IReadOnlyDictionary<long, SnapshotPlayer>? GetAllPlayersDataBySnapshotTime(DateTime snapshotStartTime)
        {
            var snapshot = SessionHistory.FirstOrDefault(s => s.StartedAt == snapshotStartTime);
            return snapshot?.Players;
        }

        /// <summary>
        /// 按快照的开始时间和玩家 UID 获取该玩家的技能数据。
        /// - 返回 (伤害技能, 治疗技能)，若找不到则返回两个空列表。
        /// </summary>
        public static (IReadOnlyList<SkillSummary> DamageSkills, IReadOnlyList<SkillSummary> HealingSkills)
            GetPlayerSkillsBySnapshotTime(DateTime snapshotStartTime, long uid)
        {
            var snapshot = SessionHistory.FirstOrDefault(s => s.StartedAt == snapshotStartTime);
            if (snapshot != null && snapshot.Players.TryGetValue(uid, out var player))
            {
                return (player.DamageSkills, player.HealingSkills);
            }
            return (Array.Empty<SkillSummary>(), Array.Empty<SkillSummary>());
        }
        #endregion

        // ======================================================================
        // # 分类 12：内部实现工具（时长/取或建/累加/投影）
        // ======================================================================

        #region 内部实现

        /// <summary>
        /// 计算会话“有效秒数”：
        /// - 若尚未开始返回 0；
        /// - 进行中：StartedAt → Now；
        /// - 已停止：StartedAt → EndedAt。
        /// </summary>
        private static double SessionSeconds()
        {
            if (StartedAt is null) return 0;

            DateTime end = IsRecording
                ? DateTime.Now           // 进行中：用 Now 做临时结束点
                : (EndedAt ?? DateTime.Now);

            var sec = (end - StartedAt.Value).TotalSeconds;
            return sec > 0 ? sec : 0;
        }

        /// <summary>
        /// 获取或创建玩家累计器，并同步基础信息（昵称/战力/职业以最近一次为准）。
        /// </summary>
        private static PlayerAcc GetOrCreate(long uid, string nickname, int combatPower, string profession, string? subProfession = null)
        {
            if (!_players.TryGetValue(uid, out var p))
            {
                p = new PlayerAcc(uid);
                _players[uid] = p;
            }
            // 以最近一次为准同步基础信息
            p.Nickname = nickname;
            p.CombatPower = combatPower;
            p.Profession = profession;
            if (subProfession != null)
            {
                p.SubProfession = subProfession;

            }

            return p;
        }

        /// <summary>
        /// 将一次数值累加到统计器：
        /// - 区分普通/暴击/幸运/暴击+幸运；
        /// - 维护总和、hpLessen、次数与最大/最小单次值；
        /// - 通过 FirstAt/LastAt 与时间差累加 ActiveSeconds（含空档封顶）。
        /// </summary>
        private static void Accumulate(
            StatAcc acc, ulong value, bool isCrit, bool isLucky, ulong hpLessen,
            bool isCauseLucky = false // ★ 新增：是否因果幸运
        )
        {
            // 原有四象限累计
            if (isCrit && isLucky) acc.CritLucky += value;
            else if (isCrit) acc.Critical += value;
            else if (isLucky) acc.Lucky += value;
            else acc.Normal += value;

            acc.Total += value;
            acc.HpLessen += hpLessen;

            // 次数
            if (isCrit) acc.CountCritical++;
            if (isLucky) acc.CountLucky++;
            if (!isCrit && !isLucky) acc.CountNormal++;
            acc.CountTotal++;

            // ★ 新增：因果幸运
            if (isLucky && isCauseLucky)
            {
                acc.CauseLucky += value;
                acc.CountCauseLucky++;
            }

            // 极值...
            if (value > 0)
            {
                if (value > acc.MaxSingleHit) acc.MaxSingleHit = value;
                if (acc.MinSingleHit == 0 || value < acc.MinSingleHit) acc.MinSingleHit = value;
            }

            // 时序/活跃时长（保持你原逻辑不变）
            var now = DateTime.Now;
            if (acc.FirstAt is null) { acc.FirstAt = now; }
            else
            {
                const double GAP_CAP_SECONDS = 3.0;
                var gap = (now - (acc.LastAt ?? acc.FirstAt.Value)).TotalSeconds;
                if (gap < 0) gap = 0;
                if (gap > GAP_CAP_SECONDS) gap = GAP_CAP_SECONDS;
                acc.ActiveSeconds += gap;
            }
            acc.LastAt = now;
        }


        /// <summary>
        /// 将内部技能统计转为快照中的技能汇总项（含DPS、命中均值、暴击/幸运率等）。
        /// - Realtime 字段快照中不赋值（保持 0）。
        /// </summary>
        private static SkillSummary ToSkillSummary(long skillId, StatAcc s, TimeSpan duration)
        {
            var meta = SkillBook.Get(skillId);
            return new SkillSummary
            {
                SkillId = skillId,
                SkillName = meta.Name,
                Total = s.Total,
                HitCount = s.CountTotal,
                AvgPerHit = s.CountTotal > 0 ? R2((double)s.Total / s.CountTotal) : 0.0,
                CritRate = s.CountTotal > 0 ? R2((double)s.CountCritical * 100.0 / s.CountTotal) : 0.0,
                LuckyRate = s.CountTotal > 0 ? R2((double)s.CountLucky * 100.0 / s.CountTotal) : 0.0,
                MaxSingleHit = s.MaxSingleHit,
                MinSingleHit = s.MinSingleHit,
                RealtimeValue = 0,          // 快照为历史静态值，这里不赋实时
                RealtimeMax = 0,            // 同上
                TotalDps = s.ActiveSeconds > 0 ? R2(s.Total / s.ActiveSeconds) : 0,
                LastTime = null,            // 可按需扩展：记录技能最后出现时间
                ShareOfTotal = 0,            // 可按需扩展：占比（由外部渲染时计算亦可）
                LuckyDamage = s.Lucky + s.CritLucky,
                CritLuckyDamage = s.CritLucky,
                CauseLuckyDamage = s.CauseLucky, // StatAcc 已含该字段
                CountLucky = s.CountLucky,

            };
        }

        // ===== 内部数据结构 =====

        /// <summary>
        /// 玩家聚合器（会话内持续累加）。
        /// - 含基础信息（昵称/战力/职业）与三类统计（伤害/治疗/承伤）；
        /// - DamageSkills/HealingSkills/TakenSkills 逐技能累加；
        /// - RealtimeDps* 用于 UI 实时显示。
        /// </summary>
        private sealed class PlayerAcc
        {
            public long Uid { get; }
            public string Nickname { get; set; } = "未知";
            public int CombatPower { get; set; }
            public string Profession { get; set; } = "未知";
            public string? SubProfession { get; set; }//子职业
            public StatAcc Damage { get; } = new();
            public StatAcc Healing { get; } = new();
            public ulong TakenDamage { get; set; }

            public Dictionary<long, StatAcc> DamageSkills { get; } = new();
            public Dictionary<long, StatAcc> HealingSkills { get; } = new();
            public Dictionary<long, StatAcc> TakenSkills { get; } = new();

            // —— 新增：实时总DPS（聚合）
            public double RealtimeDpsDamage { get; set; }
            public double RealtimeDpsHealing { get; set; }

            public PlayerAcc(long uid) => Uid = uid;

            // ★ 新增：可选的细分维度
            public Dictionary<long, Dictionary<string, StatAcc>> DamageSkillsByElement { get; } = new();
            public Dictionary<long, Dictionary<ulong, StatAcc>> HealingSkillsByTarget { get; } = new();


        }

        /// <summary>
        /// 统计累加器（通用结构）：
        /// - 四象限数值：Normal/Critical/Lucky/CritLucky；
        /// - 计数：Count*；
        /// - 极值：MaxSingleHit/MinSingleHit；
        /// - 时序：FirstAt/LastAt/ActiveSeconds；
        /// - RealtimeDps：便于 UI 实时显示（逐技能/逐类）。
        /// </summary>
        private sealed class StatAcc
        {
            public ulong Normal, Critical, Lucky, CritLucky, HpLessen, Total;
            public ulong MaxSingleHit, MinSingleHit; // Min=0 表示未赋值
            public int CountNormal, CountCritical, CountLucky, CountTotal;
            public DateTime? FirstAt;     // 第一条记录时间
            public DateTime? LastAt;      // 最近一条记录时间
            public double ActiveSeconds;  // 事件间隔累加（单位：秒）
            // —— 新增：实时DPS（逐技能/逐类）
            public double RealtimeDps { get; set; }

            // ★★★ 新增：全程记录扩展字段
            public ulong CauseLucky;      // 因果幸运累计数值
            public int CountCauseLucky; // 因果幸运次数
            public int CountMiss;       // Miss 次数（多用于承伤）
            public int CountDead;       // 击杀次数（承伤）
        }
        #endregion
    }

    // ======================================================================
    // # 分类 13：快照结构定义（跨战斗的全程快照）
    // ======================================================================

    /// <summary>全程快照结构（与 BattleSnapshot 类似，但跨战斗）。用于历史/统计展示。</summary>
    public sealed class FullSessionSnapshot
    {
        public DateTime StartedAt { get; init; }          // 快照起始时间
        public DateTime EndedAt { get; init; }            // 快照结束时间
        public TimeSpan Duration { get; init; }           // 持续时长
        public ulong TeamTotalDamage { get; init; }       // 队伍总伤害
        public ulong TeamTotalHealing { get; init; }      // 队伍总治疗
        public Dictionary<long, SnapshotPlayer> Players { get; init; } = new(); // 逐玩家详情
        public ulong TeamTotalTakenDamage { get; init; }  // ★ 队伍总承伤

        // ★ 新增：会话快照里的 NPC 数据
        public Dictionary<long, FullSessionNpc> Npcs { get; init; } = new();
    }

    /// <summary>全程快照里的 NPC 视图（不分技能）。</summary>
    public sealed class FullSessionNpc
    {
        public long NpcId { get; init; }
        public string Name { get; init; } = "未知NPC";
        public ulong TotalTaken { get; init; }
        public double TakenPerSec { get; init; }
        public ulong MaxSingleHit { get; init; }
        public ulong MinSingleHit { get; init; }

        // Top 攻击者（只放最关键的字段；需要更多可自行扩展）
        public List<(long Uid, string Nickname, ulong DamageToNpc, double NpcOnlyDps)> TopAttackers { get; init; } = new();
    }
}
