using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Demo;

public partial class MainWindow : NocturneWindow
{
    public MainWindow()
    {
        InitializeComponent();
        BackdropBox.ItemsSource = Enum.GetValues<WindowBackdrop>();
        TitleBarBox.ItemsSource = Enum.GetValues<TitleBarStyle>();
        CornerBox.ItemsSource = Enum.GetValues<WindowCornerStyle>();
        DataContext = new DemoData();
        ShowSystemAccent();
        NocturneTheme.SystemAccentChanged += (_, _) => ShowSystemAccent();

        if (Snapshot.Folder != null)
        {
            ContentRendered += (_, _) => Snapshot.Run(this);
        }
    }

    private void ShowSystemAccent()
    {
        var color = NocturneTheme.GetSystemAccent();
        SystemSwatch.Background = color is { } c ? new SolidColorBrush(c) : Brushes.Gray;
        SystemSwatch.IsEnabled = color != null;
    }

    private void OnAccentChecked(object sender, RoutedEventArgs e)
    {
        // The blue swatch is checked during InitializeComponent; blue is already the default.
        if (IsInitialized && sender is FrameworkElement { Tag: NocturneAccent accent })
        {
            NocturneTheme.SetAccent(accent);
        }
    }

    private readonly Stack<int> history = new();
    private bool goingBack;

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        if (history.Count > 0)
        {
            goingBack = true;
            Navigation.SelectedIndex = history.Pop();
            goingBack = false;
        }
    }

    private void OnNavigationToggleClick(object sender, RoutedEventArgs e)
    {
        var show = NavigationPane.Visibility != Visibility.Visible;
        NavigationPane.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        NavigationColumn.Width = show ? new GridLength(248) : new GridLength(24);
    }

    private void OnNavigationChanged(object sender, SelectionChangedEventArgs e)
    {
        // SelectionChanged fires during InitializeComponent, before the pages exist.
        if (!IsInitialized || ControlsPage == null)
        {
            return;
        }

        // Remember the page that was left, for the back button in the title bar.
        if (!goingBack && e.RemovedItems.Count > 0 && e.RemovedItems[0] is { } left && Navigation.Items.IndexOf(left) is >= 0 and var index)
        {
            history.Push(index);
        }
        BackButton.IsEnabled = history.Count > 0;

        // In the order of the navigation items.
        UIElement[] pages = [ControlsPage, MoreControlsPage, CustomControlsPage, GridPage, TokensPage, IconsPage];
        for (var i = 0; i < pages.Length; i++)
        {
            pages[i].Visibility = i == Math.Max(0, Navigation.SelectedIndex) ? Visibility.Visible : Visibility.Collapsed;
        }
        PageScroller.ScrollToTop();
    }
}
