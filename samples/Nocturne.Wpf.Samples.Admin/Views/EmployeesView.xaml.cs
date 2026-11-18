using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Samples.Admin.Views;

public partial class EmployeesView : UserControl
{
    private readonly ObservableCollection<Employee> employees = new(SampleData.Employees);
    private NocturneDialog? dialog;

    public EmployeesView()
    {
        InitializeComponent();
        List.ItemsSource = employees;
        List.SelectedItem = employees.First(e => e.Name == "Heather Andrade");
        ShowCount(employees.Count, employees.Count);
    }

    private void OnFiltered(object? sender, GridFilteredEventArgs e) => ShowCount(e.Count, e.TotalCount);

    private void ShowCount(int shown, int total) =>
        Count.Text = shown == total ? $"{total} employees" : $"{shown} of {total} employees";

    private void OnSave(object sender, RoutedEventArgs e) => Saved.IsOpen = true;

    private async void OnDelete(object sender, RoutedEventArgs e) => await ConfirmDeleteAsync();

    /// <summary>Asks before removing the selected employee.</summary>
    public async Task ConfirmDeleteAsync()
    {
        if (List.SelectedItem is not Employee employee || Window.GetWindow(this) is not NocturneWindow window)
        {
            return;
        }
        dialog = new NocturneDialog
        {
            Title = "Delete employee?",
            Content = $"{employee.Name} will be removed from the staff list. Approved timesheets stay in the archive.",
            AffirmativeText = "Delete",
            NegativeText = "Cancel",
            AffirmativeButtonStyle = (Style)FindResource("Nocturne.Styles.Button.Danger"),
        };
        if (await window.ShowDialogAsync(dialog) == MessageDialogResult.Affirmative)
        {
            employees.Remove(employee);
        }
        dialog = null;
    }

    /// <summary>Closes an open delete dialog without deleting (for --snapshot).</summary>
    public void CloseDialog() => dialog?.Close(MessageDialogResult.Canceled);
}
