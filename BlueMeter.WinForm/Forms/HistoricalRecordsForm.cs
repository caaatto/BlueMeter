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
using BlueMeter.WinForm.Plugin;

namespace BlueMeter.WinForm.Forms
{
    public partial class HistoricalRecordsForm : BorderlessForm
    {
        public HistoricalRecordsForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }
    }
}
