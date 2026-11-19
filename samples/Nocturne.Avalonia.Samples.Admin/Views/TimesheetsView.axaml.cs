using Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Admin.Views;

public partial class TimesheetsView : UserControl
{
    public TimesheetsView()
    {
        InitializeComponent();
        Sheet.ItemsSource = SampleData.Timesheet;
    }
}
