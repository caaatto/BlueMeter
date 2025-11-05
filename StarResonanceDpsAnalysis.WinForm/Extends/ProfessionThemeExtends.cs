using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarResonanceDpsAnalysis.Assets;

namespace StarResonanceDpsAnalysis.WinForm.Extends
{
    public static class ProfessionThemeExtends
    {
        private static readonly Color DefaultColor = Color.FromArgb(0x67, 0xAE, 0xF6);
        private static readonly Dictionary<string, Color> LightThemeProfessionColorDict = new()
        {
            { "神射手", Color.FromArgb(0xff, 0xfc, 0xa3) },
            { "狼弓", Color.FromArgb(0xff, 0xfc, 0xa3) },
            { "鹰弓", Color.FromArgb(0xff, 0xfc, 0xa3) },

            { "森语者", Color.FromArgb(0x78, 0xff, 0x95) },
            { "惩戒", Color.FromArgb(0x78, 0xff, 0x95) },
            { "愈合", Color.FromArgb(0x78, 0xff, 0x95) },

            { "雷影剑士", Color.FromArgb(0xb8, 0xa3, 0xff) },
            { "居合", Color.FromArgb(0xb8, 0xa3, 0xff) },
            { "月刃", Color.FromArgb(0xb8, 0xa3, 0xff) },

            { "冰魔导师", Color.FromArgb(0xaa, 0xa6, 0xff) },
            { "冰矛", Color.FromArgb(0xaa, 0xa6, 0xff) },
            { "射线", Color.FromArgb(0xff, 0xfc, 0xa3) },

            { "青岚骑士", Color.FromArgb(0xab, 0xfa, 0xff) },
            { "重装", Color.FromArgb(0xab, 0xfa, 0xff) },
            { "空枪", Color.FromArgb(0xab, 0xfa, 0xff) },

            { "巨刃守护者", Color.FromArgb(0x8e, 0xe3, 0x92) },
            { "岩盾", Color.FromArgb(0x8e, 0xe3, 0x92) },
            { "格挡", Color.FromArgb(0x8e, 0xe3, 0x92) },

            { "神盾骑士", Color.FromArgb(0xbf, 0xe6, 0xff) },
            { "防盾", Color.FromArgb(0xbf, 0xe6, 0xff) },
            { "光盾", Color.FromArgb(0xbf, 0xe6, 0xff) },

            { "灵魂乐手", Color.FromArgb(0xff, 0x53, 0x53) },
            { "协奏", Color.FromArgb(0xff, 0x53, 0x53) },
            { "狂音", Color.FromArgb(0xff, 0x53, 0x53) },
        };

        private static readonly Dictionary<string, Color> DarkThemeProfessionColorDict = new()
        {
            { "神射手", Color.FromArgb(0x8e, 0x8b, 0x47) },
            { "狼弓", Color.FromArgb(0x8e, 0x8b, 0x47) },
            { "鹰弓", Color.FromArgb(0x8e, 0x8b, 0x47) },

            { "森语者", Color.FromArgb(0x63, 0x9c, 0x70) },
            { "惩戒", Color.FromArgb(0x63, 0x9c, 0x70) },
            { "愈合", Color.FromArgb(0x63, 0x9c, 0x70) },

            { "雷影剑士", Color.FromArgb(0x70, 0x62, 0x9c) },
            { "居合", Color.FromArgb(0x70, 0x62, 0x9c) },
            { "月刃", Color.FromArgb(0x70, 0x62, 0x9c) },

            { "冰魔导师", Color.FromArgb(0x79, 0x77, 0x9c) },
            { "冰矛", Color.FromArgb(0x79, 0x77, 0x9c) },
            { "射线", Color.FromArgb(0x8e, 0x8b, 0x47) },

            { "青岚骑士", Color.FromArgb(0x79, 0x9a, 0x9c) },
            { "重装", Color.FromArgb(0x79, 0x9a, 0x9c) },
            { "空枪", Color.FromArgb(0x79, 0x9a, 0x9c) },

            { "巨刃守护者", Color.FromArgb(0x53, 0x77, 0x58) },
            { "岩盾", Color.FromArgb(0x53, 0x77, 0x58) },
            { "格挡", Color.FromArgb(0x53, 0x77, 0x58) },

            { "神盾骑士", Color.FromArgb(0x9c, 0x9b, 0x75) },
            { "防盾", Color.FromArgb(0x9c, 0x9b, 0x75) },
            { "光盾", Color.FromArgb(0x9c, 0x9b, 0x75) },

            { "灵魂乐手", Color.FromArgb(0x9c, 0x53, 0x53) },
            { "协奏", Color.FromArgb(0x9c, 0x53, 0x53) },
            { "狂音", Color.FromArgb(0x9c, 0x53, 0x53) },
        };

        private static Dictionary<string, Image> ProfessionImageDict = new()
        {
            { "神射手", HandledAssets.神射手 },
            { "狼弓",  HandledAssets.神射手 },
            { "鹰弓",  HandledAssets.神射手 },

            { "森语者", HandledAssets.森语者 },
            { "惩戒",  HandledAssets.森语者 },
            { "愈合",  HandledAssets.森语者 },

            { "雷影剑士", HandledAssets.雷影剑士 },
            { "居合",  HandledAssets.雷影剑士 },
            { "月刃",  HandledAssets.雷影剑士 },

            { "冰魔导师", HandledAssets.冰魔导师 },
            { "冰矛",  HandledAssets.冰魔导师 },
            { "射线",  HandledAssets.冰魔导师 },

            { "青岚骑士", HandledAssets.青岚骑士 },
            { "重装",  HandledAssets.青岚骑士 },
            { "空枪",  HandledAssets.青岚骑士 },

            { "巨刃守护者", HandledAssets.巨刃守护者 },
            { "岩盾",  HandledAssets.巨刃守护者 },
            { "格挡",  HandledAssets.巨刃守护者 },

            { "神盾骑士", HandledAssets.神盾骑士 },
            { "防盾",  HandledAssets.神盾骑士 },
            { "光盾",  HandledAssets.神盾骑士 },

            { "灵魂乐手", HandledAssets.灵魂乐手 },
            { "协奏",  HandledAssets.灵魂乐手 },
            { "狂音",  HandledAssets.灵魂乐手 },
        };

        private static Bitmap? _emptyBitmap;
        public static Bitmap EmptyBitmap
        {
            get
            {
                if (_emptyBitmap == null)
                {
                    _emptyBitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
                    using var g = Graphics.FromImage(_emptyBitmap);
                    g.Clear(Color.Transparent);
                }

                return _emptyBitmap;
            }
        }

        public static Color GetProfessionThemeColor(this string professionName, bool isLightTheme)
        {
            if (TryGetProfessionThemeColor(professionName, isLightTheme, out var color))
            {
                return color;
            }

            return DefaultColor;
        }

        public static bool TryGetProfessionThemeColor(this string professionName, bool isLightTheme, out Color color) 
        {
            var dic = isLightTheme
                ? LightThemeProfessionColorDict
                : DarkThemeProfessionColorDict;

            return dic.TryGetValue(professionName, out color);
        }

        public static Image GetProfessionImage(this string professionName, Image? def = null)
        {
            if (ProfessionImageDict.TryGetValue(professionName, out var image))
            {
                return image;
            }

            return def ?? EmptyBitmap;
        }

        public static Bitmap GetProfessionBitmap(this string professionName, Bitmap? def = null) 
        {
            if (ProfessionImageDict.TryGetValue(professionName, out var image))
            {
                return (Bitmap)image;
            }

            return def ?? EmptyBitmap;
        }
    }
}
