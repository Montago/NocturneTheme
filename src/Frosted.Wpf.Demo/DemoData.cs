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
