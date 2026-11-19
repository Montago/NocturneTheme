using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo.Views;

public partial class IconsPage : UserControl
{
    public IconsPage()
    {
        InitializeComponent();
        foreach (var kind in Enum.GetValues<NocturneIconKind>().Where(k => k != NocturneIconKind.None))
            Icons.Children.Add(Cell(kind));
        Search.TextChanged += (_, _) => Filter();
    }

    // The icon on a small tile, its name under it; a click copies the XAML
    private Border Cell(NocturneIconKind kind)
    {
        var tile = new ContentControl { HorizontalAlignment = HorizontalAlignment.Center, Content = new NocturneIcon { Kind = kind } };
        if (Application.Current!.TryFindResource("Nocturne.Styles.IconTile", out var theme) && theme is ControlTheme controlTheme)
            tile.Theme = controlTheme;

        var name = new TextBlock
        {
            Text = kind.ToString(),
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        name.Bind(TextBlock.ForegroundProperty, name.GetResourceObservable("Nocturne.Brushes.Text.Secondary"));

        var cell = new Border
        {
            Width = 132,
            Padding = new Thickness(6, 10, 6, 8),
            Margin = new Thickness(0, 0, 6, 6),
            CornerRadius = new CornerRadius(8),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand),
            Tag = kind,
            Child = new StackPanel { Children = { tile, name } },
        };
        ToolTip.SetTip(cell, Xaml(kind));
        cell.PointerEntered += (_, _) => cell.Bind(Border.BackgroundProperty, cell.GetResourceObservable("Nocturne.Brushes.Item.Hover"));
        cell.PointerExited += (_, _) => cell.Background = Brushes.Transparent;
        cell.PointerReleased += async (_, _) =>
        {
            Copied.Text = Xaml(kind);
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            {
                await clipboard.SetTextAsync(Xaml(kind));
                Copied.Text = "Copied " + Xaml(kind);
            }
        };
        return cell;
    }

    private static string Xaml(NocturneIconKind kind) => $"<nc:NocturneIcon Kind=\"{kind}\" />";

    private void Filter()
    {
        var text = Search.Text?.Trim() ?? string.Empty;
        foreach (var cell in Icons.Children)
            cell.IsVisible = text.Length == 0 || cell.Tag!.ToString()!.Contains(text, StringComparison.OrdinalIgnoreCase);
    }
}
