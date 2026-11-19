using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// A line icon drawn on a 24 x 24 grid with round ends, in the colour of
/// <see cref="TemplatedControl.Foreground"/>. Use the <c>Nocturne.Icons.*</c> geometries or any path
/// data in 24 x 24 units; the stroke scales with the icon (1.6 at 24px is about 1.3 at 20px).
/// </summary>
public class StrokeIcon : TemplatedControl
{
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<StrokeIcon, Geometry?>(nameof(Data));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<StrokeIcon, double>(nameof(StrokeThickness), 1.6);

    static StrokeIcon()
    {
        FocusableProperty.OverrideDefaultValue<StrokeIcon>(false);
    }

    /// <summary>The icon's outline in 24 x 24 units.</summary>
    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>Line width in 24 x 24 units.</summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
}
