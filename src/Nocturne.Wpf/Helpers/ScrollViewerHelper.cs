using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Nocturne.Wpf.Helpers;

/// <summary>
/// Mouse wheel chaining. WPF's ScrollViewer swallows every wheel turn, even when it has nothing
/// to scroll, so a ListBox or DataGrid inside a scrolling page stops the page from scrolling.
/// With <see cref="ChainMouseWheelProperty"/> on (the Nocturne ScrollViewer style sets it), a
/// wheel turn over a scroll viewer with nothing to scroll goes to the nearest outer scroll viewer
/// that can scroll that way. One that has something to scroll keeps its turns, also at its top or
/// bottom, so the page does not move on under a list scrolled to its end.
/// </summary>
public static class ScrollViewerHelper
{
    public static readonly DependencyProperty ChainMouseWheelProperty = DependencyProperty.RegisterAttached(
        "ChainMouseWheel", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(false, OnChainMouseWheelChanged));

    public static bool GetChainMouseWheel(DependencyObject d) => (bool)d.GetValue(ChainMouseWheelProperty);

    public static void SetChainMouseWheel(DependencyObject d, bool value) => d.SetValue(ChainMouseWheelProperty, value);

    private static void OnChainMouseWheelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer viewer)
        {
            viewer.PreviewMouseWheel -= OnPreviewMouseWheel;
            if ((bool)e.NewValue)
            {
                viewer.PreviewMouseWheel += OnPreviewMouseWheel;
            }
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var viewer = (ScrollViewer)sender;

        // Preview events tunnel from the outside in; only the innermost scroll viewer decides.
        if (e.Handled || e.Delta == 0 || Keyboard.Modifiers != ModifierKeys.None
            || !ReferenceEquals(FindScrollViewer(e.OriginalSource as DependencyObject), viewer)
            || viewer.ScrollableHeight > 0.01)
        {
            return;
        }

        e.Handled = true;
        for (var parent = Parent(viewer); parent != null; parent = Parent(parent))
        {
            if (parent is ScrollViewer outer && CanScroll(outer, e.Delta))
            {
                outer.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = outer,
                });
                return;
            }
        }
    }

    /// <summary>
    /// Fades the scroll bars out while the mouse is away (the Nocturne ScrollViewer style turns it
    /// on): they fade in when the mouse enters the scroll viewer or its content scrolls (wheel, keys,
    /// code), and out again a moment after. Their room stays, so the layout does not move. Needs
    /// the template's <c>PART_VerticalScrollBar</c> / <c>PART_HorizontalScrollBar</c>.
    /// </summary>
    public static readonly DependencyProperty AutoHideProperty = DependencyProperty.RegisterAttached(
        "AutoHide", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(false, OnAutoHideChanged));

    public static bool GetAutoHide(DependencyObject d) => (bool)d.GetValue(AutoHideProperty);

    public static void SetAutoHide(DependencyObject d, bool value) => d.SetValue(AutoHideProperty, value);

    private static readonly DependencyProperty HideTimerProperty = DependencyProperty.RegisterAttached(
        "HideTimer", typeof(DispatcherTimer), typeof(ScrollViewerHelper), new PropertyMetadata(null));

    private static readonly Duration Fade = new(TimeSpan.FromMilliseconds(150));
    private static readonly TimeSpan HideAfterLeave = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan HideAfterScroll = TimeSpan.FromMilliseconds(1000);

    private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer viewer)
        {
            return;
        }
        viewer.Loaded -= OnAutoHideLoaded;
        viewer.MouseEnter -= OnAutoHideMouseEnter;
        viewer.MouseLeave -= OnAutoHideMouseLeave;
        viewer.ScrollChanged -= OnAutoHideScrollChanged;
        if ((bool)e.NewValue)
        {
            viewer.Loaded += OnAutoHideLoaded;
            viewer.MouseEnter += OnAutoHideMouseEnter;
            viewer.MouseLeave += OnAutoHideMouseLeave;
            viewer.ScrollChanged += OnAutoHideScrollChanged;
            if (viewer.IsLoaded)
            {
                SetBarsVisible(viewer, viewer.IsMouseOver, animate: false);
            }
        }
        else
        {
            (viewer.GetValue(HideTimerProperty) as DispatcherTimer)?.Stop();
            foreach (var bar in Bars(viewer))
            {
                bar.BeginAnimation(UIElement.OpacityProperty, null);
                bar.ClearValue(UIElement.OpacityProperty);
            }
        }
    }

    private static void OnAutoHideLoaded(object sender, RoutedEventArgs e)
    {
        var viewer = (ScrollViewer)sender;
        SetBarsVisible(viewer, viewer.IsMouseOver, animate: false);
    }

    private static void OnAutoHideMouseEnter(object sender, MouseEventArgs e)
    {
        var viewer = (ScrollViewer)sender;
        (viewer.GetValue(HideTimerProperty) as DispatcherTimer)?.Stop();
        SetBarsVisible(viewer, true, animate: true);
    }

    private static void OnAutoHideMouseLeave(object sender, MouseEventArgs e) => HideLater((ScrollViewer)sender, HideAfterLeave);

    private static void OnAutoHideScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var viewer = (ScrollViewer)sender;
        // Only this viewer's own scrolling (the event bubbles up from inner scroll viewers too)
        if (!ReferenceEquals(e.OriginalSource, viewer) || (e.VerticalChange == 0 && e.HorizontalChange == 0))
        {
            return;
        }
        SetBarsVisible(viewer, true, animate: true);
        if (!viewer.IsMouseOver)
        {
            HideLater(viewer, HideAfterScroll);
        }
    }

    private static void HideLater(ScrollViewer viewer, TimeSpan delay)
    {
        if (viewer.GetValue(HideTimerProperty) is not DispatcherTimer timer)
        {
            timer = new DispatcherTimer(DispatcherPriority.Background, viewer.Dispatcher);
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                // Still under the mouse, or a thumb is being dragged out of the viewer: keep them
                if (viewer.IsMouseOver || Bars(viewer).Any(b => b.IsMouseCaptureWithin))
                {
                    if (!viewer.IsMouseOver)
                    {
                        timer.Start();
                    }
                    return;
                }
                SetBarsVisible(viewer, false, animate: true);
            };
            viewer.SetValue(HideTimerProperty, timer);
        }
        timer.Stop();
        timer.Interval = delay;
        timer.Start();
    }

    private static void SetBarsVisible(ScrollViewer viewer, bool visible, bool animate)
    {
        foreach (var bar in Bars(viewer))
        {
            if (animate)
            {
                bar.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(visible ? 1 : 0, Fade));
            }
            else
            {
                bar.BeginAnimation(UIElement.OpacityProperty, null);
                bar.Opacity = visible ? 1 : 0;
            }
        }
    }

    private static IEnumerable<ScrollBar> Bars(ScrollViewer viewer)
    {
        foreach (var name in new[] { "PART_VerticalScrollBar", "PART_HorizontalScrollBar" })
        {
            if (viewer.Template?.FindName(name, viewer) is ScrollBar bar)
            {
                yield return bar;
            }
        }
    }

    private static bool CanScroll(ScrollViewer viewer, int delta) =>
        delta > 0
            ? viewer.VerticalOffset > 0.01
            : viewer.VerticalOffset < viewer.ScrollableHeight - 0.01;

    /// <summary>The scroll viewer nearest above an element (also from inside text, which is not a Visual).</summary>
    private static ScrollViewer? FindScrollViewer(DependencyObject? element)
    {
        for (var current = element; current != null; current = Parent(current))
        {
            if (current is ScrollViewer viewer)
            {
                return viewer;
            }
        }
        return null;
    }

    // Visual parents only, so a turn inside a popup (an open combo box) never scrolls the page under it.
    private static DependencyObject? Parent(DependencyObject element) => element switch
    {
        Visual or System.Windows.Media.Media3D.Visual3D => VisualTreeHelper.GetParent(element),
        FrameworkContentElement content => content.Parent,
        _ => LogicalTreeHelper.GetParent(element),
    };
}
