using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Samples;

/// <summary>
/// Support for <c>--snapshot &lt;folder&gt;</c>: the Avalonia sample apps draw their pages to PNG on
/// Avalonia's headless platform (Skia, no screen) and exit. Not needed in a real application.
/// </summary>
internal static class SampleSnapshot
{
    /// <summary>The folder after <c>--snapshot</c>, or null when the app runs normally.</summary>
    public static string? Folder(string[] args)
    {
        var index = Array.IndexOf(args, "--snapshot");
        return index < 0 ? null : index + 1 < args.Length ? args[index + 1] : "snapshots";
    }

    /// <summary>The samples are English apps with prices in US dollars: dates and numbers use en-US everywhere.</summary>
    public static void UseEnglish()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = culture;
    }

    /// <summary>
    /// Starts <typeparamref name="TApp"/> on the headless platform, shows the window from
    /// <paramref name="create"/> at 1280 × 800 and lets <paramref name="pages"/> save pictures with the
    /// action it is given (a file name without .png). Popups are drawn into the window.
    /// </summary>
    public static int Run<TApp>(string folder, Func<NocturneWindow> create, Action<NocturneWindow, Action<string>> pages)
        where TApp : Application, new()
    {
        AppBuilder.Configure<TApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false, OverlayPopups = true })
            .SetupWithoutStarting();
        // The headless platform cannot extend the client area into the frame; draw the title bar anyway.
        NocturneWindow.AlwaysDrawTitleBar = true;

        Directory.CreateDirectory(folder);
        var window = create();
        window.Width = 1280;
        window.Height = 800;
        window.Show();
        pages(window, name =>
        {
            var path = Path.Combine(Path.GetFullPath(folder), name + ".png");
            Capture(window).Save(path, PngBitmapEncoderOptions.Default);
            Console.WriteLine(path);
        });
        window.Close();
        return 0;
    }

    /// <summary>Lets layout, bindings and the 120/150 ms transitions settle (30 frames), then takes the picture.</summary>
    public static WriteableBitmap Capture(TopLevel window, int frames = 30)
    {
        for (var i = 0; i < frames; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
        return window.CaptureRenderedFrame() ?? throw new InvalidOperationException("The headless renderer returned no frame.");
    }
}
