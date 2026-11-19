namespace Nocturne.Web.Demo;

/// <summary>The fixed sample data of the WPF demo (src/Nocturne.Wpf.Demo/DemoData.cs) without its WPF types.</summary>
public static class DemoData
{
    public static IReadOnlyList<string> Languages { get; } = ["Dansk", "English", "Deutsch", "Svenska"];

    public static IReadOnlyList<Person> People { get; } =
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
    public static IReadOnlyList<Folder> Folders { get; } =
    [
        new("Projects", true,
        [
            new("Nocturne", true,
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
    public static IReadOnlyList<FileEntry> Files { get; } =
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
    public static IReadOnlyList<string> Cities { get; } =
    [
        "Aarhus", "Amsterdam", "Athens", "Barcelona", "Berlin", "Bergen", "Brussels", "Copenhagen", "Dublin", "Edinburgh",
        "Gothenburg", "Hamburg", "Helsinki", "Lisbon", "London", "Madrid", "Malmö", "Milan", "Munich", "Odense",
        "Oslo", "Paris", "Prague", "Reykjavík", "Rome", "Stockholm", "Tallinn", "Vienna", "Warsaw", "Zurich",
    ];

    /// <summary>Three days from today: selected in the calendar and the date picker.</summary>
    public static DateTime Meeting => DateTime.Today.AddDays(3);
}

public record Person(string Name, string Role, string Status, string LastActive);

public record Folder(string Name, bool IsExpanded = false, IReadOnlyList<Folder>? Children = null)
{
    public bool IsSelected { get; set; }
}

public record FileEntry(string Name, string Modified, string Kind, string Size, bool IsFolder);
