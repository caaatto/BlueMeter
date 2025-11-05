using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.WinForm.Effects
{
    public static class FontLoader
    {
        private static readonly PrivateFontCollection _pfc = new();
        private static readonly Dictionary<string, FontFamily> _fontFamilyCache = [];
        private static readonly Dictionary<(FontFamily, float, FontStyle), Font> _fontCache = [];
        private static string[] _prevFontFamilyNames = [];

        public static Font LoadFontFromBytesAndCache(string fontName, byte[] bytes, float fontSize, FontStyle fontStyle = FontStyle.Regular)
        {
            try
            {
                // var ffFlag = _fontFamilyCache.TryGetValue(fontName, out var fontFamily);
                if (!_fontFamilyCache.TryGetValue(fontName, out var fontFamily))
                {
                    fontFamily = LoadFontFamilyFromBytes(bytes);
                    _fontFamilyCache[fontName] = fontFamily;
                }

                // var fFlag = _fontCache.TryGetValue((fontFamily!, fontSize), out var font);
                if (!_fontCache.TryGetValue((fontFamily!, fontSize, fontStyle), out var font))
                {
                    font = new Font(fontFamily, fontSize, fontStyle);
                    _fontCache[(fontFamily, fontSize, fontStyle)] = font;
                }

                // Console.WriteLine($"通过 {fontName} 取得{(ffFlag ? "缓存中的" : string.Empty)}字体族: {fontFamily!.Name}, 并取得{(fFlag ? "缓存中的" : string.Empty)}字体: Size({fontSize})");

                return font;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"字体从内存转换时出错: {fontName}({fontSize}) {ex.Message}\r\n{ex.StackTrace}");

                return SystemFonts.DefaultFont;
            }
        }

        private static FontFamily LoadFontFamilyFromBytes(byte[] bytes)
        {
            var fontPtr = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, fontPtr, bytes.Length);

            try
            {
                _pfc.AddMemoryFont(fontPtr, bytes.Length);

                for (var i = 0; i < _prevFontFamilyNames.Length; ++i)
                {
                    if (_prevFontFamilyNames[i] != _pfc.Families[i].Name)
                    {
                        return _pfc.Families[i];
                    }
                }

                return _pfc.Families.Length > _prevFontFamilyNames.Length ? _pfc.Families[^1] : FontFamily.GenericSansSerif;
            }
            finally
            {
                _prevFontFamilyNames = [.. _pfc.Families.Select(e => e.Name)];
                Marshal.FreeCoTaskMem(fontPtr);
            }
        }
    }
}
