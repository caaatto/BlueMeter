using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Converts a file path string to an ImageBrush for displaying background images.
/// Returns null if the path is invalid or the file doesn't exist.
/// </summary>
public class BackgroundImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

                // Create BitmapImage with caching
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                // Create ImageBrush
                var imageBrush = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0.3 // Semi-transparent so content is still readable
                };
                imageBrush.Freeze();

                return imageBrush;
            }
            catch
            {
                // Error loading image, return null
                return null;
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
