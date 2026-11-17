using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Frosted.Wpf.Helpers;

/// <summary>
/// Mouse wheel chaining. WPF's ScrollViewer swallows every wheel turn, even when it has nothing
/// to scroll, so a ListBox or DataGrid inside a scrolling page stops the page from scrolling.
/// With <see cref="ChainMouseWheelProperty"/> on (the Frosted ScrollViewer style sets it), a
/// wheel turn that the innermost scroll viewer under the mouse cannot use (no overflow, or already
/// at the top or bottom) goes to the nearest outer scroll viewer that can scroll that way.
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
            || CanScroll(viewer, e.Delta))
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
