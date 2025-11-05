namespace BlueMeter.WinForm.Forms
{
    partial class DetailsPreviewForm
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
            virtualPanel1 = new AntdUI.VirtualPanel();
            SuspendLayout();
            // 
            // virtualPanel1
            // 
            virtualPanel1.Dock = DockStyle.Fill;
            virtualPanel1.Location = new Point(0, 0);
            virtualPanel1.Name = "virtualPanel1";
            virtualPanel1.Size = new Size(474, 894);
            virtualPanel1.TabIndex = 0;
            virtualPanel1.Text = "virtualPanel1";
            // 
            // DetailsPreviewForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(474, 894);
            Controls.Add(virtualPanel1);
            Name = "DetailsPreviewForm";
            Text = "DetailsPreviewForm";
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.VirtualPanel virtualPanel1;
    }
}