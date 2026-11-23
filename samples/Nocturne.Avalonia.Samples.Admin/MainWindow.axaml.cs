using Avalonia.Controls;
using Avalonia.Interactivity;
using Nocturne.Avalonia.Controls;
using Nocturne.Avalonia.Samples.Admin.Views;

namespace Nocturne.Avalonia.Samples.Admin;

public partial class MainWindow : NocturneWindow
{
    public MainWindow()
    {
        InitializeComponent();

        BackdropBox.ItemsSource = Enum.GetValues<WindowBackdrop>();
        BackdropBox.SelectedItem = Backdrop;
        BackdropBox.SelectionChanged += (_, _) => { if (BackdropBox.SelectedItem is WindowBackdrop b) Backdrop = b; };
        AccentBox.ItemsSource = Enum.GetValues<NocturneAccent>();
        AccentBox.SelectedItem = NocturneTheme.Accent;
        AccentBox.SelectionChanged += (_, _) =>
        {
            if (AccentBox.SelectedItem is NocturneAccent accent && accent != NocturneTheme.Accent)
                NocturneTheme.SetAccent(accent);
        };

        // The content area shows the page the selected item carries in Tag
        Navigation.SelectionChanged += (_, _) => Navigation.Content = (Navigation.SelectedItem as HamburgerMenuItem)?.Tag;
        Navigation.Content = (Navigation.SelectedItem as HamburgerMenuItem)?.Tag;
    }

    public IEnumerable<string> PageNames => Navigation.Items.OfType<HamburgerMenuItem>().Select(i => (string)i.Content!);

    public EmployeesView EmployeesPage => (EmployeesView)Navigation.Items.OfType<HamburgerMenuItem>().First(i => i.Tag is EmployeesView).Tag!;

    public void ShowPage(string name) =>
        Navigation.SelectedItem = Navigation.Items.OfType<HamburgerMenuItem>().First(i => (string?)i.Content == name);

    /// <summary>A control whose Tag names a page shows that page (the dashboard's tiles, the bell).</summary>
    public static void GoToPage(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { Tag: string page } control && TopLevel.GetTopLevel(control) is MainWindow window)
            window.ShowPage(page);
    }

    private void OnGoToPage(object? sender, RoutedEventArgs e) => GoToPage(sender, e);

    private void OnSettingsClick(object? sender, RoutedEventArgs e) => Settings.IsOpen = !Settings.IsOpen;

    /// <summary>--snapshot: one picture per page, plus the settings flyout and the delete dialog.</summary>
    public static void SavePictures(NocturneWindow nocturneWindow, Action<string> save)
    {
        var window = (MainWindow)nocturneWindow;
        foreach (var page in window.PageNames.ToList())
        {
            window.ShowPage(page);
            save("admin-" + page.ToLowerInvariant());
        }
        window.ShowPage("Employees");
        window.Settings.IsOpen = true;
        save("admin-settings");
        window.Settings.IsOpen = false;
        _ = window.EmployeesPage.ConfirmDeleteAsync();
        save("admin-dialog");
        window.EmployeesPage.CloseDialog();
        SampleSnapshot.Capture(window, 5);
    }
}
