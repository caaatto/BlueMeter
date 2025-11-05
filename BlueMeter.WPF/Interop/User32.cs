// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;

namespace BlueMeter.WPF.Interop;

internal static class User32
{
    public enum WM
    {
        WININICHANGE = 0x001A
    }

    /// <summary>
    /// Determines whether the specified window handle identifies an existing window.
    /// </summary>
    /// <param name="hWnd">A handle to the window to be tested.</param>
    /// <returns>If the window handle identifies an existing window, the return value is nonzero.</returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow([In] IntPtr hWnd);
}