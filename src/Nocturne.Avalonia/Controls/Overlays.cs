using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nocturne.Avalonia.Controls;

public enum MessageDialogResult
{
    /// <summary>Closed with Escape (or <see cref="NocturneDialog.Close"/>) without choosing.</summary>
    Canceled,

    /// <summary>The main button (<see cref="NocturneDialog.AffirmativeText"/>).</summary>
    Affirmative,

    /// <summary>The second button (<see cref="NocturneDialog.NegativeText"/>).</summary>
    Negative,
}

/// <summary>
/// A dialog inside the window: a glass card over a dimmed page, with a title, the content and
/// its buttons at the bottom right. Show it with <see cref="NocturneWindow.ShowDialogAsync"/>
/// (or <see cref="NocturneWindow.ShowMessageAsync"/> for a plain message); Escape cancels it.
/// </summary>
[TemplatePart(PartAffirmative, typeof(Button))]
[TemplatePart(PartNegative, typeof(Button))]
public class NocturneDialog : ContentControl
{
    private const string PartAffirmative = "PART_Affirmative";
    private const string PartNegative = "PART_Negative";

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NocturneDialog, string?>(nameof(Title));

    public static readonly StyledProperty<string> AffirmativeTextProperty =
        AvaloniaProperty.Register<NocturneDialog, string>(nameof(AffirmativeText), "OK");

    public static readonly StyledProperty<string?> NegativeTextProperty =
        AvaloniaProperty.Register<NocturneDialog, string?>(nameof(NegativeText));

    public static readonly StyledProperty<ControlTheme?> AffirmativeButtonThemeProperty =
        AvaloniaProperty.Register<NocturneDialog, ControlTheme?>(nameof(AffirmativeButtonTheme));

    private TaskCompletionSource<MessageDialogResult>? completion;
    private Button? affirmative;
    private Button? negative;

    protected override Type StyleKeyOverride => typeof(NocturneDialog);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Text of the main (primary) button.</summary>
    public string AffirmativeText
    {
        get => GetValue(AffirmativeTextProperty);
        set => SetValue(AffirmativeTextProperty, value);
    }

    /// <summary>Text of the second button; the button is hidden while this is null.</summary>
    public string? NegativeText
    {
        get => GetValue(NegativeTextProperty);
        set => SetValue(NegativeTextProperty, value);
    }

    /// <summary>Theme of the main button (Nocturne.Styles.Button.Primary when null; Nocturne.Styles.Button.Danger for a deletion).</summary>
    public ControlTheme? AffirmativeButtonTheme
    {
        get => GetValue(AffirmativeButtonThemeProperty);
        set => SetValue(AffirmativeButtonThemeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (affirmative != null)
            affirmative.Click -= OnAffirmativeClick;
        if (negative != null)
            negative.Click -= OnNegativeClick;

        base.OnApplyTemplate(e);

        affirmative = e.NameScope.Find<Button>(PartAffirmative);
        negative = e.NameScope.Find<Button>(PartNegative);
        if (affirmative != null)
        {
            affirmative.Click += OnAffirmativeClick;
            ApplyAffirmativeTheme();
            Dispatcher.UIThread.Post(() => affirmative?.Focus(NavigationMethod.Tab), DispatcherPriority.Input);
        }
        if (negative != null)
            negative.Click += OnNegativeClick;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == AffirmativeButtonThemeProperty)
            ApplyAffirmativeTheme();
    }

    private void ApplyAffirmativeTheme()
    {
        if (affirmative != null)
            affirmative.Theme = AffirmativeButtonTheme ?? (this.TryFindResource("Nocturne.Styles.Button.Primary", out var t) ? t as ControlTheme : null);
    }

    private void OnAffirmativeClick(object? sender, RoutedEventArgs e) => Close(MessageDialogResult.Affirmative);

    private void OnNegativeClick(object? sender, RoutedEventArgs e) => Close(MessageDialogResult.Negative);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            Close(MessageDialogResult.Canceled);
            e.Handled = true;
        }
    }

    internal Task<MessageDialogResult> WaitForResultAsync()
    {
        completion = new TaskCompletionSource<MessageDialogResult>();
        return completion.Task;
    }

    /// <summary>Closes the dialog with the given result.</summary>
    public void Close(MessageDialogResult result) => completion?.TrySetResult(result);
}

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
/// <see cref="IsOpen"/>; Escape closes an open one. Opening moves keyboard focus to the first field
/// in the flyout, and closing gives it back to the element that had it. (Not Avalonia's
/// <c>Flyout</c>, the popup attached to a control: this is a panel inside the window.)
/// </summary>
[TemplatePart(PartClose, typeof(Button))]
public class Flyout : HeaderedContentControl
{
    private const string PartClose = "PART_Close";

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<FlyoutPosition> PositionProperty =
        AvaloniaProperty.Register<Flyout, FlyoutPosition>(nameof(Position));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(IsClosable), true);

    public static readonly StyledProperty<bool> IsAutoCloseEnabledProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(IsAutoCloseEnabled));

    public static readonly StyledProperty<long> AutoCloseIntervalProperty =
        AvaloniaProperty.Register<Flyout, long>(nameof(AutoCloseInterval), 5000);

    private readonly TranslateTransform slide = new();
    private DispatcherTimer? autoClose;
    private Button? close;
    private IInputElement? returnFocus;
    private int slideVersion;

    public Flyout()
    {
        RenderTransform = slide;
        IsVisible = false;
    }

    protected override Type StyleKeyOverride => typeof(Flyout);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The window edge the flyout slides in from (Right by default).</summary>
    public FlyoutPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>Show the close button and close on Escape (true by default).</summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Close <see cref="AutoCloseInterval"/> milliseconds after opening. The timer waits while the
    /// pointer is over the flyout and starts again when it leaves.
    /// </summary>
    public bool IsAutoCloseEnabled
    {
        get => GetValue(IsAutoCloseEnabledProperty);
        set => SetValue(IsAutoCloseEnabledProperty, value);
    }

    public long AutoCloseInterval
    {
        get => GetValue(AutoCloseIntervalProperty);
        set => SetValue(AutoCloseIntervalProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (close != null)
            close.Click -= OnCloseClick;
        base.OnApplyTemplate(e);
        close = e.NameScope.Find<Button>(PartClose);
        if (close != null)
            close.Click += OnCloseClick;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Apply(IsOpen, animate: false);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        autoClose?.Stop();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOpenProperty)
            Apply(change.GetNewValue<bool>(), animate: true);
        else if (change.Property == IsAutoCloseEnabledProperty || change.Property == AutoCloseIntervalProperty)
            RestartAutoClose();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => IsOpen = false;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && IsClosable && IsOpen)
        {
            IsOpen = false;
            e.Handled = true;
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        autoClose?.Stop();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        RestartAutoClose();
    }

    private void RestartAutoClose()
    {
        autoClose?.Stop();
        if (!IsOpen || !IsAutoCloseEnabled || AutoCloseInterval <= 0 || IsPointerOver)
            return;
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

    private async void Apply(bool open, bool animate)
    {
        RestartAutoClose();
        var attached = TopLevel.GetTopLevel(this) != null;
        if (animate)
        {
            if (open)
                ReceiveFocus();
            else
                GiveFocusBack();
        }

        var vertical = Position is FlyoutPosition.Top or FlyoutPosition.Bottom;
        var axis = vertical ? TranslateTransform.YProperty : TranslateTransform.XProperty;
        slide.SetValue(vertical ? TranslateTransform.XProperty : TranslateTransform.YProperty, 0.0);

        if (open && !IsVisible)
        {
            IsVisible = true;
            // Measure now, so a flyout without a fixed size knows how far it has to travel.
            if (attached)
                UpdateLayout();
        }

        var hidden = HiddenOffset(vertical);
        var version = ++slideVersion;
        // Like the theme's other transitions, the slide ignores the system's animation setting.
        if (!animate || !attached)
        {
            slide.SetValue(axis, open ? 0.0 : hidden);
            IsVisible = open;
            return;
        }

        // Driven frame by frame: Avalonia's Animation runs only on a Visual, and the slide is a transform.
        var from = open ? hidden : slide.GetValue(axis);
        var to = open ? 0.0 : hidden;
        slide.SetValue(axis, from);
        await Slide(axis, from, to, version);
        if (!open && !IsOpen && version == slideVersion)
            IsVisible = false;
    }

    /// <summary>Moves the slide from one offset to another in 220 ms (cubic ease-out); a newer slide stops it.</summary>
    private Task Slide(AvaloniaProperty<double> axis, double from, double to, int version)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top == null)
        {
            slide.SetValue(axis, to);
            return Task.CompletedTask;
        }

        var done = new TaskCompletionSource();
        TimeSpan? start = null;
        void Frame(TimeSpan now)
        {
            if (version != slideVersion)
            {
                done.TrySetResult();
                return;
            }
            start ??= now;
            var t = Math.Clamp((now - start.Value).TotalMilliseconds / 220, 0, 1);
            var eased = 1 - Math.Pow(1 - t, 3);
            slide.SetValue(axis, from + (to - from) * eased);
            if (t < 1)
                top.RequestAnimationFrame(Frame);
            else
                done.TrySetResult();
        }
        top.RequestAnimationFrame(Frame);
        return done.Task;
    }

    /// <summary>
    /// Moves keyboard focus into the flyout once it is laid out: to the first field of its content, or
    /// the close button when the content has none. The element that had focus gets it back on closing.
    /// </summary>
    private void ReceiveFocus()
    {
        var focusManager = TopLevel.GetTopLevel(this)?.FocusManager;
        if (!IsKeyboardFocusWithin)
            returnFocus = focusManager?.GetFocusedElement();
        Dispatcher.UIThread.Post(() =>
        {
            if (!IsOpen || IsKeyboardFocusWithin)
                return;
            var target = (Content as Visual is { } content ? FirstFocusable(content) : null) ?? FirstFocusable(this, skipSelf: true);
            target?.Focus(NavigationMethod.Tab);
        }, DispatcherPriority.Loaded);
    }

    private void GiveFocusBack()
    {
        var back = returnFocus;
        returnFocus = null;
        if (IsKeyboardFocusWithin && back is InputElement { IsEffectivelyVisible: true, IsEffectivelyEnabled: true } element)
            element.Focus(); // no navigation method: giving focus back is not the user moving into the control
    }

    private static InputElement? FirstFocusable(Visual parent, bool skipSelf = false)
    {
        if (!skipSelf && parent is InputElement { Focusable: true, IsEffectivelyEnabled: true, IsVisible: true } self && KeyboardNavigation.GetIsTabStop(self))
            return self;
        foreach (var child in parent.GetVisualChildren())
        {
            if (child.IsVisible && FirstFocusable(child) is { } found)
                return found;
        }
        return null;
    }

    /// <summary>The offset that puts the flyout, shadow included, just outside its edge.</summary>
    private double HiddenOffset(bool vertical)
    {
        var size = vertical ? Bounds.Height : Bounds.Width;
        if (size <= 0)
        {
            var fixedSize = vertical ? Height : Width;
            size = double.IsNaN(fixedSize) ? (vertical ? 120 : 300) : fixedSize;
        }
        size += 40;
        return Position is FlyoutPosition.Right or FlyoutPosition.Bottom ? size : -size;
    }
}
