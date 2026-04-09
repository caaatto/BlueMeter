using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace BlueMeter.Services;

/// <summary>
/// Default Avalonia implementation of <see cref="IMousePenetrationService"/>.
/// </summary>
/// <remarks>
/// Implementation details:
/// - Uses Win32 extended window style <c>WS_EX_TRANSPARENT</c> to make the window click-through at the OS level.
/// - Mirrors the native behavior at the Avalonia level by toggling <see cref="InputElement.IsHitTestVisible"/>.
/// - Safely handles calls before the window handle exists by deferring until <see cref="Window.Opened"/>.
/// - Intended to be invoked on the UI thread that owns the target <see cref="Window"/>.
/// Notes:
/// - Only mouse hit-testing is affected; keyboard focus and other input modalities are not altered.
/// - This service assumes a Windows desktop environment.
/// </remarks>
public sealed class MousePenetrationService : IMousePenetrationService
{
    /// <summary>
    /// Index for extended window styles used with Get/SetWindowLongPtr.
    /// </summary>
    private const int GWL_EXSTYLE = -20;

    /// <summary>
    /// Extended window style flag that causes the window to be transparent to mouse events.
    /// </summary>
    private const int WS_EX_TRANSPARENT = 0x00000020;

    /// <inheritdoc />
    /// <remarks>
    /// If the window has not created its native handle yet, the native style
    /// toggle will be applied as soon as <see cref="Window.Opened"/> fires.
    /// </remarks>
    public void SetMousePenetrate(Window window, bool enable)
    {
        // Apply immediate Avalonia-level behavior regardless of native handle state
        window.IsHitTestVisible = !enable;

        void ApplyNative()
        {
            var hWnd = GetHandle(window);
            if (hWnd == nint.Zero) return;
            var exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt32();
            if (enable)
                exStyle |= WS_EX_TRANSPARENT;
            else
                exStyle &= ~WS_EX_TRANSPARENT;
            SetWindowLongPtr(hWnd, GWL_EXSTYLE, new nint(exStyle));
        }

        // If handle not ready yet, delay until Opened
        if (GetHandle(window) == nint.Zero)
        {
            void Handler(object? s, EventArgs e)
            {
                window.Opened -= Handler;
                ApplyNative();
            }

            window.Opened += Handler;
        }
        else
        {
            ApplyNative();
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    private static nint GetWindowLongPtr(nint hWnd, int nIndex)
    {
        return nint.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new nint(GetWindowLong32(hWnd, nIndex));
    }

    private static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong)
    {
        return nint.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new nint(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    /// <summary>
    /// Retrieves the HWND for the specified Avalonia <see cref="Window"/>.
    /// </summary>
    private static nint GetHandle(Window window)
    {
        return window.TryGetPlatformHandle()?.Handle ?? nint.Zero;
    }
}
