using Avalonia.Controls;
using Avalonia.Interactivity;
using Nocturne.Avalonia.Controls;
using Nocturne.Avalonia.Demo.Views;

namespace Nocturne.Avalonia.Demo;

public partial class MainWindow : NocturneWindow
{
    private readonly Dictionary<string, Control> pages = new()
    {
        ["controls"] = new ControlsPage(),
        ["more"] = new MoreControlsPage(),
        ["custom"] = new CustomControlsPage(),
        ["grid"] = new GridPage(),
        ["icons"] = new IconsPage(),
        ["tokens"] = new TokensPage(),
    };

    public MainWindow()
    {
        InitializeComponent();

        BackdropBox.ItemsSource = Enum.GetValues<WindowBackdrop>();
        BackdropBox.SelectedItem = Backdrop;
        BackdropBox.SelectionChanged += (_, _) => { if (BackdropBox.SelectedItem is WindowBackdrop b) Backdrop = b; };

        RemoteBox.IsChecked = UseBackdropInRemoteSession;
        RemoteBox.IsCheckedChanged += (_, _) => UseBackdropInRemoteSession = RemoteBox.IsChecked == true;

        TitleBarBox.ItemsSource = Enum.GetValues<TitleBarStyle>();
        TitleBarBox.SelectedItem = TitleBarStyle;
        TitleBarBox.SelectionChanged += (_, _) => { if (TitleBarBox.SelectedItem is TitleBarStyle t) TitleBarStyle = t; };

        CornerBox.ItemsSource = Enum.GetValues<WindowCornerStyle>();
        CornerBox.SelectedItem = CornerStyle;
        CornerBox.SelectionChanged += (_, _) => { if (CornerBox.SelectedItem is WindowCornerStyle c) CornerStyle = c; };

        TintSlider.Value = TintOpacity;
        TintText.Text = $"{TintOpacity:P0}";
        TintSlider.ValueChanged += (_, e) =>
        {
            TintOpacity = e.NewValue;
            TintText.Text = $"{e.NewValue:P0}";
        };

        AccentBox.ItemsSource = Enum.GetValues<NocturneAccent>();
        AccentBox.SelectedItem = NocturneTheme.Accent;
        AccentBox.SelectionChanged += (_, _) => { if (AccentBox.SelectedItem is NocturneAccent a) NocturneTheme.SetAccent(a); };

        Pages.SelectionChanged += (_, _) => ShowPage((Pages.SelectedItem as ListBoxItem)?.Tag as string ?? "controls");
        ShowPage("controls");
    }

    /// <summary>Shows a page by its tag (controls, more, custom, icons, tokens).</summary>
    public void ShowPage(string tag)
    {
        PageHost.Content = pages[tag];
        PageScroller.Offset = default;
        if ((Pages.SelectedItem as ListBoxItem)?.Tag as string != tag)
            Pages.SelectedItem = Pages.Items.OfType<ListBoxItem>().First(i => (string?)i.Tag == tag);
    }

    private void OnNavigationToggleClick(object? sender, RoutedEventArgs e)
    {
        var column = Layout.ColumnDefinitions[0];
        var show = column.Width.Value == 0;
        column.Width = new GridLength(show ? 248 : 0);
        NavigationPane.IsVisible = show;
    }
}
