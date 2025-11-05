using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts (currentValue, collectionOrMax) -> percentage (0..100).
/// If the second binding is an IEnumerable, the converter computes the maximum numeric value
/// in that collection (handles numeric items, Dictionary/KeyValuePair collections, and view-models
/// that expose a numeric property like Value or ProgressBarValue; it will also drill into a 'Data'
/// property to find a nested 'Value' if present).
/// If the second binding is a numeric scalar, it is treated as the max directly.
/// </summary>
public class PercentOfMaxConverter : IMultiValueConverter
{
    // Cache property lookup per type to avoid repeated reflection cost.
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> s_propertyCache = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return 0d;

        var current = ToDouble(values[0]);
        var second = values[1];

        var max = 0d;

        if (second is IEnumerable enumerable && !(second is string))
        {
            var hasAny = false;
            foreach (var item in enumerable)
            {
                var v = ExtractNumericFromItem(item);
                if (!hasAny || v > max)
                {
                    max = v;
                    hasAny = true;
                }
            }

            if (!hasAny)
                max = 0d;
        }
        else
        {
            max = ToDouble(second);
        }

        if (max <= 0d) return 0d;

        var perc = current / max * 100.0;
        if (double.IsNaN(perc) || double.IsInfinity(perc)) return 0d;
        return Math.Max(0d, Math.Min(100d, perc));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static double ExtractNumericFromItem(object? item)
    {
        if (item == null) return 0d;

        // If item is numeric convertible
        if (item is IConvertible) return ToDouble(item);

        var type = item.GetType();

        // Handle KeyValuePair<,> (common when binding Dictionary.Values may appear as KeyValuePair in some enumerations)
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var valueProp = type.GetProperty("Value",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (valueProp != null)
            {
                var val = valueProp.GetValue(item);
                return ExtractNumericFromItem(val);
            }
        }

        // Check cached property candidate (Value, ProgressBarValue, Data -> nested Value)
        var prop = s_propertyCache.GetOrAdd(type, t =>
        {
            // try common property names (case-insensitive)
            var pi = t.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                     ?? t.GetProperty("ProgressBarValue",
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                     ?? t.GetProperty("Data", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return pi;
        });

        if (prop != null)
        {
            var propVal = prop.GetValue(item);
            // If the 'Data' property returned, try to get its nested Value as well
            if (prop.Name.Equals("Data", StringComparison.OrdinalIgnoreCase) && propVal != null)
            {
                var innerType = propVal.GetType();
                var innerProp = innerType.GetProperty("Value",
                                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                                ?? innerType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (innerProp != null)
                {
                    var innerVal = innerProp.GetValue(propVal);
                    return ExtractNumericFromItem(innerVal);
                }
            }

            return ExtractNumericFromItem(propVal);
        }

        // Last resort: try to find any numeric property
        var numericProp = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => IsNumericType(p.PropertyType) && p.CanRead);
        if (numericProp != null)
        {
            var val = numericProp.GetValue(item);
            return ToDouble(val);
        }

        return 0d;
    }

    private static double ToDouble(object? obj)
    {
        if (obj == null) return 0d;
        try
        {
            if (obj is double d) return d;
            if (obj is float f) return f;
            if (obj is decimal dec) return (double)dec;
            if (obj is ulong ul) return ul;
            if (obj is long l) return l;
            if (obj is uint ui) return ui;
            if (obj is int i) return i;
            if (obj is short s) return s;
            if (obj is ushort us) return us;
            if (obj is byte b) return b;
            if (obj is sbyte sb) return sb;
            if (obj is string str &&
                double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var r)) return r;
        }
        catch
        {
            // ignore and fall through
        }

        return 0d;
    }

    private static bool IsNumericType(Type t)
    {
        if (t.IsEnum) return false;
        var tc = Type.GetTypeCode(t);
        return tc is
                TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64
                or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double
                or TypeCode.Single
            ;
    }
}