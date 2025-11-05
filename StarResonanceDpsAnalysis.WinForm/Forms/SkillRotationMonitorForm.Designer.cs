namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    partial class SkillRotationMonitorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            button2 = new AntdUI.Button();
            button1 = new AntdUI.Button();
            panel1 = new AntdUI.Panel();
            select1 = new AntdUI.Select();
            button3 = new AntdUI.Button();
            button4 = new AntdUI.Button();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(TitleText);
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Mode = AntdUI.TAMode.Dark;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.Size = new Size(1089, 52);
            pageHeader1.TabIndex = 16;
            pageHeader1.Text = "";
            // 
            // TitleText
            // 
            TitleText.BackColor = Color.Transparent;
            TitleText.ColorScheme = AntdUI.TAMode.Dark;
            TitleText.Dock = DockStyle.Fill;
            TitleText.Font = new Font("SAO Welcome TT", 12F, FontStyle.Bold);
            TitleText.Location = new Point(0, 0);
            TitleText.Name = "TitleText";
            TitleText.Size = new Size(1089, 52);
            TitleText.TabIndex = 26;
            TitleText.Text = "Skill Loop";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel6
            // 
            panel6.Controls.Add(button2);
            panel6.Controls.Add(button1);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 837);
            panel6.Name = "panel6";
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1089, 90);
            panel6.TabIndex = 31;
            panel6.Text = "panel6";
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom;
            button2.Ghost = true;
            button2.Icon = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Normal;
            button2.IconHover = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Hover;
            button2.IconPosition = AntdUI.TAlignMini.None;
            button2.IconRatio = 1.5F;
            button2.Location = new Point(585, 12);
            button2.Name = "button2";
            button2.Size = new Size(57, 78);
            button2.TabIndex = 1;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom;
            button1.Ghost = true;
            button1.Icon = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Normal;
            button1.IconHover = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Hover;
            button1.IconPosition = AntdUI.TAlignMini.None;
            button1.IconRatio = 1.5F;
            button1.Location = new Point(441, 12);
            button1.Name = "button1";
            button1.Size = new Size(57, 78);
            button1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Location = new Point(12, 136);
            panel1.Name = "panel1";
            panel1.Size = new Size(1065, 695);
            panel1.TabIndex = 32;
            panel1.Text = "panel1";
            // 
            // select1
            // 
            select1.Location = new Point(12, 64);
            select1.Name = "select1";
            select1.Radius = 3;
            select1.SelectionStart = 5;
            select1.Size = new Size(289, 50);
            select1.TabIndex = 33;
            select1.Text = "玩家选择：";
            // 
            // button3
            // 
            button3.Location = new Point(327, 64);
            button3.Name = "button3";
            button3.Size = new Size(153, 50);
            button3.TabIndex = 34;
            button3.Text = "开始检测循环";
            button3.Type = AntdUI.TTypeMini.Primary;
            // 
            // button4
            // 
            button4.Location = new Point(489, 64);
            button4.Name = "button4";
            button4.Size = new Size(153, 50);
            button4.TabIndex = 35;
            button4.Text = "清空历史";
            button4.Type = AntdUI.TTypeMini.Primary;
            // 
            // SkillRotationMonitorForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1089, 927);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(select1);
            Controls.Add(panel1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Margin = new Padding(5, 4, 5, 4);
            MaximizeBox = false;
            Name = "SkillRotationMonitorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "技能释放循环监测";
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label TitleText;
        private AntdUI.Panel panel6;
        private AntdUI.Button button2;
        private AntdUI.Button button1;
        private AntdUI.Panel panel1;
        private AntdUI.Select select1;
        private AntdUI.Button button3;
        private AntdUI.Button button4;
    }
}