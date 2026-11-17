using System.Windows.Controls;
using System.Windows.Input;

namespace Nocturne.Wpf.Demo.Views;

public partial class ControlsPage : UserControl
{
    private int newTabCount;

    public ControlsPage()
    {
        InitializeComponent();
    }

    /// <summary>The "+" after the document tabs: add an empty tab and select it.</summary>
    private void OnAddTab(object sender, ExecutedRoutedEventArgs e)
    {
        newTabCount++;
        var tab = new TabItem
        {
            Header = $"New tab {newTabCount}",
            Content = new TextBlock { Text = "No content yet", Style = (System.Windows.Style)FindResource("Demo.Key") },
        };
        DocumentTabs.Items.Add(tab);
        DocumentTabs.SelectedItem = tab;
    }
}
