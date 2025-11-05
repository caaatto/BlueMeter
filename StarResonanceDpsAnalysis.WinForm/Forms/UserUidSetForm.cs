using System;
using System.Runtime.InteropServices;

using AntdUI;
using StarResonanceDpsAnalysis.WinForm.Plugin;
using StarResonanceDpsAnalysis.WinForm.Plugin.DamageStatistics;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class UserUidSetForm : BorderlessForm
    {
        public UserUidSetForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
        }

        /// <summary>
        /// 优化的UID设置控件 - 增强验证和用户体验
        /// </summary>
        private void UserUidSet_Load(object sender, EventArgs e)
        {
            // 从AppConfig加载已保存的设置到界面
            LoadCurrentSettingsToUI();

            // 添加实时验证
            InitializeValidation();

            // 显示当前用户信息
            DisplayCurrentUserInfo();
        }

        /// <summary>
        /// 从AppConfig加载当前设置到界面控件
        /// </summary>
        private void LoadCurrentSettingsToUI()
        {
            try
            {
                // 加载昵称设置
                string savedNickname = AppConfig.GetValue("UserConfig", "NickName", "未知昵称");
                input2.Text = savedNickname;

                // 安全地加载UID设置
                string savedUidStr = AppConfig.GetValue("UserConfig", "Uid", "0");
                if (ulong.TryParse(savedUidStr, out ulong savedUid))
                {
                    inputNumber1.Value = savedUid;
                    Console.WriteLine($"已加载保存的设置 - UID: {savedUid}, 昵称: {savedNickname}");
                }
                else
                {
                    inputNumber1.Value = 0;
                    Console.WriteLine($"UID配置格式错误: {savedUidStr}，已重置为0");

                    // 修复损坏的配置
                    AppConfig.SetValue("UserConfig", "Uid", "0");
                }
                select1.SelectedValue = AppConfig.GetValue("UserConfig", "Profession", "未知职业");



                // 确保AppConfig的全局属性与界面同步
                AppConfig.Uid = (long)inputNumber1.Value;
                AppConfig.NickName = input2.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载用户设置时出错: {ex.Message}");

                // 出错时设置默认值
                inputNumber1.Value = 0;
                input2.Text = "未知昵称";
            }
        }

        /// <summary>
        /// 初始化输入验证
        /// </summary>
        private void InitializeValidation()
        {
            // UID输入验证 - 确保是有效的ulong范围
            inputNumber1.ValueChanged += (s, e) =>
            {
                if (inputNumber1.Value > ulong.MaxValue || inputNumber1.Value < 0)
                {
                    inputNumber1.Value = 0;
                    MessageBox.Show("UID必须是0到" + ulong.MaxValue + "之间的数字", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // 昵称输入验证
            input2.TextChanged += (s, e) =>
            {
                string nickname = input2.Text.Trim();
                if (nickname.Length > 20)
                {
                    input2.Text = nickname.Substring(0, 20);
                    input2.SelectionStart = input2.Text.Length;
                    MessageBox.Show("昵称长度不能超过20个字符", "输入提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
        }

        /// <summary>
        /// 显示当前用户信息
        /// </summary>
        private void DisplayCurrentUserInfo()
        {
            var currentUid = (long)inputNumber1.Value;
            if (currentUid > 0)
            {
                var (nickname, combatPower, profession) = StatisticData._manager.GetPlayerBasicInfo(currentUid);

                // 可以添加一个信息显示区域
                Console.WriteLine($"当前用户信息 - UID: {currentUid}, 昵称: {nickname}, 战力: {combatPower}, 职业: {profession}");

                // 如果有UI标签可以显示这些信息
                // lblCurrentInfo.Text = $"当前: {nickname} (战力: {combatPower})";
            }
        }

        /// <summary>
        /// 公开的保存用户设置方法，供Modal调用
        /// </summary>
        public void SaveUserSettings()
        {
            // 验证输入数据
            if (!ValidateInput(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // 从界面获取当前输入的值
            var newUid = (long)inputNumber1.Value;
            string newNickname = input2.Text.Trim();
            string profession = select1.SelectedValue?.ToString()?.Trim() ?? string.Empty;

            // 获取原始配置值用于比较
            string oldUidStr = AppConfig.GetValue("UserConfig", "Uid", "0");
            string oldNickname = AppConfig.GetValue("UserConfig", "NickName", "未知昵称");
            string oldProfession = AppConfig.GetValue("UserConfig", "Profession", "未知职业");


            bool uidChanged = !long.TryParse(oldUidStr, out long oldUid) || oldUid != newUid;
            bool nicknameChanged = oldNickname != newNickname;
            bool professionChanged = oldProfession != profession;

            // 只有当值真正发生变化时才保存
            if (!uidChanged && !nicknameChanged && !professionChanged)
            {
                Console.WriteLine("用户信息没有变化，无需保存");
                return;
            }

            // 保存界面配置到AppConfig
            if (uidChanged)
            {
                AppConfig.SetValue("UserConfig", "Uid", newUid.ToString());
                Console.WriteLine($"UID已更新: {oldUid} → {newUid}");
            }

            if (nicknameChanged)
            {
                AppConfig.SetValue("UserConfig", "NickName", newNickname);
                Console.WriteLine($"昵称已更新: {oldNickname} → {newNickname}");
            }

            if (professionChanged)
            {
                AppConfig.SetValue("UserConfig", "Profession", profession);
                Console.WriteLine($"职业已更新: {oldProfession} → {profession}");
            }

            // 更新全局AppConfig属性以保持一致性
            AppConfig.Uid = newUid;
            AppConfig.NickName = newNickname;
            AppConfig.Profession = profession;

            // 同步到统计数据管理器
            StatisticData._manager.SetNickname(newUid, newNickname);
            StatisticData._manager.SetProfession(newUid, profession);

            // 如果UID发生变化，询问用户是否清空统计数据
            if (uidChanged && oldUid != 0)
            {
                var result = MessageBox.Show(
                    $"检测到UID从 {oldUid} 更改为 {newUid}\n" +
                    "这可能会影响当前的统计数据关联。\n" +
                    "是否需要清空当前统计数据以避免混淆？",
                    "UID变更提醒",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    StatisticData._manager.ClearAll(false);
                    Console.WriteLine("因UID变更已清空统计数据");
                }
            }

            // 显示保存成功的反馈
            Console.WriteLine($"界面设置已成功保存 - UID: {newUid}, 昵称: {newNickname}");
        }

        /// <summary>
        /// 验证用户输入
        /// </summary>
        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            // 验证UID
            if (inputNumber1.Value <= 0)
            {
                errorMessage = "UID必须大于0";
                return false;
            }

            // 验证昵称
            string nickname = input2.Text.Trim();
            if (string.IsNullOrEmpty(nickname))
            {
                errorMessage = "昵称不能为空";
                return false;
            }

            if (nickname.Length > 20)
            {
                errorMessage = "昵称长度不能超过20个字符";
                return false;
            }

            // 可以添加更多验证规则，如特殊字符检查
            if (nickname.Contains("<") || nickname.Contains(">") || nickname.Contains("&"))
            {
                errorMessage = "昵称不能包含特殊字符 < > &";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 原始的保存按钮逻辑 - 保留向后兼容性
        /// </summary>
        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SaveUserSettings();
                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存用户设置时发生错误：{ex.Message}", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"保存用户设置异常: {ex}");
            }
        }

        private void UserUidSet_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel6_Click(object sender, EventArgs e)
        {

        }


        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

    }
}
