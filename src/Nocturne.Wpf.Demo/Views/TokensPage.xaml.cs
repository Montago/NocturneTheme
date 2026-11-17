using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Demo.Views;

public partial class TokensPage : UserControl
{
    private static readonly (string Name, string Key)[] SwatchKeys =
    [
        ("Accent", "Nocturne.Brushes.Accent"),
        ("Primary button", "Nocturne.Brushes.Button.Primary"),
        ("Accent light", "Nocturne.Brushes.Accent.Light"),
        ("Text", "Nocturne.Brushes.Text"),
        ("Text secondary", "Nocturne.Brushes.Text.Secondary"),
        ("Focus ring", "Nocturne.Brushes.Focus"),
        ("Surface 1 · card", "Nocturne.Brushes.Surface1"),
        ("Surface 2 · selected", "Nocturne.Brushes.Surface2"),
        ("Stroke", "Nocturne.Brushes.Stroke"),
        ("Input field", "Nocturne.Brushes.Field"),
        ("Popup", "Nocturne.Brushes.Popup"),
        ("Error", "Nocturne.Brushes.Error"),
        ("Warning", "Nocturne.Brushes.Warning"),
        ("Success", "Nocturne.Brushes.Success"),
        ("Window tint", "Nocturne.Brushes.Window.Tint"),
        ("Fallback ground", "Nocturne.Brushes.Window.Fallback"),
        ("Hover", "Nocturne.Brushes.Item.Hover"),
        ("Close hover", "Nocturne.Brushes.Close.Hover"),
    ];

    public TokensPage()
    {
        InitializeComponent();
        ShowSwatches();

        // The accent buttons replace brushes; show the new ones.
        NocturneTheme.AccentChanged += (_, _) => ShowSwatches();
    }

    private void ShowSwatches()
    {
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
