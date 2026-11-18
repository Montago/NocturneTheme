using System.Windows;

namespace Nocturne.Wpf.Samples.Admin;

public partial class App : Application
{
    public App()
    {
        WindowSnapshot.UseEnglish();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new MainWindow();
        MainWindow = window;
        if (WindowSnapshot.Folder == null)
        {
            window.Show();
            return;
        }

        // --snapshot <folder>: one picture per page, plus the settings flyout and a dialog.
        WindowSnapshot.ShowOffScreen(window);
        WindowSnapshot.Run(async () =>
        {
            foreach (var page in window.PageNames)
            {
                window.ShowPage(page);
                await WindowSnapshot.SaveAsync(window, "admin-" + page.ToLowerInvariant());
            }
            window.ShowPage("Employees");
            window.Settings.IsOpen = true;
            await WindowSnapshot.SaveAsync(window, "admin-settings");
            window.Settings.IsOpen = false;
            var answer = window.EmployeesPage.ConfirmDeleteAsync();
            await WindowSnapshot.SaveAsync(window, "admin-dialog");
            window.EmployeesPage.CloseDialog();
            await answer;
        });
    }
}
