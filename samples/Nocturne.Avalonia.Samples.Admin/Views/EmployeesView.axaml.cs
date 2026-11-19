using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Admin.Views;

public partial class EmployeesView : UserControl
{
    private readonly ObservableCollection<Employee> employees = new(SampleData.Employees);
    private readonly DataGridCollectionView view;
    private NocturneDialog? dialog;

    public EmployeesView()
    {
        InitializeComponent();

        // The search box filters the rows on every word, in any column but the dates
        view = new DataGridCollectionView(employees) { Filter = Matches };
        List.ItemsSource = view;
        List.SelectedItem = employees.First(e => e.Name == "Heather Andrade");
        Search.TextChanged += (_, _) =>
        {
            view.Refresh();
            ShowCount();
        };
        employees.CollectionChanged += (_, _) => ShowCount();
        ShowCount();
    }

    private bool Matches(object item)
    {
        var words = (Search.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return item is Employee e && words.All(w =>
            e.Name.Contains(w, StringComparison.CurrentCultureIgnoreCase)
            || e.Title.Contains(w, StringComparison.CurrentCultureIgnoreCase)
            || e.Department.Contains(w, StringComparison.CurrentCultureIgnoreCase));
    }

    private void ShowCount()
    {
        var shown = view.Count;
        Count.Text = shown == employees.Count ? $"{employees.Count} employees" : $"{shown} of {employees.Count} employees";
    }

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
