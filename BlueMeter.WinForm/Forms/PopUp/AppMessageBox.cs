using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AntdUI;
using DocumentFormat.OpenXml.Bibliography;
using BlueMeter.WinForm.Plugin;

namespace BlueMeter.WinForm.Forms.PopUp
{
    public partial class AppMessageBox : BorderlessForm
    {
        public AppMessageBox(string message)
        {
            InitializeComponent();

            FormGui.SetDefaultGUI(this);

            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色

            labelMessage.Text = message;
        }

        public static DialogResult ShowMessage(string message, IWin32Window? owner = null)
        {
            using var box = new AppMessageBox(message);

            if (owner == null) return box.ShowDialog(); // 默认居中屏幕
            else return box.ShowDialog(owner); // 相对 owner 居中
        }


        private void AppMessageBox_Load(object sender, EventArgs e)
        {
            label1.Font = AppConfig.SaoFont;
            labelMessage.Font = AppConfig.DigitalFont;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK; // 表示用户确认
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // 表示用户关闭
            this.Close();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }
    }
}
