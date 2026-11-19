using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Nocturne.Avalonia.Samples.Admin.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void OnGoToPage(object? sender, RoutedEventArgs e) => MainWindow.GoToPage(sender, e);
}
