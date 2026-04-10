using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Developer diagnostics window: live log tailing, localization probing, sample
/// data injection, and pcap replay hooks for the combat pipeline.
///
/// Port notes:
/// - WPF used a <c>ListBoxItem</c> container style to paint the row background
///   from <c>Level</c>. Avalonia has no direct equivalent; the data-driven
///   background now lives on the <c>Border</c> inside the DataTemplate.
/// - <c>VirtualizingPanel.*</c> attached properties removed — Avalonia's
///   default <c>VirtualizingStackPanel</c> is good enough for this window.
/// - <c>Dispatcher.BeginInvoke</c> becomes <c>Dispatcher.UIThread.Post</c>.
/// </summary>
public partial class DebugView : Window
{
    private bool _isAutoScrollPending;

    public DebugView()
    {
        InitializeComponent();
    }

    public DebugView(DebugFunctions debugFunctions)
        : this()
    {
        DataContext = debugFunctions;
        debugFunctions.LogAdded += OnLogAdded;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnLogAdded(object? sender, EventArgs e)
    {
        if (DataContext is DebugFunctions debugFunctions && debugFunctions.AutoScrollEnabled && !_isAutoScrollPending)
        {
            _isAutoScrollPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    if (this.FindControl<ListBox>("LogListBox") is { } listBox && listBox.ItemCount > 0)
                    {
                        listBox.ScrollIntoView(listBox.ItemCount - 1);
                    }
                }
                finally
                {
                    _isAutoScrollPending = false;
                }
            }, DispatcherPriority.Background);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is DebugFunctions debugFunctions)
        {
            debugFunctions.LogAdded -= OnLogAdded;

            // MEMORY LEAK FIX: Call Dispose() to clean up event subscriptions and timers.
            debugFunctions.Dispose();
        }

        base.OnClosed(e);
    }
}

/// <summary>
/// Converter for log level to brush color with caching for better performance.
/// </summary>
public class LogLevelToBrushConverter : IValueConverter
{
    public static readonly LogLevelToBrushConverter Instance = new();

    private static readonly Dictionary<LogLevel, SolidColorBrush> BrushCache = new()
    {
        [LogLevel.Trace] = new SolidColorBrush(Colors.Gray),
        [LogLevel.Debug] = new SolidColorBrush(Colors.DarkBlue),
        [LogLevel.Information] = new SolidColorBrush(Colors.Green),
        [LogLevel.Warning] = new SolidColorBrush(Colors.Orange),
        [LogLevel.Error] = new SolidColorBrush(Colors.Red),
        [LogLevel.Critical] = new SolidColorBrush(Colors.DarkRed),
    };

    private static readonly SolidColorBrush DefaultBrush = new(Colors.Black);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogLevel level && BrushCache.TryGetValue(level, out var brush))
        {
            return brush;
        }

        return DefaultBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

/// <summary>
/// Converter for log level to background color with caching for better performance.
/// </summary>
public class LogLevelToBackgroundConverter : IValueConverter
{
    public static readonly LogLevelToBackgroundConverter Instance = new();

    private static readonly Dictionary<LogLevel, SolidColorBrush> BackgroundCache = new()
    {
        [LogLevel.Error] = new SolidColorBrush(Color.FromRgb(255, 245, 245)),
        [LogLevel.Critical] = new SolidColorBrush(Color.FromRgb(255, 235, 235)),
        [LogLevel.Warning] = new SolidColorBrush(Color.FromRgb(255, 250, 240)),
    };

    private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogLevel level && BackgroundCache.TryGetValue(level, out var brush))
        {
            return brush;
        }

        return TransparentBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

/// <summary>
/// Converter for exception to string with null handling.
/// </summary>
public class ExceptionToStringConverter : IValueConverter
{
    public static readonly ExceptionToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Exception ex)
        {
            return $"Exception: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
