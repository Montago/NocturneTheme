using System.Windows;
using System.Windows.Controls;
using Frosted.Wpf.Controls;

namespace Frosted.Wpf.Demo;

public partial class MainWindow : FrostedWindow
{
    public MainWindow()
    {
        InitializeComponent();
        BackdropBox.ItemsSource = Enum.GetValues<WindowBackdrop>();
        DataContext = new DemoData();

        if (Snapshot.Folder != null)
        {
            ContentRendered += (_, _) => Snapshot.Run(this);
        }
    }

    private void OnNavigationChanged(object sender, SelectionChangedEventArgs e)
    {
        // SelectionChanged fires during InitializeComponent, before the pages exist.
        if (!IsInitialized || ControlsPage == null)
        {
            return;
        }

        ControlsPage.Visibility = Navigation.SelectedIndex == 1 ? Visibility.Collapsed : Visibility.Visible;
        TokensPage.Visibility = Navigation.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        PageScroller.ScrollToTop();
    }
}
