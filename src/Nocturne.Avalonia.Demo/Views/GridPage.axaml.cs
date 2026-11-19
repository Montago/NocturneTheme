using Avalonia.Controls;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo.Views;

public partial class GridPage : UserControl
{
    public GridPage()
    {
        InitializeComponent();
        Cars.Filtered += (_, e) => Status.Text = $"Showing {e.Count:N0} of {e.TotalCount:N0} cars";
        Cars.SelectedItemChanged += (_, e) => ShowSelection(e.NewValue);
        GroupBy.SelectionChanged += (_, _) => Cars.GroupMemberPath = (GroupBy.SelectedItem as ComboBoxItem)?.Tag as string;
        PageSizeBox.SelectionChanged += (_, _) => Cars.PageSize = int.Parse((PageSizeBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "0");
        ClearButton.Click += (_, _) => Cars.ClearFilters();

        Cars.PageSize = 100;
        Cars.ItemsSource = CarData.Cars;
        Cars.ApplyFilters();
        ShowSelection(null);
    }

    /// <summary>The grid of the page, for the snapshot's pictures and checks.</summary>
    public NocturneGrid Grid => Cars;

    private void ShowSelection(object? item) =>
        Selection.Text = item is Car car ? $"Selected: {car.StockNo}, {car.Make} {car.Model} {car.Year}" : "Nothing selected";
}
