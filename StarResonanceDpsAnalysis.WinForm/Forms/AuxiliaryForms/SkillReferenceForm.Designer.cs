namespace StarResonanceDpsAnalysis.WinForm.Forms.AuxiliaryForms
{
    partial class SkillReferenceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkillReferenceForm));
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            button2 = new AntdUI.Button();
            button1 = new AntdUI.Button();
            table_DpsDetailDataTable = new AntdUI.Table();
            divider1 = new AntdUI.Divider();
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
            pageHeader1.Size = new Size(1101, 52);
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
            TitleText.Size = new Size(1101, 52);
            TitleText.TabIndex = 26;
            TitleText.Text = "Skill Reference";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            TitleText.MouseDown += TitleText_MouseDown;
            // 
            // panel6
            // 
            panel6.Controls.Add(button2);
            panel6.Controls.Add(button1);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 936);
            panel6.Name = "panel6";
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1101, 90);
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
            button2.Location = new Point(596, 9);
            button2.Name = "button2";
            button2.Size = new Size(57, 78);
            button2.TabIndex = 1;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom;
            button1.Ghost = true;
            button1.Icon = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Normal;
            button1.IconHover = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Hover;
            button1.IconPosition = AntdUI.TAlignMini.None;
            button1.IconRatio = 1.5F;
            button1.Location = new Point(442, 9);
            button1.Name = "button1";
            button1.Size = new Size(57, 78);
            button1.TabIndex = 0;
            button1.Click += button1_Click;
            // 
            // table_DpsDetailDataTable
            // 
            table_DpsDetailDataTable.BackgroundImageLayout = ImageLayout.Zoom;
            table_DpsDetailDataTable.Dock = DockStyle.Fill;
            table_DpsDetailDataTable.EmptyImage = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Hover;
            table_DpsDetailDataTable.FixedHeader = false;
            table_DpsDetailDataTable.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            table_DpsDetailDataTable.Gap = 8;
            table_DpsDetailDataTable.Gaps = new Size(8, 8);
            table_DpsDetailDataTable.Location = new Point(0, 106);
            table_DpsDetailDataTable.Name = "table_DpsDetailDataTable";
            table_DpsDetailDataTable.RowHeight = 40;
            table_DpsDetailDataTable.RowSelectedBg = Color.FromArgb(174, 212, 251);
            table_DpsDetailDataTable.Size = new Size(1101, 830);
            table_DpsDetailDataTable.TabIndex = 32;
            table_DpsDetailDataTable.Text = "table1";
            // 
            // divider1
            // 
            divider1.Dock = DockStyle.Top;
            divider1.Font = new Font("HarmonyOS Sans SC", 9F);
            divider1.Location = new Point(0, 52);
            divider1.Name = "divider1";
            divider1.Size = new Size(1101, 54);
            divider1.TabIndex = 33;
            divider1.Text = "玩家 ** 技能伤害参考";
            // 
            // SkillReferenceForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1101, 1026);
            Controls.Add(table_DpsDetailDataTable);
            Controls.Add(divider1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SkillReferenceForm";
            Text = "SkillReferenceForm";
            Load += SkillReferenceForm_Load;
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
        private AntdUI.Table table_DpsDetailDataTable;
        private AntdUI.Divider divider1;
    }
}