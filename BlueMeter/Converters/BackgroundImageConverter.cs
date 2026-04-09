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
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
        {
            try
            {
                // Check if file exists
                if (!File.Exists(imagePath))
                {
                    return null;
                }

                // Load bitmap from disk (Avalonia's Bitmap fully decodes on construction)
                using var stream = File.OpenRead(imagePath);
                var bitmap = new Bitmap(stream);

                // Create ImageBrush
                return new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = 1.0 // Full opacity for bright, vibrant background
                };
            }
            catch
            {
                // Error loading image, return null
                return null;
            }
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
