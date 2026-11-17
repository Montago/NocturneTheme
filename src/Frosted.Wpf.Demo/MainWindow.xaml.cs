using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Frosted.Wpf.Controls;

namespace Frosted.Wpf.Demo;

public partial class MainWindow : FrostedWindow
{
    public MainWindow()
    {
        InitializeComponent();
        BackdropBox.ItemsSource = Enum.GetValues<WindowBackdrop>();
        DataContext = new DemoData();
        ShowSystemAccent();
        FrostedTheme.SystemAccentChanged += (_, _) => ShowSystemAccent();

        if (Snapshot.Folder != null)
        {
            ContentRendered += (_, _) => Snapshot.Run(this);
        }
    }

    private void ShowSystemAccent()
    {
        var color = FrostedTheme.GetSystemAccent();
        SystemSwatch.Background = color is { } c ? new SolidColorBrush(c) : Brushes.Gray;
        SystemSwatch.IsEnabled = color != null;
    }

    private void OnAccentChecked(object sender, RoutedEventArgs e)
    {
        // The first swatch is checked during InitializeComponent; green is already the default.
        if (IsInitialized && sender is FrameworkElement { Tag: FrostedAccent accent })
        {
            FrostedTheme.SetAccent(accent);
        }
    }

    private void OnNavigationChanged(object sender, SelectionChangedEventArgs e)
    {
        // SelectionChanged fires during InitializeComponent, before the pages exist.
        if (!IsInitialized || ControlsPage == null)
        {
            return;
        }

        // In the order of the navigation items.
        UIElement[] pages = [ControlsPage, MoreControlsPage, TokensPage];
        for (var i = 0; i < pages.Length; i++)
        {
            pages[i].Visibility = i == Math.Max(0, Navigation.SelectedIndex) ? Visibility.Visible : Visibility.Collapsed;
        }
        PageScroller.ScrollToTop();
    }
}
