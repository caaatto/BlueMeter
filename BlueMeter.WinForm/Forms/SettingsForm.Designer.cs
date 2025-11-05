namespace BlueMeter.WinForm.Forms
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            pageHeader_MainHeader = new AntdUI.PageHeader();
            label_TitleText = new AntdUI.Label();
            panel_MainTitlePanel = new AntdUI.Panel();
            label_SettingTitle = new AntdUI.Label();
            panel_CombatSettings = new AntdUI.Panel();
            panel_CombatSettingsColorBar = new Panel();
            select_DamageDisplayType = new AntdUI.Select();
            label_DpsShowType = new AntdUI.Label();
            slider_Transparency = new AntdUI.Slider();
            label_Transparent = new AntdUI.Label();
            inputNumber_ClearSectionedDataTime = new AntdUI.InputNumber();
            label_CombatSettingsTip = new AntdUI.Label();
            label_ClearAllDataWhenSwitch = new AntdUI.Label();
            switch_ClearAllDataWhenSwitch = new AntdUI.Switch();
            divider_CombatSettings_1 = new AntdUI.Divider();
            label_CombatSettingsTitle = new AntdUI.Label();
            panel_KeySettings = new AntdUI.Panel();
            panel_KeySettingsColorBar = new Panel();
            label_KeySettingsTip = new AntdUI.Label();
            input_ClearData = new AntdUI.Input();
            input_MouseThroughKey = new AntdUI.Input();
            divider_KeySettings_1 = new AntdUI.Divider();
            label_KeySettingsTitle = new AntdUI.Label();
            panel_BasicSetup = new AntdUI.Panel();
            panel_BasicSetupColorBar = new Panel();
            label_BasicSetupTip = new AntdUI.Label();
            select_NetcardSelector = new AntdUI.Select();
            divider_BasicSetup_1 = new AntdUI.Divider();
            label_BasicSetupTitle = new AntdUI.Label();
            panel_FooterPanel = new AntdUI.Panel();
            button_FormCancel = new AntdUI.Button();
            button_Save = new AntdUI.Button();
            stackPanel_MainPanel = new AntdUI.StackPanel();
            pageHeader_MainHeader.SuspendLayout();
            panel_MainTitlePanel.SuspendLayout();
            panel_CombatSettings.SuspendLayout();
            panel_KeySettings.SuspendLayout();
            panel_BasicSetup.SuspendLayout();
            panel_FooterPanel.SuspendLayout();
            stackPanel_MainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // pageHeader_MainHeader
            // 
            pageHeader_MainHeader.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader_MainHeader.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader_MainHeader.Controls.Add(label_TitleText);
            pageHeader_MainHeader.DividerShow = true;
            pageHeader_MainHeader.DividerThickness = 2F;
            pageHeader_MainHeader.Dock = DockStyle.Top;
            pageHeader_MainHeader.Location = new Point(0, 0);
            pageHeader_MainHeader.Margin = new Padding(2);
            pageHeader_MainHeader.MaximizeBox = false;
            pageHeader_MainHeader.Mode = AntdUI.TAMode.Dark;
            pageHeader_MainHeader.Name = "pageHeader_MainHeader";
            pageHeader_MainHeader.Size = new Size(455, 27);
            pageHeader_MainHeader.TabIndex = 29;
            pageHeader_MainHeader.Text = "";
            // 
            // label_TitleText
            // 
            label_TitleText.BackColor = Color.Transparent;
            label_TitleText.ColorScheme = AntdUI.TAMode.Dark;
            label_TitleText.Dock = DockStyle.Fill;
            label_TitleText.Font = new Font("SAO Welcome TT", 12F, FontStyle.Bold);
            label_TitleText.Location = new Point(0, 0);
            label_TitleText.Margin = new Padding(2);
            label_TitleText.Name = "label_TitleText";
            label_TitleText.Size = new Size(455, 27);
            label_TitleText.TabIndex = 0;
            label_TitleText.Text = "BasicSetup";
            label_TitleText.TextAlign = ContentAlignment.MiddleCenter;
            label_TitleText.MouseDown += label_TitleText_MouseDown;
            // 
            // panel_MainTitlePanel
            // 
            panel_MainTitlePanel.Back = Color.FromArgb(34, 151, 244);
            panel_MainTitlePanel.Controls.Add(label_SettingTitle);
            panel_MainTitlePanel.Dock = DockStyle.Top;
            panel_MainTitlePanel.Location = new Point(0, 27);
            panel_MainTitlePanel.Margin = new Padding(2);
            panel_MainTitlePanel.Name = "panel_MainTitlePanel";
            panel_MainTitlePanel.Radius = 0;
            panel_MainTitlePanel.Size = new Size(455, 36);
            panel_MainTitlePanel.TabIndex = 0;
            panel_MainTitlePanel.Text = "panel2";
            // 
            // label_SettingTitle
            // 
            label_SettingTitle.BackColor = Color.Transparent;
            label_SettingTitle.Font = new Font("阿里妈妈数黑体", 10F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label_SettingTitle.ForeColor = Color.White;
            label_SettingTitle.Location = new Point(12, 5);
            label_SettingTitle.Margin = new Padding(2);
            label_SettingTitle.Name = "label_SettingTitle";
            label_SettingTitle.Size = new Size(231, 22);
            label_SettingTitle.TabIndex = 0;
            label_SettingTitle.Text = "设置";
            // 
            // panel_CombatSettings
            // 
            panel_CombatSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel_CombatSettings.Back = Color.White;
            panel_CombatSettings.BackColor = Color.Transparent;
            panel_CombatSettings.Controls.Add(panel_CombatSettingsColorBar);
            panel_CombatSettings.Controls.Add(select_DamageDisplayType);
            panel_CombatSettings.Controls.Add(label_DpsShowType);
            panel_CombatSettings.Controls.Add(slider_Transparency);
            panel_CombatSettings.Controls.Add(label_Transparent);
            panel_CombatSettings.Controls.Add(inputNumber_ClearSectionedDataTime);
            panel_CombatSettings.Controls.Add(label_CombatSettingsTip);
            panel_CombatSettings.Controls.Add(label_ClearAllDataWhenSwitch);
            panel_CombatSettings.Controls.Add(switch_ClearAllDataWhenSwitch);
            panel_CombatSettings.Controls.Add(divider_CombatSettings_1);
            panel_CombatSettings.Controls.Add(label_CombatSettingsTitle);
            panel_CombatSettings.Location = new Point(13, 367);
            panel_CombatSettings.Margin = new Padding(13, 14, 13, 14);
            panel_CombatSettings.Name = "panel_CombatSettings";
            panel_CombatSettings.Size = new Size(413, 315);
            panel_CombatSettings.TabIndex = 2;
            panel_CombatSettings.Text = "panel6";
            // 
            // panel_CombatSettingsColorBar
            // 
            panel_CombatSettingsColorBar.BackColor = Color.FromArgb(34, 151, 244);
            panel_CombatSettingsColorBar.Location = new Point(0, 9);
            panel_CombatSettingsColorBar.Name = "panel_CombatSettingsColorBar";
            panel_CombatSettingsColorBar.Size = new Size(3, 42);
            panel_CombatSettingsColorBar.TabIndex = 0;
            // 
            // select_DamageDisplayType
            // 
            select_DamageDisplayType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            select_DamageDisplayType.Items.AddRange(new object[] { "三位分节法 (KMBT)", "四位分节法 (万亿兆)" });
            select_DamageDisplayType.List = true;
            select_DamageDisplayType.Location = new Point(235, 191);
            select_DamageDisplayType.Margin = new Padding(2);
            select_DamageDisplayType.Name = "select_DamageDisplayType";
            select_DamageDisplayType.Radius = 3;
            select_DamageDisplayType.SelectedValue = "KBM显示";
            select_DamageDisplayType.SelectionStart = 12;
            select_DamageDisplayType.Size = new Size(158, 48);
            select_DamageDisplayType.TabIndex = 8;
            select_DamageDisplayType.Text = "三位分节法 (KMBT)";
            select_DamageDisplayType.SelectedValueChanged += select_DamageDisplayType_SelectedValueChanged;
            // 
            // label_DpsShowType
            // 
            label_DpsShowType.Font = new Font("HarmonyOS Sans SC", 9F);
            label_DpsShowType.Location = new Point(29, 198);
            label_DpsShowType.Margin = new Padding(2);
            label_DpsShowType.Name = "label_DpsShowType";
            label_DpsShowType.Size = new Size(149, 41);
            label_DpsShowType.TabIndex = 7;
            label_DpsShowType.Text = "DPS统计伤害显示类型";
            // 
            // slider_Transparency
            // 
            slider_Transparency.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            slider_Transparency.Location = new Point(110, 258);
            slider_Transparency.Margin = new Padding(2);
            slider_Transparency.MinValue = 25;
            slider_Transparency.Name = "slider_Transparency";
            slider_Transparency.ShowValue = true;
            slider_Transparency.Size = new Size(283, 34);
            slider_Transparency.TabIndex = 10;
            slider_Transparency.Text = "slider1";
            slider_Transparency.Value = 90;
            slider_Transparency.ValueChanged += slider_Transparency_ValueChanged;
            // 
            // label_Transparent
            // 
            label_Transparent.Font = new Font("HarmonyOS Sans SC", 9F);
            label_Transparent.Location = new Point(29, 254);
            label_Transparent.Margin = new Padding(2);
            label_Transparent.Name = "label_Transparent";
            label_Transparent.Size = new Size(88, 41);
            label_Transparent.TabIndex = 9;
            label_Transparent.Text = "窗体透明度";
            // 
            // inputNumber_ClearSectionedDataTime
            // 
            inputNumber_ClearSectionedDataTime.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputNumber_ClearSectionedDataTime.Location = new Point(23, 80);
            inputNumber_ClearSectionedDataTime.Margin = new Padding(2);
            inputNumber_ClearSectionedDataTime.Name = "inputNumber_ClearSectionedDataTime";
            inputNumber_ClearSectionedDataTime.PrefixText = "脱战";
            inputNumber_ClearSectionedDataTime.Radius = 3;
            inputNumber_ClearSectionedDataTime.SelectionStart = 1;
            inputNumber_ClearSectionedDataTime.Size = new Size(370, 46);
            inputNumber_ClearSectionedDataTime.SuffixText = "/秒后清除当前统计";
            inputNumber_ClearSectionedDataTime.TabIndex = 4;
            inputNumber_ClearSectionedDataTime.Text = "5";
            inputNumber_ClearSectionedDataTime.TextAlign = HorizontalAlignment.Center;
            inputNumber_ClearSectionedDataTime.Value = new decimal(new int[] { 5, 0, 0, 0 });
            inputNumber_ClearSectionedDataTime.TextChanged += inputNumber_ClearSectionedDataTime_TextChanged;
            // 
            // label_CombatSettingsTip
            // 
            label_CombatSettingsTip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label_CombatSettingsTip.BackColor = Color.Transparent;
            label_CombatSettingsTip.Font = new Font("HarmonyOS Sans SC", 7F);
            label_CombatSettingsTip.ForeColor = Color.FromArgb(34, 151, 244);
            label_CombatSettingsTip.Location = new Point(235, 18);
            label_CombatSettingsTip.Margin = new Padding(2);
            label_CombatSettingsTip.Name = "label_CombatSettingsTip";
            label_CombatSettingsTip.Size = new Size(158, 22);
            label_CombatSettingsTip.TabIndex = 2;
            label_CombatSettingsTip.Text = "脱战清空为当前统计非全程统计";
            label_CombatSettingsTip.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label_ClearAllDataWhenSwitch
            // 
            label_ClearAllDataWhenSwitch.Font = new Font("HarmonyOS Sans SC", 9F);
            label_ClearAllDataWhenSwitch.Location = new Point(29, 142);
            label_ClearAllDataWhenSwitch.Margin = new Padding(2);
            label_ClearAllDataWhenSwitch.Name = "label_ClearAllDataWhenSwitch";
            label_ClearAllDataWhenSwitch.Size = new Size(149, 41);
            label_ClearAllDataWhenSwitch.TabIndex = 5;
            label_ClearAllDataWhenSwitch.Text = "换地图是否清空全程统计";
            // 
            // switch_ClearAllDataWhenSwitch
            // 
            switch_ClearAllDataWhenSwitch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            switch_ClearAllDataWhenSwitch.Checked = true;
            switch_ClearAllDataWhenSwitch.Location = new Point(340, 154);
            switch_ClearAllDataWhenSwitch.Margin = new Padding(2);
            switch_ClearAllDataWhenSwitch.Name = "switch_ClearAllDataWhenSwitch";
            switch_ClearAllDataWhenSwitch.Size = new Size(53, 29);
            switch_ClearAllDataWhenSwitch.TabIndex = 6;
            switch_ClearAllDataWhenSwitch.Text = "switch1";
            switch_ClearAllDataWhenSwitch.CheckedChanged += switch_ClearAllDataWhenSwitch_CheckedChanged;
            // 
            // divider_CombatSettings_1
            // 
            divider_CombatSettings_1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            divider_CombatSettings_1.BackColor = Color.Transparent;
            divider_CombatSettings_1.Location = new Point(23, 55);
            divider_CombatSettings_1.Margin = new Padding(2);
            divider_CombatSettings_1.Name = "divider_CombatSettings_1";
            divider_CombatSettings_1.OrientationMargin = 0F;
            divider_CombatSettings_1.Size = new Size(388, 10);
            divider_CombatSettings_1.TabIndex = 3;
            divider_CombatSettings_1.Text = "";
            // 
            // label_CombatSettingsTitle
            // 
            label_CombatSettingsTitle.BackColor = Color.Transparent;
            label_CombatSettingsTitle.Font = new Font("HarmonyOS Sans SC", 9.999999F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label_CombatSettingsTitle.ForeColor = Color.FromArgb(34, 151, 244);
            label_CombatSettingsTitle.Location = new Point(23, 18);
            label_CombatSettingsTitle.Margin = new Padding(2);
            label_CombatSettingsTitle.Name = "label_CombatSettingsTitle";
            label_CombatSettingsTitle.Size = new Size(67, 22);
            label_CombatSettingsTitle.TabIndex = 1;
            label_CombatSettingsTitle.Text = "战斗设置";
            // 
            // panel_KeySettings
            // 
            panel_KeySettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel_KeySettings.Back = Color.White;
            panel_KeySettings.BackColor = Color.Transparent;
            panel_KeySettings.Controls.Add(panel_KeySettingsColorBar);
            panel_KeySettings.Controls.Add(label_KeySettingsTip);
            panel_KeySettings.Controls.Add(input_ClearData);
            panel_KeySettings.Controls.Add(input_MouseThroughKey);
            panel_KeySettings.Controls.Add(divider_KeySettings_1);
            panel_KeySettings.Controls.Add(label_KeySettingsTitle);
            panel_KeySettings.Location = new Point(13, 191);
            panel_KeySettings.Margin = new Padding(13, 14, 13, 14);
            panel_KeySettings.Name = "panel_KeySettings";
            panel_KeySettings.Size = new Size(413, 148);
            panel_KeySettings.TabIndex = 1;
            panel_KeySettings.Text = "panel5";
            // 
            // panel_KeySettingsColorBar
            // 
            panel_KeySettingsColorBar.BackColor = Color.FromArgb(34, 151, 244);
            panel_KeySettingsColorBar.Location = new Point(0, 9);
            panel_KeySettingsColorBar.Name = "panel_KeySettingsColorBar";
            panel_KeySettingsColorBar.Size = new Size(3, 42);
            panel_KeySettingsColorBar.TabIndex = 0;
            // 
            // label_KeySettingsTip
            // 
            label_KeySettingsTip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label_KeySettingsTip.BackColor = Color.Transparent;
            label_KeySettingsTip.Font = new Font("HarmonyOS Sans SC", 7F);
            label_KeySettingsTip.ForeColor = Color.FromArgb(34, 151, 244);
            label_KeySettingsTip.Location = new Point(267, 18);
            label_KeySettingsTip.Margin = new Padding(2);
            label_KeySettingsTip.Name = "label_KeySettingsTip";
            label_KeySettingsTip.Size = new Size(126, 22);
            label_KeySettingsTip.TabIndex = 2;
            label_KeySettingsTip.Text = "Delete删除当前键位";
            label_KeySettingsTip.TextAlign = ContentAlignment.MiddleRight;
            // 
            // input_ClearData
            // 
            input_ClearData.Anchor = AnchorStyles.Top;
            input_ClearData.Font = new Font("HarmonyOS Sans SC", 9F);
            input_ClearData.Location = new Point(213, 79);
            input_ClearData.Margin = new Padding(2);
            input_ClearData.Name = "input_ClearData";
            input_ClearData.PrefixText = "清空数据:";
            input_ClearData.Radius = 3;
            input_ClearData.ReadOnly = true;
            input_ClearData.Size = new Size(180, 46);
            input_ClearData.TabIndex = 5;
            input_ClearData.VerifyKeyboard += input_ClearData_VerifyKey;
            // 
            // input_MouseThroughKey
            // 
            input_MouseThroughKey.Anchor = AnchorStyles.Top;
            input_MouseThroughKey.Font = new Font("HarmonyOS Sans SC", 9F);
            input_MouseThroughKey.Location = new Point(22, 79);
            input_MouseThroughKey.Margin = new Padding(2);
            input_MouseThroughKey.Name = "input_MouseThroughKey";
            input_MouseThroughKey.PrefixText = "鼠标穿透:";
            input_MouseThroughKey.Radius = 3;
            input_MouseThroughKey.ReadOnly = true;
            input_MouseThroughKey.Size = new Size(180, 46);
            input_MouseThroughKey.TabIndex = 4;
            input_MouseThroughKey.VerifyKeyboard += input_MouseThroughKey_VerifyKey;
            // 
            // divider_KeySettings_1
            // 
            divider_KeySettings_1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            divider_KeySettings_1.BackColor = Color.Transparent;
            divider_KeySettings_1.Location = new Point(23, 55);
            divider_KeySettings_1.Margin = new Padding(2);
            divider_KeySettings_1.Name = "divider_KeySettings_1";
            divider_KeySettings_1.OrientationMargin = 0F;
            divider_KeySettings_1.Size = new Size(388, 10);
            divider_KeySettings_1.TabIndex = 3;
            divider_KeySettings_1.Text = "";
            // 
            // label_KeySettingsTitle
            // 
            label_KeySettingsTitle.BackColor = Color.Transparent;
            label_KeySettingsTitle.Font = new Font("HarmonyOS Sans SC", 9.999999F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label_KeySettingsTitle.ForeColor = Color.FromArgb(34, 151, 244);
            label_KeySettingsTitle.Location = new Point(23, 18);
            label_KeySettingsTitle.Margin = new Padding(2);
            label_KeySettingsTitle.Name = "label_KeySettingsTitle";
            label_KeySettingsTitle.Size = new Size(67, 22);
            label_KeySettingsTitle.TabIndex = 1;
            label_KeySettingsTitle.Text = "按键设置";
            // 
            // panel_BasicSetup
            // 
            panel_BasicSetup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel_BasicSetup.Back = Color.White;
            panel_BasicSetup.BackColor = Color.Transparent;
            panel_BasicSetup.Controls.Add(panel_BasicSetupColorBar);
            panel_BasicSetup.Controls.Add(label_BasicSetupTip);
            panel_BasicSetup.Controls.Add(select_NetcardSelector);
            panel_BasicSetup.Controls.Add(divider_BasicSetup_1);
            panel_BasicSetup.Controls.Add(label_BasicSetupTitle);
            panel_BasicSetup.Location = new Point(13, 14);
            panel_BasicSetup.Margin = new Padding(13, 14, 13, 14);
            panel_BasicSetup.Name = "panel_BasicSetup";
            panel_BasicSetup.Size = new Size(413, 149);
            panel_BasicSetup.TabIndex = 0;
            panel_BasicSetup.Text = "panel4";
            // 
            // panel_BasicSetupColorBar
            // 
            panel_BasicSetupColorBar.BackColor = Color.FromArgb(34, 151, 244);
            panel_BasicSetupColorBar.Location = new Point(0, 9);
            panel_BasicSetupColorBar.Name = "panel_BasicSetupColorBar";
            panel_BasicSetupColorBar.Size = new Size(3, 42);
            panel_BasicSetupColorBar.TabIndex = 0;
            // 
            // label_BasicSetupTip
            // 
            label_BasicSetupTip.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label_BasicSetupTip.BackColor = Color.Transparent;
            label_BasicSetupTip.Font = new Font("HarmonyOS Sans SC", 7F);
            label_BasicSetupTip.ForeColor = Color.FromArgb(34, 151, 244);
            label_BasicSetupTip.Location = new Point(267, 18);
            label_BasicSetupTip.Margin = new Padding(2);
            label_BasicSetupTip.Name = "label_BasicSetupTip";
            label_BasicSetupTip.Size = new Size(126, 22);
            label_BasicSetupTip.TabIndex = 2;
            label_BasicSetupTip.Text = "自动设置错误时可手动设置";
            label_BasicSetupTip.TextAlign = ContentAlignment.MiddleRight;
            // 
            // select_NetcardSelector
            // 
            select_NetcardSelector.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            select_NetcardSelector.Font = new Font("HarmonyOS Sans SC", 9F);
            select_NetcardSelector.List = true;
            select_NetcardSelector.Location = new Point(23, 81);
            select_NetcardSelector.Margin = new Padding(2);
            select_NetcardSelector.Name = "select_NetcardSelector";
            select_NetcardSelector.PrefixText = "请选择网卡：";
            select_NetcardSelector.Radius = 3;
            select_NetcardSelector.Size = new Size(370, 40);
            select_NetcardSelector.TabIndex = 4;
            select_NetcardSelector.SelectedIndexChanged += select_NetcardSelector_SelectedIndexChanged;
            // 
            // divider_BasicSetup_1
            // 
            divider_BasicSetup_1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            divider_BasicSetup_1.BackColor = Color.Transparent;
            divider_BasicSetup_1.Location = new Point(23, 55);
            divider_BasicSetup_1.Margin = new Padding(2);
            divider_BasicSetup_1.Name = "divider_BasicSetup_1";
            divider_BasicSetup_1.OrientationMargin = 0F;
            divider_BasicSetup_1.Size = new Size(388, 10);
            divider_BasicSetup_1.TabIndex = 3;
            divider_BasicSetup_1.Text = "";
            // 
            // label_BasicSetupTitle
            // 
            label_BasicSetupTitle.BackColor = Color.Transparent;
            label_BasicSetupTitle.Font = new Font("HarmonyOS Sans SC", 9.999999F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label_BasicSetupTitle.ForeColor = Color.FromArgb(34, 151, 244);
            label_BasicSetupTitle.Location = new Point(23, 18);
            label_BasicSetupTitle.Margin = new Padding(2);
            label_BasicSetupTitle.Name = "label_BasicSetupTitle";
            label_BasicSetupTitle.Size = new Size(67, 22);
            label_BasicSetupTitle.TabIndex = 1;
            label_BasicSetupTitle.Text = "基础设置";
            // 
            // panel_FooterPanel
            // 
            panel_FooterPanel.Controls.Add(button_FormCancel);
            panel_FooterPanel.Controls.Add(button_Save);
            panel_FooterPanel.Dock = DockStyle.Bottom;
            panel_FooterPanel.Location = new Point(0, 723);
            panel_FooterPanel.Margin = new Padding(2);
            panel_FooterPanel.Name = "panel_FooterPanel";
            panel_FooterPanel.Radius = 3;
            panel_FooterPanel.Shadow = 6;
            panel_FooterPanel.ShadowAlign = AntdUI.TAlignMini.Top;
            panel_FooterPanel.Size = new Size(455, 57);
            panel_FooterPanel.TabIndex = 2;
            panel_FooterPanel.Text = "panel7";
            // 
            // button_FormCancel
            // 
            button_FormCancel.Anchor = AnchorStyles.Bottom;
            button_FormCancel.Ghost = true;
            button_FormCancel.Icon = (Image)resources.GetObject("button_FormCancel.Icon");
            button_FormCancel.IconHover = (Image)resources.GetObject("button_FormCancel.IconHover");
            button_FormCancel.IconPosition = AntdUI.TAlignMini.None;
            button_FormCancel.IconRatio = 1.3F;
            button_FormCancel.Location = new Point(261, 14);
            button_FormCancel.Margin = new Padding(2);
            button_FormCancel.Name = "button_FormCancel";
            button_FormCancel.Size = new Size(36, 35);
            button_FormCancel.TabIndex = 1;
            button_FormCancel.Click += button_FormCancel_Click;
            // 
            // button_Save
            // 
            button_Save.Anchor = AnchorStyles.Bottom;
            button_Save.Ghost = true;
            button_Save.Icon = (Image)resources.GetObject("button_Save.Icon");
            button_Save.IconHover = (Image)resources.GetObject("button_Save.IconHover");
            button_Save.IconPosition = AntdUI.TAlignMini.None;
            button_Save.IconRatio = 1.3F;
            button_Save.Location = new Point(157, 14);
            button_Save.Margin = new Padding(2);
            button_Save.Name = "button_Save";
            button_Save.Size = new Size(36, 35);
            button_Save.TabIndex = 0;
            button_Save.Click += button_Save_Click;
            // 
            // stackPanel_MainPanel
            // 
            stackPanel_MainPanel.AutoScroll = true;
            stackPanel_MainPanel.Back = Color.FromArgb(239, 239, 239);
            stackPanel_MainPanel.Controls.Add(panel_CombatSettings);
            stackPanel_MainPanel.Controls.Add(panel_KeySettings);
            stackPanel_MainPanel.Controls.Add(panel_BasicSetup);
            stackPanel_MainPanel.Dock = DockStyle.Fill;
            stackPanel_MainPanel.Location = new Point(0, 63);
            stackPanel_MainPanel.Margin = new Padding(6, 7, 6, 7);
            stackPanel_MainPanel.Name = "stackPanel_MainPanel";
            stackPanel_MainPanel.Size = new Size(455, 660);
            stackPanel_MainPanel.TabIndex = 1;
            stackPanel_MainPanel.Text = "stackPanel1";
            stackPanel_MainPanel.Vertical = true;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(455, 780);
            Controls.Add(stackPanel_MainPanel);
            Controls.Add(panel_FooterPanel);
            Controls.Add(panel_MainTitlePanel);
            Controls.Add(pageHeader_MainHeader);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            MinimumSize = new Size(450, 100);
            Name = "SettingsForm";
            Opacity = 0.95D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "基础设置";
            FormClosing += SettingsForm_FormClosing;
            Load += SettingsForm_Load;
            ForeColorChanged += SettingsForm_ForeColorChanged;
            pageHeader_MainHeader.ResumeLayout(false);
            panel_MainTitlePanel.ResumeLayout(false);
            panel_CombatSettings.ResumeLayout(false);
            panel_KeySettings.ResumeLayout(false);
            panel_BasicSetup.ResumeLayout(false);
            panel_FooterPanel.ResumeLayout(false);
            stackPanel_MainPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.PageHeader pageHeader_MainHeader;
        private AntdUI.Panel panel_MainTitlePanel;
        private AntdUI.Label label_SettingTitle;
        private AntdUI.Panel panel_BasicSetup;
        private AntdUI.Label label_BasicSetupTitle;
        private AntdUI.Divider divider_BasicSetup_1;
        public AntdUI.Select select_NetcardSelector;
        private AntdUI.Panel panel_KeySettings;
        private AntdUI.Divider divider_KeySettings_1;
        private AntdUI.Label label_KeySettingsTitle;
        private AntdUI.Input input_ClearData;
        private AntdUI.Input input_MouseThroughKey;
        private AntdUI.Panel panel_CombatSettings;
        private AntdUI.Divider divider_CombatSettings_1;
        private AntdUI.Label label_CombatSettingsTitle;
        private AntdUI.Label label_ClearAllDataWhenSwitch;
        private AntdUI.Switch switch_ClearAllDataWhenSwitch;
        private AntdUI.Label label_TitleText;
        private AntdUI.Label label_KeySettingsTip;
        private AntdUI.Panel panel_FooterPanel;
        private AntdUI.Button button_FormCancel;
        private AntdUI.Button button_Save;
        private AntdUI.Label label_BasicSetupTip;
        private AntdUI.Label label_CombatSettingsTip;
        private AntdUI.InputNumber inputNumber_ClearSectionedDataTime;
        private AntdUI.Label label_Transparent;
        private AntdUI.Slider slider_Transparency;
        private AntdUI.StackPanel stackPanel_MainPanel;
        private AntdUI.Label label_DpsShowType;
        private AntdUI.Select select_DamageDisplayType;
        private Panel panel_BasicSetupColorBar;
        private Panel panel_CombatSettingsColorBar;
        private Panel panel_KeySettingsColorBar;
    }
}