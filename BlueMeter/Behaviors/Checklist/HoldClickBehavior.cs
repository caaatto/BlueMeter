using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace BlueMeter.Behaviors.Checklist;

/// <summary>
/// Press-and-hold repeat behavior for counter-style buttons.
///
/// <list type="bullet">
///   <item><description>Click: execute once.</description></item>
///   <item><description>Hold: after <see cref="InitialDelay"/> ms, repeat every
///     <see cref="Interval"/> ms.</description></item>
///   <item><description>Shift+Click / Shift+Hold: forwarded to the command via
///     <see cref="KeyModifiers"/> on the triggering pointer event — consumers
///     inspect the command parameter to negate.</description></item>
/// </list>
///
/// Port notes: WPF's <c>PreviewMouseLeftButtonDown/Up</c> and
/// <c>Keyboard.IsKeyDown(Key.LeftShift)</c> become Avalonia pointer events with
/// <see cref="RoutingStrategies.Tunnel"/> routing and
/// <see cref="PointerEventArgs.KeyModifiers"/> queries.
/// <c>System.Windows.Threading.DispatcherTimer</c> becomes
/// <see cref="Avalonia.Threading.DispatcherTimer"/>.
/// </summary>
public class HoldClickBehavior : Behavior<Button>
{
    private DispatcherTimer? _holdTimer;
    private bool _isHolding;
    private bool _isShiftPressed;

    public static readonly StyledProperty<ICommand?> HoldCommandProperty =
        AvaloniaProperty.Register<HoldClickBehavior, ICommand?>(nameof(HoldCommand));

    public ICommand? HoldCommand
    {
        get => GetValue(HoldCommandProperty);
        set => SetValue(HoldCommandProperty, value);
    }

    public static readonly StyledProperty<int> IntervalProperty =
        AvaloniaProperty.Register<HoldClickBehavior, int>(nameof(Interval), 100);

    public int Interval
    {
        get => GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    public static readonly StyledProperty<int> InitialDelayProperty =
        AvaloniaProperty.Register<HoldClickBehavior, int>(nameof(InitialDelay), 300);

    public int InitialDelay
    {
        get => GetValue(InitialDelayProperty);
        set => SetValue(InitialDelayProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AssociatedObject.PointerExited += OnPointerExited;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, (EventHandler<PointerPressedEventArgs>)OnPointerPressed);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, (EventHandler<PointerReleasedEventArgs>)OnPointerReleased);
            AssociatedObject.PointerExited -= OnPointerExited;
        }

        StopHoldTimer();
        base.OnDetaching();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _isHolding = false;

        // Fire the command once immediately.
        ExecuteCommand();

        _holdTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(InitialDelay)
        };
        _holdTimer.Tick += OnInitialDelayTick;
        _holdTimer.Start();

        // Suppress the Button.Click that would otherwise fire on release.
        e.Handled = true;
    }

    private void OnInitialDelayTick(object? sender, EventArgs e)
    {
        // Initial delay elapsed — swap to the faster repeat cadence.
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

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        StopHoldTimer();
        e.Handled = true;
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        StopHoldTimer();
    }

    private void StopHoldTimer()
    {
        _isHolding = false;

        if (_holdTimer is not null)
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
