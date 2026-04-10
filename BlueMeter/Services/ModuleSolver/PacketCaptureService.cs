using System.IO;
using BlueMeter.Services.ModuleSolver.Models;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace BlueMeter.Services.ModuleSolver;

/// <summary>
/// Service for capturing network packets and extracting module data
/// </summary>
public class PacketCaptureService : IDisposable
{
    private readonly ILogger<PacketCaptureService> _logger;
    private ILiveDevice? _captureDevice;
    private CancellationTokenSource? _captureCts;
    private readonly Dictionary<uint, byte[]> _tcpCache = new();
    private readonly object _cacheLock = new();

    public event EventHandler<List<ModuleInfo>>? ModulesCapture;
    public bool IsCapturing { get; private set; }

    public PacketCaptureService(ILogger<PacketCaptureService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get list of available network devices
    /// </summary>
    public List<NetworkDevice> GetNetworkDevices()
    {
        var devices = new List<NetworkDevice>();
        var deviceList = LibPcapLiveDeviceList.Instance;

        for (int i = 0; i < deviceList.Count; i++)
        {
            var device = deviceList[i];
            devices.Add(new NetworkDevice
            {
                Index = i,
                Name = device.Name,
                Description = device.Description ?? device.Name,
                Addresses = device.Addresses.Select(a => a.Addr?.ToString() ?? "").Where(a => !string.IsNullOrEmpty(a)).ToList()
            });
        }

        return devices;
    }

    /// <summary>
    /// Start capturing packets on the specified device
    /// </summary>
    public async Task StartCaptureAsync(int deviceIndex, CancellationToken cancellationToken = default)
    {
        if (IsCapturing)
        {
            _logger.LogWarning("Packet capture is already running");
            return;
        }

        var devices = LibPcapLiveDeviceList.Instance;
        if (deviceIndex < 0 || deviceIndex >= devices.Count)
        {
            throw new ArgumentException($"Invalid device index: {deviceIndex}");
        }

        _captureDevice = devices[deviceIndex];
        _captureCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _logger.LogInformation("Starting packet capture on device: {Device}", _captureDevice.Description);

        // Open device for capturing
        _captureDevice.Open(DeviceModes.Promiscuous, 1000);

        // Set filter for TCP packets (game traffic typically uses TCP)
        _captureDevice.Filter = "tcp";

        // Register packet handler
        _captureDevice.OnPacketArrival += OnPacketArrival;

        // Start capture
        _captureDevice.StartCapture();
        IsCapturing = true;

        _logger.LogInformation("Packet capture started. Waiting for module data...");

        // Keep the capture running until cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, _captureCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Packet capture cancelled");
        }
    }

    /// <summary>
    /// Stop packet capture
    /// </summary>
    public void StopCapture()
    {
        if (!IsCapturing)
            return;

        _logger.LogInformation("Stopping packet capture...");

        _captureCts?.Cancel();
        _captureDevice?.StopCapture();
        _captureDevice?.Close();

        IsCapturing = false;
        _logger.LogInformation("Packet capture stopped");
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var rawPacket = e.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            var tcpPacket = packet.Extract<TcpPacket>();
            if (tcpPacket?.PayloadData == null || tcpPacket.PayloadData.Length == 0)
                return;

            // Process the TCP payload
            ProcessTcpPayload(tcpPacket.PayloadData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing packet");
        }
    }

    private void ProcessTcpPayload(byte[] payload)
    {
        try
        {
            // Try to parse as game protocol
            using var ms = new MemoryStream(payload);
            using var reader = new BinaryReader(ms);

            // Check if this looks like game data (simple heuristic)
            if (payload.Length < 8)
                return;

            // Try to parse directly first
            TryParseModuleData(payload);

            // TODO: If direct parsing fails, try decompressing with Zstandard
            // This will be implemented once we have test data to work with
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Failed to process TCP payload (this is normal for non-game packets)");
        }
    }

    private void TryParseModuleData(byte[] data)
    {
        try
        {
            // Try to parse as CharSerialize protobuf message
            var charSerialize = BlueprotobufPb2.CharSerialize.Parser.ParseFrom(data);

            if (charSerialize.ItemPackage == null || charSerialize.ItemPackage.Packages.Count == 0)
                return;

            _logger.LogInformation("Module data captured! Parsing modules...");

            // Extract modules from the data
            var modules = ExtractModules(charSerialize);

            if (modules.Count > 0)
            {
                _logger.LogInformation("Successfully parsed {Count} modules", modules.Count);
                ModulesCapture?.Invoke(this, modules);

                // Auto-stop after successful capture
                StopCapture();
            }
        }
        catch (InvalidProtocolBufferException)
        {
            // Not a valid protobuf message, ignore
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Failed to parse as CharSerialize (expected for non-module packets)");
        }
    }

    private List<ModuleInfo> ExtractModules(BlueprotobufPb2.CharSerialize charSerialize)
    {
        var modules = new List<ModuleInfo>();

        foreach (var packageEntry in charSerialize.ItemPackage.Packages)
        {
            var package = packageEntry.Value;
            foreach (var itemEntry in package.Items)
            {
                var item = itemEntry.Value;

                // Check if this item has module attributes
                if (item.ModNewAttr == null || item.ModNewAttr.ModParts.Count == 0)
                    continue;

                var moduleParts = new List<ModulePart>();
                var modParts = item.ModNewAttr.ModParts.ToList();

                // Get module info from the Mod table
                if (charSerialize.Mod != null && charSerialize.Mod.ModInfos.ContainsKey(itemEntry.Key))
                {
                    var modInfo = charSerialize.Mod.ModInfos[itemEntry.Key];
                    var initLinkNums = modInfo.InitLinkNums.ToList();

                    for (int i = 0; i < modParts.Count && i < initLinkNums.Count; i++)
                    {
                        int attrId = modParts[i];
                        int attrValue = initLinkNums[i];
                        string attrName = ModuleConstants.GetAttributeName(attrId);

                        moduleParts.Add(new ModulePart(attrId, attrName, attrValue));
                    }
                }

                var moduleInfo = new ModuleInfo
                {
                    Name = ModuleConstants.GetModuleName(item.ConfigId),
                    ConfigId = item.ConfigId,
                    Uuid = item.Uuid,
                    Quality = item.Quality,
                    Parts = moduleParts,
                    Category = ModuleConstants.GetModuleCategory(item.ConfigId)
                };

                modules.Add(moduleInfo);

                _logger.LogDebug("Module: {Name} (Q{Quality}) - {Parts}",
                    moduleInfo.Name, moduleInfo.Quality, string.Join(", ", moduleParts));
            }
        }

        return modules;
    }

    public void Dispose()
    {
        StopCapture();
        _captureCts?.Dispose();
        _captureDevice?.Dispose();
    }
}

/// <summary>
/// Represents a network device available for packet capture
/// </summary>
public class NetworkDevice
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Addresses { get; set; } = new();
}
