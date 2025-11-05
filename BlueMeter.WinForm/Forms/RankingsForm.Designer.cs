namespace BlueMeter.WinForm.Forms
{
    partial class RankingsForm
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
            AntdUI.SegmentedItem segmentedItem3 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem4 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem5 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem6 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem7 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem8 = new AntdUI.SegmentedItem();
            AntdUI.SegmentedItem segmentedItem9 = new AntdUI.SegmentedItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RankingsForm));
            pageHeader1 = new AntdUI.PageHeader();
            label1 = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            select2 = new AntdUI.Select();
            button3 = new AntdUI.Button();
            button4 = new AntdUI.Button();
            table_DpsDetailDataTable = new AntdUI.Table();
            panel3 = new AntdUI.Panel();
            segmented1 = new AntdUI.Segmented();
            button1 = new AntdUI.Button();
            divider3 = new AntdUI.Divider();
            panel1 = new AntdUI.Panel();
            panel2 = new AntdUI.Panel();
            label2 = new AntdUI.Label();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            panel3.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
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
            pageHeader1.Size = new Size(1213, 52);
            pageHeader1.TabIndex = 28;
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
            label1.Size = new Size(1213, 52);
            label1.TabIndex = 26;
            label1.Text = "Damage Reference";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.MouseDown += TitleText_MouseDown;
            // 
            // panel6
            // 
            panel6.Controls.Add(select2);
            panel6.Controls.Add(button3);
            panel6.Controls.Add(button4);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 1105);
            panel6.Name = "panel6";
            panel6.Radius = 3;
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1213, 86);
            panel6.TabIndex = 32;
            panel6.Text = "panel6";
            // 
            // select2
            // 
            select2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            select2.DropDownTextAlign = AntdUI.TAlign.Top;
            select2.List = true;
            select2.Location = new Point(2642, 35);
            select2.Name = "select2";
            select2.Radius = 0;
            select2.Size = new Size(204, 47);
            select2.TabIndex = 27;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom;
            button3.Ghost = true;
            button3.Icon = BlueMeter.Assets.HandledAssets.Cancel_Normal;
            button3.IconHover = BlueMeter.Assets.HandledAssets.Cancel_Hover;
            button3.IconPosition = AntdUI.TAlignMini.None;
            button3.IconRatio = 1.5F;
            button3.Location = new Point(660, 24);
            button3.Name = "button3";
            button3.Size = new Size(57, 49);
            button3.TabIndex = 1;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Bottom;
            button4.Ghost = true;
            button4.Icon = BlueMeter.Assets.HandledAssets.Flushed_Normal;
            button4.IconHover = BlueMeter.Assets.HandledAssets.Flushed_Hover;
            button4.IconPosition = AntdUI.TAlignMini.None;
            button4.IconRatio = 1.5F;
            button4.Location = new Point(487, 24);
            button4.Name = "button4";
            button4.Size = new Size(57, 49);
            button4.TabIndex = 0;
            button4.Click += button4_Click;
            // 
            // table_DpsDetailDataTable
            // 
            table_DpsDetailDataTable.BackgroundImageLayout = ImageLayout.Zoom;
            table_DpsDetailDataTable.Dock = DockStyle.Fill;
            table_DpsDetailDataTable.EmptyImage = BlueMeter.Assets.HandledAssets.Cancel_Hover;
            table_DpsDetailDataTable.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            table_DpsDetailDataTable.Gap = 8;
            table_DpsDetailDataTable.Gaps = new Size(8, 8);
            table_DpsDetailDataTable.Location = new Point(0, 40);
            table_DpsDetailDataTable.Name = "table_DpsDetailDataTable";
            table_DpsDetailDataTable.RowHeight = 40;
            table_DpsDetailDataTable.RowSelectedBg = Color.FromArgb(174, 212, 251);
            table_DpsDetailDataTable.Size = new Size(1213, 893);
            table_DpsDetailDataTable.TabIndex = 33;
            table_DpsDetailDataTable.Text = "table1";
            table_DpsDetailDataTable.CellClick += table_DpsDetailDataTable_CellClick;
            table_DpsDetailDataTable.CellButtonDown += table_DpsDetailDataTable_CellButtonDown;
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel3.BackColor = Color.Transparent;
            panel3.Controls.Add(segmented1);
            panel3.Location = new Point(279, 5);
            panel3.Name = "panel3";
            panel3.Radius = 3;
            panel3.Shadow = 6;
            panel3.ShadowOpacityHover = 0F;
            panel3.Size = new Size(925, 65);
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
            segmentedItem1.Text = "全职业";
            segmentedItem2.Text = "冰魔导师";
            segmentedItem3.Text = "巨刃守护者";
            segmentedItem4.Text = "雷影剑士";
            segmentedItem5.Text = "灵魂乐手";
            segmentedItem6.Text = "青岚骑士";
            segmentedItem7.Text = "森语者";
            segmentedItem8.Text = "神盾骑士";
            segmentedItem9.Text = "神射手";
            segmented1.Items.Add(segmentedItem1);
            segmented1.Items.Add(segmentedItem2);
            segmented1.Items.Add(segmentedItem3);
            segmented1.Items.Add(segmentedItem4);
            segmented1.Items.Add(segmentedItem5);
            segmented1.Items.Add(segmentedItem6);
            segmented1.Items.Add(segmentedItem7);
            segmented1.Items.Add(segmentedItem8);
            segmented1.Items.Add(segmentedItem9);
            segmented1.Location = new Point(9, 9);
            segmented1.Name = "segmented1";
            segmented1.Radius = 2;
            segmented1.SelectIndex = 0;
            segmented1.Size = new Size(907, 47);
            segmented1.TabIndex = 16;
            segmented1.Text = "segmented1";
            segmented1.SelectIndexChanged += segmented1_SelectIndexChanged;
            // 
            // button1
            // 
            button1.ColorScheme = AntdUI.TAMode.Light;
            button1.DefaultBack = Color.FromArgb(153, 204, 255);
            button1.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);
            button1.ForeColor = Color.White;
            button1.Location = new Point(18, 9);
            button1.Name = "button1";
            button1.Radius = 3;
            button1.Size = new Size(249, 56);
            button1.TabIndex = 35;
            button1.Text = "伤害参考";
            button1.Click += button1_Click;
            // 
            // divider3
            // 
            divider3.BackColor = Color.Transparent;
            divider3.Dock = DockStyle.Top;
            divider3.Font = new Font("阿里妈妈数黑体", 9F, FontStyle.Bold);
            divider3.Location = new Point(0, 0);
            divider3.Name = "divider3";
            divider3.OrientationMargin = 1F;
            divider3.Size = new Size(1213, 40);
            divider3.TabIndex = 36;
            divider3.Text = "伤害参考";
            // 
            // panel1
            // 
            panel1.Controls.Add(panel3);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 52);
            panel1.Name = "panel1";
            panel1.Radius = 3;
            panel1.Shadow = 6;
            panel1.ShadowAlign = AntdUI.TAlignMini.Bottom;
            panel1.Size = new Size(1213, 84);
            panel1.TabIndex = 38;
            panel1.Text = "panel1";
            // 
            // panel2
            // 
            panel2.Controls.Add(table_DpsDetailDataTable);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(divider3);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 136);
            panel2.Name = "panel2";
            panel2.Size = new Size(1213, 969);
            panel2.TabIndex = 38;
            panel2.Text = "panel2";
            // 
            // label2
            // 
            label2.Dock = DockStyle.Bottom;
            label2.Font = new Font("HarmonyOS Sans SC", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            label2.Location = new Point(0, 933);
            label2.Name = "label2";
            label2.Size = new Size(1213, 36);
            label2.TabIndex = 37;
            label2.Text = "以上数据仅供自身职业DPS参考[无需焦虑可能有人吃药了]，切勿用于战力歧视等破坏游戏社区环境行为，一经发现，关闭此功能";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // RankingsForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1213, 1191);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "RankingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "伤害参考";
            Load += RankingsForm_Load;
            ForeColorChanged += RankingsForm_ForeColorChanged;
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label label1;
        private AntdUI.Panel panel6;
        private AntdUI.Select select2;
        private AntdUI.Button button3;
        private AntdUI.Button button4;
        private AntdUI.Table table_DpsDetailDataTable;
        private AntdUI.Panel panel3;
        private AntdUI.Segmented segmented1;
        private AntdUI.Button button1;
        private AntdUI.Divider divider3;
        private AntdUI.Panel panel1;
        private AntdUI.Panel panel2;
        private AntdUI.Label label2;
    }
}