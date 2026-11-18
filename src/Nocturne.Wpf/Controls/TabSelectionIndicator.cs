using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Nocturne.Wpf.Controls;

/// <summary>
/// Draws the selected tab's pill and its light stripe behind the tab headers of the segmented
/// TabControl, and moves them when the selection changes: the pill glides to the new tab, and the
/// stripe stretches towards it with its leading edge first, then pulls its trailing edge after,
/// so it rolls across. Put it in the TabControl template under the panel that hosts the items;
/// while it is there the tabs leave out their own pill and stripe (<see cref="DrawsSelectionProperty"/>).
/// </summary>
public class TabSelectionIndicator : FrameworkElement
{
    private const double Duration = 320; // ms, the whole move
    private const double StripeWidth = 16;
    private const double StripeHeight = 3;
    private const double StripeBottom = 3;
    private const double PillRadius = 8;

    private TabControl? owner;
    private Rect pill = Rect.Empty;   // where the pill rests (or was last drawn while gliding)
    private Rect fromPill;
    private Rect toPill;
    private TimeSpan? start;
    private double? progress;         // 0..1 while gliding, null at rest

    static TabSelectionIndicator()
    {
        IsHitTestVisibleProperty.OverrideMetadata(typeof(TabSelectionIndicator), new FrameworkPropertyMetadata(false));
    }

    public TabSelectionIndicator()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty PillBrushProperty = DependencyProperty.Register(
        nameof(PillBrush), typeof(Brush), typeof(TabSelectionIndicator),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>The fill of the selected tab.</summary>
    public Brush? PillBrush
    {
        get => (Brush?)GetValue(PillBrushProperty);
        set => SetValue(PillBrushProperty, value);
    }

    public static readonly DependencyProperty StripeBrushProperty = DependencyProperty.Register(
        nameof(StripeBrush), typeof(Brush), typeof(TabSelectionIndicator),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>The stripe under the selected tab's header.</summary>
    public Brush? StripeBrush
    {
        get => (Brush?)GetValue(StripeBrushProperty);
        set => SetValue(StripeBrushProperty, value);
    }

    private static readonly DependencyPropertyKey DrawsSelectionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "DrawsSelection", typeof(bool), typeof(TabSelectionIndicator),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// True on a TabControl (and, inherited, on its tabs) while an indicator draws its selection,
    /// so the TabItem template leaves out its own pill and stripe.
    /// </summary>
    public static readonly DependencyProperty DrawsSelectionProperty = DrawsSelectionPropertyKey.DependencyProperty;

    public static bool GetDrawsSelection(DependencyObject d) => (bool)d.GetValue(DrawsSelectionProperty);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        owner = TemplatedParent as TabControl ?? FindOwner();
        if (owner == null)
        {
            return;
        }
        owner.SetValue(DrawsSelectionPropertyKey, true);
        owner.SelectionChanged += OnSelectionChanged;
        LayoutUpdated += OnLayoutUpdated;
        pill = Target() ?? Rect.Empty;
        InvalidateVisual();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (owner != null)
        {
            owner.SelectionChanged -= OnSelectionChanged;
            owner.ClearValue(DrawsSelectionPropertyKey);
        }
        LayoutUpdated -= OnLayoutUpdated;
        StopGliding();
        owner = null;
    }

    private TabControl? FindOwner()
    {
        for (DependencyObject? d = this; d != null; d = VisualTreeHelper.GetParent(d))
        {
            if (d is TabControl tabs)
            {
                return tabs;
            }
        }
        return null;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Bubbles up from lists inside the pages too; only the tabs' own selection counts
        if (ReferenceEquals(e.OriginalSource, owner))
        {
            // The new tab's position is known after layout, which runs first; until then the
            // pill stays where it is (OnLayoutUpdated must not jump it to the new tab)
            gliding = true;
            Dispatcher.BeginInvoke(Glide, DispatcherPriority.Loaded);
        }
    }

    private bool gliding;

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
            pill = Rect.Empty;
            InvalidateVisual();
            return;
        }
        // Runs whatever Windows' animation effects setting says, like the theme's other transitions
        // (that setting is off in most Remote Desktop sessions)
        if (pill.IsEmpty || pill == target || !IsVisible)
        {
            StopGliding();
            pill = target;
            InvalidateVisual();
            return;
        }
        // From wherever the pill is drawn now, so a click during a glide turns it smoothly
        fromPill = pill;
        toPill = target;
        start = null;
        if (progress == null)
        {
            CompositionTarget.Rendering += OnRendering;
        }
        progress = 0;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var now = ((RenderingEventArgs)e).RenderingTime;
        start ??= now;
        progress = Math.Min(1, (now - start.Value).TotalMilliseconds / Duration);
        if (progress >= 1)
        {
            StopGliding();
            pill = toPill;
        }
        InvalidateVisual();
    }

    private void StopGliding()
    {
        if (progress != null)
        {
            CompositionTarget.Rendering -= OnRendering;
            progress = null;
        }
        start = null;
        gliding = false;
    }

    /// <summary>The selected tab's bounds in this element, or null when nothing is selected or laid out.</summary>
    private Rect? Target()
    {
        if (owner?.ItemContainerGenerator.ContainerFromItem(owner.SelectedItem) is not TabItem item
            || !item.IsVisible || item.ActualWidth <= 0 || !owner.IsAncestorOf(item) || !owner.IsAncestorOf(this))
        {
            return null;
        }
        var origin = item.TranslatePoint(new Point(0, 0), this);
        return new Rect(origin, new Size(item.ActualWidth, item.ActualHeight));
    }

    protected override void OnRender(DrawingContext dc)
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
            if (pill.IsEmpty)
            {
                return;
            }
            drawn = pill;
            (stripeLeft, stripeRight) = StripeOf(pill);
        }

        if (PillBrush != null)
        {
            dc.DrawRoundedRectangle(PillBrush, null, drawn, PillRadius, PillRadius);
        }
        if (StripeBrush != null && stripeRight > stripeLeft)
        {
            var stripe = new Rect(stripeLeft, drawn.Bottom - StripeBottom - StripeHeight, stripeRight - stripeLeft, StripeHeight);
            dc.DrawRoundedRectangle(StripeBrush, null, stripe, StripeHeight / 2, StripeHeight / 2);
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
