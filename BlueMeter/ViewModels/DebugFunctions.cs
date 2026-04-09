using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Events;
using BlueMeter.Config;
using BlueMeter.Core.Analyze;
using BlueMeter.Core.Data;
using BlueMeter.Localization;
using BlueMeter.Models;
using BlueMeter.Services;

namespace BlueMeter.ViewModels;

public partial class DebugFunctions : BaseViewModel, IDisposable
{
    private const int MaxLogEntries = 2000; // allow more lines for context
    private const int FilterDebounceMs = 250;
    private const int BatchSize = 20; // Process logs in larger batches

    private readonly LocalizationManager _localizationManager;
    private readonly ILogger<DebugFunctions> _logger;
    private readonly IDisposable? _logSubscription;
    private readonly IPacketAnalyzer _packetAnalyzer;
    private readonly IFileDialogService? _fileDialogService;
    private readonly Queue<LogEntry> _pendingLogs = new();
    [ObservableProperty] private bool _autoScrollEnabled = true;
    [ObservableProperty] private bool _queueDetectionLoggingEnabled = false;

    [ObservableProperty] private List<Option<Language>> _availableLanguages =
    [
        new(Language.Auto, Language.Auto.GetLocalizedDescription()),
        new(Language.ZhCn, Language.ZhCn.GetLocalizedDescription()),
        new(Language.EnUs, Language.EnUs.GetLocalizedDescription()),
        new(Language.PtBr, Language.PtBr.GetLocalizedDescription())
    ];

    [ObservableProperty] private bool _enabled;
    private Timer? _filterDebounceTimer;
    [ObservableProperty] private int _filteredLogCount;
    [ObservableProperty] private ObservableCollection<LogEntry> _filteredLogs = new();
    [ObservableProperty] private string _filterText = string.Empty;
    private volatile bool _isBatchProcessing;
    [ObservableProperty] private DateTime? _lastLogTime;
    [ObservableProperty] private int _logCount;

    [ObservableProperty] private ObservableCollection<LogEntry> _logs = new();
    private CancellationTokenSource? _replayCts;
    private Task? _replayTask;
    [ObservableProperty] private Option<Language>? _selectedLanguage;
    [ObservableProperty] private LogLevel _selectedLogLevel = LogLevel.Information;

    public DebugFunctions(
        ILogger<DebugFunctions> logger,
        IObservable<LogEvent> observer,
        IOptionsMonitor<AppConfig> options,
        IPacketAnalyzer packetAnalyzer,
        LocalizationManager localizationManager,
        IFileDialogService? fileDialogService = null)
    {
        _logger = logger;

        _logSubscription = observer.Subscribe(OnSerilogEvent);

        PropertyChanged += OnPropertyChanged;
        SetProperty(options.CurrentValue, null);
        options.OnChange(SetProperty);
        _packetAnalyzer = packetAnalyzer;
        _localizationManager = localizationManager;
        _fileDialogService = fileDialogService;

        _logger.LogInformation("Debug panel initialized");
    }

    public LogLevel[] AvailableLogLevels { get; } =
    [
        LogLevel.Trace, LogLevel.Debug, LogLevel.Information,
        LogLevel.Warning, LogLevel.Error, LogLevel.Critical
    ];

    public void Dispose()
    {
        // MEMORY LEAK FIX: Unsubscribe from PropertyChanged event to prevent memory leak.
        PropertyChanged -= OnPropertyChanged;

        _filterDebounceTimer?.Dispose();
        _logSubscription?.Dispose();

        // Clear any pending logs
        lock (_pendingLogs)
        {
            _pendingLogs.Clear();
        }
    }

    public event EventHandler? LogAdded;

    // Event to request sample data addition - removes direct dependency on DpsStatisticsViewModel
    public event EventHandler? SampleDataRequested;

    partial void OnSelectedLanguageChanged(Option<Language>? value)
    {
        if (value == null) return;
        _localizationManager.ApplyLanguage(value.Value);
    }

    private void SetProperty(AppConfig arg1, string? arg2)
    {
        Enabled = arg1.DebugEnabled;
    }

    private void OnSerilogEvent(LogEvent evt)
    {
        var mappedLevel = evt.Level switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.Information
        };

        var sourceContext = evt.Properties.TryGetValue("SourceContext", out var sc)
            ? sc.ToString().Trim('"')
            : string.Empty;
        var rendered = evt.RenderMessage();
        var timestamp = evt.Timestamp.LocalDateTime;

        var entry = new LogEntry(timestamp, mappedLevel, rendered, sourceContext, evt.Exception);

        // Add to pending queue for batch processing
        lock (_pendingLogs)
        {
            _pendingLogs.Enqueue(entry);
        }

        // Trigger batch processing if not already running
        if (!_isBatchProcessing)
        {
            _isBatchProcessing = true;
            Dispatcher.UIThread.Post(ProcessLogBatch, DispatcherPriority.Background);
        }
    }

    private void ProcessLogBatch()
    {
        try
        {
            var processedCount = 0;
            var shouldRefresh = false;
            LogEntry? lastEntry = null;

            // Process logs in batches
            lock (_pendingLogs)
            {
                while (_pendingLogs.Count > 0 && processedCount < BatchSize)
                {
                    var entry = _pendingLogs.Dequeue();

                    // Remove oldest entries if we're at the limit
                    while (Logs.Count >= MaxLogEntries)
                    {
                        Logs.RemoveAt(0);
                    }

                    Logs.Add(entry);
                    lastEntry = entry;
                    processedCount++;

                    // Check if this entry would be visible after filtering
                    if (LogFilter(entry))
                    {
                        FilteredLogs.Add(entry);
                        shouldRefresh = true;
                    }
                }
            }

            // Trim FilteredLogs to keep it bounded
            while (FilteredLogs.Count > MaxLogEntries)
            {
                FilteredLogs.RemoveAt(0);
            }

            // Update properties only once per batch
            if (processedCount > 0)
            {
                LogCount = Logs.Count;
                if (lastEntry != null)
                {
                    LastLogTime = lastEntry.Timestamp;
                }

                if (shouldRefresh)
                {
                    UpdateFilteredLogCount();
                    LogAdded?.Invoke(this, EventArgs.Empty);
                }
            }

            // Continue processing if there are more logs
            lock (_pendingLogs)
            {
                if (_pendingLogs.Count > 0)
                {
                    Dispatcher.UIThread.Post(ProcessLogBatch, DispatcherPriority.Background);
                }
                else
                {
                    _isBatchProcessing = false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing log batch");
            _isBatchProcessing = false;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(FilterText):
                // Debounce filter text changes
                _filterDebounceTimer?.Dispose();
                _filterDebounceTimer = new Timer(_ =>
                {
                    Dispatcher.UIThread.Post(RefreshFilteredLogs, DispatcherPriority.Background);
                }, null, FilterDebounceMs, Timeout.Infinite);
                break;
            case nameof(SelectedLogLevel):
                RefreshFilteredLogs();
                break;
        }
    }

    private void RefreshFilteredLogs()
    {
        FilteredLogs.Clear();
        foreach (var log in Logs)
        {
            if (LogFilter(log))
            {
                FilteredLogs.Add(log);
            }
        }
        UpdateFilteredLogCount();
    }

    private bool LogFilter(LogEntry log)
    {
        if (log.Level < SelectedLogLevel) return false;
        if (string.IsNullOrWhiteSpace(FilterText)) return true;
        return log.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
               log.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateFilteredLogCount()
    {
        FilteredLogCount = FilteredLogs.Count;
    }

    [RelayCommand]
    private void CallDebugWindow()
    {
        // DebugView is ported in Phase 10 — this command becomes a no-op until then.
        _logger.LogInformation("CallDebugWindow requested (DebugView not yet ported)");
    }

    [RelayCommand]
    private void ClearLogs()
    {
        // Clear pending logs as well
        lock (_pendingLogs)
        {
            _pendingLogs.Clear();
        }

        Logs.Clear();
        FilteredLogs.Clear();
        LogCount = 0;
        FilteredLogCount = 0;
        LastLogTime = null;
        _logger.LogInformation("Logs cleared");
    }

    [RelayCommand]
    private async Task SaveLogs()
    {
        if (_fileDialogService == null)
        {
            _logger.LogWarning("SaveLogs requested but IFileDialogService is not available");
            return;
        }

        var filters = new List<(string, IReadOnlyList<string>)>
        {
            ("Log files", new[] { "log" }),
            ("Text files", new[] { "txt" }),
            ("All files", new[] { "*" })
        };

        var path = await _fileDialogService.ShowSaveFileDialogAsync(
            "Save Debug Logs",
            $"debug_logs_{DateTime.Now:yyyyMMdd_HHmmss}.log",
            filters);

        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var logsToSave = (IEnumerable<LogEntry>)FilteredLogs;
            await using var writer = new StreamWriter(path);
            foreach (var log in logsToSave)
            {
                await writer.WriteLineAsync(
                    $"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.Level}] [{log.Category}] {log.Message}");
                if (log.Exception != null)
                    await writer.WriteLineAsync($"Exception: {log.Exception}");
            }

            _logger.LogInformation("Logs saved to {File}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save logs to {File}", path);
        }
    }

    [RelayCommand]
    private void AddTestLog()
    {
        _logger.LogInformation("Test log entry {Id}", Guid.NewGuid().ToString("N")[..8]);
    }

    #region AddData

    [RelayCommand]
    private void AddSampleData()
    {
        // Fire event instead of directly calling DpsStatisticsViewModel
        SampleDataRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Replay

    [RelayCommand]
    private async Task LoadDebugDataSource()
    {
        if (_fileDialogService == null)
        {
            _logger.LogWarning("LoadDebugDataSource requested but IFileDialogService is not available");
            return;
        }

        var filters = new List<(string, IReadOnlyList<string>)>
        {
            ("Capture files", new[] { "pcap", "pcapng" }),
            ("All files", new[] { "*" })
        };

        var path = await _fileDialogService.ShowOpenFileDialogAsync("Open pcap/pcapng file to replay", filters);
        if (string.IsNullOrEmpty(path)) return;

        StartPcapReplay(path);
        _logger.LogInformation("Replaying PCAP: {File}", Path.GetFileName(path));
    }

    private void StartPcapReplay(string filePath, bool realtime = true, double speed = 1.0)
    {
        StopPcapReplay();
        _replayCts = new CancellationTokenSource();
        var token = _replayCts.Token;
        _replayTask = Task.Run(async () =>
        {
            try
            {
                await _packetAnalyzer.ReplayFileAsync(filePath, realtime, speed, token).ConfigureAwait(false);
                _logger.LogInformation("PCAP replay completed: {File}", Path.GetFileName(filePath));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PCAP replay cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PCAP replay failed: {File}", Path.GetFileName(filePath));
            }
            finally
            {
                try
                {
                    _replayCts?.Dispose();
                }
                catch
                {
                    // ignored
                }

                _replayCts = null;
                _replayTask = null;
            }
        }, token);
    }

    private void StopPcapReplay()
    {
        if (_replayCts == null) return;
        try
        {
            _replayCts.Cancel();
            _replayTask?.Wait(3000);
            _logger.LogInformation("PCAP replay stopped");
        }
        catch (AggregateException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping PCAP replay");
        }
        finally
        {
            try
            {
                _replayCts.Dispose();
            }
            catch
            {
                // ignored
            }

            _replayCts = null;
            _replayTask = null;
        }
    }

    partial void OnQueueDetectionLoggingEnabledChanged(bool value)
    {
        DataStorageV2.EnableQueueDetectionLogging = value;
        _logger.LogInformation("[QUEUE DETECTION] Logging {Status}", value ? "ENABLED" : "DISABLED");
    }

    #endregion
}
