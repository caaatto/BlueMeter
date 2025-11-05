using Microsoft.Extensions.Logging;

namespace StarResonanceDpsAnalysis.Core.Logging;

public static class CoreLogEvents
{
    // Analyzer lifecycle
    public static readonly EventId PacketStart = new(1000, nameof(PacketStart));
    public static readonly EventId PacketStop = new(1001, nameof(PacketStop));

    // Stream state
    public static readonly EventId Reconnect = new(1100, nameof(Reconnect));
    public static readonly EventId Resync = new(1101, nameof(Resync));
    public static readonly EventId ServerDetected = new(1200, nameof(ServerDetected));

    // Errors
    public static readonly EventId AnalyzerError = new(1300, nameof(AnalyzerError));
    public static readonly EventId ChannelError = new(1301, nameof(ChannelError));

    // Sync/Delta
    public static readonly EventId DeltaProcessed = new(1400, nameof(DeltaProcessed));
    public static readonly EventId SyncContainerData = new(1500, nameof(SyncContainerData));
    public static readonly EventId SyncNearDelta = new(1501, nameof(SyncNearDelta));
    public static readonly EventId SyncToMeDelta = new(1502, nameof(SyncToMeDelta));
}
