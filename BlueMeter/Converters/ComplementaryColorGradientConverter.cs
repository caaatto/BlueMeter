using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Converts a theme color to a LinearGradientBrush using its complementary color
/// The complementary color is 180° opposite on the color wheel
/// </summary>
public class ComplementaryColorGradientConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hexColor || string.IsNullOrEmpty(hexColor))
            return BuildHorizontalGradient(Colors.Gray, Colors.DarkGray);

        try
        {
            // Parse the hex color
            if (!Color.TryParse(hexColor, out var color))
                return BuildHorizontalGradient(Colors.Gray, Colors.DarkGray);

            // Convert to HSL
            RgbToHsl(color.R, color.G, color.B, out double h, out double s, out double l);

            // Get complementary hue (180° opposite)
            double complementaryHue = (h + 180) % 360;

            // Create two shades for gradient effect
            // Main complementary color (full saturation)
            var complementaryColor1 = HslToRgb(complementaryHue, s, Math.Min(l, 0.6));

            // Slightly lighter/more saturated variant for gradient
            var complementaryColor2 = HslToRgb(complementaryHue, Math.Min(s * 1.1, 1.0), Math.Min(l * 1.15, 0.7));

            return BuildHorizontalGradient(complementaryColor1, complementaryColor2);
        }
        catch
        {
            // Fallback to default gradient
            return BuildHorizontalGradient(Colors.Gray, Colors.DarkGray);
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static LinearGradientBrush BuildHorizontalGradient(Color start, Color end)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative)
        };

        brush.GradientStops.Add(new GradientStop(start, 0));
        brush.GradientStops.Add(new GradientStop(end, 1));

        return brush;
    }

    /// <summary>
    /// Convert RGB to HSL color space
    /// </summary>
    private static void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        // Lightness
        l = (max + min) / 2.0;

        if (delta == 0)
        {
            h = 0;
            s = 0;
        }
        else
        {
            // Saturation
            s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            // Hue
            if (max == rd)
                h = ((gd - bd) / delta + (gd < bd ? 6 : 0)) / 6.0;
            else if (max == gd)
                h = ((bd - rd) / delta + 2) / 6.0;
            else
                h = ((rd - gd) / delta + 4) / 6.0;

            h *= 360;
        }
    }

    /// <summary>
    /// Convert HSL to RGB color
    /// </summary>
    private static Color HslToRgb(double h, double s, double l)
    {
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;

            h /= 360.0;

            r = HueToRgb(p, q, h + 1.0 / 3.0);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3.0);
        }

        return Color.FromRgb(
            (byte)Math.Round(r * 255),
            (byte)Math.Round(g * 255),
            (byte)Math.Round(b * 255)
        );
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }
}
