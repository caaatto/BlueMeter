using BlueMeter.Core.Data;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

/// <summary>
/// Manages queue pop alerts by subscribing to DataStorage events and triggering sound notifications
/// </summary>
public interface IQueueAlertManager : IDisposable
{
    /// <summary>
    /// Initialize the alert manager with the data storage instance
    /// </summary>
    void Initialize(DataStorageV2 dataStorage);
}

/// <summary>
/// Implementation of queue alert manager
/// </summary>
public sealed class QueueAlertManager : IQueueAlertManager
{
    private readonly ILogger<QueueAlertManager> _logger;
    private readonly ISoundPlayerService _soundPlayerService;
    private DataStorageV2? _dataStorage;
    private bool _disposed;

    public QueueAlertManager(
        ILogger<QueueAlertManager> logger,
        ISoundPlayerService soundPlayerService)
    {
        _logger = logger;
        _soundPlayerService = soundPlayerService;
    }

    public void Initialize(DataStorageV2 dataStorage)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(QueueAlertManager));

        if (_dataStorage != null)
        {
            _logger.LogWarning("QueueAlertManager already initialized, unsubscribing from previous DataStorage");
            _dataStorage.QueuePopDetected -= OnQueuePopDetected;
        }

        _dataStorage = dataStorage;
        _dataStorage.QueuePopDetected += OnQueuePopDetected;

        _logger.LogInformation("QueueAlertManager initialized and subscribed to QueuePopDetected events");
    }

    private void OnQueuePopDetected()
    {
        try
        {
            _logger.LogInformation("Queue pop detected! Playing alert sound...");
            _soundPlayerService.PlayQueuePopSound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play queue pop alert sound");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_dataStorage != null)
        {
            _dataStorage.QueuePopDetected -= OnQueuePopDetected;
            _dataStorage = null;
        }

        _logger.LogDebug("QueueAlertManager disposed");
    }
}
