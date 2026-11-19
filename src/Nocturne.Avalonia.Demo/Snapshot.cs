using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo;

/// <summary>
/// <c>--snapshot &lt;folder&gt; [--scale 1]</c>: draws the demo to PNG files in the folder and exits:
/// the pages (the controls page at the top and the bottom), a combo box and a menu drawn open,
/// the controls page in every accent and the Separated and Accent title bars; then checks.txt
/// (<see cref="Checks"/>). The headless
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
        return Checks(folder) ? 0 : 1;
    }

    /// <summary>
    /// Behaviour the pictures cannot show, driven with the headless mouse: the page's scroll bars hide
    /// a moment after scrolling and appear while it scrolls, a document tab's × closes it, and the "+"
    /// button adds one. Writes
    /// checks.txt (PASS or FAIL per line); the command fails when one fails.
    /// </summary>
    private static bool Checks(string folder)
    {
        var lines = new List<string>();
        void Check(string name, bool ok) => lines.Add($"{(ok ? "PASS" : "FAIL")} {name}");

        // A window of its own, so nothing has scrolled or been hovered yet
        var window = new MainWindow();
        window.Show();
        Headless.Capture(window);

        var scroller = window.FindControl<ScrollViewer>("PageScroller")!;
        window.MouseMove(new Point(4, window.Bounds.Height - 4)); // over the navigation, away from the page
        Headless.Capture(window);
        // The page settles at start, which shows the bars for a second; they fade once that has passed
        Wait(TimeSpan.FromSeconds(1.5));
        Check("the page's scroll bars hide a moment after scrolling", !scroller.Classes.Contains("barsVisible"));
        scroller.Offset = new Vector(0, 200);
        Headless.Capture(window);
        Check("the page's scroll bars appear while it scrolls", scroller.Classes.Contains("barsVisible"));

        // Scroll the document tabs to the middle of the window, then click their buttons
        var tabs = window.GetVisualDescendants().OfType<TabControl>().First(t => t.Name == "DocumentTabs");
        var top = tabs.TranslatePoint(default, window)!.Value.Y;
        scroller.Offset = new Vector(0, Math.Max(0, scroller.Offset.Y + top - 300));
        Headless.Capture(window);

        var logs = tabs.Items.OfType<TabItem>().First(t => (string?)t.Header == "Logs");
        Click(window, logs.GetVisualDescendants().OfType<Button>().First(b => b.Name == "CloseButton"));
        Check("a document tab's close button closes it", tabs.ItemCount == 2 && !tabs.Items.Contains(logs));

        Click(window, tabs.GetVisualDescendants().OfType<Button>().First(b => b.Name == "AddButton"));
        Check("the + button adds a tab", tabs.ItemCount == 3 && (tabs.SelectedItem as TabItem)?.Header as string == "New 1");

        window.Close();
        var path = Path.Combine(folder, "checks.txt");
        File.WriteAllLines(path, lines);
        Console.WriteLine(path);
        foreach (var line in lines.Where(l => l.StartsWith("FAIL")))
            Console.Error.WriteLine(line);
        return lines.All(l => l.StartsWith("PASS"));
    }

    /// <summary>Runs the dispatcher for a while of real time, so its timers fire.</summary>
    private static void Wait(TimeSpan time)
    {
        using var stop = new CancellationTokenSource(time);
        Dispatcher.UIThread.MainLoop(stop.Token);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Click(Window window, Control target)
    {
        var centre = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)
                     ?? throw new InvalidOperationException($"{target.Name} is not in the window.");
        window.MouseMove(centre);
        window.MouseDown(centre, MouseButton.Left);
        window.MouseUp(centre, MouseButton.Left);
        Headless.Capture(window);
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
