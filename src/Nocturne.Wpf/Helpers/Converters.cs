using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Nocturne.Wpf.Helpers;

/// <summary>
/// Left margin of a TreeViewItem's header: <see cref="Indent"/> per level, so the row itself can
/// span the full width of the tree while its content steps in.
/// </summary>
public sealed class TreeViewIndentConverter : IValueConverter
{
    /// <summary>Indent per level (16 by default).</summary>
    public double Indent { get; set; } = 16;

    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var depth = 0;
        for (var parent = value as DependencyObject; parent != null && parent is not TreeView; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent != value && parent is TreeViewItem)
            {
                depth++;
            }
        }
        return new Thickness(depth * Indent, 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>A <see cref="Color"/> as a frozen SolidColorBrush (ColorPicker swatches).</summary>
public sealed class ColorToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color c)
        {
            return null;
        }
        var brush = new SolidColorBrush(c);
        brush.Freeze();
        return brush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is SolidColorBrush b ? b.Color : Binding.DoNothing;
}

/// <summary>A <see cref="Color"/> as #RRGGBB (#AARRGGBB when not opaque).</summary>
public sealed class ColorToHexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is Color c
        ? (c.A == 255 ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}")
        : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return value is string s ? (Color)ColorConverter.ConvertFromString(s) : Binding.DoNothing;
        }
        catch (FormatException)
        {
            return Binding.DoNothing;
        }
    }
}

/// <summary>
/// The direction a GridSplitter resizes in, with <see cref="GridResizeDirection.Auto"/> worked out
/// the way GridSplitter does it. Bind ResizeDirection, HorizontalAlignment, VerticalAlignment,
/// ActualWidth and ActualHeight, in that order.
/// </summary>
public sealed class GridSplitterDirectionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 5 || values[0] is not GridResizeDirection direction)
        {
            return GridResizeDirection.Columns;
        }
        if (direction != GridResizeDirection.Auto)
        {
            return direction;
        }
        if (values[1] is not HorizontalAlignment.Stretch)
        {
            return GridResizeDirection.Columns;
        }
        if (values[2] is not VerticalAlignment.Stretch)
        {
            return GridResizeDirection.Rows;
        }
        return values[3] is double width && values[4] is double height && width > height
            ? GridResizeDirection.Rows
            : GridResizeDirection.Columns;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
