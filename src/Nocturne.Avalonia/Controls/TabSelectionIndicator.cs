using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// Draws the selected tab's pill and its light stripe behind the tab headers of the segmented
/// TabControl, and moves them when the selection changes: the pill glides to the new tab, and the
/// stripe stretches towards it with its leading edge first, then pulls its trailing edge after,
/// so it rolls across. Put it in the TabControl template under the panel that hosts the items;
/// while it is there the tabs leave out their own pill and stripe (<see cref="DrawsSelectionProperty"/>).
/// </summary>
public class TabSelectionIndicator : Control
{
    private const double Duration = 320; // ms, the whole move
    private const double StripeWidth = 16;
    private const double StripeHeight = 3;
    private const double StripeBottom = 3;
    private const double PillRadius = 8;

    public static readonly StyledProperty<IBrush?> PillBrushProperty =
        AvaloniaProperty.Register<TabSelectionIndicator, IBrush?>(nameof(PillBrush));

    public static readonly StyledProperty<IBrush?> StripeBrushProperty =
        AvaloniaProperty.Register<TabSelectionIndicator, IBrush?>(nameof(StripeBrush));

    /// <summary>
    /// True on a TabControl (and, inherited, on its tabs) while an indicator draws its selection,
    /// so the TabItem template leaves out its own pill and stripe.
    /// </summary>
    public static readonly AttachedProperty<bool> DrawsSelectionProperty =
        AvaloniaProperty.RegisterAttached<TabSelectionIndicator, Control, bool>("DrawsSelection", inherits: true);

    private TabControl? owner;
    private Rect? pill;              // where the pill rests (or was last drawn while gliding)
    private Rect fromPill;
    private Rect toPill;
    private TimeSpan? start;
    private double? progress;        // 0..1 while gliding, null at rest
    private bool gliding;

    static TabSelectionIndicator()
    {
        IsHitTestVisibleProperty.OverrideDefaultValue<TabSelectionIndicator>(false);
        AffectsRender<TabSelectionIndicator>(PillBrushProperty, StripeBrushProperty);
    }

    /// <summary>The fill of the selected tab.</summary>
    public IBrush? PillBrush
    {
        get => GetValue(PillBrushProperty);
        set => SetValue(PillBrushProperty, value);
    }

    /// <summary>The stripe under the selected tab's header.</summary>
    public IBrush? StripeBrush
    {
        get => GetValue(StripeBrushProperty);
        set => SetValue(StripeBrushProperty, value);
    }

    public static bool GetDrawsSelection(Control c) => c.GetValue(DrawsSelectionProperty);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        owner = TemplatedParent as TabControl ?? this.FindAncestorOfType<TabControl>();
        if (owner == null)
            return;
        owner.SetValue(DrawsSelectionProperty, true);
        owner.SelectionChanged += OnSelectionChanged;
        LayoutUpdated += OnLayoutUpdated;
        pill = Target();
        InvalidateVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (owner != null)
        {
            owner.SelectionChanged -= OnSelectionChanged;
            owner.ClearValue(DrawsSelectionProperty);
        }
        LayoutUpdated -= OnLayoutUpdated;
        progress = null;
        gliding = false;
        owner = null;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Bubbles up from lists inside the pages too; only the tabs' own selection counts
        if (!ReferenceEquals(e.Source, owner))
            return;
        // The new tab's position is known after layout; until then the pill stays where it is
        gliding = true;
        Dispatcher.UIThread.Post(Glide, DispatcherPriority.Loaded);
    }

    // A resize or a new tab moves the target: follow it at once unless a glide is on its way.
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (!gliding && progress == null && Target() is { } target && target != pill)
        {
            pill = target;
            InvalidateVisual();
        }
    }

    private void Glide()
    {
        if (Target() is not { } target)
        {
            gliding = false;
            pill = null;
            InvalidateVisual();
            return;
        }
        // Runs whatever the system's animation setting says, like the theme's other transitions
        if (pill is not { } current || current == target || !IsEffectivelyVisible)
        {
            progress = null;
            gliding = false;
            pill = target;
            InvalidateVisual();
            return;
        }
        // From wherever the pill is drawn now, so a click during a glide turns it smoothly
        fromPill = current;
        toPill = target;
        start = null;
        var running = progress != null;
        progress = 0;
        if (!running)
            TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
    }

    private void OnFrame(TimeSpan now)
    {
        if (progress == null)
            return;
        start ??= now;
        progress = Math.Min(1, (now - start.Value).TotalMilliseconds / Duration);
        if (progress >= 1)
        {
            progress = null;
            gliding = false;
            pill = toPill;
        }
        else
        {
            TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
        }
        InvalidateVisual();
    }

    /// <summary>The selected tab's bounds in this control, or null when nothing is selected or laid out.</summary>
    private Rect? Target()
    {
        if (owner?.ContainerFromIndex(owner.SelectedIndex) is not TabItem item
            || !item.IsVisible || item.Bounds.Width <= 0 || item.TranslatePoint(default, this) is not { } origin)
        {
            return null;
        }
        return new Rect(origin, item.Bounds.Size);
    }

    public override void Render(DrawingContext context)
    {
        Rect drawn;
        double stripeLeft, stripeRight;
        if (progress is { } t)
        {
            drawn = Lerp(fromPill, toPill, EaseOut(t));

            // The stripe: the edge on the side of the move leads, the other follows
            var (fromLeft, fromRight) = StripeOf(fromPill);
            var (toLeft, toRight) = StripeOf(toPill);
            var rightward = toPill.X >= fromPill.X;
            var lead = EaseOut(Math.Clamp(t / 0.65, 0, 1));
            var trail = EaseInOut(Math.Clamp((t - 0.3) / 0.7, 0, 1));
            stripeLeft = Lerp(fromLeft, toLeft, rightward ? trail : lead);
            stripeRight = Lerp(fromRight, toRight, rightward ? lead : trail);
            pill = drawn;
        }
        else
        {
            if (pill is not { } rest)
                return;
            drawn = rest;
            (stripeLeft, stripeRight) = StripeOf(rest);
        }

        if (PillBrush != null)
            context.DrawRectangle(PillBrush, null, drawn, PillRadius, PillRadius);
        if (StripeBrush != null && stripeRight > stripeLeft)
        {
            var stripe = new Rect(stripeLeft, drawn.Bottom - StripeBottom - StripeHeight, stripeRight - stripeLeft, StripeHeight);
            context.DrawRectangle(StripeBrush, null, stripe, StripeHeight / 2, StripeHeight / 2);
        }
    }

    private static (double Left, double Right) StripeOf(Rect tab)
    {
        var centre = tab.X + tab.Width / 2;
        return (centre - StripeWidth / 2, centre + StripeWidth / 2);
    }

    private static double EaseOut(double t) => 1 - Math.Pow(1 - t, 3);

    private static double EaseInOut(double t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private static Rect Lerp(Rect a, Rect b, double t) =>
        new(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t), Lerp(a.Width, b.Width, t), Lerp(a.Height, b.Height, t));
}
