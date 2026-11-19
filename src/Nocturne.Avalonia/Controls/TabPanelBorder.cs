using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// The panel under document tabs: rounded on three corners, square at the top left where it
/// meets the first tab. Its frame has no top edge (the tab strip draws that line) except for
/// the rounded top-right corner.
/// </summary>
public class TabPanelBorder : Decorator
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<TabPanelBorder, IBrush?>(nameof(Background));

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<TabPanelBorder, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<TabPanelBorder, double>(nameof(CornerRadius), 10);

    static TabPanelBorder()
    {
        AffectsRender<TabPanelBorder>(BackgroundProperty, BorderBrushProperty, CornerRadiusProperty);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 2 || h <= 2)
            return;

        var r = Math.Max(0, Math.Min(CornerRadius, Math.Min(w, h) / 2));

        if (Background != null)
        {
            var fill = new StreamGeometry();
            using (var g = fill.Open())
            {
                g.BeginFigure(new Point(0, 0), true);
                g.LineTo(new Point(w - r, 0));
                g.ArcTo(new Point(w, r), new Size(r, r), 0, false, SweepDirection.Clockwise);
                g.LineTo(new Point(w, h - r));
                g.ArcTo(new Point(w - r, h), new Size(r, r), 0, false, SweepDirection.Clockwise);
                g.LineTo(new Point(r, h));
                g.ArcTo(new Point(0, h - r), new Size(r, r), 0, false, SweepDirection.Clockwise);
                g.EndFigure(true);
            }
            context.DrawGeometry(Background, null, fill);
        }

        if (BorderBrush != null)
        {
            // Half-pixel insets keep the 1px line sharp.
            const double i = 0.5;
            var ri = Math.Max(0, r - i);
            var frame = new StreamGeometry();
            using (var g = frame.Open())
            {
                g.BeginFigure(new Point(i, 0), false);
                g.LineTo(new Point(i, h - r));
                g.ArcTo(new Point(r, h - i), new Size(ri, ri), 0, false, SweepDirection.CounterClockwise);
                g.LineTo(new Point(w - r, h - i));
                g.ArcTo(new Point(w - i, h - r), new Size(ri, ri), 0, false, SweepDirection.CounterClockwise);
                g.LineTo(new Point(w - i, r));
                g.ArcTo(new Point(w - r, i), new Size(ri, ri), 0, false, SweepDirection.CounterClockwise);
                g.EndFigure(false);
            }
            context.DrawGeometry(null, new Pen(BorderBrush, 1), frame);
        }
    }
}
