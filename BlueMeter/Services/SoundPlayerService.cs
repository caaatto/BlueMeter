using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BlueMeter.Config;
using BlueMeter.Models;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

/// <summary>
/// Service for playing queue pop alert sounds
/// </summary>
public interface ISoundPlayerService : IDisposable
{
    /// <summary>
    /// Play the configured queue pop sound
    /// </summary>
    void PlayQueuePopSound();

    /// <summary>
    /// Test a specific sound with given volume
    /// </summary>
    void TestSound(QueuePopSound sound, double volume);
}

/// <summary>
/// Implementation of sound player service backed by the Win32 MCI API.
/// MCI handles MP3 natively on Windows without any third-party audio dependencies,
/// which is what we need now that WPF's <c>System.Windows.Media.MediaPlayer</c> is gone.
/// </summary>
public sealed class SoundPlayerService : ISoundPlayerService
{
    private readonly ILogger<SoundPlayerService> _logger;
    private readonly IConfigManager _configManager;
    private readonly object _playerLock = new();
    private readonly string _aliasPrefix = "blueMeterSound_" + Guid.NewGuid().ToString("N");
    private string? _currentAlias;
    private bool _disposed;

    // Sound file paths relative to application directory
    private static readonly string SoundsDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Assets",
        "sounds"
    );

    public SoundPlayerService(
        ILogger<SoundPlayerService> logger,
        IConfigManager configManager)
    {
        _logger = logger;
        _configManager = configManager;

        _logger.LogDebug("SoundPlayerService initialized. Sounds directory: {Directory}", SoundsDirectory);
    }

    public void PlayQueuePopSound()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SoundPlayerService));

        var config = _configManager.CurrentConfig;

        if (!config.QueuePopSoundEnabled)
        {
            _logger.LogDebug("Queue pop sound is disabled");
            return;
        }

        PlaySound(config.QueuePopSound, config.QueuePopSoundVolume);
    }

    public void TestSound(QueuePopSound sound, double volume)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SoundPlayerService));

        PlaySound(sound, volume);
    }

    private void PlaySound(QueuePopSound sound, double volume)
    {
        try
        {
            lock (_playerLock)
            {
                var soundFile = GetSoundFilePath(sound);

                if (!File.Exists(soundFile))
                {
                    _logger.LogWarning("Sound file not found: {File}.", soundFile);
                    return;
                }

                CloseCurrent();

                var alias = _aliasPrefix + "_" + Environment.TickCount;
                // mciSendString expects paths in quotes when they may contain spaces
                var openCmd = $"open \"{soundFile}\" type mpegvideo alias {alias}";
                var openResult = mciSendString(openCmd, null, 0, IntPtr.Zero);
                if (openResult != 0)
                {
                    _logger.LogWarning("MCI open failed for {File} (code {Code})", soundFile, openResult);
                    return;
                }

                _currentAlias = alias;

                // MCI volume range is 0..1000
                var mciVolume = (int)Math.Clamp(Math.Round(volume * 10), 0, 1000);
                mciSendString($"setaudio {alias} volume to {mciVolume}", null, 0, IntPtr.Zero);

                var playResult = mciSendString($"play {alias}", null, 0, IntPtr.Zero);
                if (playResult != 0)
                {
                    _logger.LogWarning("MCI play failed for {File} (code {Code})", soundFile, playResult);
                    CloseCurrent();
                    return;
                }

                _logger.LogDebug("Playing sound: {Sound} at {Volume}% volume", sound, volume);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play sound: {Sound}", sound);
        }
    }

    private void CloseCurrent()
    {
        if (_currentAlias is null) return;
        try
        {
            mciSendString($"close {_currentAlias}", null, 0, IntPtr.Zero);
        }
        catch
        {
            // ignore
        }
        _currentAlias = null;
    }

    private static string GetSoundFilePath(QueuePopSound sound)
    {
        var fileName = sound switch
        {
            QueuePopSound.Drum => "drum.mp3",
            QueuePopSound.Harp => "harp.mp3",
            QueuePopSound.Wow => "wow.mp3",
            QueuePopSound.Yoooo => "yoooo.mp3",
            QueuePopSound.DungeonFound => "dungeonfound.mp3",
            QueuePopSound.QPop => "qpop.mp3",
            _ => throw new ArgumentOutOfRangeException(nameof(sound), sound, "Unknown sound type")
        };

        return Path.Combine(SoundsDirectory, fileName);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        lock (_playerLock)
        {
            CloseCurrent();
        }

        _logger.LogDebug("SoundPlayerService disposed");
    }

    [DllImport("winmm.dll", CharSet = CharSet.Unicode, EntryPoint = "mciSendStringW")]
    private static extern int mciSendString(string command, StringBuilder? returnValue, int returnLength, IntPtr hwndCallback);
}
