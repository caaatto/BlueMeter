using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlueMeter.Assets.Properties;

namespace BlueMeter.Assets
{
    public static class HandledAssets
    {
        #region AlimamaShuHeiTi
        private const string ALIMAMASHUHEITI_FONT_KEY = "阿里妈妈数黑体";
        public static FontFamily AliMaMaShuHeiTi()
            => FontLoader.LoadFontFamilyFromBytesAndCache(ALIMAMASHUHEITI_FONT_KEY, Resources.AlimamaShuHeiTi);
        public static Font AliMaMaShuHeiTi(float fontSize = 9, FontStyle fontStyle = FontStyle.Regular)
            => FontLoader.LoadFontFromBytesAndCache(ALIMAMASHUHEITI_FONT_KEY, Resources.AlimamaShuHeiTi, fontSize, fontStyle);
        #endregion

        #region ApplicationIcon_256x256
        private static Image? _applicationIcon_256x256;
        public static Image ApplicationIcon_256x256
        {
            get
            {
                _applicationIcon_256x256 ??= new Bitmap(new MemoryStream(Resources.ApplicationIcon_256x256));
                return _applicationIcon_256x256;
            }
        }
        #endregion

        #region cancel_hover
        private static Image? _cancel_hover;
        public static Image Cancel_Hover
        {
            get
            {
                _cancel_hover ??= new Bitmap(new MemoryStream(Resources.cancel_hover));
                return _cancel_hover;
            }
        }
        #endregion

        #region cancel_normal
        private static Image? _cancel_normal;
        public static Image Cancel_Normal
        {
            get
            {
                _cancel_normal ??= new Bitmap(new MemoryStream(Resources.cancel_normal));
                return _cancel_normal;
            }
        }
        #endregion

        #region data-display
        public static string Data_Display => Resources.data_display;
        #endregion

        #region diaryIcon
        public static string DiaryIcon => Resources.diaryIcon;
        #endregion

        #region exclude
        public static string Exclude => Resources.exclude;
        #endregion

        #region flushed_hover
        private static Image? _flushed_hover;
        public static Image Flushed_Hover
        {
            get
            {
                _flushed_hover ??= new Bitmap(new MemoryStream(Resources.flushed_hover));
                return _flushed_hover;
            }
        }
        #endregion

        #region flushed_normal
        private static Image? _flushed_normal;
        public static Image Flushed_Normal
        {
            get
            {
                _flushed_normal ??= new Bitmap(new MemoryStream(Resources.flushed_normal));
                return _flushed_normal;
            }
        }
        #endregion

        #region handoff_hover
        private static Image? _handoff_hover;
        public static Image Handoff_Hover
        {
            get
            {
                _handoff_hover ??= new Bitmap(new MemoryStream(Resources.handoff_hover));
                return _handoff_hover;
            }
        }
        #endregion

        #region handoff_normal
        private static Image? _handoff_normal;
        public static Image Handoff_Normal
        {
            get
            {
                _handoff_normal ??= new Bitmap(new MemoryStream(Resources.handoff_normal));
                return _handoff_normal;
            }
        }
        #endregion

        #region IconFont
        private const string ICON_FONT_KEY = "IconFont";
        public static FontFamily IconFont()
            => FontLoader.LoadFontFamilyFromBytesAndCache(ICON_FONT_KEY, Resources.IconFont);
        public static Font IconFont(float fontSize = 9, FontStyle fontStyle = FontStyle.Regular)
            => FontLoader.LoadFontFromBytesAndCache(ICON_FONT_KEY, Resources.IconFont, fontSize, fontStyle);
        #endregion

        #region HarmonyOS_Sans
        private const string HARMONY_OS_SANS_FONT_KEY = "HarmonyOS Sans SC";
        public static FontFamily HarmonyOS_Sans()
            => FontLoader.LoadFontFamilyFromBytesAndCache(HARMONY_OS_SANS_FONT_KEY, Resources.HarmonyOS_Sans);
        public static Font HarmonyOS_Sans(float fontSize = 9, FontStyle fontStyle = FontStyle.Regular)
            => FontLoader.LoadFontFromBytesAndCache(HARMONY_OS_SANS_FONT_KEY, Resources.HarmonyOS_Sans, fontSize, fontStyle);
        #endregion

        #region HarmonyOS_Sans_SC_Bold
        private const string HARMONU_OS_SANS_SC_BOLD_FONT_KEY = "HarmonyOS Sans SC";
        public static FontFamily HarmonyOS_Sans_Bold()
            => FontLoader.LoadFontFamilyFromBytesAndCache(HARMONU_OS_SANS_SC_BOLD_FONT_KEY, Resources.HarmonyOS_Sans_SC_Bold);
        public static Font HarmonyOS_Sans_Bold(float fontSize = 9, FontStyle fontStyle = FontStyle.Bold)
            => FontLoader.LoadFontFromBytesAndCache(HARMONU_OS_SANS_SC_BOLD_FONT_KEY, Resources.HarmonyOS_Sans_SC_Bold, fontSize, fontStyle);
        #endregion

        #region historicalRecords
        public static string HistoricalRecords => Resources.historicalRecords;
        #endregion

        #region HomeIcon
        public static string HomeIcon => Resources.HomeIcon;
        #endregion

        #region hp_icon
        private static Image? _hp_icon;
        public static Image Hp_Icon
        {
            get
            {
                _hp_icon ??= new Bitmap(new MemoryStream(Resources.hp_icon));
                return _hp_icon;
            }
        }
        #endregion

        #region left_hover
        private static Image? _left_hover;
        public static Image Left_Hover
        {
            get
            {
                _left_hover ??= new Bitmap(new MemoryStream(Resources.left_hover));
                return _left_hover;
            }
        }
        #endregion

        #region left_normal
        private static Image? _left_normal;
        public static Image Left_Normal
        {
            get
            {
                _left_normal ??= new Bitmap(new MemoryStream(Resources.left_normal));
                return _left_normal;
            }
        }
        #endregion

        #region minimize_normal
        private static Image? _minimize_normal;
        public static Image Minimize_Normal
        {
            get
            {
                _minimize_normal ??= new Bitmap(new MemoryStream(Resources.minimize_normal));
                return _minimize_normal;
            }
        }
        #endregion

        #region moduleIcon
        public static string ModuleIcon => Resources.moduleIcon;
        #endregion

        #region Npc
        private static Image? _npc;
        public static Image Npc
        {
            get
            {
                _npc ??= new Bitmap(new MemoryStream(Resources.Npc));
                return _npc;
            }
        }
        #endregion

        #region NpcWhite
        private static Image? _npcWhite;
        public static Image NpcWhite
        {
            get
            {
                _npcWhite ??= new Bitmap(new MemoryStream(Resources.NpcWhite));
                return _npcWhite;
            }
        }
        #endregion

        #region ok_hover
        private static Image? _ok_hover;
        public static Image Ok_Hover
        {
            get
            {
                _ok_hover ??= new Bitmap(new MemoryStream(Resources.ok_hover));
                return _ok_hover;
            }
        }
        #endregion

        #region ok_normal
        private static Image? _ok_normal;
        public static Image Ok_Normal
        {
            get
            {
                _ok_normal ??= new Bitmap(new MemoryStream(Resources.ok_normal));
                return _ok_normal;
            }
        }
        #endregion

        #region quit
        public static string Quit => Resources.quit;
        #endregion

        #region reference
        public static string Reference => Resources.reference;
        #endregion

        #region right_hover
        private static Image? _right_hover;
        public static Image Right_Hover
        {
            get
            {
                _right_hover ??= new Bitmap(new MemoryStream(Resources.right_hover));
                return _right_hover;
            }
        }
        #endregion

        #region right_normal
        private static Image? _right_normal;
        public static Image Right_Normal
        {
            get
            {
                _right_normal ??= new Bitmap(new MemoryStream(Resources.right_normal));
                return _right_normal;
            }
        }
        #endregion

        #region SAOWelcomeTT
        private const string SAO_WELCOME_TT_FONT_KEY = "SAO Welcome TT";
        public static FontFamily SAOWelcomeTT()
            => FontLoader.LoadFontFamilyFromBytesAndCache(SAO_WELCOME_TT_FONT_KEY, Resources.SAOWelcomeTT);
        public static Font SAOWelcomeTT(float fontSize = 9, FontStyle fontStyle = FontStyle.Regular)
            => FontLoader.LoadFontFromBytesAndCache(SAO_WELCOME_TT_FONT_KEY, Resources.SAOWelcomeTT, fontSize, fontStyle);
        #endregion

        #region set_up
        public static string Set_Up => Resources.set_up;
        #endregion

        #region setting_hover
        private static Image? _setting_hover;
        public static Image Setting_Hover
        {
            get
            {
                _setting_hover ??= new Bitmap(new MemoryStream(Resources.setting_hover));
                return _setting_hover;
            }
        }
        #endregion

        #region setting_normal
        private static Image? _setting_normal;
        public static Image Setting_Normal
        {
            get
            {
                _setting_normal ??= new Bitmap(new MemoryStream(Resources.setting_normal));
                return _setting_normal;
            }
        }
        #endregion

        #region Stakes
        public static string Stakes => Resources.Stakes;
        #endregion

        #region top_hover
        private static Image? _top_hover;
        public static Image Top_Hover
        {
            get
            {
                _top_hover ??= new Bitmap(new MemoryStream(Resources.top_hover));
                return _top_hover;
            }
        }
        #endregion

        #region top_normal
        private static Image? _top_normal;
        public static Image Top_Normal
        {
            get
            {
                _top_normal ??= new Bitmap(new MemoryStream(Resources.top_normal));
                return _top_normal;
            }
        }
        #endregion

        #region userUid
        public static string UserUid => Resources.userUid;
        #endregion

        #region Damage
        private static Image? _damage;
        public static Image Damage
        {
            get
            {
                _damage ??= new Bitmap(new MemoryStream(Resources.Damage));
                return _damage;
            }
        }
        #endregion

        #region Damage_White
        private static Image? _damageWhite;
        public static Image Damage_White
        {
            get
            {
                _damageWhite ??= new Bitmap(new MemoryStream(Resources.Damage_White));
                return _damageWhite;
            }
        }
        #endregion

        #region FrostMage
        private static Image? _frostMage;
        public static Image FrostMage
        {
            get
            {
                _frostMage ??= new Bitmap(new MemoryStream(Resources.FrostMage));
                return _frostMage;
            }
        }
        #endregion

        #region FrostMage_Opacity10
        private static Image? _frostMageOpacity10;
        public static Image FrostMage_Opacity10
        {
            get
            {
                _frostMageOpacity10 ??= new Bitmap(new MemoryStream(Resources.FrostMage_Opacity10));
                return _frostMageOpacity10;
            }
        }
        #endregion

        #region HeavyGuardian
        private static Image? _heavyGuardian;
        public static Image HeavyGuardian
        {
            get
            {
                _heavyGuardian ??= new Bitmap(new MemoryStream(Resources.HeavyGuardian));
                return _heavyGuardian;
            }
        }
        #endregion

        #region HeavyGuardian_Opacity10
        private static Image? _heavyGuardianOpacity10;
        public static Image HeavyGuardian_Opacity10
        {
            get
            {
                _heavyGuardianOpacity10 ??= new Bitmap(new MemoryStream(Resources.HeavyGuardian_Opacity10));
                return _heavyGuardianOpacity10;
            }
        }
        #endregion

        #region DamageTaken
        private static Image? _damageTaken;
        public static Image DamageTaken
        {
            get
            {
                _damageTaken ??= new Bitmap(new MemoryStream(Resources.DamageTaken));
                return _damageTaken;
            }
        }
        #endregion

        #region DamageTaken_White
        private static Image? _damageTakenWhite;
        public static Image DamageTaken_White
        {
            get
            {
                _damageTakenWhite ??= new Bitmap(new MemoryStream(Resources.DamageTaken_White));
                return _damageTakenWhite;
            }
        }
        #endregion

        #region VerdantOracle
        private static Image? _verdantOracle;
        public static Image VerdantOracle
        {
            get
            {
                _verdantOracle ??= new Bitmap(new MemoryStream(Resources.VerdantOracle));
                return _verdantOracle;
            }
        }
        #endregion

        #region VerdantOracle_Opacity10
        private static Image? _verdantOracleOpacity10;
        public static Image VerdantOracle_Opacity10
        {
            get
            {
                _verdantOracleOpacity10 ??= new Bitmap(new MemoryStream(Resources.VerdantOracle_Opacity10));
                return _verdantOracleOpacity10;
            }
        }
        #endregion

        #region Healing
        private static Image? _healing;
        public static Image Healing
        {
            get
            {
                _healing ??= new Bitmap(new MemoryStream(Resources.Healing));
                return _healing;
            }
        }
        #endregion

        #region Healing_White
        private static Image? _healingWhite;
        public static Image Healing_White
        {
            get
            {
                _healingWhite ??= new Bitmap(new MemoryStream(Resources.Healing_White));
                return _healingWhite;
            }
        }
        #endregion

        #region SoulMusician
        private static Image? _soulMusician;
        public static Image SoulMusician
        {
            get
            {
                _soulMusician ??= new Bitmap(new MemoryStream(Resources.SoulMusician));
                return _soulMusician;
            }
        }
        #endregion

        #region SoulMusician_Opacity10
        private static Image? _soulMusicianOpacity10;
        public static Image SoulMusician_Opacity10
        {
            get
            {
                _soulMusicianOpacity10 ??= new Bitmap(new MemoryStream(Resources.SoulMusician_Opacity10));
                return _soulMusicianOpacity10;
            }
        }
        #endregion

        #region Crown
        private static Image? _crown;
        public static Image Crown
        {
            get
            {
                _crown ??= new Bitmap(new MemoryStream(Resources.Crown));
                return _crown;
            }
        }
        #endregion

        #region Crown_White
        private static Image? _crownWhite;
        public static Image Crown_White
        {
            get
            {
                _crownWhite ??= new Bitmap(new MemoryStream(Resources.Crown_White));
                return _crownWhite;
            }
        }
        #endregion

        #region Marksman
        private static Image? _marksman;
        public static Image Marksman
        {
            get
            {
                _marksman ??= new Bitmap(new MemoryStream(Resources.Marksman));
                return _marksman;
            }
        }
        #endregion

        #region Marksman_Opacity10
        private static Image? _marksmanOpacity10;
        public static Image Marksman_Opacity10
        {
            get
            {
                _marksmanOpacity10 ??= new Bitmap(new MemoryStream(Resources.Marksman_Opacity10));
                return _marksmanOpacity10;
            }
        }
        #endregion

        #region ShieldKnight
        private static Image? _shieldKnight;
        public static Image ShieldKnight
        {
            get
            {
                _shieldKnight ??= new Bitmap(new MemoryStream(Resources.ShieldKnight));
                return _shieldKnight;
            }
        }
        #endregion

        #region ShieldKnight_Opacity10
        private static Image? _shieldKnightOpacity10;
        public static Image ShieldKnight_Opacity10
        {
            get
            {
                _shieldKnightOpacity10 ??= new Bitmap(new MemoryStream(Resources.ShieldKnight_Opacity10));
                return _shieldKnightOpacity10;
            }
        }
        #endregion

        #region Stormblade
        private static Image? _stormblade;
        public static Image Stormblade
        {
            get
            {
                _stormblade ??= new Bitmap(new MemoryStream(Resources.Stormblade));
                return _stormblade;
            }
        }
        #endregion

        #region Stormblade_Opacity10
        private static Image? _stormbladeOpacity10;
        public static Image Stormblade_Opacity10
        {
            get
            {
                _stormbladeOpacity10 ??= new Bitmap(new MemoryStream(Resources.Stormblade_Opacity10));
                return _stormbladeOpacity10;
            }
        }
        #endregion

        #region WindKnight
        private static Image? _windKnight;
        public static Image WindKnight
        {
            get
            {
                _windKnight ??= new Bitmap(new MemoryStream(Resources.WindKnight));
                return _windKnight;
            }
        }
        #endregion

        #region WindKnight_Opacity10
        private static Image? _windKnightOpacity10;
        public static Image WindKnight_Opacity10
        {
            get
            {
                _windKnightOpacity10 ??= new Bitmap(new MemoryStream(Resources.WindKnight_Opacity10));
                return _windKnightOpacity10;
            }
        }
        #endregion  

    }
}
