using Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Portal.Views;

public partial class CustomersView : UserControl
{
    public CustomersView()
    {
        InitializeComponent();
        // Germany's first customer
        Tree.SelectedItem = SampleData.Countries.First(c => c.Name == "Germany").Customers[0];
    }
}
