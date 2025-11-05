namespace BlueMeter.WinForm.Forms
{
    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            sortedProgressBarList1 = new BlueMeter.WinForm.Control.SortedProgressBarList();
            numericUpDown1 = new NumericUpDown();
            numericUpDown2 = new NumericUpDown();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            SuspendLayout();
            // 
            // sortedProgressBarList1
            // 
            sortedProgressBarList1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            sortedProgressBarList1.BackColor = Color.White;
            sortedProgressBarList1.Location = new Point(13, 11);
            sortedProgressBarList1.Margin = new Padding(5, 4, 5, 4);
            sortedProgressBarList1.Name = "sortedProgressBarList1";
            sortedProgressBarList1.OrderColor = Color.Black;
            sortedProgressBarList1.OrderFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            sortedProgressBarList1.OrderImageAlign = Control.GDI.RenderContent.ContentAlign.MiddleLeft;
            sortedProgressBarList1.OrderImageRenderSize = new Size(0, 0);
            sortedProgressBarList1.OrderImages = null;
            sortedProgressBarList1.Size = new Size(1237, 600);
            sortedProgressBarList1.TabIndex = 1;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(723, 685);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(105, 30);
            numericUpDown1.TabIndex = 2;
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(834, 685);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(105, 30);
            numericUpDown2.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(946, 683);
            button1.Name = "button1";
            button1.Size = new Size(127, 34);
            button1.TabIndex = 3;
            button1.Text = "新增/变更";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(1080, 683);
            button2.Name = "button2";
            button2.Size = new Size(127, 34);
            button2.TabIndex = 4;
            button2.Text = "删除";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(432, 648);
            button3.Name = "button3";
            button3.Size = new Size(203, 76);
            button3.TabIndex = 5;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 765);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(numericUpDown2);
            Controls.Add(numericUpDown1);
            Controls.Add(sortedProgressBarList1);
            Margin = new Padding(2, 1, 2, 1);
            Name = "TestForm";
            Text = "TestForm";
            Load += TestForm_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Control.SortedProgressBarList sortedProgressBarList1;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numericUpDown2;
        private Button button1;
        private Button button2;
        private Button button3;
    }
}