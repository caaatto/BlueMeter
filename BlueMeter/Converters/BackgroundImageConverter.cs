using System.Diagnostics;
using System.Globalization;
using System.IO;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace BlueMeter.Converters;

/// <summary>
/// Converts a file path string to an ImageBrush for displaying background images.
/// Returns null if the path is invalid or the file doesn't exist.
/// </summary>
public class BackgroundImageConverter : IValueConverter
{
    // Bitmap cache keyed by resolved absolute path. Avoids re-decoding the same file each
    // time the binding re-runs (every AppConfig clone swap and every theme toggle fires
    // the converter) and guarantees the exact same IImage identity is handed back on
    // repeated calls — without this, handing Border.Background a freshly-allocated
    // ImageBrush on every evaluation sometimes raced the layout pass and left the
    // background blank because the new brush arrived after Measure had already run.
    private static readonly Dictionary<string, Bitmap> _cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _cacheLock = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string rawPath || string.IsNullOrWhiteSpace(rawPath))
        {
            return null;
        }

        try
        {
            // Normalise the path — the storage picker may hand back a file:// URI on
            // some platforms, and the user may have hand-edited the config JSON with
            // mixed separators. Path.GetFullPath normalises both.
            var resolved = rawPath;
            if (resolved.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                resolved = new Uri(resolved).LocalPath;
            }
            resolved = Path.GetFullPath(resolved);

            if (!File.Exists(resolved))
            {
                Debug.WriteLine($"[BackgroundImageConverter] File not found: {resolved}");
                return null;
            }

            Bitmap? bitmap;
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(resolved, out bitmap))
                {
                    using var stream = File.OpenRead(resolved);
                    bitmap = new Bitmap(stream);
                    _cache[resolved] = bitmap;
                }
            }

            return new ImageBrush(bitmap)
            {
                Stretch = Stretch.UniformToFill,
                Opacity = 1.0 // Full opacity for bright, vibrant background
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BackgroundImageConverter] Failed to load '{rawPath}': {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
