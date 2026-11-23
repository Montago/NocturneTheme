using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Nocturne.Wpf.Controls;
using Nocturne.Wpf.Samples.Admin.Views;

namespace Nocturne.Wpf.Samples.Admin;

public partial class MainWindow : NocturneWindow
{
    public MainWindow()
    {
        InitializeComponent();
        BackdropBox.ItemsSource = Enum.GetValues<WindowBackdrop>();
        AccentBox.ItemsSource = Enum.GetValues<NocturneAccent>();
        AccentBox.SelectedItem = NocturneTheme.Accent;
    }

    public IEnumerable<string> PageNames => Navigation.Items.Cast<HamburgerMenuItem>().Select(i => (string)i.Content);

    public EmployeesView EmployeesPage => EmployeesView;

    public void ShowPage(string name) =>
        Navigation.SelectedItem = Navigation.Items.Cast<HamburgerMenuItem>().First(i => (string)i.Content == name);

    private void OnGoToPage(object sender, ExecutedRoutedEventArgs e) => ShowPage((string)e.Parameter);

    private void OnSettingsClick(object sender, RoutedEventArgs e) => Settings.IsOpen = !Settings.IsOpen;

    private void OnAccentChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AccentBox.SelectedItem is NocturneAccent accent && accent != NocturneTheme.Accent)
        {
            NocturneTheme.SetAccent(accent);
        }
    }
}
