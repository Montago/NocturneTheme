using System.Globalization;
using Avalonia;

namespace Nocturne.Avalonia.Demo;

internal static class Program
{
    /// <summary>
    /// Starts the demo, or with <c>--snapshot</c> / <c>--render</c> draws pictures off screen and
    /// exits (<see cref="Snapshot"/>, <see cref="RenderCommand"/>).
    /// </summary>
    [STAThread]
    public static int Main(string[] args)
    {
        // The demo is in English; format numbers and dates the same way everywhere (month and day names too).
        var culture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = culture;

        if (Headless.TryRun(args, out var exitCode))
            return exitCode;
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>Used by the app and by the designer.</summary>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
