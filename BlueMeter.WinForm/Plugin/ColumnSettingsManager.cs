using AntdUI;

namespace BlueMeter.WinForm.Plugin
{
    public class ColumnSetting
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public bool IsVisible { get; set; } = false; // 默认全部隐藏
        public Func<Column> Builder { get; set; }
    }

    public static class ColumnSettingsManager
    {
        public static Action? RefreshTableAction { get; set; }

        // 可配置的列设置 - 移除了战力，因为它是默认固定显示的
        public static List<ColumnSetting> AllSettings =
        [
            new() {
                Key = "TotalDamage", Title = "总伤害", IsVisible = true,
                Builder = () =>  new AntdUI.Column("TotalDamage", "总伤害",ColumnAlign.Center)
            },
            new() {
                Key = "DamageTaken", Title = "承伤", IsVisible = true,
                Builder = () => new Column("DamageTaken", "承伤", ColumnAlign.Center)
            },
            new() {
                Key = "CriticalDamage", Title = "纯暴击", IsVisible = true,
                Builder = () => new Column("CriticalDamage", "纯暴击")
            },
            new() {
                Key = "LuckyDamage", Title = "纯幸运", IsVisible = true,
                Builder = () => new Column("LuckyDamage", "纯幸运")
            },
            new() {
                Key = "CritLuckyDamage", Title = "暴击幸运", IsVisible = true,
                Builder = () => new Column("CritLuckyDamage", "暴击幸运")
            },
            new() {
                Key = "InstantDps", Title = "瞬时Dps", IsVisible = true,
                Builder = () => new Column("InstantDps", "瞬时Dps")
            },
            new() {
                Key = "MaxInstantDps", Title = "最大瞬时Dps", IsVisible = true,
                Builder = () => new Column("MaxInstantDps", "最大瞬时Dps")
            },
            new() {
                Key = "TotalDps", Title = "DPS", IsVisible = true,
                Builder = () => new Column("TotalDps", "DPS", ColumnAlign.Center)
            },
            new() {
                Key = "CritRate", Title = "暴击率", IsVisible = true,
                Builder = () => new Column("CritRate", "暴击率")
            },
            new() {
                Key = "LuckyRate", Title = "幸运率", IsVisible = true,
                Builder = () => new Column("LuckyRate", "幸运率")
            },
            new() {
                Key = "TotalHealingDone", Title = "总治疗", IsVisible = true,
                Builder = () => new Column("TotalHealingDone", "总治疗", ColumnAlign.Center)
            },
            new() {
                Key = "CriticalHealingDone", Title = "治疗暴击", IsVisible = true,
                Builder = () => new Column("CriticalHealingDone", "治疗暴击")
            },
            new() {
                Key = "LuckyHealingDone", Title = "治疗幸运", IsVisible = true,
                Builder = () => new Column("LuckyHealingDone", "治疗幸运")
            },
            new() {
                Key = "CritLuckyHealingDone", Title = "治疗暴击幸运", IsVisible = true,
                Builder = () => new Column("CritLuckyHealingDone", "治疗暴击幸运")
            },
            new() {
                Key = "InstantHps", Title = "瞬时Hps", IsVisible = true,
                Builder = () => new Column("InstantHps", "瞬时Hps")
            },
            new() {
                Key = "MaxInstantHps", Title = "最大瞬时Hps", IsVisible = true,
                Builder = () => new Column("MaxInstantHps", "最大瞬时Hps")
            },
            new() {
                Key = "TotalHps", Title = "HPS", IsVisible = true,
                Builder = () => new Column("TotalHps", "HPS", ColumnAlign.Center)
            },
        ];

        public static StackedHeaderRow[] BuildStackedHeader()
        {
            var list = new List<StackedColumn[]>();

            // 与上方 Key 一致的分组
            string[] group1 = { "TotalDamage", "CriticalDamage", "LuckyDamage", "CritLuckyDamage" };
            string[] group2 = { "InstantDps", "MaxInstantDps", "TotalDps" };
            string[] group3 = { "TotalHealingDone", "CriticalHealingDone", "LuckyHealingDone", "CritLuckyHealingDone" };
            string[] group4 = { "InstantHps", "MaxInstantHps", "TotalHps" };

            list.Add(BuildGroup(group1, "总伤害"));
            list.Add(BuildGroup(group2, "DPS"));
            list.Add(BuildGroup(group3, "总治疗"));
            list.Add(BuildGroup(group4, "HPS"));

            return [new StackedHeaderRow([.. list.SelectMany(x => x)])];
        }

        private static StackedColumn[] BuildGroup(string[] keys, string title)
        {
            var visible = keys.Where(k => AllSettings.FirstOrDefault(x => x.Key == k)?.IsVisible ?? false).ToList();
            return visible.Count > 1
                ? [new StackedColumn(string.Join(',', visible), title)]
                : [];
        }

        public static ColumnCollection BuildColumns()
        {
            var columns = new List<Column>
            {
                new("", "序号")
                {
                    Width = "50",
                    Render = (value, record, rowIndex) => rowIndex + 1,
                    Fixed = true
                },
                // 基础信息列 - 固定显示，不可配置
                new("Uid", "角色ID",ColumnAlign.Center){ SortOrder = true },
                new("NickName", "角色昵称",ColumnAlign.Center){ SortOrder = true },
                new("Profession", "职业",ColumnAlign.Center),
                // 战力 - 固定显示，不在设置中配置
                new("CombatPower", "战力", ColumnAlign.Center){ SortOrder = true }
            };

            // 添加可配置的列
            columns.AddRange(AllSettings.Where(s => s.IsVisible).Select(s => s.Builder()));

            return [.. columns];
        }
    }
}
