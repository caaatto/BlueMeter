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
using StarResonanceDpsAnalysis.WinForm.Plugin;
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class DeathStatisticsForm : BorderlessForm
    {
        public DeathStatisticsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this); // 统一设置窗体默认 GUI 风格（字体、间距、阴影等）
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色 // 根据配置设置窗体的颜色主题（明亮/深色）
            //加载死亡信息
            ToggleTableView();
            //设置字体
            SetDefaultFontFromResources();
        }

        /// <summary>
        /// 设置字体
        /// </summary>
        private void SetDefaultFontFromResources()
        {
            TitleText.Font = AppConfig.SaoFont;

            table_DpsDetailDataTable.Font = AppConfig.ContentFont;

        }

        public void ToggleTableView()
        {



            table_DpsDetailDataTable.Columns = new AntdUI.ColumnCollection
            {   new("", "序号")
                {

                    Render = (value, record, rowIndex) => rowIndex + 1,
                    Fixed = true
                },
                new AntdUI.Column("NickName","玩家昵称"){ Fixed = true},
                new AntdUI.Column("TotalDeathCount","死亡次数"){ Fixed = true},


            };

            table_DpsDetailDataTable.Binding(DeathStatisticsTableDatas.DeathStatisticsTable);
            LoadInformation();


        }


        private void DeathStatisticsForm_Load(object sender, EventArgs e)
        {


        }

        /// <summary>
        /// 更新信息
        /// </summary>
        private void LoadInformation()
        {
            DeathStatisticsTableDatas.DeathStatisticsTable.Clear();
            var rows = FullRecord.GetAllPlayerDeathCounts();

            foreach (var item in rows)
            {
                var uid = item.Uid;
                string nickName = item.Nickname;
                int totalDeathCount = item.Deaths;
                // 查找是否已有该玩家
                var existing = DeathStatisticsTableDatas.DeathStatisticsTable
                    .FirstOrDefault(x => x.Uid == uid);

                if (existing != null)
                {

                    existing.TotalDeathCount = totalDeathCount;
                }
                else
                {

                    DeathStatisticsTableDatas.DeathStatisticsTable
                        .Add(new DeathStatisticsTable(uid, nickName, totalDeathCount));
                }
            }
            // === 统计总死亡数并加到表尾 ===
            int totalDeaths = rows.Sum(r => r.Deaths);

            // 查找是否已有“总计”行（Uid=0 作为标记）
            var totalRow = DeathStatisticsTableDatas.DeathStatisticsTable
                .FirstOrDefault(x => x.Uid == 0);

            if (totalRow != null)
            {
                totalRow.TotalDeathCount = totalDeaths;
                totalRow.NickName = "总计";
            }
            else
            {
                DeathStatisticsTableDatas.DeathStatisticsTable
                    .Add(new DeathStatisticsTable(0, "总计", totalDeaths));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadInformation();//刷新
        }

        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadInformation();//刷新
        }
    }
}
