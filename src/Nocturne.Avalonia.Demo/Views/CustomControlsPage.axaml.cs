using Avalonia.Controls;
using Avalonia.Interactivity;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo.Views;

public partial class CustomControlsPage : UserControl
{
    /// <summary>Suggestions for the AutoCompleteBox.</summary>
    public static readonly IReadOnlyList<string> Cities =
    [
        "Aarhus", "Amsterdam", "Athens", "Barcelona", "Berlin", "Bergen", "Brussels", "Copenhagen", "Dublin", "Edinburgh",
        "Gothenburg", "Hamburg", "Helsinki", "Lisbon", "London", "Madrid", "Malmö", "Milan", "Munich", "Odense",
        "Oslo", "Paris", "Prague", "Reykjavík", "Rome", "Stockholm", "Tallinn", "Vienna", "Warsaw", "Zurich",
    ];

    /// <summary>The people in the SearchComboBox.</summary>
    public static readonly IReadOnlyList<Person> People =
    [
        new("Anna Holm", "Administrator"), new("Ida Krogh", "Support"), new("Jonas Berg", "Developer"),
        new("Karen Juhl", "Designer"), new("Lars Mose", "Reader"), new("Mikkel Dahl", "Developer"),
        new("Nina Friis", "Administrator"), new("Ole Skov", "Support"), new("Sofie Lund", "Reader"),
        new("Thomas Bak", "Developer"),
    ];

    public CustomControlsPage()
    {
        InitializeComponent();

        // A fixed day, so the pictures do not change with the date
        MeetingDay.SelectedDate = new DateTimeOffset(2026, 11, 22, 0, 0, 0, TimeSpan.Zero);
        CityBox.ItemsSource = Cities;
        OwnerBox.ItemsSource = People;
        OwnerBox.DisplayMemberPath = nameof(Person.Name);
        OwnerBox.SelectedItem = People[2];

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

/// <summary>A person in the demo's lists.</summary>
public sealed record Person(string Name, string Role)
{
    public override string ToString() => Name;
}
