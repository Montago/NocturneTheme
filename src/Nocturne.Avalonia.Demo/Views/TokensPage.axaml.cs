using Avalonia.Controls;
using Avalonia.Layout;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo.Views;

public partial class TokensPage : UserControl
{
    private static readonly string[] BrushKeys =
    [
        "Accent", "Accent.Light", "Accent.Text", "Text", "Text.Secondary", "Text.Hint",
        "Surface1", "Surface2", "Surface.Sunken", "Stroke", "Field", "Item.Selected",
        "Popup", "Error", "Warning", "Success", "Window.Fallback", "Window.Tint",
    ];

    private static readonly string[] IconKeys =
    [
        "Check", "ChevronDown", "ChevronRight", "Close", "Plus", "Search", "Refresh", "Eye", "Copy", "Cut",
        "Paste", "Undo", "SelectAll", "Edit", "Delete", "Sync", "Shield", "Globe", "Database", "Grid",
        "Layers", "Text", "Clock", "Calendar", "Folder", "Info", "CheckCircle", "Warning", "Error",
        "Components", "Filter", "Menu", "File",
    ];

    public TokensPage()
    {
        InitializeComponent();

        foreach (var key in BrushKeys)
        {
            var swatch = new Border
            {
                Width = 120,
                Height = 56,
                CornerRadius = new(10),
                BorderThickness = new(1),
            };
            swatch.Bind(Border.BackgroundProperty, swatch.GetResourceObservable("Nocturne.Brushes." + key));
            swatch.Bind(Border.BorderBrushProperty, swatch.GetResourceObservable("Nocturne.Brushes.Stroke"));
            Swatches.Children.Add(new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    swatch,
                    new TextBlock { Text = key, FontSize = 12, Opacity = 0.8 },
                },
            });
        }

        foreach (var key in IconKeys)
        {
            var icon = new StrokeIcon { Width = 22, Height = 22, HorizontalAlignment = HorizontalAlignment.Center };
            icon.Bind(StrokeIcon.DataProperty, icon.GetResourceObservable("Nocturne.Icons." + key));
            LineIcons.Children.Add(new StackPanel
            {
                Width = 72,
                Spacing = 6,
                Children =
                {
                    icon,
                    new TextBlock { Text = key, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, Opacity = 0.7 },
                },
            });
        }
    }
}
