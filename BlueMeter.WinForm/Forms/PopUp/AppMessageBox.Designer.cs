namespace BlueMeter.WinForm.Forms.PopUp
{
    partial class AppMessageBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppMessageBox));
            panel1 = new AntdUI.Panel();
            label1 = new AntdUI.Label();
            panel2 = new AntdUI.Panel();
            DialogResultCancel = new AntdUI.Button();
            DialogResultOK = new AntdUI.Button();
            labelMessage = new AntdUI.Label();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Radius = 0;
            panel1.Shadow = 6;
            panel1.ShadowAlign = AntdUI.TAlignMini.Bottom;
            panel1.Size = new Size(558, 107);
            panel1.TabIndex = 0;
            panel1.Text = "panel1";
            panel1.MouseDown += panel1_MouseDown;
            // 
            // label1
            // 
            label1.BackColor = Color.Transparent;
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("SAO Welcome TT", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(558, 98);
            label1.TabIndex = 0;
            label1.Text = "Message";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.MouseDown += panel1_MouseDown;
            // 
            // panel2
            // 
            panel2.Controls.Add(DialogResultCancel);
            panel2.Controls.Add(DialogResultOK);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 308);
            panel2.Name = "panel2";
            panel2.Radius = 0;
            panel2.Shadow = 6;
            panel2.ShadowAlign = AntdUI.TAlignMini.Top;
            panel2.Size = new Size(558, 104);
            panel2.TabIndex = 1;
            panel2.Text = "panel2";
            panel2.MouseDown += panel1_MouseDown;
            // 
            // DialogResultCancel
            // 
            DialogResultCancel.Anchor = AnchorStyles.Bottom;
            DialogResultCancel.Ghost = true;
            DialogResultCancel.Icon = BlueMeter.Assets.HandledAssets.Cancel_Normal;
            DialogResultCancel.IconHover = BlueMeter.Assets.HandledAssets.Cancel_Hover;
            DialogResultCancel.IconPosition = AntdUI.TAlignMini.None;
            DialogResultCancel.IconRatio = 1.7F;
            DialogResultCancel.Location = new Point(387, 33);
            DialogResultCancel.Name = "DialogResultCancel";
            DialogResultCancel.Size = new Size(57, 49);
            DialogResultCancel.TabIndex = 3;
            DialogResultCancel.Click += button3_Click;
            // 
            // DialogResultOK
            // 
            DialogResultOK.Anchor = AnchorStyles.Bottom;
            DialogResultOK.Ghost = true;
            DialogResultOK.Icon = BlueMeter.Assets.HandledAssets.Ok_Normal;
            DialogResultOK.IconHover = BlueMeter.Assets.HandledAssets.Ok_Hover;
            DialogResultOK.IconPosition = AntdUI.TAlignMini.None;
            DialogResultOK.IconRatio = 1.7F;
            DialogResultOK.Location = new Point(129, 33);
            DialogResultOK.Name = "DialogResultOK";
            DialogResultOK.Size = new Size(57, 49);
            DialogResultOK.TabIndex = 2;
            DialogResultOK.Click += button4_Click;
            // 
            // labelMessage
            // 
            labelMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelMessage.BackColor = Color.Transparent;
            labelMessage.Font = new Font("HarmonyOS Sans SC", 9F);
            labelMessage.Location = new Point(90, 104);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(369, 211);
            labelMessage.TabIndex = 2;
            labelMessage.Text = "";
            labelMessage.MouseDown += panel1_MouseDown;
            // 
            // AppMessageBox
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Silver;
            ClientSize = new Size(558, 412);
            Controls.Add(labelMessage);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "AppMessageBox";
            Opacity = 0.85D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AppMessageBox";
            TopMost = true;
            Load += AppMessageBox_Load;
            MouseDown += panel1_MouseDown;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.Panel panel1;
        private AntdUI.Panel panel2;
        private AntdUI.Label label1;
        private AntdUI.Button DialogResultCancel;
        private AntdUI.Button DialogResultOK;
        public AntdUI.Label labelMessage;
    }
}