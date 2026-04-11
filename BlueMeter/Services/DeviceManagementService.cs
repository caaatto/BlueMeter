using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SharpPcap;
using BlueMeter.Core.Analyze;
using BlueMeter.Logging;
using BlueMeter.Models;

namespace BlueMeter.Services;

public class DeviceManagementService(
    CaptureDeviceList captureDeviceList,
    IPacketAnalyzer packetAnalyzer,
    ILogger<DeviceManagementService> logger) : IDeviceManagementService
{
    private readonly object _filterSync = new();
    private ILiveDevice? _activeDevice;
    private ProcessPortsWatcher? _portsWatcher;

    public async Task<List<(string name, string description)>> GetNetworkAdaptersAsync()
    {
        // Npcap's `device.Description` is unreliable on Windows — for virtual / loopback
        // adapters it is often just "Microsoft" (or the vendor name) with no distinguishing
        // suffix, which made the settings dropdown show the same label many times over. Build
        // a friendly display string by cross-referencing the Npcap device name (which embeds
        // the adapter GUID as `\Device\NPF_{GUID}`) with `NetworkInterface.GetAllNetworkInterfaces()`
        // and using its `Name` ("Ethernet", "Wi-Fi", ...) plus `Description`.
        var interfacesById = new Dictionary<string, NetworkInterface>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                interfacesById[ni.Id] = ni;
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to enumerate NetworkInterfaces for friendly-name mapping");
        }

        var result = new List<(string name, string description)>(captureDeviceList.Count);
        foreach (var device in captureDeviceList)
        {
            result.Add((device.Name, BuildFriendlyDescription(device.Name, device.Description, interfacesById)));
        }

        return await Task.FromResult(result);
    }

    private static string BuildFriendlyDescription(
        string pcapName,
        string pcapDescription,
        IReadOnlyDictionary<string, NetworkInterface> interfacesById)
    {
        // `\Device\NPF_{11111111-2222-3333-4444-555555555555}` -> `{11111111-...}`
        var guid = ExtractAdapterGuid(pcapName);
        if (guid != null && interfacesById.TryGetValue(guid, out var ni))
        {
            // "Ethernet — Intel(R) I225-V" is more useful than a bare "Microsoft".
            return string.IsNullOrWhiteSpace(ni.Description) || string.Equals(ni.Name, ni.Description, StringComparison.OrdinalIgnoreCase)
                ? ni.Name
                : $"{ni.Name} — {ni.Description}";
        }

        return string.IsNullOrWhiteSpace(pcapDescription) ? pcapName : pcapDescription;
    }

    private static string? ExtractAdapterGuid(string pcapName)
    {
        if (string.IsNullOrEmpty(pcapName)) return null;
        var open = pcapName.IndexOf('{');
        var close = pcapName.IndexOf('}', open + 1);
        if (open < 0 || close <= open) return null;
        return pcapName.Substring(open, close - open + 1);
    }

    /// <summary>
    /// Attempts to auto-select the best network adapter by consulting the routing table (GetBestInterface)
    /// and mapping the resulting interface index to a SharpPcap device. Returns null if no match.
    /// </summary>
    public Task<NetworkAdapterInfo?> GetAutoSelectedNetworkAdapterAsync()
    {
        try
        {
            var routeIndex = GetBestInterfaceForExternalDestination();
            if (routeIndex == null) return Task.FromResult<NetworkAdapterInfo?>(null);

            var ni = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n =>
                {
                    try
                    {
                        var props = n.GetIPProperties();
                        var ipv4 = props.GetIPv4Properties();
                        return ipv4 != null && ipv4.Index == routeIndex.Value;
                    }
                    catch
                    {
                        return false;
                    }
                });

            if (ni == null) return Task.FromResult<NetworkAdapterInfo?>(null);

            // Prefer GUID-based matching: Npcap device names embed the adapter GUID
            // (`\Device\NPF_{GUID}`) and NetworkInterface.Id is that same GUID, so
            // cross-referencing is exact. The old "Description contains ni.Name"
            // fallback was unreliable because Npcap's description is often just
            // "Microsoft" with no adapter-specific suffix.
            ILiveDevice? matched = captureDeviceList
                .FirstOrDefault(d => string.Equals(ExtractAdapterGuid(d.Name), ni.Id, StringComparison.OrdinalIgnoreCase));

            if (matched != null)
            {
                // Reuse the same friendly-name mapping as GetNetworkAdaptersAsync so the
                // returned record round-trips through record equality on SettingsView's
                // SelectedItem binding.
                var interfacesById = new Dictionary<string, NetworkInterface>(StringComparer.OrdinalIgnoreCase) { [ni.Id] = ni };
                return Task.FromResult<NetworkAdapterInfo?>(
                    new NetworkAdapterInfo(matched.Name, BuildFriendlyDescription(matched.Name, matched.Description, interfacesById)));
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Auto select network adapter failed");
        }

        return Task.FromResult<NetworkAdapterInfo?>(null);
    }

    public void SetActiveNetworkAdapter(NetworkAdapterInfo adapter)
    {
        packetAnalyzer.ResetCaptureState();
        packetAnalyzer.Stop();

        if (_activeDevice != null)
        {
            try
            {
                _activeDevice.OnPacketArrival -= OnPacketArrival;
                _activeDevice.StopCapture();
                _activeDevice.Close();
            }
            finally
            {
                _activeDevice = null;
            }
        }

        if (_portsWatcher != null)
        {
            _portsWatcher.PortsChanged -= PortsWatcherOnPortsChanged;
            _portsWatcher.Dispose();
            _portsWatcher = null;
        }

        _portsWatcher = new ProcessPortsWatcher(["star.exe", "BPSR_STEAM.exe", "BPSR_EPIC.exe", "BPSR.exe"]);
        _portsWatcher.PortsChanged += PortsWatcherOnPortsChanged;

        var device = captureDeviceList.FirstOrDefault(d => d.Name == adapter.Name);
        Debug.Assert(device != null, "Selected device not found by name");

        device.Open(new DeviceConfiguration
        {
            Mode = DeviceModes.Promiscuous,
            Immediate = true,
            ReadTimeout = 1000,
            BufferSize = 1024 * 1024 * 4
        });

        // Start with no traffic until ports are known (use a filter that never matches)
        TrySetDeviceFilter(BuildFilter(Array.Empty<int>(), Array.Empty<int>()));

        device.OnPacketArrival += OnPacketArrival;
        device.StartCapture();
        _activeDevice = device;

        // Start the watcher after capture is active to avoid missing early events
        _portsWatcher.Start();
        // Immediately apply current snapshot (if any)
        ApplyProcessPortsFilter(_portsWatcher.TcpPorts, _portsWatcher.UdpPorts);

        packetAnalyzer.Start();
        logger.LogInformation(LogEvents.DeviceSwitched, "Active capture device switched to: {Name}", adapter.Name);
    }

    public void StopActiveCapture()
    {
        packetAnalyzer.Stop();
        if (_activeDevice == null)
        {
            _portsWatcher?.Dispose();
            _portsWatcher = null;
            return;
        }

        try
        {
            _activeDevice.OnPacketArrival -= OnPacketArrival;
            _activeDevice.StopCapture();
            _activeDevice.Close();
        }
        finally
        {
            _activeDevice = null;
            if (_portsWatcher != null)
            {
                _portsWatcher.PortsChanged -= PortsWatcherOnPortsChanged;
                _portsWatcher.Dispose();
                _portsWatcher = null;
            }
        }
    }

    private void PortsWatcherOnPortsChanged(object? sender, PortsChangedEventArgs e)
    {
        logger.LogDebug(LogEvents.PortsChanged, "Process ports changed: TCP={TcpCount}, UDP={UdpCount}", e.TcpPorts.Count, e.UdpPorts.Count);
        ApplyProcessPortsFilter(e.TcpPorts, e.UdpPorts);
    }

    private void ApplyProcessPortsFilter(IReadOnlyCollection<int> tcpPorts, IReadOnlyCollection<int> udpPorts)
    {
        var filter = BuildFilter(tcpPorts, udpPorts);
        TrySetDeviceFilter(filter);
    }

    private string BuildFilter(IReadOnlyCollection<int> tcpPorts, IReadOnlyCollection<int> udpPorts)
    {
        // TEMPORARY: Capture ALL TCP/UDP traffic to find queue pop packets
        // TODO: Revert to port-specific filtering after finding queue system port
        logger.LogWarning("[QUEUE DEBUG] Capturing ALL TCP/UDP traffic (port filter disabled)");
        return "(ip or ip6) and (tcp or udp)";

        // OLD CODE (port-specific filtering):
        /*
        // Build BPF like: (ip or ip6) and ((tcp and (port a or port b)) or (udp and (port c or port d)))
        var parts = new List<string>();
        if (tcpPorts.Count > 0)
        {
            parts.Add($"(tcp and (port {string.Join(" or port ", tcpPorts)}))");
        }

        if (udpPorts.Count > 0)
        {
            parts.Add($"(udp and (port {string.Join(" or port ", udpPorts)}))");
        }

        if (parts.Count == 0)
        {
            // No known process ports -> match nothing to avoid capturing unrelated traffic
            // Using "port 0" is a practical way to yield no matches for TCP/UDP
            return "(ip or ip6) and (port 0)";
        }

        return $"(ip or ip6) and ({string.Join(" or ", parts)})";
        */
    }

    private void TrySetDeviceFilter(string filter)
    {
        var dev = _activeDevice;
        if (dev == null) return;

        lock (_filterSync)
        {
            try
            {
                dev.Filter = filter;
                logger.LogDebug(LogEvents.CaptureFilterUpdated, "Capture filter updated: {Filter}", filter);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to set capture filter: {Filter}", filter);
#if DEBUG
                throw;
#endif
            }
        }
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var raw = e.GetPacket();
            var ret = packetAnalyzer.TryEnlistData(raw);
            if (!ret)
            {
                logger.LogWarning("Packet enlist failed from device {Device} with Packet {p}", sender, raw.ToString());
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Packet enlist failed from device {Device}", sender);
#if DEBUG
            throw;
#endif
        }
    }

    // PInvoke to call GetBestInterface from iphlpapi.dll
    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetBestInterface(uint destAddr, out uint bestIfIndex);

    private int? GetBestInterfaceForExternalDestination()
    {
        try
        {
            var dest = IPAddress.Parse("8.8.8.8");
            // Convert IP address from host byte order to the format expected by GetBestInterface (network byte order)
            var bytes = dest.GetAddressBytes();
            var addr = BitConverter.ToUInt32(bytes, 0);

            if (GetBestInterface(addr, out var index) == 0)
            {
                return (int)index;
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "GetBestInterfaceForExternalDestination failed");
        }

        return null;
    }
}
