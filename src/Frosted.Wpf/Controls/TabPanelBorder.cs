using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Frosted.Wpf.Controls;

/// <summary>
/// The panel under document tabs: rounded on three corners, square at the top left where it
/// meets the first tab. Its frame has no top edge (the tab strip draws that line) except for
/// the rounded top-right corner.
/// </summary>
public class TabPanelBorder : Decorator
{
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
        nameof(Background), typeof(Brush), typeof(TabPanelBorder),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
        nameof(BorderBrush), typeof(Brush), typeof(TabPanelBorder),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(double), typeof(TabPanelBorder),
        new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 2 || h <= 2)
        {
            return;
        }

        var r = Math.Max(0, Math.Min(CornerRadius, Math.Min(w, h) / 2));

        if (Background != null)
        {
            var fill = new StreamGeometry();
            using (var g = fill.Open())
            {
                g.BeginFigure(new Point(0, 0), true, true);
                g.LineTo(new Point(w - r, 0), false, false);
                g.ArcTo(new Point(w, r), new Size(r, r), 0, false, SweepDirection.Clockwise, false, false);
                g.LineTo(new Point(w, h - r), false, false);
                g.ArcTo(new Point(w - r, h), new Size(r, r), 0, false, SweepDirection.Clockwise, false, false);
                g.LineTo(new Point(r, h), false, false);
                g.ArcTo(new Point(0, h - r), new Size(r, r), 0, false, SweepDirection.Clockwise, false, false);
            }
            fill.Freeze();
            dc.DrawGeometry(Background, null, fill);
        }

        if (BorderBrush != null)
        {
            // Half-pixel insets keep the 1px line sharp.
            const double i = 0.5;
            var ri = Math.Max(0, r - i);
            var frame = new StreamGeometry();
            using (var g = frame.Open())
            {
                g.BeginFigure(new Point(i, 0), false, false);
                g.LineTo(new Point(i, h - r), true, false);
                g.ArcTo(new Point(r, h - i), new Size(ri, ri), 0, false, SweepDirection.Counterclockwise, true, false);
                g.LineTo(new Point(w - r, h - i), true, false);
                g.ArcTo(new Point(w - i, h - r), new Size(ri, ri), 0, false, SweepDirection.Counterclockwise, true, false);
                g.LineTo(new Point(w - i, r), true, false);
                g.ArcTo(new Point(w - r, i), new Size(ri, ri), 0, false, SweepDirection.Counterclockwise, true, false);
            }
            frame.Freeze();
            dc.DrawGeometry(null, new Pen(BorderBrush, 1), frame);
        }
    }
}
