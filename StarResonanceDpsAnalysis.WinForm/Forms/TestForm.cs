using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using StarResonanceDpsAnalysis.Assets;
using StarResonanceDpsAnalysis.Core.Analyze;
using StarResonanceDpsAnalysis.Core.Analyze.Models;
using StarResonanceDpsAnalysis.Core.Data.Models;
using StarResonanceDpsAnalysis.Core.Tools;
using StarResonanceDpsAnalysis.WinForm.Control;
using StarResonanceDpsAnalysis.WinForm.Control.GDI;
using StarResonanceDpsAnalysis.WinForm.Effects.Enum;

namespace StarResonanceDpsAnalysis.WinForm.Forms
{
    public partial class TestForm : Form
    {
        int id = 0;
        List<ProgressBarData> data = [];
        List<Image> images =
        [
            HandledAssets.FrostMage,
            HandledAssets.HeavyGuardian,
            HandledAssets.VerdantOracle,
            HandledAssets.SoulMusician,
            HandledAssets.Marksman,
            HandledAssets.Stormblade,
            HandledAssets.WindKnight
        ];
        Random rd = new();

        public TestForm()
        {
            InitializeComponent();

            sortedProgressBarList1.ProgressBarHeight = 50;
            sortedProgressBarList1.OrderOffset = new RenderContent.ContentOffset { X = 45, Y = 0 };
            sortedProgressBarList1.OrderCallback = (i) => $"{i:d2}";
            sortedProgressBarList1.OrderColor = Color.Fuchsia;
            sortedProgressBarList1.OrderFont = new Font("平方韶华体", 24f, FontStyle.Bold, GraphicsUnit.Pixel);
            sortedProgressBarList1.OrderImages =
            [
                HandledAssets.Crown,
                HandledAssets.Crown_White
            ];
            sortedProgressBarList1.OrderImageOffset = new RenderContent.ContentOffset { X = 10, Y = 0 };
            //sortedProgressBarList1.OrderImageRenderSize = new Size(32, 32);

            numericUpDown1.Minimum = -1;
            numericUpDown2.Minimum = -1;

            button1.Click += (s, e) =>
            {
                if (numericUpDown1.Value > data.Count) return;

                if (numericUpDown1.Value <= 0)
                {
                    ++id;

                    data.Add(new ProgressBarData
                    {
                        ID = id,
                        ProgressBarCornerRadius = 5,
                        ProgressBarColor = Color.FromArgb(0xF5, 0xEB, 0xAE),
                        ProgressBarValue = (double)numericUpDown2.Value / 100d,
                        ContentList =
                        [
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Image,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                Offset = new RenderContent.ContentOffset { X = 48, Y = 0 },
                                Image = images[rd.Next(images.Count)],
                                ImageRenderSize = new Size(32, 32)
                            },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                Offset = new RenderContent.ContentOffset { X = 90, Y = 0 },
                                Text = $"{RandomName()}({id:d5})",
                                ForeColor = Color.Black,
                                Font = new Font("Microsoft YaHei UI", 24f, FontStyle.Regular, GraphicsUnit.Pixel),
                            },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleRight,
                                Offset = new RenderContent.ContentOffset { X = -90, Y = 4 },
                                Text = $"3.0万(1.4w)",
                                ForeColor = Color.Black,
                                Font = new Font("Microsoft YaHei UI", 16f, FontStyle.Regular, GraphicsUnit.Pixel),
                            },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleRight,
                                Offset = new RenderContent.ContentOffset { X = 0, Y = 0 },
                                Text = $"{numericUpDown2.Value:f2}%",
                                ForeColor = Color.Black,
                                Font = new Font("黑体", 24f, FontStyle.Regular, GraphicsUnit.Pixel),
                            },
                        ],
                    });
                }
                else
                {
                    var index = (int)numericUpDown1.Value - 1;
                    data[index].ProgressBarValue = (double)numericUpDown2.Value / 100d;
                    data[index].ContentList![3].Text = $"{numericUpDown2.Value:f2}%";
                }

                sortedProgressBarList1.Data = data;
            };

            button2.Click += (s, e) =>
            {
                var data = new List<ProgressBarData>();
                for (int i = 0; i < 10; i++)
                {
                    data.Add(new ProgressBarData 
                    {
                        ID = i,
                        ProgressBarCornerRadius = 5,
                        ProgressBarColor = Color.FromArgb(0xF5, 0xEB, 0xAE),
                        ProgressBarValue = rd.NextDouble(),
                        ContentList =
                        [
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Image,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                Offset = new RenderContent.ContentOffset { X = 48, Y = 0 },
                                Image = images[rd.Next(images.Count)],
                                ImageRenderSize = new Size(32, 32)
                            },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleLeft,
                                Offset = new RenderContent.ContentOffset { X = 90, Y = 0 },
                                Text = $"{RandomName()}({id:d5})",
                                ForeColor = Color.Black,
                                Font = new Font("Microsoft YaHei UI", 24f, FontStyle.Regular, GraphicsUnit.Pixel),
                            },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleRight,
                                Offset = new RenderContent.ContentOffset { X = -90, Y = 4 },
                                Text = $"3.0万(1.4w)",
                                ForeColor = Color.Black,
                                Font = new Font("Microsoft YaHei UI", 16f, FontStyle.Regular, GraphicsUnit.Pixel),
                            },
                            new RenderContent
                            {
                                Type = RenderContent.ContentType.Text,
                                Align = RenderContent.ContentAlign.MiddleRight,
                                Offset = new RenderContent.ContentOffset { X = 0, Y = 0 },
                                Text = $"{numericUpDown2.Value:f2}%",
                                ForeColor = Color.Black,
                                Font = new Font("黑体", 24f, FontStyle.Regular, GraphicsUnit.Pixel),
                            },
                        ],
                    });
                }
                sortedProgressBarList1.Data = data;
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void TestForm_Load(object sender, EventArgs e)
        {

        }

        private string RandomName()
        {
            var sb = new StringBuilder();

            // 随机长度 2~10
            int len = rd.Next(2, 11);

            for (int i = 0; i < len; ++i)
            {
                int code = rd.Next(0x4E00, 0x9FFF + 1);
                sb.Append((char)code);
            }

            return sb.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //// 测试功能, 已失效
            //var id = 8900L;
            //var ticks = 638923753650224636;

            //BattleLogWriter.WriteToFile(Path.Combine(Environment.CurrentDirectory, "Logs"), new() 
            //{
            //    FileVersion = LogsFileVersion.V3_0_0,
            //    PlayerInfos = [ new PlayerInfo { UID = 123, Name = "惊奇猫猫盒" }, new PlayerInfo { UID = 234, Name = "露詩" } ],
            //    BattleLogs = [ new BattleLog { PacketID = id, TimeTicks = ticks, SkillID = 0, AttackerUuid = 123, TargetUuid = 234, Value = 999 } ]
            //});

            var data = BattleLogReader.ReadFile(Path.Combine(Environment.CurrentDirectory, "Logs", "Logs_2025_09_02_02_02_45_2025_09_02_02_02_45.srlogs"));
        }

        private void button3_Click(object sender, EventArgs e)
        {

            AAA();
        }
        public static double GetScaling()
        {
            var screen = Screen.PrimaryScreen;
            if (screen == null) return 1;

            Rectangle workingArea = screen.WorkingArea;
            Rectangle bounds = screen.Bounds;
            double scale = (double)workingArea.Width / bounds.Width;

            return scale;
        }

        public static void AAA()
        {
            double scale = GetScaling();
            Console.WriteLine(scale.ToString());
        }
    }
}
