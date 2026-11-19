using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Nocturne.Avalonia.Demo;

/// <summary>
/// The command-line commands that draw pictures instead of showing the demo: <c>--snapshot</c>
/// (<see cref="Snapshot"/>), <c>--render</c> and <c>--render-gallery</c> (<see cref="RenderCommand"/>). They run on Avalonia's
/// headless platform with Skia, so they need no screen and work the same on Windows, macOS and Linux.
/// Popups are drawn into the window (OverlayPopups), so they show in the pictures.
/// </summary>
internal static class Headless
{
    public static bool TryRun(string[] args, out int exitCode)
    {
        exitCode = 0;
        if (args.Length == 0 || args[0] is not ("--snapshot" or "--render" or "--render-gallery"))
            return false;

        AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false, OverlayPopups = true })
            .SetupWithoutStarting();
        // The headless platform cannot extend the client area into the frame; draw the title bar anyway.
        Controls.NocturneWindow.AlwaysDrawTitleBar = true;

        exitCode = args[0] switch
        {
            "--snapshot" => Snapshot.Run(args),
            "--render-gallery" => RenderCommand.RunGallery(args),
            _ => RenderCommand.Run(args),
        };
        return true;
    }

    /// <summary>
    /// Lets layout, bindings and the renderer settle, then takes the window's picture. Each frame
    /// moves the clock on by 1/60 s; the default (half a second) lets the theme's 120/150 ms transitions finish,
    /// also those that start a few frames late (a popup that opens).
    /// </summary>
    public static WriteableBitmap Capture(Window window, int frames = 30)
    {
        for (var i = 0; i < frames; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
        return window.CaptureRenderedFrame()
               ?? throw new InvalidOperationException("The headless renderer returned no frame.");
    }

    /// <summary>The value after <paramref name="name"/> in <paramref name="args"/>, if any.</summary>
    public static string? Option(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
