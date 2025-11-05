namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            pageHeader_MainHeader = new AntdUI.PageHeader();
            pictureBox_AppIcon = new PictureBox();
            groupBox_About = new GroupBox();
            linkLabel_QQGroup_2 = new LinkLabel();
            label_ThankHelpFromTip_2 = new Label();
            linkLabel_NodeJsProject = new LinkLabel();
            label_ThankHelpFromTip_1 = new Label();
            label_Copyright = new Label();
            linkLabel_QQGroup_1 = new LinkLabel();
            label_OpenSourceTip_2 = new Label();
            label_OpenSourceTip_1 = new Label();
            linkLabel_GitHub = new LinkLabel();
            label_SelfIntroduce = new Label();
            label_NowVersionDevelopers = new Label();
            label_NowVersionDevelopersTip = new Label();
            label_NowVersionNumber = new Label();
            label_NowVersionTip = new Label();
            label_AppName = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox_AppIcon).BeginInit();
            groupBox_About.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader_MainHeader
            // 
            pageHeader_MainHeader.DividerShow = true;
            pageHeader_MainHeader.DividerThickness = 2F;
            pageHeader_MainHeader.Dock = DockStyle.Top;
            pageHeader_MainHeader.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            pageHeader_MainHeader.Icon = (Image)resources.GetObject("pageHeader_MainHeader.Icon");
            pageHeader_MainHeader.Location = new Point(0, 0);
            pageHeader_MainHeader.Margin = new Padding(2);
            pageHeader_MainHeader.MaximizeBox = false;
            pageHeader_MainHeader.Name = "pageHeader_MainHeader";
            pageHeader_MainHeader.ShowButton = true;
            pageHeader_MainHeader.Size = new Size(758, 30);
            pageHeader_MainHeader.SubText = "";
            pageHeader_MainHeader.TabIndex = 8;
            pageHeader_MainHeader.Text = "别查我DPS";
            // 
            // pictureBox_AppIcon
            // 
            pictureBox_AppIcon.Location = new Point(13, 35);
            pictureBox_AppIcon.Name = "pictureBox_AppIcon";
            pictureBox_AppIcon.Size = new Size(80, 80);
            pictureBox_AppIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_AppIcon.TabIndex = 10;
            pictureBox_AppIcon.TabStop = false;
            // 
            // groupBox_About
            // 
            groupBox_About.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox_About.Controls.Add(linkLabel_QQGroup_2);
            groupBox_About.Controls.Add(label_ThankHelpFromTip_2);
            groupBox_About.Controls.Add(linkLabel_NodeJsProject);
            groupBox_About.Controls.Add(label_ThankHelpFromTip_1);
            groupBox_About.Controls.Add(label_Copyright);
            groupBox_About.Controls.Add(linkLabel_QQGroup_1);
            groupBox_About.Controls.Add(label_OpenSourceTip_2);
            groupBox_About.Controls.Add(label_OpenSourceTip_1);
            groupBox_About.Controls.Add(linkLabel_GitHub);
            groupBox_About.Controls.Add(label_SelfIntroduce);
            groupBox_About.Controls.Add(label_NowVersionDevelopers);
            groupBox_About.Controls.Add(label_NowVersionDevelopersTip);
            groupBox_About.Controls.Add(label_NowVersionNumber);
            groupBox_About.Controls.Add(label_NowVersionTip);
            groupBox_About.Controls.Add(label_AppName);
            groupBox_About.Controls.Add(pictureBox_AppIcon);
            groupBox_About.Font = new Font("HarmonyOS Sans SC", 11.9999981F, FontStyle.Bold, GraphicsUnit.Point, 134);
            groupBox_About.Location = new Point(19, 42);
            groupBox_About.Margin = new Padding(10);
            groupBox_About.Name = "groupBox_About";
            groupBox_About.Padding = new Padding(10);
            groupBox_About.Size = new Size(720, 408);
            groupBox_About.TabIndex = 11;
            groupBox_About.TabStop = false;
            groupBox_About.Text = "关于";
            // 
            // linkLabel_QQGroup_2
            // 
            linkLabel_QQGroup_2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel_QQGroup_2.AutoSize = true;
            linkLabel_QQGroup_2.Font = new Font("HarmonyOS Sans SC", 9F);
            linkLabel_QQGroup_2.Location = new Point(556, 325);
            linkLabel_QQGroup_2.Margin = new Padding(0);
            linkLabel_QQGroup_2.Name = "linkLabel_QQGroup_2";
            linkLabel_QQGroup_2.Size = new Size(107, 16);
            linkLabel_QQGroup_2.TabIndex = 25;
            linkLabel_QQGroup_2.TabStop = true;
            linkLabel_QQGroup_2.Text = "1022797551 (2群)";
            linkLabel_QQGroup_2.LinkClicked += linkLabel_QQGroup_2_LinkClicked;
            // 
            // label_ThankHelpFromTip_2
            // 
            label_ThankHelpFromTip_2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label_ThankHelpFromTip_2.AutoSize = true;
            label_ThankHelpFromTip_2.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_ThankHelpFromTip_2.Location = new Point(292, 341);
            label_ThankHelpFromTip_2.Margin = new Padding(0);
            label_ThankHelpFromTip_2.Name = "label_ThankHelpFromTip_2";
            label_ThankHelpFromTip_2.Size = new Size(163, 16);
            label_ThankHelpFromTip_2.TabIndex = 24;
            label_ThankHelpFromTip_2.Text = "项目对于本项目的帮助与支持";
            // 
            // linkLabel_NodeJsProject
            // 
            linkLabel_NodeJsProject.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel_NodeJsProject.AutoSize = true;
            linkLabel_NodeJsProject.Font = new Font("HarmonyOS Sans SC", 9F);
            linkLabel_NodeJsProject.Location = new Point(68, 341);
            linkLabel_NodeJsProject.Margin = new Padding(0);
            linkLabel_NodeJsProject.Name = "linkLabel_NodeJsProject";
            linkLabel_NodeJsProject.Size = new Size(224, 16);
            linkLabel_NodeJsProject.TabIndex = 23;
            linkLabel_NodeJsProject.TabStop = true;
            linkLabel_NodeJsProject.Text = "dmlgzs/StarResonanceDamageCounter";
            linkLabel_NodeJsProject.LinkClicked += linkLabel_NodeJsProject_LinkClicked;
            // 
            // label_ThankHelpFromTip_1
            // 
            label_ThankHelpFromTip_1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label_ThankHelpFromTip_1.AutoSize = true;
            label_ThankHelpFromTip_1.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_ThankHelpFromTip_1.Location = new Point(13, 341);
            label_ThankHelpFromTip_1.Margin = new Padding(3, 0, 0, 0);
            label_ThankHelpFromTip_1.Name = "label_ThankHelpFromTip_1";
            label_ThankHelpFromTip_1.Size = new Size(55, 16);
            label_ThankHelpFromTip_1.TabIndex = 22;
            label_ThankHelpFromTip_1.Text = "在此感谢";
            // 
            // label_Copyright
            // 
            label_Copyright.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label_Copyright.AutoSize = true;
            label_Copyright.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Copyright.Location = new Point(13, 366);
            label_Copyright.Name = "label_Copyright";
            label_Copyright.Size = new Size(320, 32);
            label_Copyright.TabIndex = 21;
            label_Copyright.Text = "Copyright (C) 2025 anying1073/StarResonanceDps Team\r\nPowered by .NET 8, Licensed under the GNU AGPL v3.";
            // 
            // linkLabel_QQGroup_1
            // 
            linkLabel_QQGroup_1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel_QQGroup_1.AutoSize = true;
            linkLabel_QQGroup_1.Font = new Font("HarmonyOS Sans SC", 9F);
            linkLabel_QQGroup_1.Location = new Point(456, 325);
            linkLabel_QQGroup_1.Margin = new Padding(0);
            linkLabel_QQGroup_1.Name = "linkLabel_QQGroup_1";
            linkLabel_QQGroup_1.Size = new Size(100, 16);
            linkLabel_QQGroup_1.TabIndex = 20;
            linkLabel_QQGroup_1.TabStop = true;
            linkLabel_QQGroup_1.Text = "678150498 (1群)";
            linkLabel_QQGroup_1.LinkClicked += linkLabel_QQGroup_1_LinkClicked;
            // 
            // label_OpenSourceTip_2
            // 
            label_OpenSourceTip_2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label_OpenSourceTip_2.AutoSize = true;
            label_OpenSourceTip_2.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_OpenSourceTip_2.Location = new Point(113, 325);
            label_OpenSourceTip_2.Margin = new Padding(0);
            label_OpenSourceTip_2.Name = "label_OpenSourceTip_2";
            label_OpenSourceTip_2.Size = new Size(343, 16);
            label_OpenSourceTip_2.TabIndex = 19;
            label_OpenSourceTip_2.Text = "中开源，如在使用中遇到问题，或是想要寻求游戏伙伴请加群：";
            // 
            // label_OpenSourceTip_1
            // 
            label_OpenSourceTip_1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label_OpenSourceTip_1.AutoSize = true;
            label_OpenSourceTip_1.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_OpenSourceTip_1.Location = new Point(13, 325);
            label_OpenSourceTip_1.Margin = new Padding(3, 0, 0, 0);
            label_OpenSourceTip_1.Name = "label_OpenSourceTip_1";
            label_OpenSourceTip_1.Size = new Size(55, 16);
            label_OpenSourceTip_1.TabIndex = 18;
            label_OpenSourceTip_1.Text = "本项目于";
            // 
            // linkLabel_GitHub
            // 
            linkLabel_GitHub.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel_GitHub.AutoSize = true;
            linkLabel_GitHub.Font = new Font("HarmonyOS Sans SC", 9F);
            linkLabel_GitHub.Location = new Point(68, 325);
            linkLabel_GitHub.Margin = new Padding(0);
            linkLabel_GitHub.Name = "linkLabel_GitHub";
            linkLabel_GitHub.Size = new Size(45, 16);
            linkLabel_GitHub.TabIndex = 17;
            linkLabel_GitHub.TabStop = true;
            linkLabel_GitHub.Text = "GitHub";
            linkLabel_GitHub.LinkClicked += linkLabel_GitHub_LinkClicked;
            // 
            // label_SelfIntroduce
            // 
            label_SelfIntroduce.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label_SelfIntroduce.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_SelfIntroduce.Location = new Point(99, 66);
            label_SelfIntroduce.Name = "label_SelfIntroduce";
            label_SelfIntroduce.Size = new Size(608, 49);
            label_SelfIntroduce.TabIndex = 16;
            label_SelfIntroduce.Text = "一款专为《星痕共鸣》玩家打造的战斗数据统计工具。\r\n该工具无需修改游戏客户端，不违反游戏服务条款。该工具旨在帮助玩家更好地理解战斗数据，减少无效提升，提升游戏体验。使用该工具前，请确保不会将数据结果用于战力歧视等破坏游戏社区环境的行为。";
            // 
            // label_NowVersionDevelopers
            // 
            label_NowVersionDevelopers.AutoSize = true;
            label_NowVersionDevelopers.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_NowVersionDevelopers.Location = new Point(13, 226);
            label_NowVersionDevelopers.Name = "label_NowVersionDevelopers";
            label_NowVersionDevelopers.Size = new Size(209, 64);
            label_NowVersionDevelopers.TabIndex = 15;
            label_NowVersionDevelopers.Text = "惊奇猫猫盒 (anying1073: 项目发起者)\r\n露詩 (Rocy-June)\r\n青岚宗王腾\r\n尻鸭 (ihonliu)";
            // 
            // label_NowVersionDevelopersTip
            // 
            label_NowVersionDevelopersTip.AutoSize = true;
            label_NowVersionDevelopersTip.Location = new Point(13, 197);
            label_NowVersionDevelopersTip.Margin = new Padding(3, 0, 3, 10);
            label_NowVersionDevelopersTip.Name = "label_NowVersionDevelopersTip";
            label_NowVersionDevelopersTip.Size = new Size(282, 21);
            label_NowVersionDevelopersTip.TabIndex = 14;
            label_NowVersionDevelopersTip.Text = "当前版本开发者们（排名不分先后）：";
            // 
            // label_NowVersionNumber
            // 
            label_NowVersionNumber.AutoSize = true;
            label_NowVersionNumber.Font = new Font("HarmonyOS Sans SC", 9F);
            label_NowVersionNumber.Location = new Point(13, 163);
            label_NowVersionNumber.Name = "label_NowVersionNumber";
            label_NowVersionNumber.Size = new Size(31, 16);
            label_NowVersionNumber.TabIndex = 13;
            label_NowVersionNumber.Text = "-.-.-";
            // 
            // label_NowVersionTip
            // 
            label_NowVersionTip.AutoSize = true;
            label_NowVersionTip.Location = new Point(13, 132);
            label_NowVersionTip.Margin = new Padding(3, 0, 3, 10);
            label_NowVersionTip.Name = "label_NowVersionTip";
            label_NowVersionTip.Size = new Size(106, 21);
            label_NowVersionTip.TabIndex = 12;
            label_NowVersionTip.Text = "当前版本号：";
            // 
            // label_AppName
            // 
            label_AppName.AutoSize = true;
            label_AppName.Location = new Point(99, 35);
            label_AppName.Margin = new Padding(3, 0, 3, 10);
            label_AppName.Name = "label_AppName";
            label_AppName.Size = new Size(89, 21);
            label_AppName.TabIndex = 11;
            label_AppName.Text = "别查我DPS";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(758, 469);
            Controls.Add(groupBox_About);
            Controls.Add(pageHeader_MainHeader);
            Dark = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            Mode = AntdUI.TAMode.Dark;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DPS 统计工具";
            ForeColorChanged += MainForm_ForeColorChanged;
            ((System.ComponentModel.ISupportInitialize)pictureBox_AppIcon).EndInit();
            groupBox_About.ResumeLayout(false);
            groupBox_About.PerformLayout();
            ResumeLayout(false);
        }
        private AntdUI.PageHeader pageHeader_MainHeader;
        private PictureBox pictureBox_AppIcon;
        private GroupBox groupBox_About;
        private Label label_AppName;
        private Label label_NowVersionNumber;
        private Label label_NowVersionTip;
        private Label label_NowVersionDevelopersTip;
        private Label label_NowVersionDevelopers;
        private Label label_SelfIntroduce;
        private Label label_OpenSourceTip_2;
        private Label label_OpenSourceTip_1;
        private LinkLabel linkLabel_GitHub;
        private LinkLabel linkLabel_QQGroup_1;
        private Label label_Copyright;
        private Label label_ThankHelpFromTip_2;
        private LinkLabel linkLabel_NodeJsProject;
        private Label label_ThankHelpFromTip_1;
        private LinkLabel linkLabel_QQGroup_2;
    }
}
