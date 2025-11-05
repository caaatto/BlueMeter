using System;
using System.Text;

using ClosedXML.Excel;
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics;

namespace StarResonanceDpsAnalysis.WinForm.Plugin
{
    /// <summary>
    /// 数据导出服务，支持Excel和CSV格式
    /// </summary>
    public static class DataExportService
    {
        #region Excel导出

        /// <summary>
        /// 导出DPS数据到Excel文件
        /// </summary>
        /// <param name="players">玩家数据列表</param>
        /// <param name="includeSkillDetails">是否包含技能详情</param>
        /// <returns>是否导出成功</returns>
        public static bool ExportToExcel(List<PlayerData> players, bool includeSkillDetails = true)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel文件 (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"DPS统计_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx",
                    Title = "保存DPS统计数据"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return false;

                using var workbook = new XLWorkbook();

                // 创建玩家总览表
                CreatePlayerOverviewSheet(workbook, players);

                if (includeSkillDetails)
                {
                    // 创建技能详情表
                    CreateSkillDetailsSheet(workbook, players);

                    // 创建团队技能统计表
                    CreateTeamSkillStatsSheet(workbook, players);
                }

                workbook.SaveAs(saveDialog.FileName);

                MessageBox.Show($"数据已成功导出到:\n{saveDialog.FileName}", "导出成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出Excel文件时发生错误:\n{ex.Message}", "导出失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 创建玩家总览工作表
        /// </summary>
        private static void CreatePlayerOverviewSheet(XLWorkbook workbook, List<PlayerData> players)
        {
            var worksheet = workbook.Worksheets.Add("玩家总览");

            // 设置表头
            var headers = new[]
            {
                "玩家昵称", "职业", "战力", "总伤害", "总DPS", "暴击伤害", "幸运伤害",
                "暴击率", "幸运率", "瞬时DPS峰值", "总治疗", "总HPS", "承受伤害", "命中次数"
            };

            // 写入表头
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            }

            // 写入数据
            int row = 2;
            foreach (var player in players.OrderByDescending(p => p.DamageStats.Total))
            {
                worksheet.Cell(row, 1).Value = player.Nickname;
                worksheet.Cell(row, 2).Value = player.Profession;
                worksheet.Cell(row, 3).Value = player.CombatPower;
                worksheet.Cell(row, 4).Value = (double)player.DamageStats.Total;
                worksheet.Cell(row, 5).Value = Math.Round(player.GetTotalDps(), 1);
                worksheet.Cell(row, 6).Value = (double)player.DamageStats.Critical;
                worksheet.Cell(row, 7).Value = (double)player.DamageStats.Lucky;
                worksheet.Cell(row, 8).Value = $"{player.DamageStats.GetCritRate()}%";
                worksheet.Cell(row, 9).Value = $"{player.DamageStats.GetLuckyRate()}%";
                worksheet.Cell(row, 10).Value = (double)player.DamageStats.RealtimeMax;
                worksheet.Cell(row, 11).Value = (double)player.HealingStats.Total;
                worksheet.Cell(row, 12).Value = Math.Round(player.GetTotalHps(), 1);
                worksheet.Cell(row, 13).Value = (double)player.TakenDamage;
                worksheet.Cell(row, 14).Value = player.DamageStats.CountTotal;

                row++;
            }

            // 自动调整列宽
            worksheet.ColumnsUsed().AdjustToContents();

            // 添加筛选
            worksheet.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
        }

        /// <summary>
        /// 创建技能详情工作表
        /// </summary>
        private static void CreateSkillDetailsSheet(XLWorkbook workbook, List<PlayerData> players)
        {
            var worksheet = workbook.Worksheets.Add("技能详情");

            // 设置表头
            var headers = new[]
            {
                "玩家昵称", "技能名称", "总伤害", "命中次数", "平均伤害",
                "暴击率", "幸运率", "技能DPS", "伤害占比"
            };

            // 写入表头
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }

            // 写入数据
            int row = 2;
            foreach (var player in players.OrderByDescending(p => p.DamageStats.Total))
            {
                var skills = StatisticData._manager.GetPlayerSkillSummaries(
                    player.Uid, topN: null, orderByTotalDesc: true);

                foreach (var skill in skills)
                {
                    worksheet.Cell(row, 1).Value = player.Nickname;
                    worksheet.Cell(row, 2).Value = skill.SkillName;
                    worksheet.Cell(row, 3).Value = (double)skill.Total;
                    worksheet.Cell(row, 4).Value = skill.HitCount;
                    worksheet.Cell(row, 5).Value = Math.Round(skill.AvgPerHit, 1);
                    worksheet.Cell(row, 6).Value = $"{skill.CritRate * 100:F1}%";
                    worksheet.Cell(row, 7).Value = $"{skill.LuckyRate * 100:F1}%";
                    worksheet.Cell(row, 8).Value = Math.Round(skill.TotalDps, 1);
                    worksheet.Cell(row, 9).Value = $"{skill.ShareOfTotal * 100:F1}%";

                    row++;
                }
            }

            // 自动调整列宽
            worksheet.ColumnsUsed().AdjustToContents();

            // 添加筛选
            if (row > 2)
                worksheet.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
        }

        /// <summary>
        /// 创建团队技能统计工作表
        /// </summary>
        private static void CreateTeamSkillStatsSheet(XLWorkbook workbook, List<PlayerData> players)
        {
            var worksheet = workbook.Worksheets.Add("团队技能统计");

            // 获取团队技能数据
            var teamSkills = StatisticData._manager.GetTeamTopSkillsByTotal(50);

            // 设置表头
            var headers = new[]
            {
                "技能名称", "总伤害", "总命中次数", "团队占比"
            };

            // 写入表头
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            }

            // 计算总伤害用于百分比计算
            ulong totalTeamDamage = (ulong)teamSkills.Sum(s => (double)s.Total);

            // 写入数据
            int row = 2;
            foreach (var skill in teamSkills)
            {
                worksheet.Cell(row, 1).Value = skill.SkillName;
                worksheet.Cell(row, 2).Value = (double)skill.Total;
                worksheet.Cell(row, 3).Value = skill.HitCount;
                worksheet.Cell(row, 4).Value = totalTeamDamage > 0 ?
                    $"{((double)skill.Total / totalTeamDamage) * 100:F1}%" : "0%";

                row++;
            }

            // 自动调整列宽
            worksheet.ColumnsUsed().AdjustToContents();

            // 添加筛选
            if (row > 2)
                worksheet.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
        }

        #endregion

        #region CSV导出

        /// <summary>
        /// 导出DPS数据到CSV文件
        /// </summary>
        /// <param name="players">玩家数据列表</param>
        /// <returns>是否导出成功</returns>
        public static bool ExportToCsv(List<PlayerData> players)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV文件 (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"DPS统计_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv",
                    Title = "保存DPS统计数据"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return false;

                var csv = new StringBuilder();

                // 添加BOM以确保Excel正确显示中文
                csv.Append('\uFEFF');

                // CSV表头
                csv.AppendLine("玩家昵称,职业,战力,总伤害,总DPS,暴击伤害,幸运伤害,暴击率,幸运率,瞬时DPS峰值,总治疗,总HPS,承受伤害,命中次数");

                // 数据行
                foreach (var player in players.OrderByDescending(p => p.DamageStats.Total))
                {
                    csv.AppendLine($"\"{EscapeCsvField(player.Nickname)}\"," +
                                 $"\"{EscapeCsvField(player.Profession)}\"," +
                                 $"{player.CombatPower}," +
                                 $"{player.DamageStats.Total}," +
                                 $"{player.GetTotalDps():F1}," +
                                 $"{player.DamageStats.Critical}," +
                                 $"{player.DamageStats.Lucky}," +
                                 $"{player.DamageStats.GetCritRate()}%," +
                                 $"{player.DamageStats.GetLuckyRate()}%," +
                                 $"{player.DamageStats.RealtimeMax}," +
                                 $"{player.HealingStats.Total}," +
                                 $"{player.GetTotalHps():F1}," +
                                 $"{player.TakenDamage}," +
                                 $"{player.DamageStats.CountTotal}");
                }

                File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);

                MessageBox.Show($"数据已成功导出到:\n{saveDialog.FileName}", "导出成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出CSV文件时发生错误:\n{ex.Message}", "导出失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 转义CSV字段中的特殊字符
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // 如果包含逗号、引号或换行符，需要用引号包围并转义内部引号
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return field.Replace("\"", "\"\"");
            }

            return field;
        }

        #endregion

        #region 截图功能

        /// <summary>
        /// 保存窗口截图
        /// </summary>
        /// <param name="form">要截图的窗口</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveScreenshot(Form form)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "PNG图片 (*.png)|*.png|JPEG图片 (*.jpg)|*.jpg",
                    DefaultExt = "png",
                    FileName = $"DPS截图_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png",
                    Title = "保存DPS界面截图"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return false;

                // 创建与窗口大小相同的位图
                var bounds = form.Bounds;
                using var bitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height);
                using var graphics = System.Drawing.Graphics.FromImage(bitmap);

                // 截取窗口内容
                graphics.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);

                // 根据文件扩展名保存
                var extension = Path.GetExtension(saveDialog.FileName).ToLower();
                var format = extension switch
                {
                    ".jpg" or ".jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
                    _ => System.Drawing.Imaging.ImageFormat.Png
                };

                bitmap.Save(saveDialog.FileName, format);

                MessageBox.Show($"截图已成功保存到:\n{saveDialog.FileName}", "截图成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存截图时发生错误:\n{ex.Message}", "截图失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取当前有战斗数据的玩家列表
        /// </summary>
        /// <returns>玩家数据列表</returns>
        public static List<PlayerData> GetCurrentPlayerData()
        {
            return StatisticData._manager
                .GetPlayersWithCombatData()
                .ToList();
        }

        /// <summary>
        /// 检查是否有数据可导出
        /// </summary>
        /// <returns>是否有数据</returns>
        public static bool HasDataToExport()
        {
            return GetCurrentPlayerData().Count > 0;
        }

        #endregion
    }
}