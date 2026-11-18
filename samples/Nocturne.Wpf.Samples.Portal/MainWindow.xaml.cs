using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Samples.Portal;

public partial class MainWindow : NocturneWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ShowSelectedPage();
    }

    public IEnumerable<string> PageNames => Navigation.Items.Cast<ListBoxItem>().Select(i => (string)i.Content);

    public void ShowPage(string name) =>
        Navigation.SelectedItem = Navigation.Items.Cast<ListBoxItem>().First(i => (string)i.Content == name);

    private void OnGoToPage(object sender, ExecutedRoutedEventArgs e) => ShowPage((string)e.Parameter);

    private void OnNavigationChanged(object sender, SelectionChangedEventArgs e) => ShowSelectedPage();

    // The pages sit in the same order as the side bar entries.
    private void ShowSelectedPage()
    {
        // SelectionChanged fires during InitializeComponent, before the pages exist.
        if (Pages == null)
        {
            return;
        }
        for (var i = 0; i < Pages.Children.Count; i++)
        {
            Pages.Children[i].Visibility = i == Math.Max(0, Navigation.SelectedIndex) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
