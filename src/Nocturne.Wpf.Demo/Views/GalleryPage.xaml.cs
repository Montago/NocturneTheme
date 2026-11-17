using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Nocturne.Wpf.Demo.Views;

/// <summary>One sample per control, rendered by --snapshot for the documentation.</summary>
public partial class GalleryPage : UserControl
{
    public GalleryPage()
    {
        InitializeComponent();
        DataContext = new DemoData();

        // Each grid gets its own array: grids on the same collection share its view, filters included.
        SearchGrid.ItemsSource = CarData.Cars.Take(400).ToArray();
        FilterGrid.ItemsSource = CarData.Cars.Take(200).ToArray();
        GroupGrid.ItemsSource = CarData.Cars.Where(c => c.Make is "Skoda" or "Tesla").Take(12).OrderBy(c => c.Make).ToArray();
        PagedGrid.ItemsSource = CarData.Cars.Skip(100).Take(95).ToArray();
        PagedGrid.PageIndex = 1;

        // Fixed dates keep the pictures the same from run to run.
        var day = new DateTime(2026, 11, 19);
        MeetingPicker.SelectedDate = day;
        MeetingPicker.DisplayDate = day;
        EndPicker.SelectedDate = new DateTime(2026, 11, 2);
        CreatedPicker.SelectedDate = new DateTime(2026, 10, 1);
        Calendar.DisplayDate = day;
        Calendar.SelectedDate = day;

        Icons.ItemsSource = IconKeys(Application.Current.Resources)
            .Where(k => !k.Key.StartsWith("Caption.", StringComparison.Ordinal))
            .OrderBy(k => k.Key, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>The samples; each one's <c>Tag</c> is its file name.</summary>
    public IEnumerable<ContentControl> Samples => SamplesHost.Children.OfType<ContentControl>();

    /// <summary>Every <c>Nocturne.Icons.*</c> geometry in the merged dictionaries, without the prefix.</summary>
    private static IEnumerable<KeyValuePair<string, Geometry>> IconKeys(ResourceDictionary dictionary)
    {
        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry is { Key: string key, Value: Geometry geometry } && key.StartsWith("Nocturne.Icons.", StringComparison.Ordinal))
            {
                yield return new(key["Nocturne.Icons.".Length..], geometry);
            }
        }
        foreach (var merged in dictionary.MergedDictionaries)
        {
            foreach (var icon in IconKeys(merged))
            {
                yield return icon;
            }
        }
    }
}
