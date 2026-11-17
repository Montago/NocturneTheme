using System.Windows;
using System.Windows.Controls;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Demo.Views;

public partial class GridPage : UserControl
{
    public GridPage()
    {
        InitializeComponent();
        Cars.ApplyFilters();
        OnSelectedItemChanged(Cars, new RoutedPropertyChangedEventArgs<object?>(null, null));
    }

    private void OnFiltered(object sender, GridFilteredEventArgs e)
    {
        if (Status != null)
        {
            Status.Text = $"Showing {e.Count:N0} of {e.TotalCount:N0} cars";
        }
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object?> e)
    {
        if (Selection != null)
        {
            Selection.Text = e.NewValue is Car car ? $"Selected: {car.StockNo}, {car.Make} {car.Model} {car.Year}" : "Nothing selected";
        }
    }
}
