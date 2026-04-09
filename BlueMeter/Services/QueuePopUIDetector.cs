using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using BlueMeter.Config;
using BlueMeter.Logging;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace BlueMeter.Services;

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
    private readonly IConfigManager _configManager;

    // Game process names to monitor
    private static readonly string[] GameProcessNames =
    {
        "star",           // Original game
        "BPSR_STEAM",     // Steam version
        "BPSR_EPIC",      // Epic version
        "BPSR"            // Generic version
    };

    // Blacklist - if these are found, it's NOT a queue pop (e.g., "Challenge" button, dungeon UI)
    private static readonly string[] BlacklistPatterns =
    {
        // Challenge/Match UI
        "challenge", "challen", "challe", "match", "single", "dual", "team",
        // Dungeon/Combat UI indicators
        "hp", "damage", "dps", "party", "objective", "boss", "elite", "mission",
        "combo", "skill", "attack", "defense", "enemy", "wave", "round",
        // Player status indicators that appear in dungeons
        "health", "shield", "buff", "debuff", "cooldown", "stamina",
        // Common dungeon UI text
        "defeat", "victory", "reward", "loot", "exp", "level up",
        // Results/Completion screens (when leaving dungeon)
        "result", "results", "complete", "completed", "clear", "cleared",
        "time", "rank", "score", "rating", "grade", "total", "summary"
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

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    private const uint PW_RENDERFULLCONTENT = 0x00000002;

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
    private readonly TimeSpan _alertCooldown = TimeSpan.FromSeconds(30); // Cooldown between alerts (30s to reduce false positives)
    private TesseractEngine? _ocrEngine;
    private readonly SemaphoreSlim _ocrLock = new(1, 1); // Ensure only one OCR operation at a time

    public bool IsRunning => _isRunning;

    public QueuePopUIDetector(
        ILogger<QueuePopUIDetector> logger,
        ISoundPlayerService soundPlayerService,
        IConfigManager configManager)
    {
        _logger = logger;
        _soundPlayerService = soundPlayerService;
        _configManager = configManager;
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

        _logger.LogInformation(LogEvents.QueueDetector,
            "QueuePopUIDetector started - monitoring via screen capture + OCR");
        _logger.LogInformation("[Queue Alert] Detector is RUNNING and will check every 500ms");
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _pollingTimer?.Dispose();
        _pollingTimer = null;

        _logger.LogInformation(LogEvents.QueueDetector, "QueuePopUIDetector stopped");
    }

    private void CheckForQueuePopUI()
    {
        if (!_isRunning)
        {
            _logger.LogDebug("[Queue Alert] Detector not running, skipping check");
            return;
        }

        if (_ocrEngine == null)
        {
            _logger.LogWarning("[Queue Alert] OCR engine is null, skipping check");
            return;
        }

        try
        {
            // Find game process
            var gameProcess = FindGameProcess();
            if (gameProcess == null)
            {
                _logger.LogDebug("[Queue Alert] Game process not found, skipping check");
                return;
            }

            _logger.LogDebug("[Queue Alert] Found game process: {ProcessName}", gameProcess.ProcessName);

            // Get main window handle
            var mainWindowHandle = gameProcess.MainWindowHandle;
            if (mainWindowHandle == IntPtr.Zero)
            {
                _logger.LogWarning("[Queue Alert] Game window handle is zero, skipping check");
                return;
            }

            _logger.LogDebug("[Queue Alert] Capturing window screenshot...");

            // Capture screenshot of game window
            using var screenshot = CaptureWindow(mainWindowHandle);
            if (screenshot == null)
            {
                _logger.LogWarning("[Queue Alert] Failed to capture window screenshot");
                return;
            }

            _logger.LogDebug("[Queue Alert] Screenshot captured, running OCR...");

            // Run OCR on screenshot (use synchronous version to avoid Task deadlocks)
            var hasQueuePop = DetectQueuePopTextSync(screenshot);

            if (hasQueuePop)
            {
                // Check cooldown to prevent duplicate alerts
                if (DateTime.Now - _lastAlertTime < _alertCooldown)
                    return;

                _lastAlertTime = DateTime.Now;
                OnQueuePopDetected();
            }
        }
        catch (ObjectDisposedException)
        {
            // Expected during shutdown, ignore
            _isRunning = false;
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
        IntPtr hdcSrc = IntPtr.Zero;
        IntPtr hdcDest = IntPtr.Zero;
        IntPtr hBitmap = IntPtr.Zero;
        Bitmap? fullBitmap = null;

        try
        {
            var rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            int fullWidth = rect.Right - rect.Left;
            int fullHeight = rect.Bottom - rect.Top;

            if (fullWidth <= 0 || fullHeight <= 0)
                return null;

            // First, capture the full window using PrintWindow (works even when minimized/covered)
            hdcSrc = GetDC(hWnd);
            if (hdcSrc == IntPtr.Zero)
                return null;

            hdcDest = CreateCompatibleDC(hdcSrc);
            if (hdcDest == IntPtr.Zero)
                return null;

            hBitmap = CreateCompatibleBitmap(hdcSrc, fullWidth, fullHeight);
            if (hBitmap == IntPtr.Zero)
                return null;

            var hOld = SelectObject(hdcDest, hBitmap);

            // Use PrintWindow instead of BitBlt - this captures even minimized/background windows
            bool success = PrintWindow(hWnd, hdcDest, PW_RENDERFULLCONTENT);

            SelectObject(hdcDest, hOld);

            if (!success)
                return null;

            fullBitmap = Image.FromHbitmap(hBitmap);

            // Now crop to bottom area (where queue pop UI appears)
            // Capture 100% width x 60% height from the bottom
            // This includes queue pop UI while minimizing party UI at top
            int captureWidth = fullWidth;
            int captureHeight = (int)(fullHeight * 0.6);
            int captureX = 0;
            int captureY = (int)(fullHeight * 0.4);  // Start at 40% from top (bottom 60%)

            var croppedBitmap = new Bitmap(captureWidth, captureHeight);
            using (var g = Graphics.FromImage(croppedBitmap))
            {
                g.DrawImage(fullBitmap,
                    new Rectangle(0, 0, captureWidth, captureHeight),
                    new Rectangle(captureX, captureY, captureWidth, captureHeight),
                    GraphicsUnit.Pixel);
            }

            return croppedBitmap;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OCR] Error capturing window");
            return null;
        }
        finally
        {
            // Clean up GDI resources to prevent memory leaks
            if (hBitmap != IntPtr.Zero)
                DeleteObject(hBitmap);

            if (hdcDest != IntPtr.Zero)
                DeleteDC(hdcDest);

            if (hdcSrc != IntPtr.Zero)
                ReleaseDC(hWnd, hdcSrc);

            fullBitmap?.Dispose();
        }
    }

    private bool DetectQueuePopTextSync(Bitmap screenshot)
    {
        // Use semaphore to ensure only one OCR operation at a time (Tesseract is not thread-safe)
        if (!_ocrLock.Wait(0)) // Try to acquire immediately, don't block
            return false;

        try
        {
            if (_ocrEngine == null || _disposed)
                return false;

            // Save bitmap to memory stream and process with Tesseract
            using var ms = new MemoryStream();
            screenshot.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            using var img = Pix.LoadFromMemory(ms.ToArray());
            using var page = _ocrEngine.Process(img);

            var allText = page.GetText()?.ToLowerInvariant() ?? string.Empty;

            // Count player cards first
            int playerCardCount = System.Text.RegularExpressions.Regex.Matches(allText, @"lv\.?\s*\d+").Count;

            // Always log when we find ANY player cards (changed to Information level for visibility)
            if (playerCardCount > 0)
            {
                _logger.LogInformation("[Queue Alert] OCR found {Count} player cards. Full text: {Text}",
                    playerCardCount,
                    allText.Length > 200 ? allText.Substring(0, 200) + "..." : allText);
            }

            // Check for blacklisted patterns (Challenge button, dungeon UI, etc.)
            var foundBlacklistPatterns = BlacklistPatterns.Where(pattern => allText.Contains(pattern)).ToList();
            if (foundBlacklistPatterns.Any() && playerCardCount >= 3)
            {
                _logger.LogInformation("[Queue Alert] Blacklist blocked alert. Patterns: {Patterns}, Cards: {Count}",
                    string.Join(", ", foundBlacklistPatterns), playerCardCount);
                return false;
            }

            // Detect queue pop by player card layout pattern
            // Queue pop shows 5 player cards with "Lv.60" or similar level text
            // Threshold: 3+ player cards (OCR doesn't always catch all 5)
            if (playerCardCount >= 3)
            {
                _logger.LogInformation("[Queue Alert] QUEUE POP DETECTED! Cards: {Count}, Playing sound...", playerCardCount);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OCR] Error during text recognition");
            return false;
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
            // Check if queue pop alerts are enabled before playing sound
            if (!_configManager.CurrentConfig.QueuePopSoundEnabled)
            {
                _logger.LogWarning("[Queue Alert] Detection succeeded but QueuePopSoundEnabled is OFF - Enable in Settings!");
                return;
            }

            _logger.LogInformation("[Queue Alert] Playing queue pop sound (setting is enabled)");
            _soundPlayerService.PlayQueuePopSound();
            _logger.LogInformation("[Queue Alert] Sound played successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Queue Alert] Failed to play queue pop alert sound");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Stop the timer first to prevent new operations
        Stop();

        // Wait for any ongoing OCR operations to complete (max 2 seconds)
        try
        {
            _ocrLock.Wait(TimeSpan.FromSeconds(2));
            _ocrLock.Release();
        }
        catch
        {
            // Ignore timeout or disposal errors
        }

        // Dispose OCR engine and semaphore
        try
        {
            _ocrEngine?.Dispose();
            _ocrEngine = null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OCR] Error disposing OCR engine");
        }

        try
        {
            _ocrLock.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }

        _logger.LogDebug("QueuePopUIDetector disposed");
    }
}
