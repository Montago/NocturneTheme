using System.Windows;
using System.Windows.Media;

namespace Frosted.Wpf.Controls;

/// <summary>
/// The check box of a <see cref="FrostedGridCheckBoxColumn"/> cell that is not being edited: the 20px
/// rounded box of the CheckBox style with its mark, drawn by one element instead of a CheckBox and its
/// template.
/// </summary>
/// <remarks>
/// Sizes and brushes are those of the CheckBox template in Controls.Toggles.xaml (a 20px box with 5px
/// corners and a 1px border; checked or undecided: the accent with the check mark <c>Frosted.Icons.Check</c>
/// in 14px or a 10 x 2 bar), so the cell looks like the CheckBox it turns into for editing. The brushes are
/// resource references, so accent changes reach the cells.
/// </remarks>
public sealed class GridCheckBox : FrameworkElement
{
    private const double Size = 20;
    private const double Radius = 5;
    private const double MarkSize = 14;

    private static readonly DependencyProperty BoxBackgroundProperty = Register("BoxBackground");
    private static readonly DependencyProperty BoxBorderProperty = Register("BoxBorder");
    private static readonly DependencyProperty AccentProperty = Register("Accent");
    private static readonly DependencyProperty GlyphProperty = Register("Glyph");
    private static readonly DependencyProperty CheckProperty = DependencyProperty.Register(
        "Check", typeof(Geometry), typeof(GridCheckBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    [ThreadStatic]
    private static (Geometry Source, Geometry Mark)? lastMark;

    private bool? isChecked = false;

    static GridCheckBox()
    {
        SnapsToDevicePixelsProperty.OverrideMetadata(typeof(GridCheckBox), new FrameworkPropertyMetadata(true));
        HorizontalAlignmentProperty.OverrideMetadata(typeof(GridCheckBox), new FrameworkPropertyMetadata(HorizontalAlignment.Center));
        VerticalAlignmentProperty.OverrideMetadata(typeof(GridCheckBox), new FrameworkPropertyMetadata(VerticalAlignment.Center));
        FocusableProperty.OverrideMetadata(typeof(GridCheckBox), new FrameworkPropertyMetadata(false));
        IsEnabledProperty.OverrideMetadata(typeof(GridCheckBox), new UIPropertyMetadata(true, (d, _) => ((GridCheckBox)d).InvalidateVisual()));
    }

    public GridCheckBox()
    {
        SetResourceReference(BoxBackgroundProperty, "Frosted.Brushes.Toggle.Fill");
        SetResourceReference(BoxBorderProperty, "Frosted.Brushes.Toggle.Border");
        SetResourceReference(AccentProperty, "Frosted.Brushes.Accent");
        SetResourceReference(GlyphProperty, "Frosted.Brushes.Text.OnAccent");
        SetResourceReference(CheckProperty, "Frosted.Icons.Check");
    }

    private static DependencyProperty Register(string name) => DependencyProperty.Register(
        name, typeof(Brush), typeof(GridCheckBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>The value shown: true draws the check mark, null the bar of an undecided CheckBox.</summary>
    public bool? IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked != value)
            {
                isChecked = value;
                InvalidateVisual();
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize) => new(Size, Size);

    protected override Size ArrangeOverride(Size finalSize) => new(Size, Size);

    protected override void OnRender(DrawingContext dc)
    {
        // The template dims the whole check box when disabled.
        if (!IsEnabled)
        {
            dc.PushOpacity(0.45);
        }

        var on = isChecked != false;
        var fill = (Brush?)GetValue(on ? AccentProperty : BoxBackgroundProperty);
        var border = (Brush?)GetValue(on ? AccentProperty : BoxBorderProperty);
        // A 1px Border: the pen runs half a pixel inside the edge.
        dc.DrawRoundedRectangle(fill, border == null ? null : new Pen(border, 1), new Rect(0.5, 0.5, Size - 1, Size - 1), Radius - 0.5, Radius - 0.5);

        if (on && GetValue(GlyphProperty) is Brush glyph)
        {
            if (isChecked == null)
            {
                // The 10 x 2 Rectangle with round ends, centred.
                dc.DrawRoundedRectangle(glyph, null, new Rect((Size - 10) / 2, (Size - 2) / 2, 10, 2), 1, 1);
            }
            else if (GetValue(CheckProperty) is Geometry geometry)
            {
                // The StrokeIcon: the 24 x 24 icon grid in a 14px square, stroke 3 at that grid.
                var scale = MarkSize / 24;
                var pen = new Pen(glyph, 3 * scale) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
                dc.DrawGeometry(null, pen, Mark(geometry, scale));
            }
        }

        if (!IsEnabled)
        {
            dc.Pop();
        }
    }

    // The check mark scaled to 14px and centred in the box, shared by every cell.
    private static Geometry Mark(Geometry geometry, double scale)
    {
        if (lastMark is { } last && ReferenceEquals(last.Source, geometry))
        {
            return last.Mark;
        }
        var matrix = Matrix.Identity;
        matrix.Scale(scale, scale);
        matrix.Translate((Size - MarkSize) / 2, (Size - MarkSize) / 2);
        var placed = geometry.Clone();
        placed.Transform = new MatrixTransform(matrix);
        placed.Freeze();
        lastMark = (geometry, placed);
        return placed;
    }
}
