using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

using StarResonanceDpsAnalysis.WinForm.Forms;
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.WinForm.Plugin;

namespace StarResonanceDpsAnalysis.WinForm.Core
{
    internal static class SkillDiaryGate
    {
        private class PendingEntry
        {
            public long StartTick;      // ★ 新增：窗口开始时刻
            public long LastTick;       // 上次记录的时间戳
            public int Count;           // 当前窗口累计次数
            public ulong TotalDamage;   // 当前窗口累计伤害
            public int CritCount;       // ★ 新增：窗口内累计暴击次数
            public int LuckyCount;      // ★ 新增：窗口内累计幸运次数
        }

        // 只按 (玩家, 技能ID) 合并，不处理变体
        private static readonly ConcurrentDictionary<(long Uid, long SkillId, bool Treat), PendingEntry> _pending = new();

        // 窗口阈值（ms）：窗口内只累计，不输出；出现 >WindowMs 的间隔才“自然断开”
        private const int WindowMs = 700;

        // 最长憋单段（ms）：即使一直“不断段”，达到这个时长也强制写出
        private const int MaxHoldMs = 1000;

        private static double TicksPerMs => Stopwatch.Frequency / 1000.0;

        /// <summary>
        /// 记录一次“技能命中”，用于技能日记合并。
        /// 返回 (shouldWrite, countToOutput, damageToOutput) 表示是否需要将“上一段”写出。
        /// </summary>
        public static (bool shouldWrite, int countToOutput, ulong damageToOutput, int critToOutput, int luckyToOutput)
      Register(long uid, long skillId, ulong damage, bool isCrit, bool isLucky, bool treat)
        {
            long now = Stopwatch.GetTimestamp();
            long windowTicks = (long)(WindowMs * TicksPerMs);
            long holdTicks = (long)(MaxHoldMs * TicksPerMs);
            var key = (uid, skillId, treat);   // ★ 关键

            var entry = _pending.GetOrAdd(key, _ => new PendingEntry
            {
                StartTick = now,
                LastTick = now,
                Count = 0,
                TotalDamage = 0,
                CritCount = 0,
                LuckyCount = 0
            });

            lock (entry)
            {
                long dtSinceLast = now - entry.LastTick;
                long dtSinceStart = now - entry.StartTick;

                if (entry.Count == 0)
                {
                    entry.Count = 1;
                    entry.TotalDamage = damage;
                    entry.CritCount = isCrit ? 1 : 0;
                    entry.LuckyCount = isLucky ? 1 : 0;
                    entry.StartTick = now;
                    entry.LastTick = now;
                    return (false, 0, 0, 0, 0);
                }

                if (dtSinceLast <= windowTicks && dtSinceStart < holdTicks)
                {
                    entry.Count++;
                    entry.TotalDamage += damage;
                    if (isCrit) entry.CritCount++;
                    if (isLucky) entry.LuckyCount++;
                    entry.LastTick = now;
                    return (false, 0, 0, 0, 0);
                }

                // 需要写出上一段
                int outCount = entry.Count;
                ulong outDmg = entry.TotalDamage;
                int outCrit = entry.CritCount;
                int outLucky = entry.LuckyCount;

                // 当前一下作为新段首
                entry.Count = 1;
                entry.TotalDamage = damage;
                entry.CritCount = isCrit ? 1 : 0;
                entry.LuckyCount = isLucky ? 1 : 0;
                entry.StartTick = now;
                entry.LastTick = now;

                return (true, outCount, outDmg, outCrit, outLucky);
            }
        }


        /// <summary>
        /// 在“命中事件”里调用：按 (uid, skillId) 合并多段，并在窗口断开时写日记。
        /// - 仅当 skillDiary 窗口存在、且是“我自己”的事件（uid == AppConfig.Uid）才记录；
        /// - 使用 SkillDiaryGate.Register 进行窗口合并；
        /// - 窗口断开时写出一行（单段或“技能 * 次数”），带上伤害信息。
        /// </summary>
        /// <param name="uid">本次命中所属玩家 UID</param>
        /// <param name="skillId">技能 ID（不做变体合并）</param>
        /// <param name="damage">本次命中的伤害值（用于累计到窗口总伤害，以及单段时显示）</param>
        /// <param name="iscrit">是否暴击</param>
        /// <param name="isLucky">是否幸运</param>
        /// <param name="treat">是否疗伤</param>
        public static void OnHit(long uid, long skillId, ulong damage, bool iscrit, bool isLucky, bool treat = false)
        {
            //// 1) 只处理本人的
            //if (uid != AppConfig.Uid) return;

            //// 2) 拿一个局部快照，避免检查之后被别的线程置空
            //var form = FormManager.skillDiary;
            //if (form == null || form.IsDisposed || !form.IsHandleCreated) return;

            //var (shouldWrite, count, totalDamage, critCount, luckyCount) =
            //    SkillDiaryGate.Register(uid, skillId, damage, iscrit, isLucky, treat); // ★ 传 treat

            //if (!shouldWrite || count <= 0) return;

            //var duration = StatisticData._manager.GetFormattedCombatDuration();
            //if (FormManager.showTotal) duration = FullRecord.GetEffectiveDurationString();

            //var name = EmbeddedSkillConfig.GetName(skillId.ToString());

            //string line;
            //if (count > 1)
            //{
            //    // 多段
            //    var parts = new List<string>
            //{
            //    $"{name}",
            //    $"{(treat ? "治疗" : "伤害")}:{totalDamage}",
            //    $"释放次数:{count}"
            //};
            //    if (critCount > 0) parts.Add($"暴击:{critCount}");
            //    if (luckyCount > 0) parts.Add($"幸运:{luckyCount}");

            //    line = $"[{duration}] " + string.Join(" | ", parts);
            //}
            //else
            //{
            //    // 单段
            //    var parts = new List<string>
            //    {
            //        $"{name}",
            //        $"{(treat ? "治疗" : "伤害")}:{damage}"
            //    };
            //    if (iscrit) parts.Add("暴击");
            //    if (isLucky) parts.Add("幸运");

            //    line = $"[{duration}] " + string.Join(" | ", parts);
            //}



            //FormManager.skillDiary.AppendDiaryLine(line);
        }



        /// <summary>
        /// 定期冲刷：把“超过 WindowMs 没继续”的段落写出（避免一直等不到下一击）。
        /// 建议在你的 1s 定时器里调用。
        /// </summary>
        public static IEnumerable<(long Uid, long SkillId, int Count, ulong Damage)>
            DrainStalePending()
        {
            long now = Stopwatch.GetTimestamp();
            long windowTicks = (long)(WindowMs * TicksPerMs);

            foreach (var kv in _pending)
            {
                var entry = kv.Value;
                lock (entry)
                {
                    if (entry.Count > 0 && (now - entry.LastTick) > windowTicks)
                    {
                        var outCount = entry.Count;
                        var outDmg = entry.TotalDamage;
                        entry.Count = 0;
                        entry.TotalDamage = 0;
                        // 注意：不重置 Start/Last，这个条目会在下次 Register 时复用
                        yield return (kv.Key.Uid, kv.Key.SkillId, outCount, outDmg);
                    }
                }
            }
        }
        /// <summary>
        /// 清场/结束战斗时调用：把所有仍在窗口里的段落一次性吐出（然后再由调用方写入日记）。
        /// 返回后，这些段落会被清零，但不会删除字典项；如需彻底清空，请再调用 Reset()。
        /// </summary>
        public static IEnumerable<(long Uid, long SkillId, int Count, ulong Damage)>
            DrainAllPending()
        {
            foreach (var kv in _pending)
            {
                var entry = kv.Value;
                lock (entry)
                {
                    if (entry.Count > 0)
                    {
                        var outCount = entry.Count;
                        var outDmg = entry.TotalDamage;
                        entry.Count = 0;
                        entry.TotalDamage = 0;
                        yield return (kv.Key.Uid, kv.Key.SkillId, outCount, outDmg);
                    }
                }
            }
        }

        /// <summary>
        /// 彻底清空内部缓存（会丢弃尚未写出的段落）。
        /// 一般应在先 DrainAllPending() 冲刷后，再调用 Reset()。
        /// </summary>
        public static void Reset() => _pending.Clear();


    }
}

      
