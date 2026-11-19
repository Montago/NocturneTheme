using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Nocturne.Avalonia.Samples.Portal.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private void OnGoToPage(object? sender, RoutedEventArgs e) => MainWindow.GoToPage(sender, e);
}
