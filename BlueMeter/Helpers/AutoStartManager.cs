using System.Reflection;
using Microsoft.Win32;

namespace BlueMeter.Helpers;

/// <summary>
/// Manages Windows auto-start functionality via the HKCU Run registry key.
/// </summary>
public static class AutoStartManager
{
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "BlueMeter";

    /// <summary>
    /// The exe path with the <c>--autostart</c> argument that gets stored in the registry.
    /// </summary>
    private static string ExecutablePath =>
        $"\"{Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe")}\" --autostart";

    /// <summary>
    /// Returns whether the Run key contains a BlueMeter entry pointing at our exe.
    /// </summary>
    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            var value = key?.GetValue(AppName) as string;
            return value != null && value.Contains("--autostart");
        }
        catch
        {
            return false;
        }
    }

    public static bool EnableAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key != null)
            {
                key.SetValue(AppName, ExecutablePath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool DisableAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key != null)
            {
                key.DeleteValue(AppName, false);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool SetAutoStart(bool enabled)
    {
        return enabled ? EnableAutoStart() : DisableAutoStart();
    }
}
