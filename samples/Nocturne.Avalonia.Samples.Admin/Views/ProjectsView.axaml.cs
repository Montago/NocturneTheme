using Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Admin.Views;

public partial class ProjectsView : UserControl
{
    public ProjectsView()
    {
        InitializeComponent();
        Tree.SelectedItem = SampleData.Customers[0].Projects[0];
    }
}
