using System;
using System.Runtime.InteropServices;

using AntdUI;
using BlueMeter.WinForm.Forms.AuxiliaryForms;
using BlueMeter.WinForm.Plugin;

namespace BlueMeter.WinForm.Forms
{
    public partial class RankingsForm : BorderlessForm
    {
        public RankingsForm()
        {
            InitializeComponent();
            SetDefaultFontFromResources();
            FormGui.SetDefaultGUI(this);
            ToggleTableView();

        }
        private void SetDefaultFontFromResources()
        {
            label1.Font = AppConfig.TitleFont;
            button1.Font = AppConfig.HeaderFont;
            segmented1.Font = AppConfig.ContentFont;
            divider3.Font = AppConfig.ContentFont;
            table_DpsDetailDataTable.Font = AppConfig.ContentFont;
            label2.Font = AppConfig.ContentFont;
        }
        private void RankingsForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }



        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            divider3.Text = "伤害参考";
            get_dps_rank();
        }


        private void segmented1_SelectIndexChanged(object sender, IntEventArgs e)
        {
            get_dps_rank();
        }

        private void RankingsForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                //浅色
                table_DpsDetailDataTable.RowSelectedBg = ColorTranslator.FromHtml("#AED4FB");
                button1.DefaultBack = ColorTranslator.FromHtml("#67AEF6");
            }
            else
            {
                //深色
                table_DpsDetailDataTable.RowSelectedBg = ColorTranslator.FromHtml("#10529a");
                button1.DefaultBack = ColorTranslator.FromHtml("#255AD0");

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ToggleTableView();
        }

        private void table_DpsDetailDataTable_CellClick(object sender, TableClickEventArgs e)
        {

        }

        private void table_DpsDetailDataTable_CellButtonDown(object sender, TableButtonEventArgs e)
        {
            //int row = e.RowIndex - 1;
            //if (row >= 0)
            //{
            //    if (e.Btn.Text == "查看技能数据")
            //    {
            //        if (FormManager.skillReferenceForm == null || FormManager.skillReferenceForm.IsDisposed)
            //        {
            //            FormManager.skillReferenceForm = new SkillReferenceForm();
            //        }
            //        string bayyleid = LeaderboardTableDatas.LeaderboardTable[row].BattleId;
            //        string nickname = LeaderboardTableDatas.LeaderboardTable[row].NickName;
            //        FormManager.skillReferenceForm.LoadInformation(bayyleid, nickname);
            //        FormManager.skillReferenceForm.Show();
            //    }
            //}


        }
    }
}
