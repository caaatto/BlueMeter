using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;

namespace BlueMeter.WPF.Behaviors.Checklist;

/// <summary>
/// Behavior für Hold-Click auf Counter-Buttons
/// Click: Einmal ausführen
/// Hold: Wiederholt ausführen (alle 100ms)
/// Shift+Click: Negativ ausführen
/// Shift+Hold: Wiederholt negativ ausführen
/// </summary>
public class HoldClickBehavior : Behavior<Button>
{
    private DispatcherTimer? _holdTimer;
    private bool _isHolding;
    private bool _isShiftPressed;

    /// <summary>
    /// Command das beim Hold ausgeführt werden soll (optional, nutzt sonst Button.Command)
    /// </summary>
    public static readonly DependencyProperty HoldCommandProperty =
        DependencyProperty.Register(
            nameof(HoldCommand),
            typeof(ICommand),
            typeof(HoldClickBehavior),
            new PropertyMetadata(null));

    public ICommand? HoldCommand
    {
        get => (ICommand?)GetValue(HoldCommandProperty);
        set => SetValue(HoldCommandProperty, value);
    }

    /// <summary>
    /// Intervall zwischen den Ausführungen (in Millisekunden, Standard: 100)
    /// </summary>
    public static readonly DependencyProperty IntervalProperty =
        DependencyProperty.Register(
            nameof(Interval),
            typeof(int),
            typeof(HoldClickBehavior),
            new PropertyMetadata(100));

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    /// <summary>
    /// Verzögerung bis Hold-Modus startet (in Millisekunden, Standard: 300)
    /// </summary>
    public static readonly DependencyProperty InitialDelayProperty =
        DependencyProperty.Register(
            nameof(InitialDelay),
            typeof(int),
            typeof(HoldClickBehavior),
            new PropertyMetadata(300));

    public int InitialDelay
    {
        get => (int)GetValue(InitialDelayProperty);
        set => SetValue(InitialDelayProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnMouseUp;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null)
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseUp;
            AssociatedObject.MouseLeave -= OnMouseLeave;
        }

        StopHoldTimer();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        _isHolding = false;

        // Erste Ausführung sofort
        ExecuteCommand();

        // Starte Timer für Hold
        _holdTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(InitialDelay)
        };
        _holdTimer.Tick += OnInitialDelayTick;
        _holdTimer.Start();
    }

    private void OnInitialDelayTick(object? sender, EventArgs e)
    {
        // Initial Delay ist vorbei, starte schnellere Wiederholung
        _holdTimer?.Stop();
        _isHolding = true;

        _holdTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Interval)
        };
        _holdTimer.Tick += OnHoldTimerTick;
        _holdTimer.Start();
    }

    private void OnHoldTimerTick(object? sender, EventArgs e)
    {
        if (_isHolding)
        {
            ExecuteCommand();
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        StopHoldTimer();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        StopHoldTimer();
    }

    private void StopHoldTimer()
    {
        _isHolding = false;

        if (_holdTimer != null)
        {
            _holdTimer.Stop();
            _holdTimer.Tick -= OnInitialDelayTick;
            _holdTimer.Tick -= OnHoldTimerTick;
            _holdTimer = null;
        }
    }

    private void ExecuteCommand()
    {
        var command = HoldCommand ?? AssociatedObject?.Command;
        var parameter = AssociatedObject?.CommandParameter;

        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }
}
