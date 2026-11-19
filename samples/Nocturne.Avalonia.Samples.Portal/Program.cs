using Avalonia;

namespace Nocturne.Avalonia.Samples.Portal;

internal static class Program
{
    /// <summary>Starts the app, or with <c>--snapshot &lt;folder&gt;</c> draws its pages to PNG and exits.</summary>
    [STAThread]
    public static int Main(string[] args)
    {
        SampleSnapshot.UseEnglish();
        if (SampleSnapshot.Folder(args) is { } folder)
            return SampleSnapshot.Run<App>(folder, () => new MainWindow(), MainWindow.SavePictures);
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>Used by the app and by the designer.</summary>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
