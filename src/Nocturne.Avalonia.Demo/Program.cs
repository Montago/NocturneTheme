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
