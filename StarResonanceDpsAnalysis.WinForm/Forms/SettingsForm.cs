using System;
using System.Runtime.InteropServices;

using AntdUI;
using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Extends.System;
using StarResonanceDpsAnalysis.WinForm.Forms.PopUp;
using StarResonanceDpsAnalysis.WinForm.Plugin;
using StarResonanceDpsAnalysis.WinForm.Plugin.LaunchFunction;

using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class SettingsForm : BorderlessForm
    {
        public SettingsForm()
        {
            InitializeComponent();

            FormGui.SetDefaultGUI(this);

            FormGui.SetColorMode(this, AppConfig.IsLight);

            SetDefaultFontFromResources();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            LoadConfigSetUI();
        }

        /// <summary>
        /// 检测窗体字体颜色(主题)变动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsForm_ForeColorChanged(object sender, EventArgs e)
        {
            if (Config.IsLight)
            {
                //浅色
                panel_BasicSetup.Back = panel_KeySettings.Back = panel_CombatSettings.Back = ColorTranslator.FromHtml("#FFFFFF");
                stackPanel_MainPanel.Back = ColorTranslator.FromHtml("#EFEFEF");
            }
            else
            {
                panel_BasicSetup.Back = panel_KeySettings.Back = panel_CombatSettings.Back = ColorTranslator.FromHtml("#282828");
                stackPanel_MainPanel.Back = ColorTranslator.FromHtml("#1E1E1E");
            }
        }

        /// <summary>
        /// 标题鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// 处理鼠标拖拽窗口
        /// </remarks>
        private void label_TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void select_NetcardSelector_SelectedIndexChanged(object sender, IntEventArgs e)
        {
            if (AppConfig.NetworkCardName != select_NetcardSelector.Text)
            {
                AppConfig.NetworkCardName = select_NetcardSelector.Text;

                _netcardChanged = true;

                return;
            }
        }

        /// <summary>
        /// 鼠标穿透键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input_MouseThroughKey_VerifyKey(object sender, InputVerifyKeyboardEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                input_MouseThroughKey.Text = string.Empty;
                AppConfig.MouseThroughKey = null;
                return;
            }
            input_MouseThroughKey.Text = e.KeyData.KeysToString();
            AppConfig.MouseThroughKey = e.KeyData;
        }

        /// <summary>
        /// 清空数据键位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input_ClearData_VerifyKey(object sender, InputVerifyKeyboardEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                input_ClearData.Text = string.Empty;
                AppConfig.ClearDataKey = null;
                return;
            }
            input_ClearData.Text = e.KeyData.KeysToString();
            AppConfig.ClearDataKey = e.KeyData;
        }

        private void inputNumber_ClearSectionedDataTime_TextChanged(object sender, EventArgs e)
        {
            var value = inputNumber_ClearSectionedDataTime.Value.ToInt();

            DataStorage.SectionTimeout = TimeSpan.FromSeconds(value);
            AppConfig.CombatTimeClearDelaySeconds = value;
        }

        private void switch_ClearAllDataWhenSwitch_CheckedChanged(object sender, BoolEventArgs e)
        {
            AppConfig.ClearAllDataWhenSwitch = switch_ClearAllDataWhenSwitch.Checked;
        }

        private void select_DamageDisplayType_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            AppConfig.DamageDisplayType = select_DamageDisplayType.SelectedIndex;
        }

        private void slider_Transparency_ValueChanged(object sender, IntEventArgs e)
        {
            FormManager.FullFormTransparency((double)e.Value / 100, true);

            AppConfig.Transparency = slider_Transparency.Value;
        }

        /// <summary>
        /// 确定按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Save_Click(object sender, EventArgs e)
        {
            SaveDataToConfig();

            if (_netcardChanged)
            {
                if (AppMessageBox.ShowMessage("""
                    您已更改网卡设置。

                    请注意，修改网卡后需要重新启动应用程序以使更改生效。
                    是否立刻重新启动应用程序？
                    """, this) == DialogResult.OK)
                {
                    // 重新启动应用程序
                    Application.Restart();
                    // 确保退出当前进程
                    Environment.Exit(0);
                }
                else
                {
                    AppMessageBox.ShowMessage("您的网卡设置将在下次启动应用时生效。", this);
                }
            }

            Close();
        }

        private void button_FormCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!FormManager.IsMouseThrough)
            {
                FormManager.FullFormTransparency(1, true);
            }
        }
    }
}
