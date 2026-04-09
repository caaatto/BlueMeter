using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace BlueMeter.Services;

/// <summary>
/// Detects whether Npcap (or legacy WinPcap) is installed on the host. This is the
/// runtime that <c>SharpPcap</c> drives — without it, BlueMeter cannot capture
/// any traffic, so the bootstrap aborts and points the user at the installer.
/// </summary>
public static class NpcapChecker
{
    public static bool IsNpcapInstalled()
    {
        // Check for Npcap in registry
        if (RegKeyExists(@"SOFTWARE\Npcap"))
            return true;

        // Check for WinPcap in registry (legacy)
        if (RegKeyExists(@"SOFTWARE\WinPcap"))
            return true;

        // Check for Npcap DLL in system32
        var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        if (File.Exists(Path.Combine(system32, "Npcap", "wpcap.dll")))
            return true;

        // Check for WinPcap DLL in system32 (legacy)
        if (File.Exists(Path.Combine(system32, "wpcap.dll")))
            return true;

        return false;
    }

    private static bool RegKeyExists(string keyPath)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Open the Npcap download page in the user's default browser. The actual
    /// "Npcap is required" dialog is shown by the message-dialog service further
    /// up the bootstrap chain — this helper just handles the URL launch.
    /// </summary>
    public static void OpenNpcapDownloadPage()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://npcap.com/#download",
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser doesn't open
        }
    }
}
