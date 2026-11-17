using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Frosted.Wpf.Controls;

/// <summary>
/// A line icon drawn on a 24 x 24 grid with round ends, in the colour of
/// <see cref="Control.Foreground"/>. Use the <c>Frosted.Icons.*</c> geometries or any path
/// data in 24 x 24 units; the stroke scales with the icon (1.6 at 24px is about 1.3 at 20px).
/// </summary>
public class StrokeIcon : Control
{
    static StrokeIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StrokeIcon), new FrameworkPropertyMetadata(typeof(StrokeIcon)));
        FocusableProperty.OverrideMetadata(typeof(StrokeIcon), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(StrokeIcon), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data), typeof(Geometry), typeof(StrokeIcon), new PropertyMetadata(null));

    /// <summary>The icon's outline in 24 x 24 units.</summary>
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(StrokeIcon), new PropertyMetadata(1.6));

    /// <summary>Line width in 24 x 24 units.</summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
}
