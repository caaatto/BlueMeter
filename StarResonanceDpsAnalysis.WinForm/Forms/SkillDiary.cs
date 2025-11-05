using System;
using System.Text.RegularExpressions;

using AntdUI;
using StarResonanceDpsAnalysis.WinForm.Core;
using StarResonanceDpsAnalysis.WinForm.Forms;
using StarResonanceDpsAnalysis.WinForm.Plugin;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class SkillDiary : BorderlessForm
    {
        public SkillDiary()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
            TitleText.Font = AppConfig.SaoFont;
            label10.Font = AppConfig.ContentFont;
            richTextBox1.Font = AppConfig.ContentFont;

        }




        public void AppendDiaryLine(string line)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new Action<string>(AppendDiaryLine), line);
                return;
            }

            // ---- 样式主题（可按需微调）----
            Color colorTime = Color.FromArgb(140, 140, 140);
            Color colorSep = Color.FromArgb(170, 170, 170);
            Color colorName = Color.FromArgb(30, 30, 30);
            Color colorDmg = Color.IndianRed;
            Color colorHeal = Color.SeaGreen;
            Color colorCount = Color.DimGray;
            Color badgeCritBack = Color.FromArgb(255, 236, 204); // 柔和橙底
            Color badgeCritFore = Color.FromArgb(178, 99, 0);
            Color badgeLuckyBack = Color.FromArgb(234, 223, 255); // 柔和紫底
            Color badgeLuckyFore = Color.FromArgb(84, 46, 158);

            // 小工具：普通写入
            void Write(string text, Color? color = null, FontStyle style = FontStyle.Regular)
            {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;

                richTextBox1.SelectionColor = color ?? richTextBox1.ForeColor;
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, style);
                // 清空背景色（避免继承前一个徽标的底色）
                if (HasSelectionBackColor()) richTextBox1.SelectionBackColor = Color.Transparent;

                richTextBox1.AppendText(text);

                // 还原
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
                richTextBox1.SelectionFont = richTextBox1.Font;
                if (HasSelectionBackColor()) richTextBox1.SelectionBackColor = Color.Transparent;
            }

            // 小工具：胶囊徽标（用背景色 + 前后空格模拟）
            void Badge(string text, Color back, Color fore, bool bold = false)
            {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;

                if (HasSelectionBackColor()) richTextBox1.SelectionBackColor = back;
                richTextBox1.SelectionColor = fore;
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, bold ? FontStyle.Bold : FontStyle.Regular);

                // 两侧加空格让“徽标”看起来更舒服
                richTextBox1.AppendText(" " + text + " ");

                // 还原
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
                richTextBox1.SelectionFont = richTextBox1.Font;
                if (HasSelectionBackColor()) richTextBox1.SelectionBackColor = Color.Transparent;
            }

            // 检查是否支持 SelectionBackColor（旧框架可能没有）
            bool HasSelectionBackColor()
            {
                try
                {
                    var _ = richTextBox1.SelectionBackColor;
                    return true;
                }
                catch { return false; }
            }

            // --------- 解析：[duration] 前缀 ----------
            var m = Regex.Match(line, @"^\[(?<dur>[^\]]+)\]\s*(?<rest>.*)$");
            if (m.Success)
            {
                // 方括号时间块：浅灰
                Write("[", colorTime);
                Write(m.Groups["dur"].Value, colorTime);
                Write("] ", colorTime);

                line = m.Groups["rest"].Value; // 剩余部分
            }

            // --------- 用 " | " 分段（与你当前的输出格式一致）----------
            var parts = line.Split(new[] { " | " }, StringSplitOptions.None);

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();

                // 第一段通常是技能名
                if (i == 0)
                {
                    // 技能名加粗，略深色
                    Write(part, colorName, FontStyle.Bold);
                }
                else
                {
                    // 匹配“伤害:12345” 或 “治疗:54321”
                    var kv = Regex.Match(part, @"^(?<k>伤害|治疗)\s*:\s*(?<v>\d+)$");
                    if (kv.Success)
                    {
                        string k = kv.Groups["k"].Value;
                        string v = kv.Groups["v"].Value; // 保留完整数字（不做 K/M 简化）

                        if (k == "伤害")
                            Write($"{k}:{v}", colorDmg, FontStyle.Regular);
                        else
                            Write($"{k}:{v}", colorHeal, FontStyle.Regular);
                    }
                    else if (part.StartsWith("释放次数:") || part.StartsWith("次数:"))
                    {
                        Write(part, colorCount, FontStyle.Regular);
                    }
                    else if (part.StartsWith("暴击"))
                    {
                        // 支持 “暴击” 或 “暴击:3”
                        var n = Regex.Match(part, @"^暴击(?::\s*(?<n>\d+))?$");
                        if (n.Success)
                        {
                            string label = n.Groups["n"].Success ? $"暴击 ×{n.Groups["n"].Value}" : "暴击";
                            Badge(label, badgeCritBack, badgeCritFore, bold: true);
                        }
                        else
                        {
                            Badge("暴击", badgeCritBack, badgeCritFore, bold: true);
                        }
                    }
                    else if (part.StartsWith("幸运"))
                    {
                        var n = Regex.Match(part, @"^幸运(?::\s*(?<n>\d+))?$");
                        if (n.Success)
                        {
                            string label = n.Groups["n"].Success ? $"幸运 ×{n.Groups["n"].Value}" : "幸运";
                            Badge(label, badgeLuckyBack, badgeLuckyFore, bold: true);
                        }
                        else
                        {
                            Badge("幸运", badgeLuckyBack, badgeLuckyFore, bold: true);
                        }
                    }
                    else
                    {
                        // 其他未知片段：保持默认色
                        Write(part);
                    }
                }

                // 分隔符（最后一段不加）
                if (i < parts.Length - 1) Write("  |  ", colorSep);
            }

            // 换行 + 滚动到底部
            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }



        private void SkillDiary_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);//设置窗体颜色
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = string.Empty;
            SkillDiaryGate.Reset();
        }

        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        private void SkillDiary_ForeColorChanged(object sender, EventArgs e)
        {

        }
    }
}
