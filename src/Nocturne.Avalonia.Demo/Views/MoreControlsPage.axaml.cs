using Avalonia.Controls;

namespace Nocturne.Avalonia.Demo.Views;

public partial class MoreControlsPage : UserControl
{
    public MoreControlsPage()
    {
        InitializeComponent();

        FolderTree.ItemsSource = DemoData.Folders;
        // Projects > Nocturne > Screenshots
        FolderTree.SelectedItem = DemoData.Folders[0].Items[0].Items[1];

        FileList.ItemsSource = DemoData.Files;
        FileList.SelectedIndex = 1;

        RowHeaderGrid.ItemsSource = DemoData.People;
        RowHeaderGrid.SelectedIndex = 1;

        // Fixed dates, so the pictures do not change with the day
        var meeting = new DateTime(2026, 11, 22);
        MeetingCalendar.DisplayDate = meeting;
        MeetingCalendar.SelectedDate = meeting;
        MeetingCalendar.IsTodayHighlighted = false;
        MeetingPicker.SelectedDate = meeting;
    }
}
