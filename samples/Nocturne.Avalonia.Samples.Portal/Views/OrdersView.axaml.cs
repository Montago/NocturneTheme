using Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Portal.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
        From.SelectedDate = new DateTime(2026, 11, 1);
        To.SelectedDate = new DateTime(2026, 11, 18);

        OrderGrid.ItemsSource = SampleData.Orders;
        OrderGrid.SelectionChanged += (_, _) => Lines.ItemsSource = (OrderGrid.SelectedItem as Order)?.Lines;
        OrderGrid.SelectedIndex = 1;
    }
}
