using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;

namespace Frosted.Wpf.Demo;

/// <summary>Fixed sample data for the demo pages.</summary>
public class DemoData : INotifyPropertyChanged
{
    private string port = "99999";
    private double bandwidth = 62;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ServerName { get; set; } = "intranet.local";

    /// <summary>Starts out of range, so the field shows its error state.</summary>
    public string Port
    {
        get => port;
        set => Set(ref port, value);
    }

    public double Bandwidth
    {
        get => bandwidth;
        set => Set(ref bandwidth, value);
    }

    public IReadOnlyList<string> Languages { get; } = ["Dansk", "English", "Deutsch", "Svenska"];

    public string Language { get; set; } = "English";

    public IReadOnlyList<Person> People { get; } =
    [
        new("Anna Holm", "Administrator", "Active", "Today 09:14"),
        new("Ida Krogh", "Support", "Active", "Nov 12"),
        new("Jonas Berg", "Developer", "Active", "Yesterday 17:40"),
        new("Karen Juhl", "Designer", "Active", "Nov 15"),
        new("Lars Mose", "Reader", "Invited", "—"),
        new("Mikkel Dahl", "Developer", "Blocked", "Nov 3"),
        new("Nina Friis", "Administrator", "Active", "Today 08:02"),
        new("Ole Skov", "Support", "Invited", "—"),
        new("Sofie Lund", "Reader", "Invited", "—"),
        new("Thomas Bak", "Developer", "Active", "Nov 16"),
    ];

    /// <summary>The folder tree on the "More controls" page.</summary>
    public IReadOnlyList<Folder> Folders { get; } =
    [
        new("Projects", true,
        [
            new("Frosted Glass", true,
            [
                new("Design"),
                new("Screenshots") { IsSelected = true },
                new("Source"),
            ]),
            new("Website relaunch"),
            new("Archive", false, [new("2025"), new("2024")]),
        ]),
        new("Documents", false, [new("Invoices"), new("Contracts")]),
        new("Pictures"),
        new("Downloads"),
    ];

    /// <summary>The files in the selected folder.</summary>
    public IReadOnlyList<FileEntry> Files { get; } =
    [
        new("controls.png", "Today 07:12", "PNG image", "412 KB", false),
        new("controls-bottom.png", "Today 07:12", "PNG image", "388 KB", false),
        new("Exports", "Yesterday 16:40", "Folder", "", true),
        new("tokens.png", "Today 07:12", "PNG image", "296 KB", false),
        new("light.png", "Nov 14", "PNG image", "1.2 MB", false),
        new("popup-combobox.png", "Nov 14", "PNG image", "38 KB", false),
        new("Notes.md", "Nov 12", "Markdown file", "4 KB", false),
    ];

    /// <summary>Suggestions for the AutoSuggestBox.</summary>
    public IReadOnlyList<string> Cities { get; } =
    [
        "Aarhus", "Amsterdam", "Athens", "Barcelona", "Berlin", "Bergen", "Brussels", "Copenhagen", "Dublin", "Edinburgh",
        "Gothenburg", "Hamburg", "Helsinki", "Lisbon", "London", "Madrid", "Malmö", "Milan", "Munich", "Odense",
        "Oslo", "Paris", "Prague", "Reykjavík", "Rome", "Stockholm", "Tallinn", "Vienna", "Warsaw", "Zurich",
    ];

    /// <summary>Three days from today: selected in the calendar and the date picker.</summary>
    public DateTime? Meeting { get; set; } = DateTime.Today.AddDays(3);

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

public record Person(string Name, string Role, string Status, string LastActive);

public record Folder(string Name, bool IsExpanded = false, IReadOnlyList<Folder>? Children = null)
{
    public bool IsSelected { get; set; }
}

public record FileEntry(string Name, string Modified, string Kind, string Size, bool IsFolder);

/// <summary>A TCP port: a whole number from 1 to 65535.</summary>
public class PortRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) =>
        int.TryParse(value as string, NumberStyles.None, cultureInfo, out var port) && port is >= 1 and <= 65535
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "Must be a number from 1 to 65535.");
}

/// <summary>true → "On", false → "Off" (the line under a switch).</summary>
public class OnOffConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is true ? "On" : "Off";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
