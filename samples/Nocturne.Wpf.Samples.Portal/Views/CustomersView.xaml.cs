using System.Windows.Controls;
using System.Windows.Threading;

namespace Nocturne.Wpf.Samples.Portal.Views;

public partial class CustomersView : UserControl
{
    public CustomersView()
    {
        InitializeComponent();
        // The page starts hidden, and a hidden tree makes no item containers: select once it shows.
        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible)
            {
                Dispatcher.BeginInvoke(SelectCustomer, DispatcherPriority.Loaded);
            }
        };
    }

    // TreeView has no SelectedIndex; select a customer (Germany's first) once the containers exist.
    private void SelectCustomer()
    {
        if (Tree.SelectedItem != null
            || Tree.ItemContainerGenerator.ContainerFromIndex(3) is not TreeViewItem country)
        {
            return;
        }
        country.UpdateLayout();
        if (country.ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem customer)
        {
            customer.IsSelected = true;
        }
    }
}
