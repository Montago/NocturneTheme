using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace Nocturne.Avalonia.Helpers;

/// <summary>
/// Fades the scroll bars out while the pointer is away (the Nocturne ScrollViewer theme turns it on
/// when <see cref="ScrollViewer.AllowAutoHide"/> is true): they fade in when the pointer enters the
/// scroll viewer or its content scrolls (wheel, keys, code), and out again a moment after (0.4 s
/// after the pointer leaves, 1 s after scrolling). Their room stays, so the layout does not move.
/// It sets the style class <c>barsVisible</c>, which the theme turns into the bars' opacity.
/// (Wheel chaining, which the WPF version adds, is built into Avalonia: IsScrollChainingEnabled.)
/// </summary>
public class ScrollViewerHelper
{
    public static readonly AttachedProperty<bool> AutoHideProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewerHelper, ScrollViewer, bool>("AutoHide");

    private static readonly AttachedProperty<DispatcherTimer?> HideTimerProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewerHelper, ScrollViewer, DispatcherTimer?>("HideTimer");

    private static readonly AttachedProperty<bool> IsDraggingProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewerHelper, ScrollViewer, bool>("IsDragging");

    private static readonly TimeSpan HideAfterLeave = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan HideAfterScroll = TimeSpan.FromMilliseconds(1000);

    static ScrollViewerHelper()
    {
        AutoHideProperty.Changed.AddClassHandler<ScrollViewer>(OnAutoHideChanged);
    }

    public static bool GetAutoHide(ScrollViewer viewer) => viewer.GetValue(AutoHideProperty);

    public static void SetAutoHide(ScrollViewer viewer, bool value) => viewer.SetValue(AutoHideProperty, value);

    private static void OnAutoHideChanged(ScrollViewer viewer, AvaloniaPropertyChangedEventArgs e)
    {
        viewer.PointerEntered -= OnPointerEntered;
        viewer.PointerExited -= OnPointerExited;
        viewer.ScrollChanged -= OnScrollChanged;
        viewer.RemoveHandler(Thumb.DragStartedEvent, OnDragStarted);
        viewer.RemoveHandler(Thumb.DragCompletedEvent, OnDragCompleted);
        if (e.NewValue is true)
        {
            viewer.PointerEntered += OnPointerEntered;
            viewer.PointerExited += OnPointerExited;
            viewer.ScrollChanged += OnScrollChanged;
            viewer.AddHandler(Thumb.DragStartedEvent, OnDragStarted);
            viewer.AddHandler(Thumb.DragCompletedEvent, OnDragCompleted);
            viewer.Classes.Set("barsVisible", viewer.IsPointerOver);
        }
        else
        {
            viewer.GetValue(HideTimerProperty)?.Stop();
            viewer.Classes.Set("barsVisible", false);
        }
    }

    private static void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        var viewer = (ScrollViewer)sender!;
        viewer.GetValue(HideTimerProperty)?.Stop();
        viewer.Classes.Set("barsVisible", true);
    }

    private static void OnPointerExited(object? sender, PointerEventArgs e) => HideLater((ScrollViewer)sender!, HideAfterLeave);

    private static void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var viewer = (ScrollViewer)sender!;
        // Only this viewer's own scrolling (the event bubbles up from inner scroll viewers too)
        if (!ReferenceEquals(e.Source, viewer) || e.OffsetDelta == default)
            return;
        viewer.Classes.Set("barsVisible", true);
        if (!viewer.IsPointerOver)
            HideLater(viewer, HideAfterScroll);
    }

    // A thumb dragged out of the viewer keeps the bars until it is let go
    private static void OnDragStarted(object? sender, VectorEventArgs e) => ((ScrollViewer)sender!).SetValue(IsDraggingProperty, true);

    private static void OnDragCompleted(object? sender, VectorEventArgs e)
    {
        var viewer = (ScrollViewer)sender!;
        viewer.SetValue(IsDraggingProperty, false);
        if (!viewer.IsPointerOver)
            HideLater(viewer, HideAfterLeave);
    }

    private static void HideLater(ScrollViewer viewer, TimeSpan delay)
    {
        if (viewer.GetValue(HideTimerProperty) is not { } timer)
        {
            timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (viewer.IsPointerOver || viewer.GetValue(IsDraggingProperty))
                    return;
                viewer.Classes.Set("barsVisible", false);
            };
            viewer.SetValue(HideTimerProperty, timer);
        }
        timer.Stop();
        timer.Interval = delay;
        timer.Start();
    }
}
