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
using BlueMeter.WinForm.Plugin;

using static System.Net.Mime.MediaTypeNames;

namespace BlueMeter.WinForm.Forms.AuxiliaryForms
{
    public partial class SkillReferenceForm : BorderlessForm
    {
        public SkillReferenceForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this); // 统一设置窗体默认 GUI 风格（字体、间距、阴影等）
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色 // 根据配置设置窗体的颜色主题（明亮/深色）
            ToggleTableView();
        }

        private void SkillReferenceForm_Load(object sender, EventArgs e)
        {
            TitleText.Font = AppConfig.SaoFont;
            divider1.Font = AppConfig.ContentFont;
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
                new AntdUI.Column("Name","技能名称"){ Fixed = true},
                new AntdUI.Column("Damage","总伤害"){ Fixed = true},
                new AntdUI.Column("HitCount","命中次数") { Fixed = true},
                new AntdUI.Column("CritRate","爆击率") { Fixed = true},
                new AntdUI.Column("LuckyRate","幸运率") { Fixed = true},
                new Column("AvgPerHit","平均值") { Fixed = true},
                new AntdUI.Column("TotalDps","秒伤") { Fixed = true},
                new AntdUI.Column("Share","技能占比") { Fixed = true},



            };

            table_DpsDetailDataTable.Binding(DamageReferenceSkillData.DamageReferenceSkillTable);

        }

        public async void LoadInformation(string battleId, string nickName)
        {
            //divider1.Text = nickName + "的技能参考数据";
            //DamageReferenceSkillData.DamageReferenceSkillTable.Clear();
            //string url = @$"{AppConfig.url}/get_user_dps";
            //var query = new
            //{
            //    battleId
            //};
            //var data = await Common.RequestGet(url, query);
            //if (data["code"].ToString() == "200")
            //{
            //    foreach (var item in data["data"])
            //    {
            //        string name = item["name"].ToString();//用户名
            //        string damage = Common.FormatWithEnglishUnits(item["damage"]);//总伤害
            //        int hitCount = Convert.ToInt32(item["hitCount"]);//命中次数
            //        string critRate = item["critRate"].ToString() + "%";//爆击率
            //        string luckyRate = item["luckyRate"].ToString() + "%";//幸运率
            //        string avgPerHit = Common.FormatWithEnglishUnits(item["avgPerHit"]);//平均值
            //        string totalDps = Common.FormatWithEnglishUnits(item["totalDps"]);//秒伤
            //        double share = Convert.ToDouble(item["share"]) * 100;//技能占比
            //        DamageReferenceSkillData.DamageReferenceSkillTable.Add(new DamageReferenceSkill(name, damage, hitCount, critRate, luckyRate, avgPerHit, totalDps, share));
            //    }

            //}

            //this.Activate();

        }

        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("装饰用的按钮");
        }
    }
}
