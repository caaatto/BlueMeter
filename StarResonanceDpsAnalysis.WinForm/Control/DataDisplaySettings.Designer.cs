namespace StarResonanceDpsAnalysis.WinForm.Control
{
    partial class DataDisplaySettings
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            flowPanel1 = new AntdUI.FlowPanel();
            SuspendLayout();
            // 
            // flowPanel1
            // 
            flowPanel1.Dock = DockStyle.Fill;
            flowPanel1.Location = new Point(0, 0);
            flowPanel1.Name = "flowPanel1";
            flowPanel1.Size = new Size(599, 437);
            flowPanel1.TabIndex = 0;
            flowPanel1.Text = "flowPanel1";
            flowPanel1.Click += flowPanel1_Click;
            // 
            // DataDisplaySettings
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(flowPanel1);
            Name = "DataDisplaySettings";
            Size = new Size(599, 437);
            Load += DataDisplaySettings_Load;
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.FlowPanel flowPanel1;
    }
}
