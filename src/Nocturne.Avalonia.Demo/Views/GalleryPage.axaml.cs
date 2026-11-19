using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo.Views;

/// <summary>One sample per control for the documentation (<see cref="GallerySnapshot"/> draws them).</summary>
public partial class GalleryPage : UserControl
{
    private static readonly string[] Languages = ["Dansk", "English", "Deutsch", "Svenska"];

    public GalleryPage()
    {
        InitializeComponent();

        LanguageBox.ItemsSource = Languages;
        LanguageBox.SelectedItem = "English";
        EditableBox.ItemsSource = Languages;
        EmptyBox.ItemsSource = Languages;
        DisabledBox.ItemsSource = Languages;
        DisabledBox.SelectedIndex = 1;

        CityBox.ItemsSource = DemoData.Cities;
        OwnerBox.ItemsSource = DemoData.People;
        OwnerBox.DisplayMemberPath = nameof(Person.Name);

        FolderTree.ItemsSource = DemoData.Folders;
        FolderTree.SelectedItem = DemoData.Folders[0].Items[0].Items[1];
        PeopleGrid.ItemsSource = DemoData.People.OrderBy(p => p.Name).ToList();
        PeopleGrid.SelectedIndex = 0;
        RowHeaderGrid.ItemsSource = DemoData.People;
        RowHeaderGrid.SelectedIndex = 1;
        var groups = new DataGridCollectionView(DemoData.People.OrderBy(p => p.Role));
        groups.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(Person.Role)));
        GroupGrid.ItemsSource = groups;
        GroupGrid.SelectedItem = null;

        SampleMenu.SelectionChanged += (_, _) => SampleMenu.Content = (SampleMenu.SelectedItem as HamburgerMenuItem)?.Content;
        SampleMenu.Content = (SampleMenu.SelectedItem as HamburgerMenuItem)?.Content;

        // Fixed dates, so the pictures do not change with the day
        var meeting = new DateTime(2026, 11, 22);
        MeetingCalendar.DisplayDate = meeting;
        MeetingCalendar.SelectedDate = meeting;
        MeetingPicker.SelectedDate = meeting;
        MeetingPicker.DisplayDate = meeting;
        EndPicker.SelectedDate = new DateTime(2026, 11, 2);
        CreatedPicker.SelectedDate = new DateTime(2026, 10, 1);
        DayPicker.SelectedDate = new DateTimeOffset(meeting);

        DataValidationErrors.SetError(PortBox, new Exception("Must be a number from 1 to 65535."));
        SelectedText.SelectionStart = 0;
        SelectedText.SelectionEnd = 8;

        foreach (var (key, geometry) in LineIcons())
            IconGrid.Children.Add(IconCell(key, geometry));
    }

    /// <summary>The samples, each a ContentControl with the class <c>sample</c> and its file name as Tag.</summary>
    public IReadOnlyList<ContentControl> Samples => SamplesHost.Children.OfType<ContentControl>().ToList();

    /// <summary>The theme's line icons (Nocturne.Icons.*), as the templates use them.</summary>
    private static IEnumerable<(string Key, Geometry Geometry)> LineIcons()
    {
        var found = new SortedDictionary<string, Geometry>(StringComparer.Ordinal);
        foreach (var provider in Application.Current!.Styles.OfType<IResourceProvider>())
            Collect(provider, found);
        return found.Select(p => (p.Key["Nocturne.Icons.".Length..], p.Value));
    }

    private static void Collect(IResourceProvider? provider, IDictionary<string, Geometry> found)
    {
        switch (provider)
        {
            case Styles styles:
                Collect(styles.Resources, found);
                foreach (var child in styles.OfType<IResourceProvider>())
                    Collect(child, found);
                break;
            case ResourceInclude include: // MergeResourceInclude too
                Collect(include.Loaded as IResourceProvider, found);
                break;
            case ResourceDictionary dictionary:
                foreach (var key in dictionary.Keys)
                {
                    // TryGetResource creates a resource that nothing has used yet (the dictionary holds it deferred)
                    if (key is string name && name.StartsWith("Nocturne.Icons.", StringComparison.Ordinal)
                        && !name.StartsWith("Nocturne.Icons.Caption.", StringComparison.Ordinal)
                        && dictionary.TryGetResource(name, null, out var value) && value is Geometry geometry)
                        found[name] = geometry;
                }
                foreach (var merged in dictionary.MergedDictionaries)
                    Collect(merged, found);
                break;
        }
    }

    // One icon with its key under it
    private static Control IconCell(string key, Geometry geometry)
    {
        var name = new TextBlock
        {
            Text = key,
            Margin = new Thickness(0, 6, 0, 0),
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        name.Bind(TextBlock.ForegroundProperty, name.GetResourceObservable("Nocturne.Brushes.Text.Secondary"));
        return new StackPanel
        {
            Width = 96,
            Margin = new Thickness(0, 0, 0, 14),
            Children =
            {
                new StrokeIcon { Width = 20, Height = 20, HorizontalAlignment = HorizontalAlignment.Center, Data = geometry },
                name,
            },
        };
    }
}
