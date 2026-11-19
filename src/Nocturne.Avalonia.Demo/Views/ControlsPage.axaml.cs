using Avalonia.Controls;
using Nocturne.Avalonia.Helpers;

namespace Nocturne.Avalonia.Demo.Views;

public partial class ControlsPage : UserControl
{
    private int added;

    public ControlsPage()
    {
        InitializeComponent();

        // The document tabs' "+" button asks for a new tab
        TabHelper.AddAddTabRequestedHandler(DocumentTabs, (_, _) =>
        {
            var tab = new TabItem
            {
                Header = $"New {++added}",
                Content = new TextBlock { Text = "A new tab." },
            };
            DocumentTabs.Items.Add(tab);
            DocumentTabs.SelectedItem = tab;
        });
    }
}
