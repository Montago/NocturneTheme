using System.Windows;
using System.Windows.Controls;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Demo.Views;

public partial class CustomControlsPage : UserControl
{
    public CustomControlsPage()
    {
        InitializeComponent();
    }

    private async void OnShowDialog(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is not NocturneWindow window)
        {
            return;
        }

        var result = await window.ShowMessageAsync(
            "Discard changes?",
            "You have changed 3 files since they were last synchronised. Discarding puts back the versions on the server.",
            "Discard",
            "Keep editing");
        DialogResultText.Text = $"The answer was {result}.";
    }

    private void OnOpenFlyout(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainWindow window)
        {
            window.SettingsFlyout.IsOpen = true;
        }
    }
}
