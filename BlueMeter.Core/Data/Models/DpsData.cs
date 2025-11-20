using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMeter.Core.Data.Models
{
    public class DpsData
    {
        /// <summary>
        /// 玩家UID
        /// </summary>
        public long UID { get; internal set; }
        /// <summary>
        /// 统计开始的时间戳 (Ticks)
        /// </summary>
        public long? StartLoggedTick { get; internal set; }
        /// <summary>
        /// 最后一次统计的时间戳 (Ticks)
        /// </summary>
        public long LastLoggedTick { get; internal set; } // TODO: 改成ulong
        /// <summary>
        /// Active combat time in ticks (excluding downtime >1s)
        /// Used for accurate DPS calculation without pauses
        /// </summary>
        public long ActiveCombatTicks { get; internal set; }
        /// <summary>
        /// 统计的总伤害
        /// </summary>
        public long TotalAttackDamage { get; internal set; }// TODO: 改成ulong
        /// <summary>
        /// 统计的总承受伤害
        /// </summary>
        public long TotalTakenDamage { get; internal set; }// TODO: 改成ulong
        /// <summary>
        /// 统计的总治疗量
        /// </summary>
        public long TotalHeal { get; internal set; }// TODO: 改成ulong
        /// <summary>
        /// 是否为NPC数据
        /// </summary>
        public bool IsNpcData { get; internal set; } = false;

        // ==================== Real-Time Windowing for Charts ====================
        // 1-second sliding window for instant DPS/HPS calculation (inspired by StarResonanceDps)

        /// <summary>
        /// Sliding window for damage events (1-second window)
        /// </summary>
        private readonly List<(DateTime timestamp, long damage)> _damageWindow = new();

        /// <summary>
        /// Sliding window for heal events (1-second window)
        /// </summary>
        private readonly List<(DateTime timestamp, long heal)> _healWindow = new();

        /// <summary>
        /// Lock for thread-safe window operations
        /// </summary>
        private readonly object _windowLock = new();

        /// <summary>
        /// Instant DPS calculated from 1-second sliding window
        /// </summary>
        public long InstantDps { get; private set; }

        /// <summary>
        /// Instant HPS calculated from 1-second sliding window
        /// </summary>
        public long InstantHps { get; private set; }
        /// <summary>
        /// 战斗日志列表
        /// </summary>
        //internal List<BattleLog> BattleLogs { get; } = new(16384);
        /// <summary>
        /// 只读战斗日志列表
        /// </summary>
        //public IReadOnlyList<BattleLog> ReadOnlyBattleLogs { get => BattleLogs.AsReadOnly(); }
        /// <summary>
        /// 技能统计数据字典
        /// </summary>
        internal Dictionary<long, SkillData> SkillDic { get; } = [];
        private readonly object _skillLock = new();

        /// <summary>
        /// 只读技能统计数据字典
        /// </summary>
        public ReadOnlyDictionary<long, SkillData> ReadOnlySkillDatas
        {
            get
            {
                lock (_skillLock)
                {
                    var copy = SkillDic.ToDictionary(static pair => pair.Key,
                        static pair => CloneSkillData(pair.Value));
                    return new ReadOnlyDictionary<long, SkillData>(copy);
                }
            }
        }

        /// <summary>
        /// 只读技能统计数据列表
        /// </summary>
        public IReadOnlyList<SkillData> ReadOnlySkillDataList
        {
            get
            {
                lock (_skillLock)
                {
                    return SkillDic.Values.Select(CloneSkillData).ToList();
                }
            }
        }
        /// <summary>
        /// 获取或创建技能统计数据
        /// </summary>
        /// <param name="skillId">技能UID</param>
        /// <returns></returns>

        public void UpdateSkillData(long skillId, Action<SkillData> updater)
        {
            if (updater is null) throw new ArgumentNullException(nameof(updater));

            lock (_skillLock)
            {
                if (!SkillDic.TryGetValue(skillId, out var skillData))
                {
                    skillData = new SkillData
                    {
                        SkillId = skillId
                    };
                    SkillDic[skillId] = skillData;
                }

                updater(skillData);
            }
        }

        private static SkillData CloneSkillData(SkillData source)
        {
            return new SkillData
            {
                SkillId = source.SkillId,
                TotalValue = source.TotalValue,
                UseTimes = source.UseTimes,
                CritTimes = source.CritTimes,
                LuckyTimes = source.LuckyTimes,
                MinDamage = source.MinDamage,
                MaxDamage = source.MaxDamage,
                HighestCrit = source.HighestCrit
            };
        }

        // ==================== Real-Time Windowing Methods ====================

        /// <summary>
        /// Add damage to sliding window and update instant DPS
        /// Called on every damage event for real-time chart data
        /// </summary>
        /// <param name="damage">Damage amount to add</param>
        public void AddDamageToWindow(long damage)
        {
            lock (_windowLock)
            {
                _damageWindow.Add((DateTime.UtcNow, damage));
                UpdateRealtimeStats();
            }
        }

        /// <summary>
        /// Add heal to sliding window and update instant HPS
        /// Called on every heal event for real-time chart data
        /// </summary>
        /// <param name="heal">Heal amount to add</param>
        public void AddHealToWindow(long heal)
        {
            lock (_windowLock)
            {
                _healWindow.Add((DateTime.UtcNow, heal));
                UpdateRealtimeStats();
            }
        }

        /// <summary>
        /// Update instant DPS/HPS from sliding windows
        /// Removes entries older than 1 second and recalculates instant values
        /// </summary>
        private void UpdateRealtimeStats()
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-1);

            // Remove old entries (older than 1 second)
            _damageWindow.RemoveAll(x => x.timestamp < cutoff);
            _healWindow.RemoveAll(x => x.timestamp < cutoff);

            // Calculate instant values (sum of last 1 second)
            InstantDps = _damageWindow.Sum(x => x.damage);
            InstantHps = _healWindow.Sum(x => x.heal);
        }

        /// <summary>
        /// Clear sliding windows (called when section ends or resets)
        /// </summary>
        public void ClearWindows()
        {
            lock (_windowLock)
            {
                _damageWindow.Clear();
                _healWindow.Clear();
                InstantDps = 0;
                InstantHps = 0;
            }
        }
    }
}
