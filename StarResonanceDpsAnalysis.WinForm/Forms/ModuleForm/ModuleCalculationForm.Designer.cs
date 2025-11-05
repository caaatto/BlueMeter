namespace StarResonanceDpsAnalysis.WinForm.Forms.ModuleForm
{
    partial class ModuleCalculationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleCalculationForm));
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            button3 = new AntdUI.Button();
            button4 = new AntdUI.Button();
            select1 = new AntdUI.Select();
            button1 = new AntdUI.Button();
            virtualPanel1 = new AntdUI.VirtualPanel();
            label1 = new AntdUI.Label();
            groupBox3 = new GroupBox();
            inputNumber5 = new AntdUI.InputNumber();
            inputNumber4 = new AntdUI.InputNumber();
            inputNumber3 = new AntdUI.InputNumber();
            inputNumber2 = new AntdUI.InputNumber();
            inputNumber1 = new AntdUI.InputNumber();
            select7 = new AntdUI.Select();
            select6 = new AntdUI.Select();
            select5 = new AntdUI.Select();
            select4 = new AntdUI.Select();
            select2 = new AntdUI.Select();
            groupBox1 = new GroupBox();
            select8 = new AntdUI.Select();
            select9 = new AntdUI.Select();
            select10 = new AntdUI.Select();
            select11 = new AntdUI.Select();
            select12 = new AntdUI.Select();
            select3 = new AntdUI.Select();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox1.SuspendLayout();
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
            pageHeader1.Size = new Size(1586, 38);
            pageHeader1.TabIndex = 31;
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
            TitleText.Size = new Size(1586, 38);
            TitleText.TabIndex = 27;
            TitleText.Text = "Death Statistics";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            TitleText.MouseDown += TitleText_MouseDown;
            // 
            // panel6
            // 
            panel6.Controls.Add(button3);
            panel6.Controls.Add(button4);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 1017);
            panel6.Name = "panel6";
            panel6.Radius = 3;
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(1586, 86);
            panel6.TabIndex = 34;
            panel6.Text = "panel6";
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom;
            button3.Ghost = true;
            button3.Icon = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Normal;
            button3.IconHover = StarResonanceDpsAnalysis.Assets.HandledAssets.Cancel_Hover;
            button3.IconPosition = AntdUI.TAlignMini.None;
            button3.IconRatio = 1.5F;
            button3.Location = new Point(863, 25);
            button3.Name = "button3";
            button3.Size = new Size(57, 49);
            button3.TabIndex = 1;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Bottom;
            button4.Ghost = true;
            button4.Icon = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Normal;
            button4.IconHover = StarResonanceDpsAnalysis.Assets.HandledAssets.Flushed_Hover;
            button4.IconPosition = AntdUI.TAlignMini.None;
            button4.IconRatio = 1.5F;
            button4.Location = new Point(666, 25);
            button4.Name = "button4";
            button4.Size = new Size(57, 49);
            button4.TabIndex = 0;
            button4.Click += button4_Click;
            // 
            // select1
            // 
            select1.Font = new Font("HarmonyOS Sans SC", 9F);
            select1.Items.AddRange(new object[] { "全部", "攻击", "辅助", "守护" });
            select1.List = true;
            select1.Location = new Point(10, 839);
            select1.Name = "select1";
            select1.PrefixText = "模组类型：";
            select1.Radius = 3;
            select1.SelectedIndex = 0;
            select1.SelectedValue = "全部";
            select1.SelectionStart = 2;
            select1.Size = new Size(230, 63);
            select1.TabIndex = 35;
            select1.Text = "全部";
            select1.SelectedIndexChanged += select1_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.Font = new Font("HarmonyOS Sans SC", 9F);
            button1.Location = new Point(12, 908);
            button1.Name = "button1";
            button1.Size = new Size(456, 63);
            button1.TabIndex = 52;
            button1.Text = "分析模组";
            button1.Type = AntdUI.TTypeMini.Primary;
            button1.Click += button1_Click;
            // 
            // virtualPanel1
            // 
            virtualPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            virtualPanel1.BackgroundImageLayout = ImageLayout.Zoom;
            virtualPanel1.Gap = 6;
            virtualPanel1.Location = new Point(495, 53);
            virtualPanel1.Margin = new Padding(0);
            virtualPanel1.Name = "virtualPanel1";
            virtualPanel1.Shadow = 3;
            virtualPanel1.Size = new Size(1081, 911);
            virtualPanel1.TabIndex = 53;
            virtualPanel1.Text = "virtualPanel1";
            // 
            // label1
            // 
            label1.Dock = DockStyle.Bottom;
            label1.Location = new Point(0, 983);
            label1.Name = "label1";
            label1.Size = new Size(1586, 34);
            label1.TabIndex = 54;
            label1.Text = "打开此界面后需要过图或者重新登录一次才能进行分析【右键属性选项为清空当前选择】";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(inputNumber5);
            groupBox3.Controls.Add(inputNumber4);
            groupBox3.Controls.Add(inputNumber3);
            groupBox3.Controls.Add(inputNumber2);
            groupBox3.Controls.Add(inputNumber1);
            groupBox3.Controls.Add(select7);
            groupBox3.Controls.Add(select6);
            groupBox3.Controls.Add(select5);
            groupBox3.Controls.Add(select4);
            groupBox3.Controls.Add(select2);
            groupBox3.Location = new Point(10, 53);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(465, 371);
            groupBox3.TabIndex = 58;
            groupBox3.TabStop = false;
            groupBox3.Text = "目标词条和等级（0为默认）[可输入文字索引]";
            // 
            // inputNumber5
            // 
            inputNumber5.Location = new Point(250, 229);
            inputNumber5.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            inputNumber5.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            inputNumber5.Name = "inputNumber5";
            inputNumber5.PrefixText = "期望等级：";
            inputNumber5.SelectionStart = 1;
            inputNumber5.Size = new Size(196, 57);
            inputNumber5.TabIndex = 8;
            inputNumber5.Tag = "3";
            inputNumber5.Text = "0";
            inputNumber5.ValueChanged += inputNumber1_ValueChanged;
            // 
            // inputNumber4
            // 
            inputNumber4.Location = new Point(250, 292);
            inputNumber4.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            inputNumber4.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            inputNumber4.Name = "inputNumber4";
            inputNumber4.PrefixText = "期望等级：";
            inputNumber4.SelectionStart = 1;
            inputNumber4.Size = new Size(196, 57);
            inputNumber4.TabIndex = 8;
            inputNumber4.Tag = "4";
            inputNumber4.Text = "0";
            inputNumber4.ValueChanged += inputNumber1_ValueChanged;
            // 
            // inputNumber3
            // 
            inputNumber3.Location = new Point(250, 166);
            inputNumber3.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            inputNumber3.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            inputNumber3.Name = "inputNumber3";
            inputNumber3.PrefixText = "期望等级：";
            inputNumber3.SelectionStart = 1;
            inputNumber3.Size = new Size(196, 57);
            inputNumber3.TabIndex = 7;
            inputNumber3.Tag = "2";
            inputNumber3.Text = "0";
            inputNumber3.ValueChanged += inputNumber1_ValueChanged;
            // 
            // inputNumber2
            // 
            inputNumber2.Location = new Point(250, 103);
            inputNumber2.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            inputNumber2.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            inputNumber2.Name = "inputNumber2";
            inputNumber2.PrefixText = "期望等级：";
            inputNumber2.SelectionStart = 1;
            inputNumber2.Size = new Size(196, 57);
            inputNumber2.TabIndex = 6;
            inputNumber2.Tag = "1";
            inputNumber2.Text = "0";
            inputNumber2.ValueChanged += inputNumber1_ValueChanged;
            // 
            // inputNumber1
            // 
            inputNumber1.Location = new Point(250, 40);
            inputNumber1.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            inputNumber1.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            inputNumber1.Name = "inputNumber1";
            inputNumber1.PrefixText = "期望等级：";
            inputNumber1.SelectionStart = 1;
            inputNumber1.Size = new Size(196, 57);
            inputNumber1.TabIndex = 5;
            inputNumber1.Tag = "0";
            inputNumber1.Text = "0";
            inputNumber1.ValueChanged += inputNumber1_ValueChanged;
            // 
            // select7
            // 
            select7.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select7.Location = new Point(9, 292);
            select7.Name = "select7";
            select7.PrefixText = "属性5：";
            select7.Size = new Size(235, 57);
            select7.TabIndex = 4;
            select7.Tag = "4";
            select7.SelectedIndexChanged += select2_SelectedIndexChanged;
            select7.MouseUp += selectClearSelection_MouseUp;
            // 
            // select6
            // 
            select6.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select6.Location = new Point(9, 229);
            select6.Name = "select6";
            select6.PrefixText = "属性4：";
            select6.Size = new Size(235, 57);
            select6.TabIndex = 3;
            select6.Tag = "3";
            select6.SelectedIndexChanged += select2_SelectedIndexChanged;
            select6.MouseUp += selectClearSelection_MouseUp;
            // 
            // select5
            // 
            select5.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select5.Location = new Point(9, 166);
            select5.Name = "select5";
            select5.PrefixText = "属性3：";
            select5.Size = new Size(235, 57);
            select5.TabIndex = 2;
            select5.Tag = "2";
            select5.SelectedIndexChanged += select2_SelectedIndexChanged;
            select5.MouseUp += selectClearSelection_MouseUp;
            // 
            // select4
            // 
            select4.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select4.Location = new Point(9, 103);
            select4.Name = "select4";
            select4.PrefixText = "属性2：";
            select4.Size = new Size(235, 57);
            select4.TabIndex = 1;
            select4.Tag = "1";
            select4.SelectedIndexChanged += select2_SelectedIndexChanged;
            select4.MouseUp += selectClearSelection_MouseUp;
            // 
            // select2
            // 
            select2.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select2.Location = new Point(9, 40);
            select2.Name = "select2";
            select2.PrefixText = "属性1：";
            select2.Size = new Size(235, 57);
            select2.TabIndex = 0;
            select2.Tag = "0";
            select2.SelectedIndexChanged += select2_SelectedIndexChanged;
            select2.ClearClick += select2_ClearClick;
            select2.MouseUp += selectClearSelection_MouseUp;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(select8);
            groupBox1.Controls.Add(select9);
            groupBox1.Controls.Add(select10);
            groupBox1.Controls.Add(select11);
            groupBox1.Controls.Add(select12);
            groupBox1.Location = new Point(10, 451);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(465, 371);
            groupBox1.TabIndex = 59;
            groupBox1.TabStop = false;
            groupBox1.Text = "排除词条（与目标词条冲突）[可输入文字索引]";
            // 
            // select8
            // 
            select8.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select8.Location = new Point(9, 292);
            select8.Name = "select8";
            select8.PrefixText = "属性5：";
            select8.Size = new Size(437, 57);
            select8.TabIndex = 4;
            select8.Tag = "4";
            select8.SelectedIndexChanged += select12_SelectedIndexChanged_1;
            select8.MouseUp += selectEmptyRule_MouseUp;
            // 
            // select9
            // 
            select9.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select9.Location = new Point(9, 229);
            select9.Name = "select9";
            select9.PrefixText = "属性4：";
            select9.Size = new Size(437, 57);
            select9.TabIndex = 3;
            select9.Tag = "3";
            select9.SelectedIndexChanged += select12_SelectedIndexChanged_1;
            select9.MouseUp += selectEmptyRule_MouseUp;
            // 
            // select10
            // 
            select10.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select10.Location = new Point(9, 166);
            select10.Name = "select10";
            select10.PrefixText = "属性3：";
            select10.Size = new Size(437, 57);
            select10.TabIndex = 2;
            select10.Tag = "2";
            select10.SelectedIndexChanged += select12_SelectedIndexChanged_1;
            select10.MouseUp += selectEmptyRule_MouseUp;
            // 
            // select11
            // 
            select11.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select11.Location = new Point(9, 103);
            select11.Name = "select11";
            select11.PrefixText = "属性2：";
            select11.Size = new Size(437, 57);
            select11.TabIndex = 1;
            select11.Tag = "1";
            select11.SelectedIndexChanged += select12_SelectedIndexChanged_1;
            select11.MouseUp += selectEmptyRule_MouseUp;
            // 
            // select12
            // 
            select12.Items.AddRange(new object[] { "力量加持", "敏捷加持", "智力加持", "特攻伤害", "精英打击", "特攻治疗加持", "专精治疗加持", "施法专注", "攻速专注", "暴击专注", "幸运专注", "抵御魔法", "抵御物理", "极-伤害叠加", "极-灵活身法", "极-生命凝聚", "极-急救措施", "极-生命波动", "极-生命汲取", "极-全队幸暴", "极-绝境守护" });
            select12.Location = new Point(9, 40);
            select12.Name = "select12";
            select12.PrefixText = "属性1：";
            select12.Size = new Size(437, 57);
            select12.TabIndex = 0;
            select12.Tag = "0";
            select12.SelectedIndexChanged += select12_SelectedIndexChanged_1;
            select12.MouseUp += selectEmptyRule_MouseUp;
            // 
            // select3
            // 
            select3.Font = new Font("HarmonyOS Sans SC", 9F);
            select3.Items.AddRange(new object[] { "属性优先", "战力优先" });
            select3.List = true;
            select3.Location = new Point(240, 839);
            select3.Name = "select3";
            select3.PrefixText = "排序方式：";
            select3.Radius = 3;
            select3.SelectedIndex = 0;
            select3.SelectedValue = "属性优先";
            select3.SelectionStart = 4;
            select3.Size = new Size(235, 63);
            select3.TabIndex = 60;
            select3.Text = "属性优先";
            select3.SelectedIndexChanged += select3_SelectedIndexChanged_1;
            // 
            // ModuleCalculationForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1586, 1103);
            Controls.Add(select3);
            Controls.Add(groupBox1);
            Controls.Add(groupBox3);
            Controls.Add(label1);
            Controls.Add(virtualPanel1);
            Controls.Add(select1);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Controls.Add(button1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ModuleCalculationForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "模组分析";
            Load += ModuleCalculationForm_Load;
            ForeColorChanged += ModuleCalculationForm_ForeColorChanged;
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label TitleText;
        private AntdUI.Panel panel6;
        private AntdUI.Button button3;
        private AntdUI.Button button4;
        private AntdUI.Select select1;
        private AntdUI.Button button1;
        public AntdUI.VirtualPanel virtualPanel1;
        private AntdUI.Label label1;
        private GroupBox groupBox3;
        private AntdUI.Select select2;
        private AntdUI.Select select4;
        private AntdUI.Select select5;
        private AntdUI.Select select6;
        private AntdUI.Select select7;
        private AntdUI.InputNumber inputNumber1;
        private AntdUI.InputNumber inputNumber5;
        private AntdUI.InputNumber inputNumber4;
        private AntdUI.InputNumber inputNumber3;
        private AntdUI.InputNumber inputNumber2;
        private GroupBox groupBox1;
        private AntdUI.Select select8;
        private AntdUI.Select select9;
        private AntdUI.Select select10;
        private AntdUI.Select select11;
        private AntdUI.Select select12;
        private AntdUI.Select select3;
    }
}