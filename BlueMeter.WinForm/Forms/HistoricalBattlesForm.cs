using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AntdUI;
using BlueMeter.Assets;
using BlueMeter.WinForm.Control;
using BlueMeter.WinForm.Effects;
using BlueMeter.WinForm.Plugin;
using BlueMeter.WinForm.Plugin.DamageStatistics;

using static BlueMeter.WinForm.Control.SkillDetailForm;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace BlueMeter.WinForm.Forms
{
    public partial class HistoricalBattlesForm : BorderlessForm
    {
        public HistoricalBattlesForm()
        {
            InitializeComponent();
            Text = FormManager.APP_NAME;
            FormGui.SetDefaultGUI(this); // 统一设置窗体默认 GUI 风格（字体、间距、阴影等）
            ToggleTableView();

            label1.Font = AppConfig.TitleFont;
            select2.Font = select1.Font = segmented1.Font = AppConfig.ContentFont;
            var harmonyOsSansFont_Size11 = HandledAssets.HarmonyOS_Sans(11);
            label6.Font = harmonyOsSansFont_Size11;
            label3.Font = label2.Font = label5.Font = AppConfig.ContentFont;

            table_DpsDetailDataTable.Font = AppConfig.ContentFont;
            TeamTotalDamageLabel.Font = TeamTotalHealingLabel.Font = TeamTotalTakenDamageLabel.Font = AppConfig.DigitalFont;

        }

        private void HistoricalBattlesForm_Load(object sender, EventArgs e)
        {
            //FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色 // 根据配置设置窗体的颜色主题（明亮/深色）

            //if (FormManager.showTotal)
            //{
            //    ReadFullSessionTime();
            //}
            //else
            //{
            //    ReadSnapshotTime();
            //}



        }

        /// <summary>
        /// 读取 “单场历史（BattleSnapshot）” 下拉
        /// </summary>
        private void ReadSnapshotTime()
        {
            select1.Items.Clear();
            var statsList = StatisticData._manager.History?.ToList();
            if (statsList.Count == 0) return;
            foreach (var snap in statsList)
            {
                select1.Items.Add(new ComboItemBattle { Snapshot = snap }); // 直接把快照塞到项里
            }
            select1.SelectedIndex = 0; // 默认选中第一个
        }

        /// <summary>
        /// 读取 “全程历史（FullSessionSnapshot）” 下拉
        /// </summary>
        private void ReadFullSessionTime()
        {
            select1.Items.Clear();
            var sessions = FullRecord.SessionHistory?.ToList();
            if (sessions == null || sessions.Count == 0) return;

            foreach (var s in sessions)
            {
                select1.Items.Add(new ComboItemFull { Snapshot = s });
            }
            select1.SelectedIndex = 0; // 默认选中第一个
        }


        // —— 下拉项类型（单场）
        private sealed class ComboItemBattle
        {
            public BattleSnapshot Snapshot { get; init; }
            public override string ToString()
            {
                var s = Snapshot;
                return $"{s.StartedAt:MM-dd HH:mm:ss} ~ {s.EndedAt:HH:mm:ss}（{s.Duration:hh\\:mm\\:ss}）";
            }
        }

        // —— 下拉项类型（全程）
        private sealed class ComboItemFull
        {
            public FullSessionSnapshot Snapshot { get; init; }
            public override string ToString()
            {
                var s = Snapshot;
                return $"[全程] {s.StartedAt:MM-dd HH:mm:ss} ~ {s.EndedAt:HH:mm:ss}（{s.Duration:hh\\:mm\\:ss}）";
            }
        }

        private void select1_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            if (segmented1.SelectIndex == 0)
            {
                BattleSnapshot? snap = null;
                if (select1.SelectedValue is ComboItemBattle v && v.Snapshot != null) snap = v.Snapshot;
                else if (select1.SelectedValue is ComboItemBattle v2 && v2.Snapshot != null) snap = v2.Snapshot;

                if (snap != null) DumpSnapshot(snap);
            }
            else
            {
                FullSessionSnapshot? snap = null;
                if (select1.SelectedValue is ComboItemFull v && v.Snapshot != null) snap = v.Snapshot;
                else if (select1.SelectedValue is ComboItemFull v2 && v2.Snapshot != null) snap = v2.Snapshot;

                if (snap != null) DumpFullSnapshot(snap);
            }
        }

        // 单场
        private void DumpSnapshot(BattleSnapshot snap)
        {
            DpsTableDatas.DpsTable.Clear(); // 清空旧数据
            var sb = new StringBuilder();
            sb.AppendLine($"[快照] {snap.StartedAt:MM-dd HH:mm:ss} ~ {snap.EndedAt:HH:mm:ss}  时长: {snap.Duration}");
            TeamTotalDamageLabel.Text =Common.FormatWithEnglishUnits(snap.TeamTotalDamage.ToString());
            TeamTotalHealingLabel.Text = Common.FormatWithEnglishUnits(snap.TeamTotalHealing.ToString());
            TeamTotalTakenDamageLabel.Text = Common.FormatWithEnglishUnits(snap.TeamTotalTakenDamage.ToString());
            var tdTotal = snap.TeamTotalDamage;

            var orderedPlayers = ApplySort(snap.Players.Values);

            foreach (var p in orderedPlayers)
            {
                double dmgShare = snap.TeamTotalDamage > 0
            ? Math.Round(p.TotalDamage * 100.0 / snap.TeamTotalDamage, 1)
            : 0.0;


                DpsTableDatas.DpsTable.Add(new DpsTable(
                   /*  1 */ p.Uid,
            /*  2 */ p.Nickname,

            /*  3 承伤（你实时里第3个就是承伤） */
            /*  3 */ p.TakenDamage,

            /*  4~9 = 治疗聚合 + 细分 + 瞬时窗口 */
            /*  4 */ p.TotalHealing,

            /*  5 */ p.HealingCritical,//暴击治疗量
            /*  6 */ p.HealingLucky,//幸运治疗量
            /*  7 */ p.HealingCritLucky,//暴击且幸运治疗量
            /*  8 */ p.HealingRealtime,//实时HPS
            /*  9 */ p.HealingRealtimeMax,//最大瞬时HPS

            /* 10 职业 */
            /* 10 */ p.Profession,//职业

            /* 11~14 = 伤害聚合 + 细分 */
            /* 11 */ p.TotalDamage,//总伤害
            /* 12 */ p.CriticalDamage,//暴击伤害
            /* 13 */ p.LuckyDamage,//幸运伤害
            /* 14 */ p.CritLuckyDamage,

            /* 15~16 = 比率（%）*/
            /* 15 */ Math.Round(p.CritRate, 1),
            /* 16 */ Math.Round(p.LuckyRate, 1),

            /* 17~18 = 伤害瞬时/峰值 */
            /* 17 */ p.RealtimeDps,//实时PDS
            /* 18 */ p.RealtimeDpsMax,//最大瞬时DPS

            /* 19~20 = 平均 DPS/HPS */
            /* 19 */ Math.Round(p.TotalDps, 1),//总DPS
            /* 20 */ Math.Round(p.TotalHps, 1),//总HPS
                    p.CombatPower,//战力
                    dmgShare


                /* 22 = 战力 */
                /* 22 */
                ));
                //sb.AppendLine(
                //    $"  UID={p.Uid}  昵称={p.Nickname}  职业={p.Profession}  战力={p.CombatPower}  " +
                //    $"总伤害={p.TotalDamage}  DPS={p.TotalDps:F1}  总治疗={p.TotalHealing}  HPS={p.TotalHps:F1}  承伤={p.TakenDamage}");
            }

        }

        // 全程
        private void DumpFullSnapshot(FullSessionSnapshot snap)
        {
            DpsTableDatas.DpsTable.Clear(); // 清空旧数据
            var sb = new StringBuilder();
            sb.AppendLine($"[全程快照] {snap.StartedAt:MM-dd HH:mm:ss} ~ {snap.EndedAt:HH:mm:ss}  时长: {snap.Duration}");
            TeamTotalDamageLabel.Text = Common.FormatWithEnglishUnits(snap.TeamTotalDamage.ToString());
            TeamTotalHealingLabel.Text = Common.FormatWithEnglishUnits(snap.TeamTotalHealing.ToString());
            TeamTotalTakenDamageLabel.Text = Common.FormatWithEnglishUnits( snap.TeamTotalTakenDamage.ToString());
            var orderedPlayers = ApplySort(snap.Players.Values);

            foreach (var p in orderedPlayers)
            {
                double dmgShare = snap.TeamTotalDamage > 0
           ? Math.Round(p.TotalDamage * 100.0 / snap.TeamTotalDamage, 1)
           : 0.0;
                // 注意：全程快照的 SnapshotPlayer 未提供逐类（暴击/幸运/暴击且幸运）的总量与实时峰值；
                // 这里这些列用 0 占位；DPS/HPS 使用快照内的 TotalDps/TotalHps。
                DpsTableDatas.DpsTable.Add(new DpsTable(
                    /*  1 */ p.Uid,
                    /*  2 */ p.Nickname,

                    /*  3 承伤 */
                    /*  3 */ p.TakenDamage,

                    /*  4~9 治疗相关（部分字段快照未提供 → 用 0 占位） */
                    /*  4 */ p.TotalHealing,
                    /*  5 */ p.HealingCritical,          // HealingCritical (未知)
                    /*  6 */ p.HealingLucky,          // HealingLucky (未知)
                    /*  7 */ p.HealingCritLucky,          // HealingCritLucky (未知)
                    /*  8 */ p.HealingRealtime,          // RealtimeHps (未知)
                    /*  9 */ p.HealingRealtimeMax,          // MaxInstantHps (未知)

                    /* 10 职业 */
                    /* 10 */ p.Profession,

                    /* 11~14 伤害相关（部分字段快照未提供 → 用 0 占位） */
                    /* 11 */ p.TotalDamage,
                    /* 12 */ p.CriticalDamage,          // CriticalDamage (未知)
                    /* 13 */ p.LuckyDamage,          // LuckyDamage (未知)
                    /* 14 */ p.CriticalDamage,          // CritLuckyDamage (未知)

                    /* 15~16 = 比率（%）（快照未提供玩家层聚合 → 用 0 占位） */
                    /* 15 */ p.CritRate,          // CritRate
                    /* 16 */ p.LuckyRate,          // LuckyRate

                    /* 17~18 实时/峰值（全程历史快照无“实时”概念 → 用 0 占位） */
                    /* 17 */ p.RealtimeDps,          // RealtimeDps
                    /* 18 */ p.RealtimeDpsMax,          // RealtimeDpsMax

                    /* 19~20 = 平均 DPS/HPS */
                    /* 19 */ Math.Round(p.TotalDps, 1),
                    /* 20 */ Math.Round(p.TotalHps, 1),

                    /* 22 = 战力 */
                    /* 22 */ p.CombatPower, dmgShare
                ));


            }


        }

        // 放在类内部（如字段区）
        private enum SortMode { ByDamage, ByHealing, ByTaken }
        private SortMode _sortMode = SortMode.ByDamage; // 默认按伤害
        // SnapshotPlayer 是你快照里的玩家模型类型，按你项目里的命名替换
        private IEnumerable<SnapshotPlayer> ApplySort(IEnumerable<SnapshotPlayer> players)
        {
            switch (_sortMode)
            {
                case SortMode.ByHealing:
                    // 先按总治疗，再按平均HPS、再按瞬时峰值HPS作为次序
                    return players
                        .OrderByDescending(p => p.TotalHealing)
                        .ThenByDescending(p => p.TotalHps)
                        .ThenByDescending(p => p.HealingRealtimeMax);

                case SortMode.ByTaken:
                    // 承伤优先，其次用总伤害打破并列（你也可换成 Taken 的峰值等）
                    return players
                        .OrderByDescending(p => p.TakenDamage)
                        .ThenByDescending(p => p.TotalDamage);

                case SortMode.ByDamage:
                default:
                    // 伤害优先，其次平均DPS，再次瞬时峰值
                    return players
                        .OrderByDescending(p => p.TotalDamage)
                        .ThenByDescending(p => p.TotalDps)
                        .ThenByDescending(p => p.MaxSingleHit);
            }
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (segmented1.SelectIndex == 0)
            {
                ReadSnapshotTime();//刷新下拉项
            }
            else
            {
                ReadFullSessionTime();//刷新全程下拉项
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void segmented1_SelectIndexChanged(object sender, IntEventArgs e)
        {
            DpsTableDatas.DpsTable.Clear(); // 清空旧数据
            select1.Items.Clear();
            if (segmented1.SelectIndex == 0)
            {
                ReadSnapshotTime();
            }
            else
            {
                ReadFullSessionTime();
            }
        }

        private void table_DpsDetailDataTable_CellClick(object sender, TableClickEventArgs e)
        {
            //if (e.ColumnIndex <= 0) return;

            //// —— 行索引安全校验（AntdUI 表通常是 0-based，这里不再减 1）——
            //int idx = e.RowIndex - 1;
            //if (idx < 0 || idx >= DpsTableDatas.DpsTable.Count) return;

            //var row = DpsTableDatas.DpsTable[idx];
            //var uid = row.Uid;
            //string nick = row.NickName;
            //int power = row.CombatPower;
            //string prof = row.Profession;

            //// —— 详情窗体准备 —— 
            //if (FormManager.skillDetailForm == null || FormManager.skillDetailForm.IsDisposed)
            //    FormManager.skillDetailForm = new SkillDetailForm();

            //var f = FormManager.skillDetailForm;
            //f.Uid = uid;
            //f.Nickname = nick;
            //f.Power = power;
            //f.Profession = prof;

            //// —— 快照上下文 + 时间 —— 
            //f.ContextType = DetailContextType.Snapshot;
            //f.SnapshotStartTime = GetSelectedSnapshotStartTime();
            //if (f.SnapshotStartTime is null)
            //{
            //    // 可留着调试
            //    // MessageBox.Show("未能取得快照时间（下拉未选中？）");
            //    return;
            //}

            //// 顶部玩家信息
            //f.GetPlayerInfo(nick, power, prof);

            //// （可选）调试：看快照技能数量是否为 0，快速定位“取不到技能” vs “UI 没渲染”
            ///*
            //var counts = BlueMeter.Plugin.DamageStatistics.FullRecord
            //             .GetPlayerSkillsBySnapshotTimeEx(f.SnapshotStartTime.Value, uid);
            //MessageBox.Show($"Snapshot Skills → D:{counts.DamageSkills.Count} H:{counts.HealingSkills.Count} T:{counts.TakenSkills.Count}");
            //*/

            //// 刷新并显示
            //f.SelectDataType();   // 这里应当会走 snapshot 分支：UpdateSkillTable_Snapshot(...)
            //if (!f.Visible) f.Show(); else f.Activate();
        }

        private DateTime? GetSelectedSnapshotStartTime()
        {
            if (segmented1.SelectIndex == 0 && select1.SelectedValue is ComboItemBattle b && b.Snapshot != null)
                return b.Snapshot.StartedAt;
            if (segmented1.SelectIndex != 0 && select1.SelectedValue is ComboItemFull f && f.Snapshot != null)
                return f.Snapshot.StartedAt;
            return null;
        }

        private void HistoricalBattlesForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                table_DpsDetailDataTable.RowSelectedBg = ColorTranslator.FromHtml("#AED4FB");
                splitter1.Panel2.BackColor = splitter1.BackColor = ColorTranslator.FromHtml("#FFFFFF");
                panel1.Back = ColorTranslator.FromHtml("#67AEF6");
                splitter1.Panel1.BackColor = ColorTranslator.FromHtml("#FFFFFF");
                table_DpsDetailDataTable.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            }
            else
            {
                splitter1.Panel2.BackColor = splitter1.BackColor = ColorTranslator.FromHtml("#1F1F1F");
                panel1.Back = ColorTranslator.FromHtml("#255AD0");
                splitter1.Panel1.BackColor = ColorTranslator.FromHtml("#141414");
                table_DpsDetailDataTable.BackColor = ColorTranslator.FromHtml("#1F1F1F");
                table_DpsDetailDataTable.RowSelectedBg = ColorTranslator.FromHtml("#10529a");
            }
        }

        private void select2_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            var val = select2?.SelectedValue?.ToString();
            _sortMode = val switch
            {
                "按治疗排序" => SortMode.ByHealing,
                "按承伤排序" => SortMode.ByTaken,
                _ => SortMode.ByDamage
            };

            // 重新渲染当前视图
            if (segmented1.SelectIndex == 0)
            {
                if (select1.SelectedValue is ComboItemBattle b && b.Snapshot != null)
                    DumpSnapshot(b.Snapshot);
            }
            else
            {
                if (select1.SelectedValue is ComboItemFull f && f.Snapshot != null)
                    DumpFullSnapshot(f.Snapshot);
            }
        }
    }
}
