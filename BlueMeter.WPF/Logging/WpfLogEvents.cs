using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.Logging;

public static class WpfLogEvents
{
    // App lifecycle
    public static readonly EventId AppStarting = new(2000, nameof(AppStarting));
    public static readonly EventId AppExiting = new(2001, nameof(AppExiting));
    public static readonly EventId StartupInit = new(2010, nameof(StartupInit));
    public static readonly EventId StartupAdapter = new(2011, nameof(StartupAdapter));
    public static readonly EventId Shutdown = new(2012, nameof(Shutdown));

    // Device/capture
    public static readonly EventId DeviceSwitched = new(2100, nameof(DeviceSwitched));
    public static readonly EventId CaptureFilterUpdated = new(2101, nameof(CaptureFilterUpdated));
    public static readonly EventId PortsChanged = new(2102, nameof(PortsChanged));

    // Tray and windows
    public static readonly EventId TrayInit = new(2200, nameof(TrayInit));
    public static readonly EventId TrayMinimize = new(2201, nameof(TrayMinimize));
    public static readonly EventId TrayRestore = new(2202, nameof(TrayRestore));
    public static readonly EventId TrayExit = new(2203, nameof(TrayExit));
    public static readonly EventId WindowCreated = new(2210, nameof(WindowCreated));
    public static readonly EventId WindowClosed = new(2211, nameof(WindowClosed));

    // UI behavior
    public static readonly EventId MouseThrough = new(2300, nameof(MouseThrough));
    public static readonly EventId TopmostToggle = new(2301, nameof(TopmostToggle));

    // ViewModels
    public static readonly EventId VmLoaded = new(2400, nameof(VmLoaded));
    public static readonly EventId VmRefresh = new(2401, nameof(VmRefresh));
    public static readonly EventId VmUpdateData = new(2402, nameof(VmUpdateData));
}
