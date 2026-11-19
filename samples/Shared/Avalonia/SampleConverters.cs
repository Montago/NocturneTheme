using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Styling;

namespace Nocturne.Avalonia.Samples;

/// <summary>A status word of the samples as a pill in the theme's colours (as the WPF samples); a plain tag for the rest (Draft, Delivered).</summary>
public static class SampleConverters
{
    public static readonly IValueConverter StatusTheme = new FuncValueConverter<string?, ControlTheme?>(status =>
    {
        var key = status switch
        {
            "Approved" or "Shipped" => "Nocturne.Styles.Pill.Success",
            "Submitted" or "New" or "Packing" => "Nocturne.Styles.Pill.Warning",
            "Rejected" => "Nocturne.Styles.Pill.Error",
            _ => "Nocturne.Styles.Tag",
        };
        return Application.Current!.TryFindResource(key, out var theme) ? theme as ControlTheme : null;
    });
}
