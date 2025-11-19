using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts IsFocused (bool) and Text (string) to Visibility.
/// Returns Visible when focused AND text is empty, otherwise Collapsed.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class FocusedAndEmptyToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 2)
        {
            return Visibility.Collapsed;
        }

        // First value: IsFocused (bool)
        var isFocused = values[0] as bool? ?? false;

        // Second value: Text (string)
        var text = values[1] as string;

        // Show placeholder only when focused AND text is empty
        return isFocused && string.IsNullOrEmpty(text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        return [Binding.DoNothing, Binding.DoNothing];
    }
}
