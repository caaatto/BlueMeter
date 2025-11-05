using System.Runtime.InteropServices;

namespace StarResonanceDpsAnalysis.WPF.Interop;

internal static class Dwmapi
{
    [Flags]
    public enum DWMWINDOWATTRIBUTE
    {
        /// <summary>
        /// Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
        /// </summary>
        DMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19,

        /// <summary>
        /// Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
        /// </summary>
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20
    }

    /// <summary>
    /// Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
    /// </summary>
    /// <param name="hWnd">The handle to the window for which the attribute value is to be set.</param>
    /// <param name="dwAttribute">A flag describing which value to set, specified as a value of the DWMWINDOWATTRIBUTE enumeration.</param>
    /// <param name="pvAttribute">A pointer to an object containing the attribute value to set.</param>
    /// <param name="cbAttribute">The size, in bytes, of the attribute value being set via the <c>pvAttribute</c> parameter.</param>
    /// <returns>If the function succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</returns>
    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(
        [In] IntPtr hWnd,
        [In] DWMWINDOWATTRIBUTE dwAttribute,
        [In] ref int pvAttribute,
        [In] int cbAttribute
    );
}