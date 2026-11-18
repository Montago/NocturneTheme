using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Demo.Views;

public partial class IconsPage : UserControl
{
    public IconsPage()
    {
        InitializeComponent();
        foreach (var kind in Enum.GetValues<NocturneIconKind>().Where(k => k != NocturneIconKind.None))
        {
            Icons.Children.Add(Cell(kind));
        }
    }

    // The icon on a small tile, its name under it; a click copies the XAML
    private Border Cell(NocturneIconKind kind)
    {
        var tile = new ContentControl { HorizontalAlignment = HorizontalAlignment.Center, Content = new NocturneIcon { Kind = kind } };
        tile.SetResourceReference(StyleProperty, "Nocturne.Styles.IconTile");

        var name = new TextBlock
        {
            Text = kind.ToString(),
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        name.SetResourceReference(TextBlock.ForegroundProperty, "Nocturne.Brushes.Text.Secondary");

        var content = new StackPanel();
        content.Children.Add(tile);
        content.Children.Add(name);

        var cell = new Border
        {
            Width = 132,
            Padding = new Thickness(6, 10, 6, 8),
            Margin = new Thickness(0, 0, 6, 6),
            CornerRadius = new CornerRadius(8),
            Background = System.Windows.Media.Brushes.Transparent,
            Cursor = Cursors.Hand,
            ToolTip = Xaml(kind),
            Tag = kind,
            Child = content,
        };
        cell.MouseEnter += (_, _) => cell.SetResourceReference(Border.BackgroundProperty, "Nocturne.Brushes.Item.Hover");
        cell.MouseLeave += (_, _) => cell.Background = System.Windows.Media.Brushes.Transparent;
        cell.MouseLeftButtonUp += (_, _) => Copy(kind);
        return cell;
    }

    private static string Xaml(NocturneIconKind kind) => $"<nc:NocturneIcon Kind=\"{kind}\" />";

    private void Copy(NocturneIconKind kind)
    {
        try
        {
            Clipboard.SetText(Xaml(kind));
            Copied.Text = "Copied " + Xaml(kind);
        }
        catch (ExternalException)
        {
            // Another program holds the clipboard
            Copied.Text = Xaml(kind);
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var text = Search.Text.Trim();
        foreach (Border cell in Icons.Children)
        {
            cell.Visibility = text.Length == 0 || cell.Tag.ToString()!.Contains(text, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
