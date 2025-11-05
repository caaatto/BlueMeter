using System;
using System.Diagnostics;

using AntdUI;
using SharpPcap;
using BlueMeter.Assets;
using BlueMeter.WinForm.Control;
using BlueMeter.WinForm.Core;
using BlueMeter.WinForm.Effects;
using BlueMeter.WinForm.Plugin;
using BlueMeter.WinForm.Plugin.DamageStatistics;

namespace BlueMeter.WinForm.Forms
{
    public partial class MainForm : BorderlessForm
    {
        public MainForm()
        {
            InitializeComponent();

            FormGui.SetDefaultGUI(this);

            FormGui.SetColorMode(this, AppConfig.IsLight);

            SetDefaultFontFromResources();

            pageHeader_MainHeader.Text = Text = $"{FormManager.APP_NAME} {FormManager.AppVersion}";

            label_NowVersionNumber.Text = FormManager.AppVersion;

            label_AppName.Text = $"{FormManager.APP_NAME}";

            pictureBox_AppIcon.Image = HandledAssets.ApplicationIcon_256x256;
        }

        private void SetDefaultFontFromResources()
        {
            groupBox_About.Font = label_AppName.Font = label_NowVersionTip.Font =
                label_NowVersionDevelopersTip.Font = HandledAssets.HarmonyOS_Sans(12, FontStyle.Bold);


            label_SelfIntroduce.Font = label_NowVersionNumber.Font = label_NowVersionDevelopers.Font =
                label_OpenSourceTip_1.Font = linkLabel_GitHub.Font = label_OpenSourceTip_2.Font =
                linkLabel_QQGroup_1.Font = label_ThankHelpFromTip_1.Font = linkLabel_NodeJsProject.Font =
                label_ThankHelpFromTip_2.Font = label_Copyright.Font = HandledAssets.HarmonyOS_Sans(9);
        }

        private void linkLabel_GitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/anying1073/StarResonanceDps",
                UseShellExecute = true
            });

            linkLabel_GitHub.LinkVisited = true;
        }

        private void linkLabel_QQGroup_1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://qm.qq.com/cgi-bin/qm/qr?k=QeIozXvLSRH9dL_CTZeisQ1Ae4CZpiSc&jump_from=webapi&authKey=HNr5BrrIhqRPyGs2R54NucKsg7Pb9/c0a03gih69PekWfSNLh9MIi/ClXXnaMzHK",
                UseShellExecute = true
            });

            linkLabel_QQGroup_1.LinkVisited = true;
        }

        private void linkLabel_QQGroup_2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://qm.qq.com/cgi-bin/qm/qr?k=ZbyXsDqWZm9mQN0R7cg-EVYlISgbik4M&jump_from=webapi&authKey=S8y8TcbKHzd2yWYUz4YA+Ojrq93PfFCyENqUgXho632ELRIiK6MmnauLuEB4BjhE",
                UseShellExecute = true
            });

            linkLabel_QQGroup_1.LinkVisited = true;
        }

        private void linkLabel_NodeJsProject_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/dmlgzs/StarResonanceDamageCounter",
                UseShellExecute = true
            });

            linkLabel_NodeJsProject.LinkVisited = true;
        }

        private void MainForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                groupBox_About.ForeColor = Color.Black;
                linkLabel_GitHub.LinkColor = linkLabel_QQGroup_1.LinkColor = linkLabel_NodeJsProject.LinkColor = Color.Blue;
                linkLabel_GitHub.VisitedLinkColor = linkLabel_QQGroup_1.VisitedLinkColor = linkLabel_NodeJsProject.VisitedLinkColor = Color.Purple;
            }
            else
            {
                groupBox_About.ForeColor = Color.White;
                linkLabel_GitHub.LinkColor = linkLabel_QQGroup_1.LinkColor = linkLabel_NodeJsProject.LinkColor = Color.LightSkyBlue;
                linkLabel_GitHub.VisitedLinkColor = linkLabel_QQGroup_1.VisitedLinkColor = linkLabel_NodeJsProject.VisitedLinkColor = Color.MediumPurple;
            }
        }
    }
}
