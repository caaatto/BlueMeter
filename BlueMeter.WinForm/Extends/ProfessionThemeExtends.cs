using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueMeter.Assets;

namespace BlueMeter.WinForm.Extends
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
            // Marksman / 神射手 - Wildpack, Falconry
            { "神射手", Color.FromArgb(0xA4, 0x83, 0x1D) },
            { "狼弓", Color.FromArgb(0xA4, 0x83, 0x1D) },
            { "鹰弓", Color.FromArgb(0xA5, 0x86, 0x1A) },

            // Verdant Oracle / 森语者 - Lifebind, Smite
            { "森语者", Color.FromArgb(0x3E, 0x6A, 0x3F) },
            { "惩戒", Color.FromArgb(0x46, 0x7D, 0x46) },
            { "愈合", Color.FromArgb(0x3E, 0x6A, 0x3F) },

            // Stormblade / 雷影剑士 - Moonstrike, Iaido Slash
            { "雷影剑士", Color.FromArgb(0xA0, 0x21, 0x2F) },
            { "居合", Color.FromArgb(0x8C, 0x1B, 0x28) },
            { "月刃", Color.FromArgb(0xA0, 0x21, 0x2F) },

            // Frost Mage / 冰魔导师 - Frostbeam, Icicle
            { "冰魔导师", Color.FromArgb(0x72, 0x50, 0xC7) },
            { "冰矛", Color.FromArgb(0x6A, 0x7B, 0xFF) },
            { "射线", Color.FromArgb(0x72, 0x50, 0xC7) },

            // Wind Knight / 青岚骑士 - Skyward, Vanguard
            { "青岚骑士", Color.FromArgb(0x1A, 0x5A, 0x93) },
            { "重装", Color.FromArgb(0x14, 0x3A, 0x55) },
            { "空枪", Color.FromArgb(0x1A, 0x5A, 0x93) },

            // Heavy Guardian / 巨刃守护者 - Block, Earthfort
            { "巨刃守护者", Color.FromArgb(0x24, 0x4A, 0x7D) },
            { "岩盾", Color.FromArgb(0x1F, 0x41, 0x65) },
            { "格挡", Color.FromArgb(0x24, 0x4A, 0x7D) },

            // Shield Knight / 神盾骑士 - Recovery, Shield
            { "神盾骑士", Color.FromArgb(0x8A, 0x5B, 0x23) },
            { "防盾", Color.FromArgb(0xAA, 0x6F, 0x25) },
            { "光盾", Color.FromArgb(0x8A, 0x5B, 0x23) },

            // Soul Musician / 灵魂乐手 - Concerto, Dissonance
            { "灵魂乐手", Color.FromArgb(0x5C, 0x1F, 0x95) },
            { "协奏", Color.FromArgb(0x5C, 0x1F, 0x95) },
            { "狂音", Color.FromArgb(0x5B, 0x2B, 0x8A) },
        };

        private static Dictionary<string, Image> ProfessionImageDict = new()
        {
            { "神射手", HandledAssets.Marksman },
            { "狼弓",  HandledAssets.Marksman },
            { "鹰弓",  HandledAssets.Marksman },

            { "森语者", HandledAssets.VerdantOracle },
            { "惩戒",  HandledAssets.VerdantOracle },
            { "愈合",  HandledAssets.VerdantOracle },

            { "雷影剑士", HandledAssets.Stormblade },
            { "居合",  HandledAssets.Stormblade },
            { "月刃",  HandledAssets.Stormblade },

            { "冰魔导师", HandledAssets.FrostMage },
            { "冰矛",  HandledAssets.FrostMage },
            { "射线",  HandledAssets.FrostMage },

            { "青岚骑士", HandledAssets.WindKnight },
            { "重装",  HandledAssets.WindKnight },
            { "空枪",  HandledAssets.WindKnight },

            { "巨刃守护者", HandledAssets.HeavyGuardian },
            { "岩盾",  HandledAssets.HeavyGuardian },
            { "格挡",  HandledAssets.HeavyGuardian },

            { "神盾骑士", HandledAssets.ShieldKnight },
            { "防盾",  HandledAssets.ShieldKnight },
            { "光盾",  HandledAssets.ShieldKnight },

            { "灵魂乐手", HandledAssets.SoulMusician },
            { "协奏",  HandledAssets.SoulMusician },
            { "狂音",  HandledAssets.SoulMusician },
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
