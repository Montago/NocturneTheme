using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Nocturne.Wpf.Controls;
using Nocturne.Wpf.Demo.Views;

namespace Nocturne.Wpf.Demo;

/// <summary>
/// Quick pictures of one control, without the demo window:
/// <list type="bullet">
/// <item><c>--render &lt;snippet.xaml&gt; &lt;out.png&gt;</c>: one XAML element (no xmlns needed; <c>nc:</c>
/// and <c>demo:</c> are declared) on the window ground, 16px around it.</item>
/// <item><c>--render-gallery &lt;Tag[,Tag…]&gt; &lt;folder&gt;</c>: those samples of Views/GalleryPage.xaml,
/// as <c>--snapshot</c> draws them, to <c>&lt;folder&gt;/&lt;Tag&gt;.png</c>.</item>
/// </list>
/// Options: <c>--scale 4</c> (more pixels for details), <c>--time 0,250,500</c> (where the endless
/// animations stop, in ms; several give one picture each, <c>out-250.png</c>), <c>--accent Green</c>,
/// <c>--card</c> (snippet on a card, as in the gallery). An error goes to <c>&lt;out&gt;.error.txt</c>
/// and the exit code is 1.
/// </summary>
internal static class RenderCommand
{
    private const string Namespaces =
        "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
        "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +
        "xmlns:nc=\"urn:nocturne-wpf\" " +
        "xmlns:demo=\"clr-namespace:Nocturne.Wpf.Demo;assembly=Nocturne.Wpf.Demo\"";

    /// <summary>Starts the render when the command line asks for one; false leaves the demo to start.</summary>
    public static bool TryStart(Application app, string[] args)
    {
        var snippet = Array.IndexOf(args, "--render");
        var gallery = Array.IndexOf(args, "--render-gallery");
        var at = snippet >= 0 ? snippet : gallery;
        if (at < 0)
        {
            return false;
        }
        if (at + 2 >= args.Length)
        {
            Environment.Exit(2);
        }

        var source = args[at + 1];
        var target = Path.GetFullPath(args[at + 2]);
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        app.Dispatcher.InvokeAsync(async () =>
        {
            var code = 0;
            try
            {
                var scale = double.Parse(Option(args, "--scale") ?? "1", CultureInfo.InvariantCulture);
                var times = (Option(args, "--time") ?? "700").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => TimeSpan.FromMilliseconds(double.Parse(t, CultureInfo.InvariantCulture))).ToList();
                if (Option(args, "--accent") is { } accent)
                {
                    NocturneTheme.SetAccent(Enum.Parse<NocturneAccent>(accent, ignoreCase: true));
                }

                if (snippet >= 0)
                {
                    await RenderSnippetAsync(source, target, scale, times, args.Contains("--card"));
                }
                else
                {
                    var tags = source.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    await GallerySnapshot.SaveAsync(target, tags, scale);
                }
            }
            catch (Exception ex)
            {
                var error = snippet >= 0 ? target + ".error.txt" : Path.Combine(target, "error.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(error)!);
                File.WriteAllText(error, ex.ToString());
                code = 1;
            }
            app.Shutdown(code);
        });
        return true;
    }

    private static async Task RenderSnippetAsync(string file, string target, double scale, List<TimeSpan> times, bool card)
    {
        var xaml = File.ReadAllText(file);
        var wrapped = card
            ? $"<ContentControl {Namespaces} Style=\"{{StaticResource Demo.Sample}}\" Width=\"Auto\">{xaml}</ContentControl>"
            : $"<Border {Namespaces} Padding=\"16\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\"><AdornerDecorator>{xaml}</AdornerDecorator></Border>";
        var sample = (FrameworkElement)XamlReader.Parse(wrapped);

        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var window = GallerySnapshot.CreateWindow(sample);
        window.Show();
        try
        {
            foreach (var time in times)
            {
                var name = times.Count == 1
                    ? target
                    : Path.Combine(Path.GetDirectoryName(target)!, $"{Path.GetFileNameWithoutExtension(target)}-{time.TotalMilliseconds:0}.png");
                await GallerySnapshot.CaptureAsync(window, sample, name, scale, time);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static string? Option(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
