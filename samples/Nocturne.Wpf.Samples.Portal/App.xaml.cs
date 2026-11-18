using System.Windows;

namespace Nocturne.Wpf.Samples.Portal;

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

        // --snapshot <folder>: one picture per page.
        WindowSnapshot.ShowOffScreen(window);
        WindowSnapshot.Run(async () =>
        {
            foreach (var page in window.PageNames)
            {
                window.ShowPage(page);
                await WindowSnapshot.SaveAsync(window, "portal-" + page.ToLowerInvariant());
            }
        });
    }
}
