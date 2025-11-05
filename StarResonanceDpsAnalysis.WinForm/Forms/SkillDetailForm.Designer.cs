namespace StarResonanceDpsAnalysis.WinForm.Control
{
    partial class SkillDetailForm
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
            components = new System.ComponentModel.Container();
            AntdUI.SegmentedItem segmentedItem1 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem2 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem3 = new AntdUI.SegmentedItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkillDetailForm));
            table_DpsDetailDataTable = new AntdUI.Table();
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            segmented1 = new AntdUI.Segmented();
            NickNameText = new AntdUI.Label();
            divider1 = new AntdUI.Divider();
            panel1 = new AntdUI.Panel();
            LuckyRate = new AntdUI.Label();
            TotalDpsText = new AntdUI.Label();
            NumberCriticalHitsLabel = new AntdUI.Label();
            CritRateText = new AntdUI.Label();
            label5 = new AntdUI.Label();
            NumberCriticalHitsText = new AntdUI.Label();
            NumberHitsLabel = new AntdUI.Label();
            label2 = new AntdUI.Label();
            TotalDamageText = new AntdUI.Label();
            label17 = new AntdUI.Label();
            label4 = new AntdUI.Label();
            label1 = new AntdUI.Label();
            label3 = new AntdUI.Label();
            panel2 = new AntdUI.Panel();
            BeatenLabel = new AntdUI.Label();
            label13 = new AntdUI.Label();
            AvgDamageText = new AntdUI.Label();
            LuckyTimesLabel = new AntdUI.Label();
            CritDamageText = new AntdUI.Label();
            label14 = new AntdUI.Label();
            LuckyDamageText = new AntdUI.Label();
            label9 = new AntdUI.Label();
            label7 = new AntdUI.Label();
            NormalDamageText = new AntdUI.Label();
            label8 = new AntdUI.Label();
            label6 = new AntdUI.Label();
            divider2 = new AntdUI.Divider();
            label19 = new AntdUI.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            UidText = new AntdUI.Label();
            PowerText = new AntdUI.Label();
            panel3 = new AntdUI.Panel();
            panel5 = new AntdUI.Panel();
            Rank_levelLabel = new AntdUI.Label();
            LevelLabel = new AntdUI.Label();
            select1 = new AntdUI.Select();
            panel6 = new AntdUI.Panel();
            button2 = new AntdUI.Button();
            button1 = new AntdUI.Button();
            splitter1 = new AntdUI.Splitter();
            collapse1 = new AntdUI.Collapse();
            collapseItem1 = new AntdUI.CollapseItem();
            collapseItem2 = new AntdUI.CollapseItem();
            collapseItem3 = new AntdUI.CollapseItem();
            splitter2 = new AntdUI.Splitter();
            label10 = new AntdUI.Label();
            pageHeader1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel5.SuspendLayout();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter1).BeginInit();
            splitter1.Panel1.SuspendLayout();
            splitter1.Panel2.SuspendLayout();
            splitter1.SuspendLayout();
            collapse1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter2).BeginInit();
            splitter2.Panel1.SuspendLayout();
            splitter2.Panel2.SuspendLayout();
            splitter2.SuspendLayout();
            SuspendLayout();
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
            table_DpsDetailDataTable.Location = new Point(0, 0);
            table_DpsDetailDataTable.Name = "table_DpsDetailDataTable";
            table_DpsDetailDataTable.RowHeight = 40;
            table_DpsDetailDataTable.RowSelectedBg = Color.FromArgb(174, 212, 251);
            table_DpsDetailDataTable.Size = new Size(1284, 917);
            table_DpsDetailDataTable.TabIndex = 14;
            table_DpsDetailDataTable.Text = "table1";
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
            pageHeader1.Size = new Size(1761, 52);
            pageHeader1.TabIndex = 15;
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
            TitleText.Size = new Size(1761, 52);
            TitleText.TabIndex = 26;
            TitleText.Text = "Skill Breakdown";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            TitleText.MouseDown += TitleText_MouseDown;
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
            segmentedItem1.Text = "技能伤害分析";
            segmentedItem2.Text = "技能治疗分析";
            segmentedItem3.Text = "人物承伤分析";
            segmented1.Items.Add(segmentedItem1);
            segmented1.Items.Add(segmentedItem2);
            segmented1.Items.Add(segmentedItem3);
            segmented1.Location = new Point(9, 9);
            segmented1.Name = "segmented1";
            segmented1.Round = true;
            segmented1.SelectIndex = 0;
            segmented1.Size = new Size(478, 47);
            segmented1.TabIndex = 16;
            segmented1.Text = "segmented1";
            segmented1.SelectIndexChanged += segmented1_SelectIndexChanged;
            // 
            // NickNameText
            // 
            NickNameText.BackColor = Color.Transparent;
            NickNameText.Font = new Font("HarmonyOS Sans SC Medium", 12F, FontStyle.Bold, GraphicsUnit.Point, 134);
            NickNameText.Location = new Point(27, 33);
            NickNameText.Name = "NickNameText";
            NickNameText.Size = new Size(181, 45);
            NickNameText.TabIndex = 17;
            NickNameText.Text = "惊奇猫猫盒";
            // 
            // divider1
            // 
            divider1.Anchor = AnchorStyles.Top;
            divider1.BackColor = Color.Transparent;
            divider1.ColorScheme = AntdUI.TAMode.Dark;
            divider1.ColorSplit = Color.White;
            divider1.Location = new Point(277, 71);
            divider1.Name = "divider1";
            divider1.OrientationMargin = 0F;
            divider1.Size = new Size(60, 132);
            divider1.TabIndex = 20;
            divider1.Text = "";
            divider1.Vertical = true;
            // 
            // panel1
            // 
            panel1.Back = Color.FromArgb(103, 174, 246);
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(LuckyRate);
            panel1.Controls.Add(TotalDpsText);
            panel1.Controls.Add(NumberCriticalHitsLabel);
            panel1.Controls.Add(CritRateText);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(NumberCriticalHitsText);
            panel1.Controls.Add(NumberHitsLabel);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(TotalDamageText);
            panel1.Controls.Add(label17);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(divider1);
            panel1.Controls.Add(label3);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(10, 10);
            panel1.Name = "panel1";
            panel1.Shadow = 6;
            panel1.Size = new Size(608, 234);
            panel1.TabIndex = 21;
            panel1.Text = "panel1";
            // 
            // LuckyRate
            // 
            LuckyRate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LuckyRate.BackColor = Color.Transparent;
            LuckyRate.ColorScheme = AntdUI.TAMode.Dark;
            LuckyRate.Font = new Font("SAO Welcome TT", 10.499999F);
            LuckyRate.Location = new Point(481, 122);
            LuckyRate.Name = "LuckyRate";
            LuckyRate.Size = new Size(96, 30);
            LuckyRate.TabIndex = 25;
            LuckyRate.Text = "0";
            LuckyRate.TextAlign = ContentAlignment.MiddleRight;
            // 
            // TotalDpsText
            // 
            TotalDpsText.BackColor = Color.Transparent;
            TotalDpsText.ColorScheme = AntdUI.TAMode.Dark;
            TotalDpsText.Font = new Font("SAO Welcome TT", 10.499999F);
            TotalDpsText.Location = new Point(189, 122);
            TotalDpsText.Name = "TotalDpsText";
            TotalDpsText.Size = new Size(96, 30);
            TotalDpsText.TabIndex = 25;
            TotalDpsText.Text = "0";
            TotalDpsText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // NumberCriticalHitsLabel
            // 
            NumberCriticalHitsLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            NumberCriticalHitsLabel.BackColor = Color.Transparent;
            NumberCriticalHitsLabel.ColorScheme = AntdUI.TAMode.Dark;
            NumberCriticalHitsLabel.Font = new Font("SAO Welcome TT", 10.499999F);
            NumberCriticalHitsLabel.Location = new Point(498, 173);
            NumberCriticalHitsLabel.Name = "NumberCriticalHitsLabel";
            NumberCriticalHitsLabel.Size = new Size(79, 30);
            NumberCriticalHitsLabel.TabIndex = 23;
            NumberCriticalHitsLabel.Text = "0";
            NumberCriticalHitsLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // CritRateText
            // 
            CritRateText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CritRateText.BackColor = Color.Transparent;
            CritRateText.ColorScheme = AntdUI.TAMode.Dark;
            CritRateText.Font = new Font("SAO Welcome TT", 10.499999F);
            CritRateText.Location = new Point(481, 73);
            CritRateText.Name = "CritRateText";
            CritRateText.Size = new Size(96, 30);
            CritRateText.TabIndex = 23;
            CritRateText.Text = "0";
            CritRateText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.BackColor = Color.Transparent;
            label5.ColorScheme = AntdUI.TAMode.Dark;
            label5.Font = new Font("HarmonyOS Sans SC", 9F);
            label5.Location = new Point(329, 115);
            label5.Name = "label5";
            label5.Size = new Size(72, 45);
            label5.TabIndex = 24;
            label5.Text = "幸运率";
            // 
            // NumberCriticalHitsText
            // 
            NumberCriticalHitsText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            NumberCriticalHitsText.BackColor = Color.Transparent;
            NumberCriticalHitsText.ColorScheme = AntdUI.TAMode.Dark;
            NumberCriticalHitsText.Font = new Font("HarmonyOS Sans SC", 9F);
            NumberCriticalHitsText.Location = new Point(329, 166);
            NumberCriticalHitsText.Name = "NumberCriticalHitsText";
            NumberCriticalHitsText.Size = new Size(86, 45);
            NumberCriticalHitsText.TabIndex = 22;
            NumberCriticalHitsText.Text = "暴击次数";
            // 
            // NumberHitsLabel
            // 
            NumberHitsLabel.BackColor = Color.Transparent;
            NumberHitsLabel.ColorScheme = AntdUI.TAMode.Dark;
            NumberHitsLabel.Font = new Font("SAO Welcome TT", 10.499999F);
            NumberHitsLabel.Location = new Point(206, 173);
            NumberHitsLabel.Name = "NumberHitsLabel";
            NumberHitsLabel.Size = new Size(79, 30);
            NumberHitsLabel.TabIndex = 23;
            NumberHitsLabel.Text = "0";
            NumberHitsLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.ColorScheme = AntdUI.TAMode.Dark;
            label2.Font = new Font("HarmonyOS Sans SC", 9F);
            label2.Location = new Point(21, 116);
            label2.Name = "label2";
            label2.Size = new Size(72, 45);
            label2.TabIndex = 24;
            label2.Text = "秒伤";
            // 
            // TotalDamageText
            // 
            TotalDamageText.BackColor = Color.Transparent;
            TotalDamageText.ColorScheme = AntdUI.TAMode.Dark;
            TotalDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            TotalDamageText.Location = new Point(189, 73);
            TotalDamageText.Name = "TotalDamageText";
            TotalDamageText.Size = new Size(96, 30);
            TotalDamageText.TabIndex = 23;
            TotalDamageText.Text = "0";
            TotalDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            label17.BackColor = Color.Transparent;
            label17.ColorScheme = AntdUI.TAMode.Dark;
            label17.Font = new Font("HarmonyOS Sans SC", 9F);
            label17.Location = new Point(21, 166);
            label17.Name = "label17";
            label17.Size = new Size(91, 45);
            label17.TabIndex = 22;
            label17.Text = "命中次数";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.BackColor = Color.Transparent;
            label4.ColorScheme = AntdUI.TAMode.Dark;
            label4.Font = new Font("HarmonyOS Sans SC", 9F);
            label4.Location = new Point(329, 66);
            label4.Name = "label4";
            label4.Size = new Size(72, 45);
            label4.TabIndex = 22;
            label4.Text = "暴击率";
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.ColorScheme = AntdUI.TAMode.Dark;
            label1.Font = new Font("HarmonyOS Sans SC", 9F);
            label1.Location = new Point(21, 66);
            label1.Name = "label1";
            label1.Size = new Size(72, 45);
            label1.TabIndex = 22;
            label1.Text = "总伤害";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top;
            label3.BackColor = Color.Transparent;
            label3.ColorScheme = AntdUI.TAMode.Dark;
            label3.Font = new Font("HarmonyOS Sans SC Medium", 10.999999F, FontStyle.Bold);
            label3.Location = new Point(219, 21);
            label3.Name = "label3";
            label3.Size = new Size(182, 30);
            label3.TabIndex = 22;
            label3.Text = "伤害信息";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.Back = Color.FromArgb(103, 174, 246);
            panel2.BackColor = Color.Transparent;
            panel2.Controls.Add(BeatenLabel);
            panel2.Controls.Add(label13);
            panel2.Controls.Add(AvgDamageText);
            panel2.Controls.Add(LuckyTimesLabel);
            panel2.Controls.Add(CritDamageText);
            panel2.Controls.Add(label14);
            panel2.Controls.Add(LuckyDamageText);
            panel2.Controls.Add(label9);
            panel2.Controls.Add(label7);
            panel2.Controls.Add(NormalDamageText);
            panel2.Controls.Add(label8);
            panel2.Controls.Add(label6);
            panel2.Controls.Add(divider2);
            panel2.Controls.Add(label19);
            panel2.Location = new Point(663, 10);
            panel2.Name = "panel2";
            panel2.Shadow = 6;
            panel2.Size = new Size(607, 234);
            panel2.TabIndex = 22;
            panel2.Text = "C";
            // 
            // BeatenLabel
            // 
            BeatenLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BeatenLabel.BackColor = Color.Transparent;
            BeatenLabel.ColorScheme = AntdUI.TAMode.Dark;
            BeatenLabel.Font = new Font("SAO Welcome TT", 10.499999F);
            BeatenLabel.Location = new Point(481, 173);
            BeatenLabel.Name = "BeatenLabel";
            BeatenLabel.Size = new Size(79, 30);
            BeatenLabel.TabIndex = 27;
            BeatenLabel.Text = "0";
            BeatenLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            label13.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label13.BackColor = Color.Transparent;
            label13.ColorScheme = AntdUI.TAMode.Dark;
            label13.Font = new Font("HarmonyOS Sans SC", 9F);
            label13.Location = new Point(329, 166);
            label13.Name = "label13";
            label13.Size = new Size(91, 45);
            label13.TabIndex = 26;
            label13.Text = "挨打次数";
            // 
            // AvgDamageText
            // 
            AvgDamageText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AvgDamageText.BackColor = Color.Transparent;
            AvgDamageText.ColorScheme = AntdUI.TAMode.Dark;
            AvgDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            AvgDamageText.Location = new Point(481, 122);
            AvgDamageText.Name = "AvgDamageText";
            AvgDamageText.Size = new Size(79, 30);
            AvgDamageText.TabIndex = 25;
            AvgDamageText.Text = "0";
            AvgDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // LuckyTimesLabel
            // 
            LuckyTimesLabel.BackColor = Color.Transparent;
            LuckyTimesLabel.ColorScheme = AntdUI.TAMode.Dark;
            LuckyTimesLabel.Font = new Font("SAO Welcome TT", 10.499999F);
            LuckyTimesLabel.Location = new Point(189, 173);
            LuckyTimesLabel.Name = "LuckyTimesLabel";
            LuckyTimesLabel.Size = new Size(79, 30);
            LuckyTimesLabel.TabIndex = 25;
            LuckyTimesLabel.Text = "0";
            LuckyTimesLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // CritDamageText
            // 
            CritDamageText.BackColor = Color.Transparent;
            CritDamageText.ColorScheme = AntdUI.TAMode.Dark;
            CritDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            CritDamageText.Location = new Point(189, 122);
            CritDamageText.Name = "CritDamageText";
            CritDamageText.Size = new Size(79, 30);
            CritDamageText.TabIndex = 25;
            CritDamageText.Text = "0";
            CritDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label14
            // 
            label14.BackColor = Color.Transparent;
            label14.ColorScheme = AntdUI.TAMode.Dark;
            label14.Font = new Font("HarmonyOS Sans SC", 9F);
            label14.Location = new Point(22, 166);
            label14.Name = "label14";
            label14.Size = new Size(91, 45);
            label14.TabIndex = 24;
            label14.Text = "幸运次数";
            // 
            // LuckyDamageText
            // 
            LuckyDamageText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LuckyDamageText.BackColor = Color.Transparent;
            LuckyDamageText.ColorScheme = AntdUI.TAMode.Dark;
            LuckyDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            LuckyDamageText.Location = new Point(481, 73);
            LuckyDamageText.Name = "LuckyDamageText";
            LuckyDamageText.Size = new Size(79, 30);
            LuckyDamageText.TabIndex = 23;
            LuckyDamageText.Text = "0";
            LuckyDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label9.BackColor = Color.Transparent;
            label9.ColorScheme = AntdUI.TAMode.Dark;
            label9.Font = new Font("HarmonyOS Sans SC", 9F);
            label9.Location = new Point(329, 115);
            label9.Name = "label9";
            label9.Size = new Size(86, 45);
            label9.TabIndex = 24;
            label9.Text = "平均伤害";
            // 
            // label7
            // 
            label7.BackColor = Color.Transparent;
            label7.ColorScheme = AntdUI.TAMode.Dark;
            label7.Font = new Font("HarmonyOS Sans SC", 9F);
            label7.Location = new Point(22, 115);
            label7.Name = "label7";
            label7.Size = new Size(91, 45);
            label7.TabIndex = 24;
            label7.Text = "暴击伤害";
            // 
            // NormalDamageText
            // 
            NormalDamageText.BackColor = Color.Transparent;
            NormalDamageText.ColorScheme = AntdUI.TAMode.Dark;
            NormalDamageText.Font = new Font("SAO Welcome TT", 10.499999F);
            NormalDamageText.Location = new Point(189, 73);
            NormalDamageText.Name = "NormalDamageText";
            NormalDamageText.Size = new Size(79, 30);
            NormalDamageText.TabIndex = 23;
            NormalDamageText.Text = "0";
            NormalDamageText.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.BackColor = Color.Transparent;
            label8.ColorScheme = AntdUI.TAMode.Dark;
            label8.Font = new Font("HarmonyOS Sans SC", 9F);
            label8.Location = new Point(329, 66);
            label8.Name = "label8";
            label8.Size = new Size(86, 45);
            label8.TabIndex = 22;
            label8.Text = "幸运伤害";
            // 
            // label6
            // 
            label6.BackColor = Color.Transparent;
            label6.ColorScheme = AntdUI.TAMode.Dark;
            label6.Font = new Font("HarmonyOS Sans SC", 9F);
            label6.Location = new Point(22, 66);
            label6.Name = "label6";
            label6.Size = new Size(91, 45);
            label6.TabIndex = 22;
            label6.Text = "普通伤害";
            // 
            // divider2
            // 
            divider2.Anchor = AnchorStyles.Top;
            divider2.BackColor = Color.Transparent;
            divider2.ColorScheme = AntdUI.TAMode.Dark;
            divider2.ColorSplit = Color.White;
            divider2.Location = new Point(298, 72);
            divider2.Name = "divider2";
            divider2.OrientationMargin = 0F;
            divider2.Size = new Size(8, 131);
            divider2.TabIndex = 20;
            divider2.Text = "";
            divider2.Vertical = true;
            // 
            // label19
            // 
            label19.Anchor = AnchorStyles.Top;
            label19.BackColor = Color.Transparent;
            label19.ColorScheme = AntdUI.TAMode.Dark;
            label19.Font = new Font("HarmonyOS Sans SC Medium", 10.999999F, FontStyle.Bold);
            label19.Location = new Point(219, 21);
            label19.Name = "label19";
            label19.Size = new Size(182, 30);
            label19.TabIndex = 22;
            label19.Text = "伤害分布";
            label19.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 5000;
            timer1.Tick += timer1_Tick;
            // 
            // UidText
            // 
            UidText.BackColor = Color.Transparent;
            UidText.Font = new Font("HarmonyOS Sans SC", 9F);
            UidText.Location = new Point(213, 33);
            UidText.Name = "UidText";
            UidText.Prefix = "UID:";
            UidText.Size = new Size(157, 45);
            UidText.TabIndex = 23;
            UidText.Text = "123";
            // 
            // PowerText
            // 
            PowerText.BackColor = Color.Transparent;
            PowerText.Font = new Font("HarmonyOS Sans SC", 9F);
            PowerText.Location = new Point(376, 33);
            PowerText.Name = "PowerText";
            PowerText.Prefix = "战力：";
            PowerText.Size = new Size(168, 45);
            PowerText.TabIndex = 24;
            PowerText.Text = "123";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel3.BackColor = Color.Transparent;
            panel3.Controls.Add(segmented1);
            panel3.Location = new Point(1254, 23);
            panel3.Name = "panel3";
            panel3.Radius = 500;
            panel3.Shadow = 6;
            panel3.ShadowOpacityHover = 0F;
            panel3.Size = new Size(496, 65);
            panel3.TabIndex = 26;
            panel3.Text = "panel3";
            // 
            // panel5
            // 
            panel5.BackColor = Color.Transparent;
            panel5.Controls.Add(Rank_levelLabel);
            panel5.Controls.Add(LevelLabel);
            panel5.Controls.Add(panel3);
            panel5.Controls.Add(PowerText);
            panel5.Controls.Add(UidText);
            panel5.Controls.Add(NickNameText);
            panel5.Dock = DockStyle.Top;
            panel5.Location = new Point(0, 52);
            panel5.Name = "panel5";
            panel5.Radius = 0;
            panel5.Shadow = 6;
            panel5.ShadowAlign = AntdUI.TAlignMini.Bottom;
            panel5.Size = new Size(1761, 123);
            panel5.TabIndex = 29;
            panel5.Text = "panel5";
            // 
            // Rank_levelLabel
            // 
            Rank_levelLabel.BackColor = Color.Transparent;
            Rank_levelLabel.Font = new Font("HarmonyOS Sans SC", 9F);
            Rank_levelLabel.Location = new Point(724, 34);
            Rank_levelLabel.Name = "Rank_levelLabel";
            Rank_levelLabel.Prefix = "臂章：";
            Rank_levelLabel.Size = new Size(168, 45);
            Rank_levelLabel.TabIndex = 28;
            Rank_levelLabel.Text = "";
            // 
            // LevelLabel
            // 
            LevelLabel.BackColor = Color.Transparent;
            LevelLabel.Font = new Font("HarmonyOS Sans SC", 9F);
            LevelLabel.Location = new Point(550, 33);
            LevelLabel.Name = "LevelLabel";
            LevelLabel.Prefix = "等级：";
            LevelLabel.Size = new Size(168, 45);
            LevelLabel.TabIndex = 27;
            LevelLabel.Text = "";
            // 
            // select1
            // 
            select1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            select1.DropDownTextAlign = AntdUI.TAlign.Top;
            select1.List = true;
            select1.Location = new Point(1513, 31);
            select1.Name = "select1";
            select1.Placement = AntdUI.TAlignFrom.Top;
            select1.Radius = 3;
            select1.Size = new Size(237, 47);
            select1.TabIndex = 27;
            select1.SelectedIndexChanged += select1_SelectedIndexChanged;
            // 
            // panel6
            // 
            panel6.Controls.Add(select1);
            panel6.Controls.Add(button2);
            panel6.Controls.Add(button1);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 1393);
            panel6.Name = "panel6";
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1761, 90);
            panel6.TabIndex = 30;
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
            button2.Location = new Point(962, 12);
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
            button1.Location = new Point(745, 12);
            button1.Name = "button1";
            button1.Size = new Size(57, 78);
            button1.TabIndex = 0;
            button1.Click += button1_Click;
            // 
            // splitter1
            // 
            splitter1.ArrowColor = SystemColors.ActiveCaption;
            splitter1.CollapsePanel = AntdUI.Splitter.ADCollapsePanel.Panel1;
            splitter1.Dock = DockStyle.Fill;
            splitter1.FixedPanel = FixedPanel.Panel1;
            splitter1.Location = new Point(0, 175);
            splitter1.Name = "splitter1";
            // 
            // splitter1.Panel1
            // 
            splitter1.Panel1.AutoScroll = true;
            splitter1.Panel1.Controls.Add(collapse1);
            // 
            // splitter1.Panel2
            // 
            splitter1.Panel2.Controls.Add(splitter2);
            splitter1.Size = new Size(1761, 1218);
            splitter1.SplitterDistance = 471;
            splitter1.SplitterWidth = 6;
            splitter1.TabIndex = 27;
            // 
            // collapse1
            // 
            collapse1.Dock = DockStyle.Fill;
            collapse1.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Bold);
            collapse1.FontExpand = new Font("HarmonyOS Sans SC", 9F, FontStyle.Bold);
            collapse1.ForeColor = Color.FromArgb(103, 174, 246);
            collapse1.Items.Add(collapseItem1);
            collapse1.Items.Add(collapseItem2);
            collapse1.Items.Add(collapseItem3);
            collapse1.Location = new Point(0, 0);
            collapse1.Name = "collapse1";
            collapse1.Size = new Size(471, 1218);
            collapse1.TabIndex = 28;
            collapse1.Text = "collapse1";
            // 
            // collapseItem1
            // 
            collapseItem1.Expand = true;
            collapseItem1.Font = new Font("HarmonyOS Sans SC", 10F, FontStyle.Bold);
            collapseItem1.Location = new Point(27, 87);
            collapseItem1.Name = "collapseItem1";
            collapseItem1.Size = new Size(417, 300);
            collapseItem1.TabIndex = 0;
            collapseItem1.Text = "Dps/Hps/DTps实时曲线图";
            // 
            // collapseItem2
            // 
            collapseItem2.Expand = true;
            collapseItem2.Location = new Point(27, 495);
            collapseItem2.Name = "collapseItem2";
            collapseItem2.Size = new Size(417, 300);
            collapseItem2.TabIndex = 1;
            collapseItem2.Text = "技能占比分布图";
            // 
            // collapseItem3
            // 
            collapseItem3.AutoScroll = true;
            collapseItem3.Expand = true;
            collapseItem3.Location = new Point(27, 903);
            collapseItem3.Name = "collapseItem3";
            collapseItem3.Size = new Size(417, 300);
            collapseItem3.TabIndex = 2;
            collapseItem3.Text = "伤害分布";
            // 
            // splitter2
            // 
            splitter2.Dock = DockStyle.Fill;
            splitter2.FixedPanel = FixedPanel.Panel1;
            splitter2.Location = new Point(0, 0);
            splitter2.Name = "splitter2";
            splitter2.Orientation = Orientation.Horizontal;
            // 
            // splitter2.Panel1
            // 
            splitter2.Panel1.Controls.Add(panel2);
            splitter2.Panel1.Controls.Add(panel1);
            splitter2.Panel1.Padding = new Padding(10);
            // 
            // splitter2.Panel2
            // 
            splitter2.Panel2.Controls.Add(table_DpsDetailDataTable);
            splitter2.Panel2.Controls.Add(label10);
            splitter2.Panel2MinSize = 0;
            splitter2.Size = new Size(1284, 1218);
            splitter2.SplitterDistance = 254;
            splitter2.SplitterWidth = 1;
            splitter2.TabIndex = 23;
            // 
            // label10
            // 
            label10.BackColor = Color.Transparent;
            label10.Dock = DockStyle.Bottom;
            label10.Font = new Font("HarmonyOS Sans SC", 9F);
            label10.Location = new Point(0, 917);
            label10.Name = "label10";
            label10.Prefix = "温馨提示：";
            label10.Size = new Size(1284, 46);
            label10.TabIndex = 25;
            label10.Text = "快照模式下曲线图 占比图 分布图将失效";
            label10.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SkillDetailForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(251, 251, 251);
            ClientSize = new Size(1761, 1483);
            Controls.Add(splitter1);
            Controls.Add(panel5);
            Controls.Add(pageHeader1);
            Controls.Add(panel6);
            Dark = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Mode = AntdUI.TAMode.Dark;
            Name = "SkillDetailForm";
            Opacity = 0.95D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "技能详情";
            Load += SkillDetailForm_Load;
            ForeColorChanged += SkillDetailForm_ForeColorChanged;
            pageHeader1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel6.ResumeLayout(false);
            splitter1.Panel1.ResumeLayout(false);
            splitter1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitter1).EndInit();
            splitter1.ResumeLayout(false);
            collapse1.ResumeLayout(false);
            splitter2.Panel1.ResumeLayout(false);
            splitter2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitter2).EndInit();
            splitter2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.Table table_DpsDetailDataTable;
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Segmented segmented1;
        private AntdUI.Label NickNameText;
        private AntdUI.Divider divider1;
        private AntdUI.Panel panel1;
        private AntdUI.Label label3;
        private AntdUI.Label label1;
        private AntdUI.Label TotalDamageText;
        private AntdUI.Label TotalDpsText;
        private AntdUI.Label label2;
        private AntdUI.Label LuckyRate;
        private AntdUI.Label label5;
        private AntdUI.Label CritRateText;
        private AntdUI.Label label4;
        private AntdUI.Panel panel2;
        private AntdUI.Label AvgDamageText;
        private AntdUI.Label CritDamageText;
        private AntdUI.Label LuckyDamageText;
        private AntdUI.Label label9;
        private AntdUI.Label label7;
        private AntdUI.Label NormalDamageText;
        private AntdUI.Label label8;
        private AntdUI.Label label6;
        private AntdUI.Divider divider2;
        private AntdUI.Label label19;
        private AntdUI.Label UidText;
        private AntdUI.Label PowerText;
        public System.Windows.Forms.Timer timer1;
        private AntdUI.Label TitleText;
        private AntdUI.Panel panel3;
        private AntdUI.Panel panel5;
        private AntdUI.Panel panel6;
        private AntdUI.Button button1;
        private AntdUI.Button button2;
        private AntdUI.Select select1;
        private AntdUI.Splitter splitter1;
        private AntdUI.Collapse collapse1;
        private AntdUI.CollapseItem collapseItem1;
        private AntdUI.CollapseItem collapseItem2;
        private AntdUI.CollapseItem collapseItem3;
        private AntdUI.Label LuckyTimesLabel;
        private AntdUI.Label NumberCriticalHitsLabel;
        private AntdUI.Label label14;
        private AntdUI.Label NumberHitsLabel;
        private AntdUI.Label NumberCriticalHitsText;
        private AntdUI.Label label17;
        private AntdUI.Label BeatenLabel;
        private AntdUI.Label label13;
        private AntdUI.Splitter splitter2;
        private AntdUI.Label label10;
        private AntdUI.Label LevelLabel;
        private AntdUI.Label Rank_levelLabel;
    }
}