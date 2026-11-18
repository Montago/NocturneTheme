using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Nocturne.Wpf.Controls;

public enum FlyoutPosition
{
    Right,
    Left,
    Top,
    Bottom,
}

/// <summary>
/// A glass panel that slides in from an edge of the window (220 ms), with a header and a close
/// button. Put flyouts in <see cref="NocturneWindow.Flyouts"/> (usually a Grid) and open them with
/// <see cref="IsOpen"/>; Escape closes an open one. Opening moves keyboard focus to the first field in the
/// flyout, and closing gives it back to the element that had it.
/// </summary>
[TemplatePart(Name = PartClose, Type = typeof(Button))]
public class Flyout : HeaderedContentControl
{
    private const string PartClose = "PART_Close";

    private readonly TranslateTransform slide = new();
    private DispatcherTimer? autoClose;
    private Button? close;
    private IInputElement? returnFocus;

    static Flyout()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Flyout), new FrameworkPropertyMetadata(typeof(Flyout)));
    }

    public Flyout()
    {
        RenderTransform = slide;
        Visibility = Visibility.Collapsed;
        Loaded += (_, _) => Apply(IsOpen, animate: false);
        Unloaded += (_, _) => autoClose?.Stop();
    }

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(Flyout),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((Flyout)d).Apply((bool)e.NewValue, animate: true)));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
        nameof(Position), typeof(FlyoutPosition), typeof(Flyout), new PropertyMetadata(FlyoutPosition.Right));

    /// <summary>The window edge the flyout slides in from (Right by default).</summary>
    public FlyoutPosition Position
    {
        get => (FlyoutPosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable), typeof(bool), typeof(Flyout), new PropertyMetadata(true));

    /// <summary>Show the close button and close on Escape (true by default).</summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public static readonly DependencyProperty IsAutoCloseEnabledProperty = DependencyProperty.Register(
        nameof(IsAutoCloseEnabled), typeof(bool), typeof(Flyout), new PropertyMetadata(false, (d, _) => ((Flyout)d).RestartAutoClose()));

    /// <summary>
    /// Close <see cref="AutoCloseInterval"/> milliseconds after opening. The timer waits while the
    /// mouse is over the flyout and starts again when it leaves.
    /// </summary>
    public bool IsAutoCloseEnabled
    {
        get => (bool)GetValue(IsAutoCloseEnabledProperty);
        set => SetValue(IsAutoCloseEnabledProperty, value);
    }

    public static readonly DependencyProperty AutoCloseIntervalProperty = DependencyProperty.Register(
        nameof(AutoCloseInterval), typeof(long), typeof(Flyout), new PropertyMetadata(5000L, (d, _) => ((Flyout)d).RestartAutoClose()));

    public long AutoCloseInterval
    {
        get => (long)GetValue(AutoCloseIntervalProperty);
        set => SetValue(AutoCloseIntervalProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (close != null)
        {
            close.Click -= OnCloseClick;
        }
        base.OnApplyTemplate();
        close = GetTemplateChild(PartClose) as Button;
        if (close != null)
        {
            close.Click += OnCloseClick;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => IsOpen = false;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && IsClosable && IsOpen)
        {
            IsOpen = false;
            e.Handled = true;
        }
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        autoClose?.Stop();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        RestartAutoClose();
    }

    private void RestartAutoClose()
    {
        autoClose?.Stop();
        if (!IsOpen || !IsAutoCloseEnabled || AutoCloseInterval <= 0 || IsMouseOver)
        {
            return;
        }

        if (autoClose == null)
        {
            autoClose = new DispatcherTimer();
            autoClose.Tick += (_, _) =>
            {
                autoClose.Stop();
                IsOpen = false;
            };
        }
        autoClose.Interval = TimeSpan.FromMilliseconds(AutoCloseInterval);
        autoClose.Start();
    }

    private void Apply(bool open, bool animate)
    {
        RestartAutoClose();
        if (animate)
        {
            if (open)
            {
                ReceiveFocus();
            }
            else
            {
                GiveFocusBack();
            }
        }

        var vertical = Position is FlyoutPosition.Top or FlyoutPosition.Bottom;
        var axis = vertical ? TranslateTransform.YProperty : TranslateTransform.XProperty;
        var otherAxis = vertical ? TranslateTransform.XProperty : TranslateTransform.YProperty;
        slide.BeginAnimation(otherAxis, null);
        slide.SetValue(otherAxis, 0.0);

        if (open && Visibility != Visibility.Visible)
        {
            Visibility = Visibility.Visible;
            if (IsLoaded)
            {
                // Measure now, so a flyout without a fixed size knows how far it has to travel.
                UpdateLayout();
            }
        }

        var hidden = HiddenOffset(vertical);
        // Like the theme's other transitions, the slide ignores Windows' animation effects setting,
        // which is off in most Remote Desktop sessions.
        if (!animate || !IsLoaded)
        {
            slide.BeginAnimation(axis, null);
            slide.SetValue(axis, open ? 0.0 : hidden);
            Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        var animation = new DoubleAnimation
        {
            From = open ? hidden : (double)slide.GetValue(axis),
            To = open ? 0 : hidden,
            Duration = TimeSpan.FromMilliseconds(220),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        if (!open)
        {
            animation.Completed += (_, _) =>
            {
                if (!IsOpen)
                {
                    Visibility = Visibility.Collapsed;
                }
            };
        }
        slide.BeginAnimation(axis, animation);
    }

    /// <summary>
    /// Moves keyboard focus into the flyout once it is laid out: to the first field of its content, or
    /// the close button when the content has none. The element that had focus gets it back on closing.
    /// </summary>
    private void ReceiveFocus()
    {
        if (!IsKeyboardFocusWithin)
        {
            returnFocus = Keyboard.FocusedElement;
        }
        Dispatcher.BeginInvoke(() =>
        {
            if (!IsOpen || IsKeyboardFocusWithin)
            {
                return;
            }
            var target = (Content as DependencyObject is { } content ? FirstFocusable(content) : null) ?? FirstFocusable(this, skipSelf: true);
            target?.Focus();
        }, DispatcherPriority.Loaded);
    }

    private void GiveFocusBack()
    {
        var back = returnFocus;
        returnFocus = null;
        if (IsKeyboardFocusWithin && back is UIElement { IsVisible: true, IsEnabled: true } element)
        {
            element.Focus();
        }
    }

    private static UIElement? FirstFocusable(DependencyObject parent, bool skipSelf = false)
    {
        if (!skipSelf && parent is UIElement { Focusable: true, IsEnabled: true, IsVisible: true } self && KeyboardNavigation.GetIsTabStop(self))
        {
            return self;
        }
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            if (VisualTreeHelper.GetChild(parent, i) is UIElement { IsVisible: true } child && FirstFocusable(child) is { } found)
            {
                return found;
            }
        }
        return null;
    }

    /// <summary>The offset that puts the flyout, shadow included, just outside its edge.</summary>
    private double HiddenOffset(bool vertical)
    {
        var size = vertical ? ActualHeight : ActualWidth;
        if (size <= 0)
        {
            var fixedSize = vertical ? Height : Width;
            size = double.IsNaN(fixedSize) ? (vertical ? 120 : 300) : fixedSize;
        }

        size += 40;
        return Position is FlyoutPosition.Right or FlyoutPosition.Bottom ? size : -size;
    }
}
