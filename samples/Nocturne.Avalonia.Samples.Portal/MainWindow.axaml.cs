using Avalonia.Controls;
using Avalonia.Interactivity;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Portal;

public partial class MainWindow : NocturneWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Navigation.SelectionChanged += (_, _) => ShowSelectedPage();
        ShowSelectedPage();
    }

    public IEnumerable<string> PageNames => Navigation.Items.OfType<ListBoxItem>().Select(i => (string)i.Tag!);

    public void ShowPage(string name) =>
        Navigation.SelectedItem = Navigation.Items.OfType<ListBoxItem>().First(i => (string?)i.Tag == name);

    /// <summary>A control whose Tag names a page shows that page (the home page's tiles, the help button).</summary>
    public static void GoToPage(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: string page } control && TopLevel.GetTopLevel(control) is MainWindow window)
            window.ShowPage(page);
    }

    private void OnGoToPage(object? sender, RoutedEventArgs e) => GoToPage(sender, e);

    // The pages sit in the same order as the side bar entries.
    private void ShowSelectedPage()
    {
        for (var i = 0; i < Pages.Children.Count; i++)
            Pages.Children[i].IsVisible = i == Math.Max(0, Navigation.SelectedIndex);
    }

    /// <summary>--snapshot: one picture per page.</summary>
    public static void SavePictures(NocturneWindow nocturneWindow, Action<string> save)
    {
        var window = (MainWindow)nocturneWindow;
        foreach (var page in window.PageNames.ToList())
        {
            window.ShowPage(page);
            save("portal-" + page.ToLowerInvariant());
        }
    }
}
