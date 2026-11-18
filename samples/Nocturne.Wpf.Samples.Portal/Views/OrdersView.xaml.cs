using System.Windows.Controls;

namespace Nocturne.Wpf.Samples.Portal.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
        From.SelectedDate = new DateTime(2026, 11, 1);
        To.SelectedDate = new DateTime(2026, 11, 18);
        OrderGrid.SelectedIndex = 1;
    }
}
