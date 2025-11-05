namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    partial class HistoricalBattlesForm
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
            AntdUI.SegmentedItem segmentedItem1 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem2 = new AntdUI.SegmentedItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HistoricalBattlesForm));
            pageHeader1 = new AntdUI.PageHeader();
            label1 = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            select2 = new AntdUI.Select();
            button1 = new AntdUI.Button();
            button3 = new AntdUI.Button();
            select1 = new AntdUI.Select();
            panel3 = new AntdUI.Panel();
            segmented1 = new AntdUI.Segmented();
            table_DpsDetailDataTable = new AntdUI.Table();
            splitter1 = new AntdUI.Splitter();
            panel1 = new AntdUI.Panel();
            label5 = new AntdUI.Label();
            label2 = new AntdUI.Label();
            label3 = new AntdUI.Label();
            label6 = new AntdUI.Label();
            TeamTotalDamageLabel = new AntdUI.Label();
            TeamTotalHealingLabel = new AntdUI.Label();
            TeamTotalTakenDamageLabel = new AntdUI.Label();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter1).BeginInit();
            splitter1.Panel1.SuspendLayout();
            splitter1.Panel2.SuspendLayout();
            splitter1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(label1);
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Mode = AntdUI.TAMode.Dark;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.Size = new Size(1130, 52);
            pageHeader1.TabIndex = 29;
            pageHeader1.Text = "";
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.ColorScheme = AntdUI.TAMode.Dark;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("SAO UI TT", 12F);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(1130, 52);
            label1.TabIndex = 26;
            label1.Text = "HistoricalBattles";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.MouseDown += label1_MouseDown;
            // 
            // panel6
            // 
            panel6.Controls.Add(select2);
            panel6.Controls.Add(button1);
            panel6.Controls.Add(button3);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 1016);
            panel6.Name = "panel6";
            panel6.Radius = 3;
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1130, 106);
            panel6.TabIndex = 33;
            panel6.Text = "panel6";
            // 
            // select2
            // 
            select2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            select2.DropDownTextAlign = AntdUI.TAlign.Top;
            select2.Items.AddRange(new object[] { "按伤害排序", "按治疗排序", "按承伤排序" });
            select2.List = true;
            select2.Location = new Point(881, 34);
            select2.Name = "select2";
            select2.Placement = AntdUI.TAlignFrom.Top;
            select2.Radius = 3;
            select2.SelectedIndex = 0;
            select2.SelectedValue = "按伤害排序";
            select2.SelectionStart = 5;
            select2.Size = new Size(237, 47);
            select2.TabIndex = 29;
            select2.Text = "按伤害排序";
            select2.SelectedIndexChanged += select2_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top;
            button1.Ghost = true;
            button1.Icon = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Normal;
            button1.IconHover = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Hover;
            button1.IconPosition = AntdUI.TAlignMini.None;
            button1.IconRatio = 1.5F;
            button1.Location = new Point(450, 9);
            button1.Name = "button1";
            button1.Size = new Size(57, 98);
            button1.TabIndex = 28;
            button1.Click += button1_Click;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top;
            button3.Ghost = true;
            button3.Icon = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Normal;
            button3.IconHover = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Hover;
            button3.IconPosition = AntdUI.TAlignMini.None;
            button3.IconRatio = 1.5F;
            button3.Location = new Point(621, 11);
            button3.Name = "button3";
            button3.Size = new Size(57, 94);
            button3.TabIndex = 1;
            button3.Click += button3_Click;
            // 
            // select1
            // 
            select1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            select1.DropDownTextAlign = AntdUI.TAlign.Bottom;
            select1.List = true;
            select1.Location = new Point(551, 19);
            select1.Name = "select1";
            select1.Placement = AntdUI.TAlignFrom.Bottom;
            select1.Radius = 3;
            select1.Size = new Size(567, 56);
            select1.TabIndex = 28;
            select1.SelectedIndexChanged += select1_SelectedIndexChanged;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Transparent;
            panel3.Controls.Add(segmented1);
            panel3.Location = new Point(12, 10);
            panel3.Name = "panel3";
            panel3.Radius = 500;
            panel3.Shadow = 6;
            panel3.ShadowOpacityHover = 0F;
            panel3.Size = new Size(496, 65);
            panel3.TabIndex = 34;
            panel3.Text = "panel3";
            // 
            // segmented1
            // 
            segmented1.BarBg = true;
            segmented1.BarPosition = AntdUI.TAlignMini.Bottom;
            segmented1.BarSize = 0F;
            segmented1.Dock = DockStyle.Fill;
            segmented1.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            segmented1.Full = true;
            segmented1.IconGap = 0F;
            segmentedItem1.Text = "单次伤害记录";
            segmentedItem2.Text = "全程伤害记录";
            segmented1.Items.Add(segmentedItem1);
            segmented1.Items.Add(segmentedItem2);
            segmented1.Location = new Point(9, 9);
            segmented1.Name = "segmented1";
            segmented1.Round = true;
            segmented1.SelectIndex = 0;
            segmented1.Size = new Size(478, 47);
            segmented1.TabIndex = 16;
            segmented1.Text = "segmented1";
            segmented1.SelectIndexChanged += segmented1_SelectIndexChanged;
            // 
            // table_DpsDetailDataTable
            // 
            table_DpsDetailDataTable.BackColor = Color.White;
            table_DpsDetailDataTable.BackgroundImageLayout = ImageLayout.Zoom;
            table_DpsDetailDataTable.Dock = DockStyle.Fill;
            table_DpsDetailDataTable.EmptyImage = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Hover;
            table_DpsDetailDataTable.FixedHeader = false;
            table_DpsDetailDataTable.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            table_DpsDetailDataTable.Gap = 8;
            table_DpsDetailDataTable.Gaps = new Size(8, 8);
            table_DpsDetailDataTable.Location = new Point(0, 0);
            table_DpsDetailDataTable.Name = "table_DpsDetailDataTable";
            table_DpsDetailDataTable.RowHeight = 40;
            table_DpsDetailDataTable.RowSelectedBg = Color.FromArgb(174, 212, 251);
            table_DpsDetailDataTable.Size = new Size(1130, 724);
            table_DpsDetailDataTable.TabIndex = 35;
            table_DpsDetailDataTable.Text = "table1";
            table_DpsDetailDataTable.CellClick += table_DpsDetailDataTable_CellClick;
            // 
            // splitter1
            // 
            splitter1.BackColor = Color.White;
            splitter1.CollapsePanel = AntdUI.Splitter.ADCollapsePanel.Panel1;
            splitter1.Dock = DockStyle.Fill;
            splitter1.FixedPanel = FixedPanel.Panel1;
            splitter1.Location = new Point(0, 52);
            splitter1.Name = "splitter1";
            splitter1.Orientation = Orientation.Horizontal;
            // 
            // splitter1.Panel1
            // 
            splitter1.Panel1.BackColor = Color.White;
            splitter1.Panel1.Controls.Add(panel1);
            splitter1.Panel1.Controls.Add(select1);
            splitter1.Panel1.Controls.Add(panel3);
            // 
            // splitter1.Panel2
            // 
            splitter1.Panel2.BackColor = Color.White;
            splitter1.Panel2.Controls.Add(table_DpsDetailDataTable);
            splitter1.Size = new Size(1130, 964);
            splitter1.SplitterDistance = 234;
            splitter1.SplitterWidth = 6;
            splitter1.TabIndex = 36;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Back = Color.FromArgb(103, 174, 246);
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(TeamTotalDamageLabel);
            panel1.Controls.Add(TeamTotalHealingLabel);
            panel1.Controls.Add(TeamTotalTakenDamageLabel);
            panel1.Location = new Point(21, 82);
            panel1.Name = "panel1";
            panel1.Shadow = 6;
            panel1.Size = new Size(1097, 152);
            panel1.TabIndex = 35;
            panel1.Text = "panel1";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.BackColor = Color.Transparent;
            label5.ColorScheme = AntdUI.TAMode.Dark;
            label5.Font = new Font("HarmonyOS Sans SC", 9F);
            label5.Location = new Point(810, 71);
            label5.Name = "label5";
            label5.Size = new Size(123, 45);
            label5.TabIndex = 26;
            label5.Text = "团队总承伤";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top;
            label2.BackColor = Color.Transparent;
            label2.ColorScheme = AntdUI.TAMode.Dark;
            label2.Font = new Font("HarmonyOS Sans SC", 9F);
            label2.Location = new Point(409, 71);
            label2.Name = "label2";
            label2.Size = new Size(115, 45);
            label2.TabIndex = 24;
            label2.Text = "团队总治疗";
            // 
            // label3
            // 
            label3.BackColor = Color.Transparent;
            label3.ColorScheme = AntdUI.TAMode.Dark;
            label3.Font = new Font("HarmonyOS Sans SC", 9F);
            label3.Location = new Point(22, 71);
            label3.Name = "label3";
            label3.Size = new Size(114, 45);
            label3.TabIndex = 22;
            label3.Text = "团队总伤害";
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top;
            label6.BackColor = Color.Transparent;
            label6.ColorScheme = AntdUI.TAMode.Dark;
            label6.Font = new Font("HarmonyOS Sans SC Medium", 10F, FontStyle.Bold);
            label6.Location = new Point(402, 22);
            label6.Name = "label6";
            label6.Size = new Size(271, 30);
            label6.TabIndex = 22;
            label6.Text = "团队信息";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TeamTotalDamageLabel
            // 
            TeamTotalDamageLabel.BackColor = Color.Transparent;
            TeamTotalDamageLabel.ColorScheme = AntdUI.TAMode.Dark;
            TeamTotalDamageLabel.Font = new Font("SAO Welcome TT", 10.499999F);
            TeamTotalDamageLabel.Location = new Point(114, 78);
            TeamTotalDamageLabel.Name = "TeamTotalDamageLabel";
            TeamTotalDamageLabel.Size = new Size(174, 30);
            TeamTotalDamageLabel.TabIndex = 23;
            TeamTotalDamageLabel.Text = "0";
            TeamTotalDamageLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // TeamTotalHealingLabel
            // 
            TeamTotalHealingLabel.Anchor = AnchorStyles.Top;
            TeamTotalHealingLabel.BackColor = Color.Transparent;
            TeamTotalHealingLabel.ColorScheme = AntdUI.TAMode.Dark;
            TeamTotalHealingLabel.Font = new Font("SAO Welcome TT", 10.499999F);
            TeamTotalHealingLabel.Location = new Point(499, 78);
            TeamTotalHealingLabel.Name = "TeamTotalHealingLabel";
            TeamTotalHealingLabel.Size = new Size(174, 30);
            TeamTotalHealingLabel.TabIndex = 25;
            TeamTotalHealingLabel.Text = "0";
            TeamTotalHealingLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // TeamTotalTakenDamageLabel
            // 
            TeamTotalTakenDamageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TeamTotalTakenDamageLabel.BackColor = Color.Transparent;
            TeamTotalTakenDamageLabel.ColorScheme = AntdUI.TAMode.Dark;
            TeamTotalTakenDamageLabel.Font = new Font("SAO Welcome TT", 10.499999F);
            TeamTotalTakenDamageLabel.Location = new Point(902, 78);
            TeamTotalTakenDamageLabel.Name = "TeamTotalTakenDamageLabel";
            TeamTotalTakenDamageLabel.Size = new Size(174, 30);
            TeamTotalTakenDamageLabel.TabIndex = 27;
            TeamTotalTakenDamageLabel.Text = "0";
            TeamTotalTakenDamageLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // HistoricalBattlesForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1130, 1122);
            Controls.Add(splitter1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Dark = true;
            EnableHitTest = false;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Mode = AntdUI.TAMode.Dark;
            Name = "HistoricalBattlesForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HistoricalBattlesForm";
            Load += HistoricalBattlesForm_Load;
            ForeColorChanged += HistoricalBattlesForm_ForeColorChanged;
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            panel3.ResumeLayout(false);
            splitter1.Panel1.ResumeLayout(false);
            splitter1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitter1).EndInit();
            splitter1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label label1;
        private AntdUI.Panel panel6;
        private AntdUI.Button button3;
        private AntdUI.Select select1;
        private AntdUI.Panel panel3;
        private AntdUI.Segmented segmented1;
        private AntdUI.Table table_DpsDetailDataTable;
        private AntdUI.Splitter splitter1;
        private AntdUI.Button button1;
        private AntdUI.Panel panel1;
        private AntdUI.Label TeamTotalHealingLabel;
        private AntdUI.Label label2;
        private AntdUI.Label TeamTotalDamageLabel;
        private AntdUI.Label label3;
        private AntdUI.Label label6;
        private AntdUI.Label TeamTotalTakenDamageLabel;
        private AntdUI.Label label5;
        private AntdUI.Select select2;
    }
}