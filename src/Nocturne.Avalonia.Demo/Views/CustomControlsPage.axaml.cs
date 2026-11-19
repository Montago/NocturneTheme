using Avalonia.Controls;
using Avalonia.Interactivity;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo.Views;

public partial class CustomControlsPage : UserControl
{
    public CustomControlsPage()
    {
        InitializeComponent();

        // A fixed day, so the pictures do not change with the date
        MeetingDay.SelectedDate = new DateTimeOffset(2026, 11, 22, 0, 0, 0, TimeSpan.Zero);
        CityBox.ItemsSource = DemoData.Cities;
        OwnerBox.ItemsSource = DemoData.People;
        OwnerBox.DisplayMemberPath = nameof(Person.Name);
        OwnerBox.SelectedItem = DemoData.People[2];

        // The content area shows the selected item's name
        DemoMenu.SelectionChanged += (_, _) => DemoMenu.Content = (DemoMenu.SelectedItem as HamburgerMenuItem)?.Content;
        DemoMenu.Content = (DemoMenu.SelectedItem as HamburgerMenuItem)?.Content;
    }

    private async void OnShowDialog(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not NocturneWindow window)
        {
            return;
        }

        var result = await window.ShowMessageAsync(
            "Discard changes?",
            "You have changed 3 files since they were last synchronised. Discarding puts back the versions on the server.",
            "Discard",
            "Keep editing");
        DialogResultText.Text = $"The answer was {result}.";
    }

    private void OnOpenFlyout(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow window)
        {
            window.SettingsFlyout.IsOpen = true;
        }
    }
}
