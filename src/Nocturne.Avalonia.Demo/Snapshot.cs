using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Raw;
using Nocturne.Avalonia.Controls;
using Nocturne.Avalonia.Demo.Views;

namespace Nocturne.Avalonia.Demo;

/// <summary>
/// <c>--snapshot &lt;folder&gt; [--scale 1]</c>: draws the demo to PNG files in the folder and exits:
/// the pages (the controls and custom controls pages at the top and the bottom), a combo box and a
/// menu drawn open, the controls page in every accent and the Separated and Accent title bars, the
/// pickers of the custom page drawn open, a dialog, the flyout and the folded HamburgerMenu; then checks.txt
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

        window.ShowPage("more");
        Save(window, folder, "more.png");
        ScrollToEnd(window);
        Save(window, folder, "more-bottom.png");
        ScrollToStart(window);

        window.ShowPage("icons");
        Save(window, folder, "icons.png");

        SaveCustomPage(window, folder);

        window.Close();
        GallerySnapshot.Save(Path.Combine(folder, "controls"), scale: scale);
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

        CheckCustomPage(window, Check);

        window.Close();
        var path = Path.Combine(folder, "checks.txt");
        File.WriteAllLines(path, lines);
        Console.WriteLine(path);
        foreach (var line in lines.Where(l => l.StartsWith("FAIL")))
            Console.Error.WriteLine(line);
        return lines.All(l => l.StartsWith("PASS"));
    }

    /// <summary>
    /// The custom controls page at the top and the bottom, its pickers drawn open (time, date, colour,
    /// SearchComboBox), an open dialog, the flyout and the folded HamburgerMenu.
    /// </summary>
    private static void SaveCustomPage(MainWindow window, string folder)
    {
        window.ShowPage("custom");
        Save(window, folder, "custom.png");
        ScrollToEnd(window);
        Save(window, folder, "custom-bottom.png");
        ScrollToStart(window);

        var page = window.GetVisualDescendants().OfType<CustomControlsPage>().First();
        Click(window, FlyoutButton(page.FindControl<TimePicker>("MeetingTime")!));
        Save(window, folder, "popup-timepicker.png");
        CloseOpenPopups(window);

        Click(window, FlyoutButton(page.FindControl<DatePicker>("MeetingDay")!));
        Save(window, folder, "popup-datepicker.png");
        CloseOpenPopups(window);

        var colour = page.FindControl<ColorPicker>("LabelColour")!;
        colour.IsDropDownOpen = true;
        Save(window, folder, "popup-colorpicker.png");
        colour.IsDropDownOpen = false;

        var owner = page.FindControl<SearchComboBox>("OwnerBox")!;
        owner.IsDropDownOpen = true;
        Save(window, folder, "popup-searchcombobox.png");
        owner.IsDropDownOpen = false;

        var dialog = new NocturneDialog
        {
            Title = "Discard changes?",
            Content = "You have changed 3 files since they were last synchronised. Discarding puts back the versions on the server.",
            AffirmativeText = "Discard",
            NegativeText = "Keep editing",
        };
        _ = window.ShowDialogAsync(dialog);
        Save(window, folder, "dialog.png");
        dialog.Close(MessageDialogResult.Canceled);
        Headless.Capture(window);

        window.SettingsFlyout.IsOpen = true;
        Save(window, folder, "flyout.png");
        window.SettingsFlyout.IsOpen = false;
        Headless.Capture(window);

        var menu = page.FindControl<HamburgerMenu>("DemoMenu")!;
        ScrollTo(window, menu);
        menu.IsPaneOpen = false;
        Save(window, folder, "hamburger-compact.png");
        menu.IsPaneOpen = true;
        ScrollToStart(window);
    }

    /// <summary>
    /// The custom controls page, driven with the headless mouse and keyboard: typing filters the
    /// SearchComboBox, a swatch click takes a colour, the TimePicker opens and accepts, a dialog returns
    /// the answer of its button and Escape closes the flyout.
    /// </summary>
    private static void CheckCustomPage(MainWindow window, Action<string, bool> check)
    {
        window.ShowPage("custom");
        Headless.Capture(window);
        var page = window.GetVisualDescendants().OfType<CustomControlsPage>().First();

        var owner = page.FindControl<SearchComboBox>("OwnerBox")!;
        var text = owner.GetVisualDescendants().OfType<TextBox>().First(t => t.Name == "PART_TextBox");
        ScrollTo(window, owner);
        Click(window, text);
        text.SelectAll();
        window.KeyTextInput("mik");
        Headless.Capture(window);
        var list = PopupPart<ListBox>(window, "PART_List");
        check("typing filters the SearchComboBox", owner.IsDropDownOpen && list?.ItemCount == 1);
        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        Headless.Capture(window);
        check("Enter takes the SearchComboBox match", (owner.SelectedItem as Person)?.Name == "Mikkel Dahl" && !owner.IsDropDownOpen);
        CloseOpenPopups(window);

        var colour = page.FindControl<ColorPicker>("LabelColour")!;
        ScrollTo(window, colour);
        Click(window, colour);
        var palette = PopupPart<ListBox>(window, "PART_Palette");
        var swatch = palette?.ContainerFromIndex(12);
        check("a click opens the ColorPicker", colour.IsDropDownOpen && swatch != null);
        if (swatch != null)
        {
            Click(window, swatch);
        }
        check("a swatch click takes the colour and closes", colour.SelectedColor == ColorPicker.DefaultPalette[12] && !colour.IsDropDownOpen);
        CloseOpenPopups(window);

        var time = page.FindControl<TimePicker>("MeetingTime")!;
        ScrollTo(window, time);
        Click(window, FlyoutButton(time));
        var timePopup = time.GetVisualDescendants().OfType<Popup>().First();
        check("a click opens the TimePicker", timePopup.IsOpen);
        var accept = window.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Name == "PART_AcceptButton" && b.IsEffectivelyVisible);
        if (accept != null)
        {
            Click(window, accept);
        }
        check("the TimePicker accept button closes it with the time", !timePopup.IsOpen && time.SelectedTime == new TimeSpan(9, 30, 0));

        var showDialog = page.FindControl<Button>("ShowDialogButton")!;
        ScrollTo(window, showDialog);
        Click(window, showDialog);
        var affirmative = window.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Name == "PART_Affirmative");
        check("Show dialog opens a dialog", affirmative != null);
        if (affirmative != null)
        {
            Click(window, affirmative);
        }
        Headless.Capture(window);
        check("the dialog returns the answer of its button", page.FindControl<TextBlock>("DialogResultText")!.Text == "The answer was Affirmative.");

        var openFlyout = page.FindControl<Button>("OpenFlyoutButton")!;
        ScrollTo(window, openFlyout);
        Click(window, openFlyout);
        check("Open flyout opens the flyout", window.SettingsFlyout.IsOpen);
        window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        Headless.Capture(window);
        check("Escape closes the flyout", !window.SettingsFlyout.IsOpen);
    }

    /// <summary>The button that makes up the field of a DatePicker or TimePicker.</summary>
    private static Button FlyoutButton(TemplatedControl picker) =>
        picker.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_FlyoutButton");

    /// <summary>A named part inside an open popup (popups are drawn in the overlay layer, not under their control).</summary>
    private static T? PopupPart<T>(Window window, string name) where T : Control =>
        window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name && c.IsEffectivelyVisible);

    private static void CloseOpenPopups(Window window)
    {
        foreach (var popup in window.GetVisualDescendants().OfType<Popup>().Where(p => p.IsOpen).ToList())
            popup.IsOpen = false;
        Headless.Capture(window);
    }

    /// <summary>Scrolls the page so the top of the control sits 300px down the window.</summary>
    private static void ScrollTo(MainWindow window, Control control)
    {
        var scroller = window.FindControl<ScrollViewer>("PageScroller")!;
        Headless.Capture(window);
        var top = control.TranslatePoint(default, window)!.Value.Y;
        scroller.Offset = new Vector(0, Math.Max(0, scroller.Offset.Y + top - 300));
        Headless.Capture(window);
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

    private static void ScrollToStart(MainWindow window)
    {
        window.FindControl<ScrollViewer>("PageScroller")!.ScrollToHome();
        Headless.Capture(window); // lay out again, so clicks find the controls where they are drawn
    }

    private static void Save(MainWindow window, string folder, string name)
    {
        var path = Path.Combine(folder, name);
        Headless.Capture(window).Save(path, PngBitmapEncoderOptions.Default);
        Console.WriteLine(path);
    }
}
