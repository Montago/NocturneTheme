using Avalonia.Collections;
using Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Portal.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
        From.SelectedDate = new DateTime(2026, 11, 1);
        To.SelectedDate = new DateTime(2026, 11, 18);

        // The customer box narrows the orders to the customers whose name contains the text
        var orders = new DataGridCollectionView(SampleData.Orders)
        {
            Filter = o => ((Order)o).Customer.Contains(CustomerBox.Text ?? "", StringComparison.CurrentCultureIgnoreCase),
        };
        OrderGrid.ItemsSource = orders;
        CustomerBox.TextChanged += (_, _) => orders.Refresh();
        OrderGrid.SelectionChanged += (_, _) => Lines.ItemsSource = (OrderGrid.SelectedItem as Order)?.Lines;
        OrderGrid.SelectedIndex = 1;
    }
}
