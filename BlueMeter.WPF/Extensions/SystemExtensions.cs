namespace BlueMeter.WPF.Extensions;

public static class SystemExtensions
{
    public static ulong ConvertToUnsigned(this long value)
    {
        return value <= 0 ? 0UL : (ulong)value;
    }
}