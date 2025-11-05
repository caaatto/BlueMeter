using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Wpf.Ui.Win32;

namespace BlueMeter.WPF.Interop;

/// <summary>
/// A set of dangerous methods to modify the appearance.
/// </summary>
public static class UnsafeNativeMethods
{
    /// <summary>
    /// Checks if provided pointer represents existing window.
    /// </summary>
    public static bool IsValidWindow(IntPtr hWnd)
    {
        return User32.IsWindow(hWnd);
    }


    /// <summary>
    /// Tries to get the pointer to the window handle.
    /// </summary>
    /// <returns><see langword="true"/> if the handle is not <see cref="IntPtr.Zero"/>.</returns>
    private static bool GetHandle(Window? window, out IntPtr windowHandle)
    {
        if (window is null)
        {
            windowHandle = IntPtr.Zero;

            return false;
        }

        windowHandle = new WindowInteropHelper(window).Handle;

        return windowHandle != IntPtr.Zero;
    }

    /// <summary>
    /// Tries to apply ImmersiveDarkMode effect for the <see cref="Window"/>.
    /// </summary>
    /// <param name="window">The window to which the effect is to be applied.</param>
    /// <returns><see langword="true"/> if invocation of native Windows function succeeds.</returns>
    public static bool ApplyWindowDarkMode(Window? window)
    {
        return GetHandle(window, out var windowHandle) && ApplyWindowDarkMode(windowHandle);
    }

    /// <summary>
    /// Tries to apply ImmersiveDarkMode effect for the window handle.
    /// </summary>
    /// <param name="handle">Window handle.</param>
    /// <returns><see langword="true"/> if invocation of native Windows function succeeds.</returns>
    public static bool ApplyWindowDarkMode(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return false;
        }

        if (!User32.IsWindow(handle))
        {
            return false;
        }

        var pvAttribute = 0x1; // Enable
        var dwAttribute = Dwmapi.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;

        if (!Utilities.IsOSWindows11Insider1OrNewer)
        {
            dwAttribute = Dwmapi.DWMWINDOWATTRIBUTE.DMWA_USE_IMMERSIVE_DARK_MODE_OLD;
        }

        // TODO: Validate HRESULT
        _ = Dwmapi.DwmSetWindowAttribute(handle, dwAttribute, ref pvAttribute, Marshal.SizeOf(typeof(int)));

        return true;
    }

    /// <summary>
    /// Tries to remove ImmersiveDarkMode effect from the <see cref="Window"/>.
    /// </summary>
    /// <param name="window">The window to which the effect is to be applied.</param>
    /// <returns><see langword="true"/> if invocation of native Windows function succeeds.</returns>
    public static bool RemoveWindowDarkMode(Window? window)
    {
        return GetHandle(window, out var windowHandle) && RemoveWindowDarkMode(windowHandle);
    }

    /// <summary>
    /// Tries to remove ImmersiveDarkMode effect from the window handle.
    /// </summary>
    /// <param name="handle">Window handle.</param>
    /// <returns><see langword="true"/> if invocation of native Windows function succeeds.</returns>
    public static bool RemoveWindowDarkMode(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return false;
        }

        if (!User32.IsWindow(handle))
        {
            return false;
        }

        var pvAttribute = 0x0; // Disable
        var dwAttribute = Dwmapi.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;

        if (!Utilities.IsOSWindows11Insider1OrNewer)
        {
            dwAttribute = Dwmapi.DWMWINDOWATTRIBUTE.DMWA_USE_IMMERSIVE_DARK_MODE_OLD;
        }

        // TODO: Validate HRESULT
        _ = Dwmapi.DwmSetWindowAttribute(handle, dwAttribute, ref pvAttribute, Marshal.SizeOf(typeof(int)));

        return true;
    }
}