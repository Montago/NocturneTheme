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

    /// <summary>Asks first, with the Danger style on the dialog's confirm button.</summary>
    private async void OnDeleteClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (System.Windows.Window.GetWindow(this) is not Nocturne.Wpf.Controls.NocturneWindow window)
        {
            return;
        }
        await window.ShowDialogAsync(new Nocturne.Wpf.Controls.NocturneDialog
        {
            Title = "Delete project?",
            Content = "The project and its files are removed for everyone. This cannot be undone.",
            AffirmativeText = "Delete",
            NegativeText = "Cancel",
            AffirmativeButtonStyle = (System.Windows.Style)FindResource("Nocturne.Styles.Button.Danger"),
        });
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
