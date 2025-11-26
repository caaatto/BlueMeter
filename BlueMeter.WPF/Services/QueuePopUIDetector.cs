using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BlueMeter.WPF.Logging;
using Tesseract;

namespace BlueMeter.WPF.Services;

/// <summary>
/// Detects queue pop by capturing the game window and using OCR to find Cancel/Confirm button text.
/// Works even when the game window is minimized or in the background.
/// 100% local - no external communication, only uses Windows OCR and screen capture.
/// </summary>
public interface IQueuePopUIDetector : IDisposable
{
    /// <summary>
    /// Start monitoring the game window for queue pop UI
    /// </summary>
    void Start();

    /// <summary>
    /// Stop monitoring
    /// </summary>
    void Stop();

    /// <summary>
    /// Whether the detector is currently running
    /// </summary>
    bool IsRunning { get; }
}

public sealed class QueuePopUIDetector : IQueuePopUIDetector
{
    private readonly ILogger<QueuePopUIDetector> _logger;
    private readonly ISoundPlayerService _soundPlayerService;

    // Game process names to monitor
    private static readonly string[] GameProcessNames =
    {
        "star",           // Original game
        "BPSR_STEAM",     // Steam version
        "BPSR_EPIC",      // Epic version
        "BPSR"            // Generic version
    };

    // Button text patterns for multi-language support
    // Cancel: Abbrechen (DE), Cancel (EN), キャンセル (JP), 取消 (CN)
    // Confirm: Bestätigen (DE), Confirm/Accept/OK (EN), 確認 (JP), 确认 (CN)
    private static readonly string[] CancelPatterns =
    {
        "cancel", "abbrechen", "キャンセル", "取消", "annuler"
    };

    private static readonly string[] ConfirmPatterns =
    {
        "confirm", "accept", "ok", "bestätigen", "確認", "确认", "accepter"
    };

    // P/Invoke declarations for screen capture
    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private Timer? _pollingTimer;
    private bool _isRunning;
    private bool _disposed;
    private DateTime _lastAlertTime = DateTime.MinValue;
    private DateTime _lastDebugLog = DateTime.MinValue;
    private readonly TimeSpan _alertCooldown = TimeSpan.FromSeconds(10); // Prevent duplicate alerts
    private TesseractEngine? _ocrEngine;
    private readonly SemaphoreSlim _ocrLock = new(1, 1); // Ensure only one OCR operation at a time

    public bool IsRunning => _isRunning;

    public QueuePopUIDetector(
        ILogger<QueuePopUIDetector> logger,
        ISoundPlayerService soundPlayerService)
    {
        _logger = logger;
        _soundPlayerService = soundPlayerService;
    }

    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(QueuePopUIDetector));

        if (_isRunning)
        {
            _logger.LogWarning("QueuePopUIDetector already running");
            return;
        }

        // Initialize Tesseract OCR engine
        try
        {
            var tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            if (!Directory.Exists(tessDataPath))
            {
                _logger.LogError("[OCR] tessdata directory not found at: {Path}", tessDataPath);
                _logger.LogError("[OCR] Please download language files from: https://github.com/tesseract-ocr/tessdata");
                return;
            }

            // Try to initialize with English language data
            _ocrEngine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            _logger.LogInformation("[OCR] Tesseract OCR engine initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OCR] Failed to initialize Tesseract engine. Make sure tessdata/eng.traineddata exists");
            return;
        }

        _isRunning = true;

        // Poll every 500ms for queue pop UI
        _pollingTimer = new Timer(
            _ => CheckForQueuePopUI(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(500));

        _logger.LogInformation(WpfLogEvents.QueueDetector,
            "QueuePopUIDetector started - monitoring via screen capture + OCR");
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _pollingTimer?.Dispose();
        _pollingTimer = null;

        _logger.LogInformation(WpfLogEvents.QueueDetector, "QueuePopUIDetector stopped");
    }

    private void CheckForQueuePopUI()
    {
        if (!_isRunning || _ocrEngine == null)
            return;

        // Log status every 30 seconds for debugging
        var now = DateTime.Now;
        var shouldLogDebug = (now - _lastDebugLog).TotalSeconds >= 30;
        if (shouldLogDebug)
            _lastDebugLog = now;

        try
        {
            // Find game process
            var gameProcess = FindGameProcess();
            if (gameProcess == null)
            {
                if (shouldLogDebug)
                    _logger.LogDebug("[OCR] Game process not found (looking for: {Processes})",
                        string.Join(", ", GameProcessNames));
                return;
            }

            if (shouldLogDebug)
                _logger.LogDebug("[OCR] Found game process: {ProcessName} (PID: {ProcessId})",
                    gameProcess.ProcessName, gameProcess.Id);

            // Get main window handle
            var mainWindowHandle = gameProcess.MainWindowHandle;
            if (mainWindowHandle == IntPtr.Zero)
            {
                if (shouldLogDebug)
                    _logger.LogDebug("[OCR] MainWindowHandle is zero");
                return;
            }

            // Capture screenshot of game window
            using var screenshot = CaptureWindow(mainWindowHandle);
            if (screenshot == null)
            {
                if (shouldLogDebug)
                    _logger.LogDebug("[OCR] Failed to capture window screenshot");
                return;
            }

            if (shouldLogDebug)
                _logger.LogDebug("[OCR] Captured screenshot: {Width}x{Height}", screenshot.Width, screenshot.Height);

            // Run OCR on screenshot
            var hasQueuePop = Task.Run(async () => await DetectQueuePopTextAsync(screenshot, shouldLogDebug)).Result;

            if (hasQueuePop)
            {
                // Check cooldown to prevent duplicate alerts
                if (DateTime.Now - _lastAlertTime < _alertCooldown)
                    return;

                _lastAlertTime = DateTime.Now;
                OnQueuePopDetected();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OCR] Error checking for queue pop UI");
        }
    }

    private Process? FindGameProcess()
    {
        foreach (var processName in GameProcessNames)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
                return processes[0];
        }

        return null;
    }

    private Bitmap? CaptureWindow(IntPtr hWnd)
    {
        try
        {
            var rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            int fullWidth = rect.Right - rect.Left;
            int fullHeight = rect.Bottom - rect.Top;

            if (fullWidth <= 0 || fullHeight <= 0)
                return null;

            // Capture only bottom RIGHT quarter (where Accept/Decline buttons are)
            // Bottom 50% of height, right 50% of width
            int captureWidth = fullWidth / 2;
            int captureHeight = fullHeight / 2;
            int captureX = fullWidth / 2;  // Start at 50% from left (right half)
            int captureY = fullHeight / 2; // Start at 50% from top (bottom half)

            var hdcSrc = GetDC(hWnd);
            var hdcDest = CreateCompatibleDC(hdcSrc);
            var hBitmap = CreateCompatibleBitmap(hdcSrc, captureWidth, captureHeight);
            var hOld = SelectObject(hdcDest, hBitmap);

            // Copy only the bottom center portion
            BitBlt(hdcDest, 0, 0, captureWidth, captureHeight, hdcSrc, captureX, captureY, CopyPixelOperation.SourceCopy);

            SelectObject(hdcDest, hOld);
            DeleteDC(hdcDest);
            ReleaseDC(hWnd, hdcSrc);

            var bitmap = Image.FromHbitmap(hBitmap);
            DeleteObject(hBitmap);

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> DetectQueuePopTextAsync(Bitmap screenshot, bool shouldLogDebug)
    {
        // Use semaphore to ensure only one OCR operation at a time (Tesseract is not thread-safe)
        if (!await _ocrLock.WaitAsync(0)) // Try to acquire immediately, don't block
        {
            if (shouldLogDebug)
                _logger.LogDebug("[OCR] Skipping OCR - previous operation still running");
            return false;
        }

        try
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_ocrEngine == null)
                        return false;

                    // Save bitmap to memory stream and process with Tesseract
                    using var ms = new MemoryStream();
                    screenshot.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;

                    using var img = Pix.LoadFromMemory(ms.ToArray());
                    using var page = _ocrEngine.Process(img);

                    var allText = page.GetText()?.ToLowerInvariant() ?? string.Empty;

                    if (shouldLogDebug && !string.IsNullOrWhiteSpace(allText))
                        _logger.LogDebug("[OCR] Detected text: {Text}", allText.Substring(0, Math.Min(200, allText.Length)));

                    // Check for Cancel and Confirm patterns
                    bool foundCancel = CancelPatterns.Any(pattern => allText.Contains(pattern));
                    bool foundConfirm = ConfirmPatterns.Any(pattern => allText.Contains(pattern));

                    if (foundCancel)
                        _logger.LogInformation("[OCR] ✓ Found Cancel text");

                    if (foundConfirm)
                        _logger.LogInformation("[OCR] ✓ Found Confirm text");

                    // If we found both, it's a queue pop!
                    return foundCancel && foundConfirm;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[OCR] Error during text recognition");
                    return false;
                }
            });
        }
        finally
        {
            _ocrLock.Release();
        }
    }

    private void OnQueuePopDetected()
    {
        try
        {
            _logger.LogInformation(WpfLogEvents.QueueDetector,
                "★★★ QUEUE POP DETECTED via OCR! ★★★ Playing alert sound...");

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
        Stop();
        _ocrEngine?.Dispose();
        _ocrLock.Dispose();

        _logger.LogDebug("QueuePopUIDetector disposed");
    }
}
