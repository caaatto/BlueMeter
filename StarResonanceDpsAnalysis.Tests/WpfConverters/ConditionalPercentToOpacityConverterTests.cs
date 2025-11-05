using System.Globalization;
using StarResonanceDpsAnalysis.WPF.Converters;

namespace StarResonanceDpsAnalysis.Tests.WpfConverters;

public class ConditionalPercentToOpacityConverterTests
{
    private readonly ConditionalPercentToOpacityConverter _converter = new();
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [Theory]
    [InlineData(0, true, 0.0)]
    [InlineData(25, true, 0.25)]
    [InlineData(50, true, 0.5)]
    [InlineData(100, true, 1.0)]
    [InlineData(150, true, 1.0)]
    [InlineData(-10, true, 0.0)]
    public void Convert_IntPercent_Enabled_ReturnsScaledOpacity(int percent, bool enabled, double expected)
    {
        var result = _converter.Convert([percent, enabled], typeof(double), null, _culture);
        var val = Assert.IsType<double>(result);
        Assert.Equal(expected, val, 3);
    }

    [Theory]
    [InlineData("0", 0.0)]
    [InlineData("12.5", 0.125)]
    [InlineData("100", 1.0)]
    [InlineData("250", 1.0)]
    public void Convert_StringPercent_ParsesAndScales(string percent, double expected)
    {
        var result = _converter.Convert([percent, true], typeof(double), null, _culture);
        var val = Assert.IsType<double>(result);
        Assert.Equal(expected, val, 3);
    }

    [Fact]
    public void Convert_Disabled_ReturnsOne()
    {
        var result = _converter.Convert([25, false], typeof(double), null, _culture);
        var val = Assert.IsType<double>(result);
        Assert.Equal(1.0, val, 3);
    }

    [Fact]
    public void Convert_InvalidInput_ReturnsOne()
    {
        var result = _converter.Convert([new object(), true], typeof(double), null, _culture);
        var val = Assert.IsType<double>(result);
        Assert.Equal(1.0, val, 3);
    }
}
