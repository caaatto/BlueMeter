using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BlueMeter.WPF.ViewModels;

/// <summary>
/// Test ViewModel for OxyPlot integration
/// </summary>
public class ChartTestViewModel
{
    public PlotModel PlotModel { get; set; }

    public ChartTestViewModel()
    {
        // Create PlotModel
        PlotModel = new PlotModel
        {
            Title = "DPS Trend - Real-Time Test",
            Background = OxyColors.Transparent,
            PlotAreaBorderColor = OxyColors.Gray,
            TextColor = OxyColors.White,
            TitleColor = OxyColors.White
        };

        // Add axes
        PlotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "DPS",
            TitleColor = OxyColors.White,
            TextColor = OxyColors.White,
            TicklineColor = OxyColors.Gray,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255)
        });

        PlotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Time (seconds)",
            TitleColor = OxyColors.White,
            TextColor = OxyColors.White,
            TicklineColor = OxyColors.Gray,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255)
        });

        // Create sample DPS data
        var lineSeries = new LineSeries
        {
            Title = "Player DPS",
            Color = OxyColors.CornflowerBlue,
            StrokeThickness = 2,
            MarkerType = MarkerType.None
        };

        // Add sample data points
        var dpsValues = new[] { 0, 1000, 2500, 3200, 4500, 5800, 7200, 6500, 5200, 3800, 2100, 1200, 500, 100, 0 };
        for (int i = 0; i < dpsValues.Length; i++)
        {
            lineSeries.Points.Add(new DataPoint(i, dpsValues[i]));
        }

        PlotModel.Series.Add(lineSeries);
    }
}
