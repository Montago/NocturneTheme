using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Frosted.Wpf.Helpers;

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
