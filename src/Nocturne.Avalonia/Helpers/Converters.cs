using System.Collections;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Nocturne.Avalonia.Helpers;

/// <summary>Converters the Nocturne templates use.</summary>
public static class NocturneConverters
{
    /// <summary>
    /// The validation callout is open while the field has errors and the keyboard focus or the
    /// pointer: values are HasErrors, IsKeyboardFocusWithin, IsPointerOver.
    /// </summary>
    public static readonly IMultiValueConverter ShowValidationCallout = new FuncMultiValueConverter<object?, bool>(values =>
    {
        var list = values.ToList();
        return list.Count == 3 && list[0] is true && (list[1] is true || list[2] is true);
    });

    /// <summary>A number plus the parameter (e.g. a popup as wide as its field less its padding).</summary>
    public static readonly IValueConverter Add = new FuncValueConverter<double, object?, double>((value, parameter) =>
        value + Convert.ToDouble(parameter, System.Globalization.CultureInfo.InvariantCulture));

    /// <summary>A colour as a solid brush (the ColorPicker's swatches).</summary>
    public static readonly IValueConverter ColorToBrush = new FuncValueConverter<object?, IBrush?>(value =>
        value is Color c ? new SolidColorBrush(c) : null);

    /// <summary>A colour as #RRGGBB, or #AARRGGBB when it is not opaque.</summary>
    public static readonly IValueConverter ColorToHex = new FuncValueConverter<object?, string>(value => value is Color c
        ? (c.A == 255 ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}")
        : string.Empty);

    /// <summary>The first validation error as text (an exception gives its message).</summary>
    public static readonly IValueConverter FirstError = new FuncValueConverter<object?, string?>(errors =>
    {
        var first = errors is IEnumerable list and not string ? list.Cast<object?>().FirstOrDefault() : errors;
        return first switch
        {
            null => null,
            Exception e => e.Message,
            _ => first.ToString(),
        };
    });
}
