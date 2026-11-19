using Avalonia.Collections;
using Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Admin.Views;

public partial class TimesheetsView : UserControl
{
    public TimesheetsView()
    {
        InitializeComponent();
        var rows = new DataGridCollectionView(SampleData.Timesheet);
        rows.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(TimesheetRow.Employee)));
        Sheet.ItemsSource = rows;
        Sheet.SelectedItem = null;
    }
}
