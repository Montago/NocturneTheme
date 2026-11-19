using Avalonia.Collections;
using Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Portal.Views;

public partial class ProductsView : UserControl
{
    private readonly DataGridCollectionView view;

    public ProductsView()
    {
        InitializeComponent();
        view = new DataGridCollectionView(SampleData.Products) { Filter = Matches };
        List.ItemsSource = view;
        List.SelectionChanged += (_, _) => LowStock.IsOpen = List.SelectedItem is Product { IsLow: true };
        List.SelectedIndex = 1;
        Search.TextChanged += (_, _) => view.Refresh();
        Category.SelectionChanged += (_, _) => view.Refresh();
    }

    private bool Matches(object item)
    {
        var product = (Product)item;
        var category = Category.SelectedIndex > 0 ? Category.SelectedItem as string : null;
        return product.Name.Contains(Search.Text ?? "", StringComparison.CurrentCultureIgnoreCase)
            && (category == null || product.Category == category);
    }
}
