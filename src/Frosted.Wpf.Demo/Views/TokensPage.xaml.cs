using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Frosted.Wpf.Demo.Views;

public partial class TokensPage : UserControl
{
    private static readonly (string Name, string Key)[] SwatchKeys =
    [
        ("Accent", "Frosted.Brushes.Accent"),
        ("Primary button", "Frosted.Brushes.Button.Primary"),
        ("Accent light", "Frosted.Brushes.Accent.Light"),
        ("Text", "Frosted.Brushes.Text"),
        ("Text secondary", "Frosted.Brushes.Text.Secondary"),
        ("Focus ring", "Frosted.Brushes.Focus"),
        ("Surface 1 · card", "Frosted.Brushes.Surface1"),
        ("Surface 2 · selected", "Frosted.Brushes.Surface2"),
        ("Stroke", "Frosted.Brushes.Stroke"),
        ("Input field", "Frosted.Brushes.Field"),
        ("Popup", "Frosted.Brushes.Popup"),
        ("Error", "Frosted.Brushes.Error"),
        ("Warning", "Frosted.Brushes.Warning"),
        ("Success", "Frosted.Brushes.Success"),
        ("Window tint", "Frosted.Brushes.Window.Tint"),
        ("Fallback ground", "Frosted.Brushes.Window.Fallback"),
        ("Hover", "Frosted.Brushes.Item.Hover"),
        ("Close hover", "Frosted.Brushes.Close.Hover"),
    ];

    public TokensPage()
    {
        InitializeComponent();
        Swatches.ItemsSource = SwatchKeys.Select(s => new Swatch(s.Name, s.Key, (Brush)FindResource(s.Key))).ToList();
    }

    public record Swatch(string Name, string Key, Brush Brush)
    {
        public string Value => Brush switch
        {
            SolidColorBrush solid => solid.Color.ToString(),
            GradientBrush gradient => string.Join(" → ", gradient.GradientStops.Select(stop => stop.Color.ToString())),
            _ => string.Empty,
        };
    }
}
