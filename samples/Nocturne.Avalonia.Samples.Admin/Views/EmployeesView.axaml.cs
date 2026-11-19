using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Admin.Views;

public partial class EmployeesView : UserControl
{
    private readonly ObservableCollection<Employee> employees = new(SampleData.Employees);
    private NocturneDialog? dialog;

    public EmployeesView()
    {
        InitializeComponent();

        // NocturneGrid searches every word in any column but the dates, and filters from its headers
        List.ItemsSource = employees;
        List.SelectedItem = employees.First(e => e.Name == "Heather Andrade");
        List.Filtered += (_, e) => ShowCount(e.Count, e.TotalCount);
        List.PropertyChanged += (_, e) =>
        {
            // A row added or removed changes the counts without a new filter
            if (e.Property == NocturneGrid.FilteredItemCountProperty || e.Property == NocturneGrid.TotalItemCountProperty)
                ShowCount(List.FilteredItemCount, List.TotalItemCount);
        };
        ShowCount(employees.Count, employees.Count);
    }

    private void ShowCount(int shown, int total) =>
        Count.Text = shown == total ? $"{total} employees" : $"{shown} of {total} employees";

    private void OnSave(object? sender, RoutedEventArgs e) => Saved.IsOpen = true;

    private async void OnDelete(object? sender, RoutedEventArgs e) => await ConfirmDeleteAsync();

    /// <summary>Asks before removing the selected employee.</summary>
    public async Task ConfirmDeleteAsync()
    {
        if (List.SelectedItem is not Employee employee || TopLevel.GetTopLevel(this) is not NocturneWindow window)
            return;
        dialog = new NocturneDialog
        {
            Title = "Delete employee?",
            Content = $"{employee.Name} will be removed from the staff list. Approved timesheets stay in the archive.",
            AffirmativeText = "Delete",
            NegativeText = "Cancel",
            AffirmativeButtonTheme = Application.Current!.TryFindResource("Nocturne.Styles.Button.Danger", out var danger) ? danger as ControlTheme : null,
        };
        if (await window.ShowDialogAsync(dialog) == MessageDialogResult.Affirmative)
            employees.Remove(employee);
        dialog = null;
    }

    /// <summary>Closes an open delete dialog without deleting (for --snapshot).</summary>
    public void CloseDialog() => dialog?.Close(MessageDialogResult.Canceled);
}
