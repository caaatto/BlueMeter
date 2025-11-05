namespace BlueMeter.WinForm.Forms.ModuleForm
{
    partial class ModuleExcludeForm
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
            button1 = new AntdUI.Button();
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            stackPanel1 = new AntdUI.StackPanel();
            groupBox5 = new GroupBox();
            chkExtremeDesperateGuardian = new AntdUI.Checkbox();
            chkExtremeTeamCrit = new AntdUI.Checkbox();
            chkExtremeLifeDrain = new AntdUI.Checkbox();
            chkExtremeLifeFluctuation = new AntdUI.Checkbox();
            chkExtremeEmergencyMeasures = new AntdUI.Checkbox();
            chkExtremeLifeConvergence = new AntdUI.Checkbox();
            chkExtremeDamageStack = new AntdUI.Checkbox();
            chkExtremeFlexibleMovement = new AntdUI.Checkbox();
            groupBox2 = new GroupBox();
            chkMagicResistance = new AntdUI.Checkbox();
            chkPhysicalResistance = new AntdUI.Checkbox();
            chkSpecialHealingBoost = new AntdUI.Checkbox();
            chkExpertHealingBoost = new AntdUI.Checkbox();
            groupBox4 = new GroupBox();
            chkStrengthBoost = new AntdUI.Checkbox();
            chkAgilityBoost = new AntdUI.Checkbox();
            chkIntelligenceBoost = new AntdUI.Checkbox();
            chkCastingFocus = new AntdUI.Checkbox();
            groupBox1 = new GroupBox();
            chkEliteStrike = new AntdUI.Checkbox();
            chkAttackSpeedFocus = new AntdUI.Checkbox();
            chkSpecialAttackDamage = new AntdUI.Checkbox();
            chkCriticalFocus = new AntdUI.Checkbox();
            chkLuckFocus = new AntdUI.Checkbox();
            pageHeader1.SuspendLayout();
            stackPanel1.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 964);
            button1.Name = "button1";
            button1.Size = new Size(150, 53);
            button1.TabIndex = 1;
            button1.Text = "清空排除";
            button1.Type = AntdUI.TTypeMini.Error;
            button1.Click += button1_Click;
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
            pageHeader1.Size = new Size(387, 38);
            pageHeader1.TabIndex = 32;
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
            TitleText.Size = new Size(387, 38);
            TitleText.TabIndex = 27;
            TitleText.Text = "Exclude Mods";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            TitleText.MouseDown += TitleText_MouseDown;
            // 
            // stackPanel1
            // 
            stackPanel1.AutoScroll = true;
            stackPanel1.Controls.Add(groupBox5);
            stackPanel1.Controls.Add(groupBox2);
            stackPanel1.Controls.Add(groupBox4);
            stackPanel1.Controls.Add(groupBox1);
            stackPanel1.Gap = 6;
            stackPanel1.Location = new Point(12, 56);
            stackPanel1.Name = "stackPanel1";
            stackPanel1.Size = new Size(356, 888);
            stackPanel1.TabIndex = 56;
            stackPanel1.Text = "stackPanel1";
            stackPanel1.Vertical = true;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(chkExtremeDesperateGuardian);
            groupBox5.Controls.Add(chkExtremeTeamCrit);
            groupBox5.Controls.Add(chkExtremeLifeDrain);
            groupBox5.Controls.Add(chkExtremeLifeFluctuation);
            groupBox5.Controls.Add(chkExtremeEmergencyMeasures);
            groupBox5.Controls.Add(chkExtremeLifeConvergence);
            groupBox5.Controls.Add(chkExtremeDamageStack);
            groupBox5.Controls.Add(chkExtremeFlexibleMovement);
            groupBox5.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox5.Location = new Point(3, 598);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(350, 286);
            groupBox5.TabIndex = 52;
            groupBox5.TabStop = false;
            groupBox5.Text = "特殊类";
            // 
            // chkExtremeDesperateGuardian
            // 
            chkExtremeDesperateGuardian.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeDesperateGuardian.Location = new Point(174, 213);
            chkExtremeDesperateGuardian.Name = "chkExtremeDesperateGuardian";
            chkExtremeDesperateGuardian.Size = new Size(153, 54);
            chkExtremeDesperateGuardian.TabIndex = 53;
            chkExtremeDesperateGuardian.Text = "极-绝境守护";
            // 
            // chkExtremeTeamCrit
            // 
            chkExtremeTeamCrit.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeTeamCrit.Location = new Point(17, 213);
            chkExtremeTeamCrit.Name = "chkExtremeTeamCrit";
            chkExtremeTeamCrit.Size = new Size(153, 54);
            chkExtremeTeamCrit.TabIndex = 52;
            chkExtremeTeamCrit.Text = "极-全队幸暴";
            // 
            // chkExtremeLifeDrain
            // 
            chkExtremeLifeDrain.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeLifeDrain.Location = new Point(174, 155);
            chkExtremeLifeDrain.Name = "chkExtremeLifeDrain";
            chkExtremeLifeDrain.Size = new Size(153, 54);
            chkExtremeLifeDrain.TabIndex = 51;
            chkExtremeLifeDrain.Text = "极-生命汲取";
            // 
            // chkExtremeLifeFluctuation
            // 
            chkExtremeLifeFluctuation.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeLifeFluctuation.Location = new Point(17, 155);
            chkExtremeLifeFluctuation.Name = "chkExtremeLifeFluctuation";
            chkExtremeLifeFluctuation.Size = new Size(153, 54);
            chkExtremeLifeFluctuation.TabIndex = 50;
            chkExtremeLifeFluctuation.Text = "极-生命波动";
            // 
            // chkExtremeEmergencyMeasures
            // 
            chkExtremeEmergencyMeasures.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeEmergencyMeasures.Location = new Point(174, 97);
            chkExtremeEmergencyMeasures.Name = "chkExtremeEmergencyMeasures";
            chkExtremeEmergencyMeasures.Size = new Size(153, 54);
            chkExtremeEmergencyMeasures.TabIndex = 49;
            chkExtremeEmergencyMeasures.Text = "极-急救措施";
            // 
            // chkExtremeLifeConvergence
            // 
            chkExtremeLifeConvergence.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeLifeConvergence.Location = new Point(17, 97);
            chkExtremeLifeConvergence.Name = "chkExtremeLifeConvergence";
            chkExtremeLifeConvergence.Size = new Size(153, 54);
            chkExtremeLifeConvergence.TabIndex = 48;
            chkExtremeLifeConvergence.Text = "极-生命凝聚";
            // 
            // chkExtremeDamageStack
            // 
            chkExtremeDamageStack.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeDamageStack.Location = new Point(17, 39);
            chkExtremeDamageStack.Name = "chkExtremeDamageStack";
            chkExtremeDamageStack.Size = new Size(153, 54);
            chkExtremeDamageStack.TabIndex = 47;
            chkExtremeDamageStack.Text = "极-伤害叠加";
            // 
            // chkExtremeFlexibleMovement
            // 
            chkExtremeFlexibleMovement.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExtremeFlexibleMovement.Location = new Point(174, 39);
            chkExtremeFlexibleMovement.Name = "chkExtremeFlexibleMovement";
            chkExtremeFlexibleMovement.Size = new Size(153, 54);
            chkExtremeFlexibleMovement.TabIndex = 46;
            chkExtremeFlexibleMovement.Text = "极-灵活身法";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(chkMagicResistance);
            groupBox2.Controls.Add(chkPhysicalResistance);
            groupBox2.Controls.Add(chkSpecialHealingBoost);
            groupBox2.Controls.Add(chkExpertHealingBoost);
            groupBox2.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox2.Location = new Point(3, 427);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(350, 156);
            groupBox2.TabIndex = 49;
            groupBox2.TabStop = false;
            groupBox2.Text = "治疗/防御类";
            // 
            // chkMagicResistance
            // 
            chkMagicResistance.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkMagicResistance.Location = new Point(174, 93);
            chkMagicResistance.Name = "chkMagicResistance";
            chkMagicResistance.Size = new Size(126, 54);
            chkMagicResistance.TabIndex = 46;
            chkMagicResistance.Text = "抵御魔法";
            // 
            // chkPhysicalResistance
            // 
            chkPhysicalResistance.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkPhysicalResistance.Location = new Point(174, 33);
            chkPhysicalResistance.Name = "chkPhysicalResistance";
            chkPhysicalResistance.Size = new Size(126, 54);
            chkPhysicalResistance.TabIndex = 47;
            chkPhysicalResistance.Text = "抵御物理";
            // 
            // chkSpecialHealingBoost
            // 
            chkSpecialHealingBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkSpecialHealingBoost.Location = new Point(17, 33);
            chkSpecialHealingBoost.Name = "chkSpecialHealingBoost";
            chkSpecialHealingBoost.Size = new Size(162, 54);
            chkSpecialHealingBoost.TabIndex = 40;
            chkSpecialHealingBoost.Text = "特攻治疗加持";
            // 
            // chkExpertHealingBoost
            // 
            chkExpertHealingBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkExpertHealingBoost.Location = new Point(17, 93);
            chkExpertHealingBoost.Name = "chkExpertHealingBoost";
            chkExpertHealingBoost.Size = new Size(162, 54);
            chkExpertHealingBoost.TabIndex = 41;
            chkExpertHealingBoost.Text = "专精治疗加持";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(chkStrengthBoost);
            groupBox4.Controls.Add(chkAgilityBoost);
            groupBox4.Controls.Add(chkIntelligenceBoost);
            groupBox4.Controls.Add(chkCastingFocus);
            groupBox4.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox4.Location = new Point(3, 235);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(350, 177);
            groupBox4.TabIndex = 51;
            groupBox4.TabStop = false;
            groupBox4.Text = "通用类";
            // 
            // chkStrengthBoost
            // 
            chkStrengthBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkStrengthBoost.Location = new Point(17, 41);
            chkStrengthBoost.Name = "chkStrengthBoost";
            chkStrengthBoost.Size = new Size(126, 54);
            chkStrengthBoost.TabIndex = 36;
            chkStrengthBoost.Text = "力量加持";
            // 
            // chkAgilityBoost
            // 
            chkAgilityBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkAgilityBoost.Location = new Point(174, 41);
            chkAgilityBoost.Name = "chkAgilityBoost";
            chkAgilityBoost.Size = new Size(126, 54);
            chkAgilityBoost.TabIndex = 37;
            chkAgilityBoost.Text = "敏捷加持";
            // 
            // chkIntelligenceBoost
            // 
            chkIntelligenceBoost.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkIntelligenceBoost.Location = new Point(17, 109);
            chkIntelligenceBoost.Name = "chkIntelligenceBoost";
            chkIntelligenceBoost.Size = new Size(126, 54);
            chkIntelligenceBoost.TabIndex = 38;
            chkIntelligenceBoost.Text = "智力加持";
            // 
            // chkCastingFocus
            // 
            chkCastingFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkCastingFocus.Location = new Point(174, 109);
            chkCastingFocus.Name = "chkCastingFocus";
            chkCastingFocus.Size = new Size(126, 54);
            chkCastingFocus.TabIndex = 42;
            chkCastingFocus.Text = "施法专注";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkEliteStrike);
            groupBox1.Controls.Add(chkAttackSpeedFocus);
            groupBox1.Controls.Add(chkSpecialAttackDamage);
            groupBox1.Controls.Add(chkCriticalFocus);
            groupBox1.Controls.Add(chkLuckFocus);
            groupBox1.Font = new Font("HarmonyOS Sans SC", 9F);
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(350, 217);
            groupBox1.TabIndex = 48;
            groupBox1.TabStop = false;
            groupBox1.Text = "攻击类";
            // 
            // chkEliteStrike
            // 
            chkEliteStrike.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkEliteStrike.Location = new Point(17, 152);
            chkEliteStrike.Name = "chkEliteStrike";
            chkEliteStrike.Size = new Size(126, 54);
            chkEliteStrike.TabIndex = 46;
            chkEliteStrike.Text = "精英打击";
            // 
            // chkAttackSpeedFocus
            // 
            chkAttackSpeedFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkAttackSpeedFocus.Location = new Point(17, 41);
            chkAttackSpeedFocus.Name = "chkAttackSpeedFocus";
            chkAttackSpeedFocus.Size = new Size(126, 54);
            chkAttackSpeedFocus.TabIndex = 43;
            chkAttackSpeedFocus.Text = "攻速专注";
            // 
            // chkSpecialAttackDamage
            // 
            chkSpecialAttackDamage.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkSpecialAttackDamage.Location = new Point(174, 41);
            chkSpecialAttackDamage.Name = "chkSpecialAttackDamage";
            chkSpecialAttackDamage.Size = new Size(126, 54);
            chkSpecialAttackDamage.TabIndex = 39;
            chkSpecialAttackDamage.Text = "特攻伤害";
            // 
            // chkCriticalFocus
            // 
            chkCriticalFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkCriticalFocus.Location = new Point(17, 92);
            chkCriticalFocus.Name = "chkCriticalFocus";
            chkCriticalFocus.Size = new Size(126, 54);
            chkCriticalFocus.TabIndex = 44;
            chkCriticalFocus.Text = "暴击专注";
            // 
            // chkLuckFocus
            // 
            chkLuckFocus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            chkLuckFocus.Location = new Point(174, 92);
            chkLuckFocus.Name = "chkLuckFocus";
            chkLuckFocus.Size = new Size(126, 54);
            chkLuckFocus.TabIndex = 45;
            chkLuckFocus.Text = "幸运专注";
            // 
            // ModuleExcludeForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(387, 1036);
            Controls.Add(stackPanel1);
            Controls.Add(pageHeader1);
            Controls.Add(button1);
            Name = "ModuleExcludeForm";
            Text = "ModuleExcludeForm";
            Load += ModuleExcludeForm_Load;
            pageHeader1.ResumeLayout(false);
            stackPanel1.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.Button button1;
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label TitleText;
        private AntdUI.StackPanel stackPanel1;
        private GroupBox groupBox5;
        private AntdUI.Checkbox chkExtremeDesperateGuardian;
        private AntdUI.Checkbox chkExtremeTeamCrit;
        private AntdUI.Checkbox chkExtremeLifeDrain;
        private AntdUI.Checkbox chkExtremeLifeFluctuation;
        private AntdUI.Checkbox chkExtremeEmergencyMeasures;
        private AntdUI.Checkbox chkExtremeLifeConvergence;
        private AntdUI.Checkbox chkExtremeDamageStack;
        private AntdUI.Checkbox chkExtremeFlexibleMovement;
        private GroupBox groupBox2;
        private AntdUI.Checkbox chkMagicResistance;
        private AntdUI.Checkbox chkPhysicalResistance;
        private AntdUI.Checkbox chkSpecialHealingBoost;
        private AntdUI.Checkbox chkExpertHealingBoost;
        private GroupBox groupBox4;
        private AntdUI.Checkbox chkStrengthBoost;
        private AntdUI.Checkbox chkAgilityBoost;
        private AntdUI.Checkbox chkIntelligenceBoost;
        private AntdUI.Checkbox chkCastingFocus;
        private GroupBox groupBox1;
        private AntdUI.Checkbox chkEliteStrike;
        private AntdUI.Checkbox chkAttackSpeedFocus;
        private AntdUI.Checkbox chkSpecialAttackDamage;
        private AntdUI.Checkbox chkCriticalFocus;
        private AntdUI.Checkbox chkLuckFocus;
    }
}