using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo;

/// <summary>
/// <c>--snapshot &lt;folder&gt; [--scale 1]</c>: draws the demo to PNG files in the folder and exits:
/// the pages (the controls page at the top and the bottom), a combo box and a menu drawn open,
/// the controls page in every accent and the Separated and Accent title bars. The headless
/// platform has no backdrop, so the pictures show the opaque fallback ground. Keep them out of the repo.
/// </summary>
internal static class Snapshot
{
    public static int Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: --snapshot <folder> [--scale 1]");
            return 2;
        }

        var folder = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(folder);
        var scale = double.Parse(Headless.Option(args, "--scale") ?? "1", CultureInfo.InvariantCulture);

        var window = new MainWindow();
        window.SetRenderScaling(scale);
        window.Show();

        Save(window, folder, "controls.png");
        ScrollToEnd(window);
        Save(window, folder, "controls-bottom.png");
        ScrollToStart(window);

        // Popups are drawn into the window (OverlayPopups)
        var combo = window.GetVisualDescendants().OfType<ComboBox>().First(c => c.SelectedIndex == 1);
        combo.IsDropDownOpen = true;
        Save(window, folder, "popup-combobox.png");
        combo.IsDropDownOpen = false;

        var file = window.GetVisualDescendants().OfType<Menu>().First().Items.OfType<MenuItem>().First();
        file.Open();
        Save(window, folder, "popup-menu.png");
        file.Close();
        ScrollToStart(window);

        foreach (var accent in new[] { NocturneAccent.Green, NocturneAccent.Orange, NocturneAccent.Yellow, NocturneAccent.Teal })
        {
            NocturneTheme.SetAccent(accent);
            Save(window, folder, $"accent-{accent.ToString().ToLowerInvariant()}.png");
        }
        NocturneTheme.SetAccent(NocturneAccent.Blue);

        foreach (var style in new[] { TitleBarStyle.Separated, TitleBarStyle.Accent })
        {
            window.TitleBarStyle = style;
            Save(window, folder, $"titlebar-{style.ToString().ToLowerInvariant()}.png");
        }
        window.TitleBarStyle = TitleBarStyle.Seamless;

        window.ShowPage("tokens");
        Save(window, folder, "tokens.png");

        window.Close();
        return 0;
    }

    private static void ScrollToEnd(MainWindow window)
    {
        var scroller = window.FindControl<ScrollViewer>("PageScroller")!;
        Headless.Capture(window); // lay out first, so the extent is known
        scroller.ScrollToEnd();
    }

    private static void ScrollToStart(MainWindow window) =>
        window.FindControl<ScrollViewer>("PageScroller")!.ScrollToHome();

    private static void Save(MainWindow window, string folder, string name)
    {
        var path = Path.Combine(folder, name);
        Headless.Capture(window).Save(path, PngBitmapEncoderOptions.Default);
        Console.WriteLine(path);
    }
}
