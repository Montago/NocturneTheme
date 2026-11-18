using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Samples;

/// <summary>
/// Support for <c>--snapshot &lt;folder&gt;</c>: the sample apps render their pages to PNG for the
/// documentation and exit. Not needed in a real application.
/// </summary>
internal static class WindowSnapshot
{
    /// <summary>The folder after <c>--snapshot</c>, or null when the app runs normally.</summary>
    public static string? Folder { get; } = GetFolder();

    private static string? GetFolder()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "--snapshot");
        return index < 0 ? null : index + 1 < args.Length ? args[index + 1] : "snapshots";
    }

    /// <summary>
    /// The samples are English apps with prices in US dollars: dates and numbers are formatted with
    /// en-US everywhere. Call from the App constructor.
    /// </summary>
    public static void UseEnglish()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = culture;
        // Bindings format with the element's Language, which is en-US unless it is set.
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
    }

    /// <summary>
    /// Shows the window off the screen at a fixed size. RenderTargetBitmap cannot see the Windows
    /// backdrop, so the window paints its opaque fallback ground instead.
    /// </summary>
    public static void ShowOffScreen(NocturneWindow window, double width = 1280, double height = 800)
    {
        window.Backdrop = WindowBackdrop.None;
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = -20000;
        window.Top = 0;
        window.Width = width;
        window.Height = height;
        window.Show();
    }

    /// <summary>Runs <paramref name="pages"/>, writes error.txt if it fails, and shuts the app down.</summary>
    public static async void Run(Func<Task> pages)
    {
        Directory.CreateDirectory(Folder!);
        try
        {
            await pages();
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(Folder!, "error.txt"), ex.ToString());
        }
        Application.Current.Shutdown();
    }

    /// <summary>Waits for layout and the 120-220 ms transitions to settle, then saves the window as PNG.</summary>
    public static async Task SaveAsync(Window window, string name, int delay = 400)
    {
        window.UpdateLayout();
        await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        await Task.Delay(delay);
        FreezeAnimations(window);
        await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

        var root = (FrameworkElement)VisualTreeHelper.GetChild(window, 0);
        var dpi = VisualTreeHelper.GetDpi(root);
        var bitmap = new RenderTargetBitmap(
            (int)Math.Ceiling(root.ActualWidth * dpi.DpiScaleX), (int)Math.Ceiling(root.ActualHeight * dpi.DpiScaleY),
            dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        bitmap.Render(root);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        await using var stream = File.Create(Path.Combine(Folder!, name + ".png"));
        encoder.Save(stream);
    }

    /// <summary>
    /// Stops the endless template animations (ProgressRing, indeterminate ProgressBar) at a fixed
    /// point, so a page gives the same picture on every run.
    /// </summary>
    private static void FreezeAnimations(DependencyObject root)
    {
        foreach (var control in Descendants(root).OfType<Control>())
        {
            if (control.Template is not { } template)
            {
                continue;
            }
            var storyboards = template.Triggers
                .SelectMany(t => t.EnterActions.Concat(t.ExitActions))
                .OfType<BeginStoryboard>()
                .Select(b => b.Storyboard)
                .Where(s => s is { RepeatBehavior.HasDuration: false, RepeatBehavior.HasCount: false });
            foreach (var storyboard in storyboards)
            {
                // Does nothing (apart from a trace message) when the storyboard is not running here.
                storyboard.SeekAlignedToLastTick(control, TimeSpan.FromMilliseconds(700), TimeSeekOrigin.BeginTime);
                storyboard.Pause(control);
            }
        }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var d in Descendants(child))
            {
                yield return d;
            }
        }
    }
}
