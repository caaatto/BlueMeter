using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;

namespace BlueMeter.WPF.Helpers;

/// <summary>
/// Manages Windows auto-start functionality via Registry
/// </summary>
public static class AutoStartManager
{
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "BlueMeter";

    /// <summary>
    /// Gets the executable path with optional --autostart argument
    /// </summary>
    private static string ExecutablePath => $"\"{Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe")}\" --autostart";

    /// <summary>
    /// Checks if auto-start is currently enabled
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

    /// <summary>
    /// Enables auto-start for the application
    /// </summary>
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

    /// <summary>
    /// Disables auto-start for the application
    /// </summary>
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

    /// <summary>
    /// Sets auto-start based on the enabled parameter
    /// </summary>
    public static bool SetAutoStart(bool enabled)
    {
        return enabled ? EnableAutoStart() : DisableAutoStart();
    }
}
