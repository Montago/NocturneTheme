namespace Nocturne.Wpf.Samples.Admin;

/// <summary>The choices of the web pages' filter boxes, which the WPF sample does not need.</summary>
public static class FilterLists
{
    public static IReadOnlyList<string> DepartmentFilter { get; } = ["All departments", .. SampleData.Departments];

    public static IReadOnlyList<string> Statuses { get; } = ["All statuses", "Draft", "Submitted", "Approved", "Rejected"];
}
