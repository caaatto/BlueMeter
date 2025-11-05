using System.Globalization;
using System.Windows.Media;
using StarResonanceDpsAnalysis.WPF.Converters;

namespace StarResonanceDpsAnalysis.Tests.WpfConverters;

public class ConditionalPercentToColorConverterTests
{
    private readonly ConditionalPercentToColorConverter _converter = new();
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [Theory]
    [InlineData(0, true, 0)]
    [InlineData(50, true, 128)]
    [InlineData(100, true, 255)]
    [InlineData(150, true, 255)] // clamp > 100
    [InlineData(-20, true, 0)] // clamp < 0
    public void Convert_FromPercentInt_Enabled_ComputesAlpha(int percent, bool enabled, int expectedAlpha)
    {
        var result = _converter.Convert([percent, enabled], typeof(Color), Colors.Red, _culture);
        Assert.IsType<Color>(result);
        var color = (Color)result;
        Assert.Equal((byte)expectedAlpha, color.A);
        Assert.Equal(Colors.Red.R, color.R);
        Assert.Equal(Colors.Red.G, color.G);
        Assert.Equal(Colors.Red.B, color.B);
    }

    [Theory]
    [InlineData(0d, true, 0)]
    [InlineData(0.5d, true, 128)]
    [InlineData(1.0d, true, 255)]
    [InlineData(150d, true, 255)]
    [InlineData(-0.1d, true, 0)]
    public void Convert_FromDoubleFactor_Enabled_ComputesAlpha(double factor, bool enabled, int expectedAlpha)
    {
        var result = _converter.Convert([factor, enabled], typeof(Color), Colors.Blue, _culture);
        var color = Assert.IsType<Color>(result);
        Assert.Equal((byte)expectedAlpha, color.A);
        Assert.Equal(Colors.Blue.R, color.R);
        Assert.Equal(Colors.Blue.G, color.G);
        Assert.Equal(Colors.Blue.B, color.B);
    }

    [Fact]
    public void Convert_Disabled_ReturnsOpaqueBaseColor()
    {
        var baseColor = Color.FromArgb(10, 1, 2, 3);
        var result = _converter.Convert([25, false], typeof(Color), baseColor, _culture);
        var color = Assert.IsType<Color>(result);
        Assert.Equal((byte)255, color.A);
        Assert.Equal(baseColor.R, color.R);
        Assert.Equal(baseColor.G, color.G);
        Assert.Equal(baseColor.B, color.B);
    }

    [Theory]
    [InlineData("#112233")]
    [InlineData("Red")]
    public void Convert_StringParameter_ParsesBaseColor(string param)
    {
        var result = _converter.Convert([100, true], typeof(Color), param, _culture);
        var color = Assert.IsType<Color>(result);
        Assert.Equal(255, color.A);
    }

    [Fact]
    public void Convert_NullValues_ReturnsBaseColor()
    {
        var result = _converter.Convert(null, typeof(Color), Colors.Green, _culture);
        var color = Assert.IsType<Color>(result);
        Assert.Equal(Colors.Green, color);
    }
}
