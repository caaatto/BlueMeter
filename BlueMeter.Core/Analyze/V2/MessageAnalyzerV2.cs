using Microsoft.Extensions.Logging;
using BlueMeter.Core.Analyze.V2.Processors;
using BlueMeter.Core.Tools;
using BlueMeter.WPF.Data;
using ZstdNet;

#pragma warning disable 0472

namespace BlueMeter.Core.Analyze;

/// <summary>
/// Orchestrates message analysis by dispatching packets to registered processors.
/// </summary>
public sealed class MessageAnalyzerV2
{
    private readonly ILogger<MessageAnalyzerV2>? _logger;
    private readonly Dictionary<MessageType, Action<ByteReader, bool>> _messageHandlerMap;
    private readonly MessageHandlerRegistry _registry;
    private readonly IDataStorage _storage;

    public MessageAnalyzerV2(IDataStorage storage, ILogger<MessageAnalyzerV2>? logger = null)
    {
        _logger = logger;
        _storage = storage;
        _registry = new MessageHandlerRegistry(storage, logger);
        _messageHandlerMap = new Dictionary<MessageType, Action<ByteReader, bool>>
        {
            { MessageType.Call, ProcessCallMsg },
            { MessageType.Notify, ProcessNotifyMsg },
            { MessageType.FrameDown, ProcessFrameDown },
            { MessageType.Return, ProcessReturnMsg }
        };
    }

    /// <summary>
    /// Main entry point for processing a batch of TCP packets.
    /// </summary>
    public void Process(byte[] packets)
    {
        if (packets is not { Length: > 0 }) return;

        var packetsReader = new ByteReader(packets);
        while (packetsReader.Remaining > 0)
        {
            if (!packetsReader.TryPeekUInt32BE(out var packetSize)) break;
            if (packetSize < 6) break;
            if (packetSize > packetsReader.Remaining) break;

            var packetReader = new ByteReader(packetsReader.ReadBytes((int)packetSize));
            if (packetReader.ReadUInt32BE() != packetSize) continue;

            var packetType = packetReader.ReadUInt16BE();
            var isZstdCompressed = (packetType & 0x8000) != 0;
            var msgTypeId = (MessageType)(packetType & 0x7FFF);

            // DEBUG: Log ALL message types to find queue pop
            if (Data.DataStorageV2.EnableQueueDetectionLogging)
            {
                try
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "all-messages.log");
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    var logMsg = $"[{timestamp}] MessageType: {msgTypeId} (0x{(int)msgTypeId:X}), Compressed: {isZstdCompressed}\n";
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                    File.AppendAllText(logPath, logMsg);
                }
                catch { }
            }

            if (!_messageHandlerMap.TryGetValue(msgTypeId, out var handler))
            {
                // Log unknown message types
                if (Data.DataStorageV2.EnableQueueDetectionLogging)
                {
                    try
                    {
                        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "unknown-messages.log");
                        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                        var logMsg = $"[{timestamp}] UNKNOWN MessageType: {msgTypeId} (0x{(int)msgTypeId:X}), Compressed: {isZstdCompressed}\n";
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                        File.AppendAllText(logPath, logMsg);
                    }
                    catch { }
                }
                continue;
            }

#if RELEASE
            try
            {
                handler(packetReader, isZstdCompressed);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to handle message type {MessageType}", msgTypeId);
            }
#else
            handler(packetReader, isZstdCompressed);
#endif
        }
    }

    /// <summary>
    /// Zero-copy entry that parses messages directly from a span.
    /// Avoids allocating an exact-sized array for each frame.
    /// </summary>
    public void Process(ReadOnlySpan<byte> packets)
    {
        if (packets.Length == 0) return;

        var reader = new SpanReader(packets);
        while (reader.Remaining > 0)
        {
            if (!reader.TryPeekUInt32BE(out var packetSize)) break;
            if (packetSize < 6) break;
            if (packetSize > reader.Remaining) break;

            var frameStartOffset = reader.Offset;
            var sizeAgain = reader.ReadUInt32BE();
            if (sizeAgain != packetSize)
            {
                // skip malformed
                reader.Offset = frameStartOffset + (int)packetSize;
                continue;
            }

            var packetType = reader.ReadUInt16BE();
            var isZstdCompressed = (packetType & 0x8000) != 0;
            var msgTypeId = (MessageType)(packetType & 0x7FFF);

            // DEBUG: Log ALL message types to find queue pop (span-based path)
            if (Data.DataStorageV2.EnableQueueDetectionLogging)
            {
                try
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "all-messages.log");
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    var logMsg = $"[{timestamp}] [SPAN] MessageType: {msgTypeId} (0x{(int)msgTypeId:X}), Compressed: {isZstdCompressed}\n";
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                    File.AppendAllText(logPath, logMsg);
                }
                catch { }
            }

            var frameConsumed = 6; // 4 length + 2 type already consumed within this frame
            var frameRemaining = (int)packetSize - frameConsumed;
            if (frameRemaining < 0 || frameRemaining > reader.Remaining)
            {
                // Not enough data
                reader.Offset = frameStartOffset; // rewind to start of frame for next attempt
                break;
            }

            // Slice the rest of the frame for inner parsing
            var inner = reader.ReadBytesSpan(frameRemaining);

            switch (msgTypeId)
            {
                case MessageType.Call:
                    ProcessCallMsg(inner, isZstdCompressed);
                    break;
                case MessageType.Notify:
                    ProcessNotifyMsg(inner, isZstdCompressed);
                    break;
                case MessageType.FrameDown:
                    ProcessFrameDown(inner, isZstdCompressed);
                    break;
                case MessageType.Return:
                    ProcessReturnMsg(inner, isZstdCompressed);
                    break;
                default:
                    // Log unknown message types
                    if (Data.DataStorageV2.EnableQueueDetectionLogging)
                    {
                        try
                        {
                            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "unknown-messages.log");
                            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                            var logMsg = $"[{timestamp}] [SPAN] UNKNOWN MessageType: {msgTypeId} (0x{(int)msgTypeId:X}), Compressed: {isZstdCompressed}\n";
                            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                            File.AppendAllText(logPath, logMsg);
                        }
                        catch { }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Processes Notify messages by dispatching them to the appropriate registered processor.
    /// </summary>
    private void ProcessNotifyMsg(ByteReader packet, bool isZstdCompressed)
    {
        var serviceUuid = packet.ReadUInt64BE();
        _ = packet.ReadUInt32BE(); // stubId
        var methodId = packet.ReadUInt32BE();

        // DEBUG: Log ALL Notify messages (all services) to find queue pop
        if (Data.DataStorageV2.EnableQueueDetectionLogging)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "all-notify-messages.log");
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var isCombat = serviceUuid == 0x0000000063335342UL;
                var logMsg = $"[{timestamp}] ServiceUuid: 0x{serviceUuid:X16}, MethodId: 0x{methodId:X8} ({methodId}), Combat: {isCombat}\n";
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, logMsg);
            }
            catch { }
        }

        // Log non-combat services to detect queue pop messages
        if (serviceUuid != 0x0000000063335342UL)
        {
            if (Data.DataStorageV2.EnableQueueDetectionLogging)
            {
                try
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "non-combat-services.log");
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    var logMsg = $"[{timestamp}] NON-COMBAT SERVICE - ServiceUuid: 0x{serviceUuid:X16}, MethodId: 0x{methodId:X8} ({methodId})\n";
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                    File.AppendAllText(logPath, logMsg);
                }
                catch { }
            }
            return; // Not a combat-related service
        }

        var msgPayload = packet.ReadRemaining();
        if (isZstdCompressed)
        {
            msgPayload = DecompressZstdIfNeeded(msgPayload);
        }

        _logger?.LogTrace("MessageTypeId:{id}", methodId);
        if (_registry.TryGetProcessor(methodId, out var processor))
        {
            processor.Process(msgPayload);
        }
    }

    /// <summary>
    /// Span-based Notify parser to avoid frame array allocation.
    /// </summary>
    private void ProcessNotifyMsg(ReadOnlySpan<byte> packet, bool isZstdCompressed)
    {
        var r = new SpanReader(packet);
        var serviceUuid = r.ReadUInt64BE();
        _ = r.ReadUInt32BE(); // stubId
        var methodId = r.ReadUInt32BE();

        // Log non-combat services to detect queue pop messages
        if (serviceUuid != 0x0000000063335342UL)
        {
            if (Data.DataStorageV2.EnableQueueDetectionLogging)
            {
                try
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "non-combat-services.log");
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    var logMsg = $"[{timestamp}] [SPAN] NON-COMBAT SERVICE - ServiceUuid: 0x{serviceUuid:X16}, MethodId: 0x{methodId:X8} ({methodId})\n";
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                    File.AppendAllText(logPath, logMsg);
                }
                catch { }
            }
            _logger?.LogWarning("[NON-COMBAT-MSG] ServiceUuid: 0x{ServiceUuid:X16}, MethodId: 0x{MethodId:X8} ({MethodId})",
                serviceUuid, methodId, methodId);
            return;
        }

        var msgPayloadSpan = r.ReadRemainingSpan();
        byte[] msgPayload;
        if (isZstdCompressed)
        {
            msgPayload = DecompressZstdIfNeeded(msgPayloadSpan.ToArray());
        }
        else
        {
            msgPayload = msgPayloadSpan.ToArray();
        }

        _logger?.LogTrace("MessageTypeId:{id}", methodId);
        if (_registry.TryGetProcessor(methodId, out var processor))
        {
            processor.Process(msgPayload);
        }
    }

    /// <summary>
    /// Processes FrameDown messages which contain nested packets.
    /// </summary>
    private void ProcessFrameDown(ByteReader reader, bool isZstdCompressed)
    {
        _ = reader.ReadUInt32BE(); // serverSequenceId
        if (reader.Remaining == 0) return;

        var nestedPacket = reader.ReadRemaining();
        if (isZstdCompressed)
        {
            nestedPacket = DecompressZstdIfNeeded(nestedPacket);
        }

        _logger?.LogTrace("ProcessFrameDown");
        Process(nestedPacket); // Recursively process the inner packet
    }

    /// <summary>
    /// Span-based FrameDown parser to avoid frame array allocation.
    /// </summary>
    private void ProcessFrameDown(ReadOnlySpan<byte> packet, bool isZstdCompressed)
    {
        var r = new SpanReader(packet);
        _ = r.ReadUInt32BE(); // serverSequenceId
        var nestedSpan = r.ReadRemainingSpan();
        if (nestedSpan.Length == 0) return;

        if (isZstdCompressed)
        {
            var nested = DecompressZstdIfNeeded(nestedSpan.ToArray());
            Process(nested);
        }
        else
        {
            Process(nestedSpan);
        }
    }

    /// <summary>
    /// Processes Return messages (RPC responses) to detect queue pops.
    /// </summary>
    private void ProcessReturnMsg(ByteReader packet, bool isZstdCompressed)
    {
        if (packet.Remaining < 4) return;

        var stubId = packet.ReadUInt32BE();
        var payload = packet.ReadRemaining();

        if (isZstdCompressed)
        {
            payload = DecompressZstdIfNeeded(payload);
        }

        // DEBUG: Log Return message details to identify queue pop
        if (Data.DataStorageV2.EnableQueueDetectionLogging)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "return-messages.log");
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMsg = $"[{timestamp}] Return StubId: 0x{stubId:X8}, PayloadSize: {payload.Length} bytes\n";
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, logMsg);

                // Log payload hex for analysis
                if (payload.Length > 0 && payload.Length <= 256)
                {
                    var hexDump = BitConverter.ToString(payload).Replace("-", " ");
                    File.AppendAllText(logPath, $"  Payload: {hexDump}\n");
                }
            }
            catch { }
        }

        _logger?.LogInformation("[RETURN MSG] StubId: 0x{StubId:X8}, PayloadSize: {Size}", stubId, payload.Length);

        // Track Return messages for queue pop detection (burst pattern)
        if (_storage is Data.DataStorageV2 dataStorageV2)
        {
            dataStorageV2.TrackReturnMessage();
        }
    }

    /// <summary>
    /// Span-based Return message parser.
    /// </summary>
    private void ProcessReturnMsg(ReadOnlySpan<byte> packet, bool isZstdCompressed)
    {
        var r = new SpanReader(packet);
        if (r.Remaining < 4) return;

        var stubId = r.ReadUInt32BE();
        var payloadSpan = r.ReadRemainingSpan();

        byte[] payload;
        if (isZstdCompressed)
        {
            payload = DecompressZstdIfNeeded(payloadSpan.ToArray());
        }
        else
        {
            payload = payloadSpan.ToArray();
        }

        // DEBUG: Log Return message details to identify queue pop
        if (Data.DataStorageV2.EnableQueueDetectionLogging)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "return-messages.log");
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMsg = $"[{timestamp}] [SPAN] Return StubId: 0x{stubId:X8}, PayloadSize: {payload.Length} bytes\n";
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, logMsg);

                // Log payload hex for analysis
                if (payload.Length > 0 && payload.Length <= 256)
                {
                    var hexDump = BitConverter.ToString(payload).Replace("-", " ");
                    File.AppendAllText(logPath, $"  Payload: {hexDump}\n");
                }
            }
            catch { }
        }

        _logger?.LogInformation("[RETURN MSG] StubId: 0x{StubId:X8}, PayloadSize: {Size}", stubId, payload.Length);

        // Track Return messages for queue pop detection (burst pattern)
        if (_storage is Data.DataStorageV2 dataStorageV2)
        {
            dataStorageV2.TrackReturnMessage();
        }
    }

    /// <summary>
    /// Processes Call messages (client->server RPC calls, may include queue requests)
    /// </summary>
    private void ProcessCallMsg(ByteReader packet, bool isZstdCompressed)
    {
        if (packet.Remaining < 16) return; // Need at least serviceUuid + stubId + methodId

        var serviceUuid = packet.ReadUInt64BE();
        var stubId = packet.ReadUInt32BE();
        var methodId = packet.ReadUInt32BE();
        var payload = packet.ReadRemaining();

        if (isZstdCompressed)
        {
            payload = DecompressZstdIfNeeded(payload);
        }

        // DEBUG: Log ALL Call messages to find queue/matchmaking requests
        if (Data.DataStorageV2.EnableQueueDetectionLogging)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "call-messages.log");
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMsg = $"[{timestamp}] Call - ServiceUuid: 0x{serviceUuid:X16}, StubId: 0x{stubId:X8}, MethodId: 0x{methodId:X8} ({methodId}), PayloadSize: {payload.Length} bytes\n";
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, logMsg);

                // Log payload hex for analysis (if small enough)
                if (payload.Length > 0 && payload.Length <= 128)
                {
                    var hexDump = BitConverter.ToString(payload).Replace("-", " ");
                    File.AppendAllText(logPath, $"  Payload: {hexDump}\n");
                }
            }
            catch { }
        }

        _logger?.LogInformation("[CALL MSG] ServiceUuid: 0x{ServiceUuid:X16}, StubId: 0x{StubId:X8}, MethodId: 0x{MethodId:X8}",
            serviceUuid, stubId, methodId);
    }

    /// <summary>
    /// Span-based Call message parser
    /// </summary>
    private void ProcessCallMsg(ReadOnlySpan<byte> packet, bool isZstdCompressed)
    {
        var r = new SpanReader(packet);
        if (r.Remaining < 16) return;

        var serviceUuid = r.ReadUInt64BE();
        var stubId = r.ReadUInt32BE();
        var methodId = r.ReadUInt32BE();
        var payloadSpan = r.ReadRemainingSpan();

        byte[] payload;
        if (isZstdCompressed)
        {
            payload = DecompressZstdIfNeeded(payloadSpan.ToArray());
        }
        else
        {
            payload = payloadSpan.ToArray();
        }

        // DEBUG: Log ALL Call messages to find queue/matchmaking requests
        if (Data.DataStorageV2.EnableQueueDetectionLogging)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "call-messages.log");
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logMsg = $"[{timestamp}] [SPAN] Call - ServiceUuid: 0x{serviceUuid:X16}, StubId: 0x{stubId:X8}, MethodId: 0x{methodId:X8} ({methodId}), PayloadSize: {payload.Length} bytes\n";
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, logMsg);

                // Log payload hex for analysis (if small enough)
                if (payload.Length > 0 && payload.Length <= 128)
                {
                    var hexDump = BitConverter.ToString(payload).Replace("-", " ");
                    File.AppendAllText(logPath, $"  Payload: {hexDump}\n");
                }
            }
            catch { }
        }

        _logger?.LogInformation("[CALL MSG] ServiceUuid: 0x{ServiceUuid:X16}, StubId: 0x{StubId:X8}, MethodId: 0x{MethodId:X8}",
            serviceUuid, stubId, methodId);
    }

    #region Zstd Decompression

    private static readonly uint ZSTD_MAGIC = 0xFD2FB528;
    private static readonly uint SKIPPABLE_MAGIC_MIN = 0x184D2A50;
    private static readonly uint SKIPPABLE_MAGIC_MAX = 0x184D2A5F;

    private static byte[] DecompressZstdIfNeeded(byte[] buffer)
    {
        if (buffer is not { Length: >= 4 }) return [];

        var off = 0;
        while (off + 4 <= buffer.Length)
        {
            var magic = BitConverter.ToUInt32(buffer, off);
            if (magic == ZSTD_MAGIC) break;
            if (magic >= SKIPPABLE_MAGIC_MIN && magic <= SKIPPABLE_MAGIC_MAX)
            {
                if (off + 8 > buffer.Length) throw new InvalidDataException("Incomplete skippable frame header");
                var size = BitConverter.ToUInt32(buffer, off + 4);
                if (off + 8 + size > buffer.Length) throw new InvalidDataException("Incomplete skippable frame data");
                off += 8 + (int)size;
                continue;
            }

            off++;
        }

        if (off + 4 > buffer.Length) return buffer;

        using var input = new MemoryStream(buffer, off, buffer.Length - off, false);
        using var decoder = new DecompressionStream(input);
        using var output = new MemoryStream();

        const long MAX_OUT = 32L * 1024 * 1024; // 32MB limit
        decoder.CopyTo(output, 8192);
        if (output.Length > MAX_OUT)
        {
            throw new InvalidDataException("Decompressed data exceeds 32MB limit.");
        }

        return output.ToArray();
    }

    #endregion

    private ref struct SpanReader
    {
        private ReadOnlySpan<byte> _buffer;
        public int Offset;

        public SpanReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            Offset = 0;
        }

        public int Remaining => _buffer.Length - Offset;

        public bool TryPeekUInt32BE(out uint value)
        {
            if (Remaining < 4)
            {
                value = 0; return false;
            }
            var span = _buffer.Slice(Offset, 4);
            value = (uint)(span[0] << 24 | span[1] << 16 | span[2] << 8 | span[3]);
            return true;
        }

        public uint ReadUInt32BE()
        {
            var span = _buffer.Slice(Offset, 4);
            Offset += 4;
            return (uint)(span[0] << 24 | span[1] << 16 | span[2] << 8 | span[3]);
        }

        public ushort ReadUInt16BE()
        {
            var span = _buffer.Slice(Offset, 2);
            Offset += 2;
            return (ushort)(span[0] << 8 | span[1]);
        }

        public ulong ReadUInt64BE()
        {
            var span = _buffer.Slice(Offset, 8);
            Offset += 8;
            return
                ((ulong)span[0] << 56) |
                ((ulong)span[1] << 48) |
                ((ulong)span[2] << 40) |
                ((ulong)span[3] << 32) |
                ((ulong)span[4] << 24) |
                ((ulong)span[5] << 16) |
                ((ulong)span[6] << 8) |
                span[7];
        }

        public ReadOnlySpan<byte> ReadBytesSpan(int length)
        {
            var span = _buffer.Slice(Offset, length);
            Offset += length;
            return span;
        }

        public ReadOnlySpan<byte> ReadRemainingSpan()
        {
            var span = _buffer.Slice(Offset);
            Offset = _buffer.Length;
            return span;
        }
    }
}