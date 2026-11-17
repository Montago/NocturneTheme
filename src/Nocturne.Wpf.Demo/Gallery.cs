using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Nocturne.Wpf.Demo;

/// <summary>Attached helpers for the gallery samples (Views/GalleryPage.xaml).</summary>
public static class Gallery
{
    /// <summary>Open this element's drop-down, menu or tool tip for the sample's picture.</summary>
    public static readonly DependencyProperty OpenProperty = DependencyProperty.RegisterAttached(
        "Open", typeof(bool), typeof(Gallery), new PropertyMetadata(false));

    public static bool GetOpen(DependencyObject o) => (bool)o.GetValue(OpenProperty);
    public static void SetOpen(DependencyObject o, bool value) => o.SetValue(OpenProperty, value);

    /// <summary>
    /// Stops the endless animations (indeterminate ProgressBar, ProgressRing) at a fixed point, so a
    /// sample gives the same picture on every run and a docs picture only changes when the look does.
    /// </summary>
    public static void FreezeAnimations(DependencyObject root)
    {
        foreach (var control in Descendants(root).OfType<Control>())
        {
            if (control.Template is not { } template)
            {
                continue;
            }
            var storyboards = template.Triggers
                .SelectMany(t => t.EnterActions.Concat(t.ExitActions))
                .OfType<BeginStoryboard>()
                .Select(b => b.Storyboard)
                .Where(s => s is { RepeatBehavior.HasDuration: false, RepeatBehavior.HasCount: false });
            foreach (var storyboard in storyboards)
            {
                // Does nothing (apart from a trace message) when the storyboard is not running here.
                storyboard.SeekAlignedToLastTick(control, TimeSpan.FromMilliseconds(700), TimeSeekOrigin.BeginTime);
                storyboard.Pause(control);
            }
        }
    }

    internal static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var d in Descendants(child))
            {
                yield return d;
            }
        }
    }
}
