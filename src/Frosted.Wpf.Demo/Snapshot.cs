using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Frosted.Wpf.Controls;

namespace Frosted.Wpf.Demo;

/// <summary>
/// <c>--snapshot &lt;folder&gt;</c>: renders each demo page to a PNG and exits. RenderTargetBitmap
/// cannot see the system backdrop, so the window paints its opaque fallback ground instead.
/// </summary>
internal static class Snapshot
{
    public static string? Folder { get; } = ReadFolder();

    private static string? ReadFolder()
    {
        var args = Environment.GetCommandLineArgs();
        var i = Array.IndexOf(args, "--snapshot");
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    public static async void Run(MainWindow window)
    {
        Directory.CreateDirectory(Folder!);
        window.Backdrop = WindowBackdrop.None;

        foreach (var (index, name) in new[] { (0, "controls"), (1, "more"), (2, "custom"), (3, "grid"), (4, "tokens") })
        {
            window.Navigation.SelectedIndex = index;
            await Settle(window);
            Save(window, Path.Combine(Folder!, name + ".png"));

            // The rest of a page that does not fit: halfway down when it is long, and at the bottom.
            if (window.PageScroller.ScrollableHeight > window.PageScroller.ViewportHeight)
            {
                window.PageScroller.ScrollToVerticalOffset(window.PageScroller.ScrollableHeight / 2);
                await Settle(window);
                Save(window, Path.Combine(Folder!, name + "-middle.png"));
            }
            if (window.PageScroller.ScrollableHeight > 0)
            {
                window.PageScroller.ScrollToBottom();
                await Settle(window);
                Save(window, Path.Combine(Folder!, name + "-bottom.png"));
            }
        }

        SaveLightSample();
        await SavePopups(window);
        await SaveOverlays(window);
        await SaveGrid(window);
        await CheckWheelChaining(window);
        await SaveAccents(window);
        window.Close();
    }

    /// <summary>accent-&lt;name&gt;.png: the top of the controls page in every accent but the default.</summary>
    private static async Task SaveAccents(MainWindow window)
    {
        window.Navigation.SelectedIndex = 0;
        window.PageScroller.ScrollToTop();
        foreach (var accent in Enum.GetValues<FrostedAccent>().Where(a => a != FrostedAccent.Green))
        {
            FrostedTheme.SetAccent(accent);
            await Settle(window);
            Save(window, Path.Combine(Folder!, $"accent-{accent.ToString().ToLowerInvariant()}.png"));
        }
        FrostedTheme.SetAccent(FrostedAccent.Green);
    }

    /// <summary>
    /// A wheel turn over the settings ListBox (which has nothing to scroll) must scroll the page.
    /// Writes the result to wheel-check.txt.
    /// </summary>
    private static async Task CheckWheelChaining(MainWindow window)
    {
        window.Navigation.SelectedIndex = 0;
        window.PageScroller.ScrollToTop();
        await Settle(window);

        var target = window.ControlsPage.SettingsList.Items.OfType<ListBoxItem>().First();
        var before = window.PageScroller.VerticalOffset;
        var preview = new System.Windows.Input.MouseWheelEventArgs(System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount, -120)
        {
            RoutedEvent = UIElement.PreviewMouseWheelEvent,
        };
        target.RaiseEvent(preview);
        if (!preview.Handled)
        {
            target.RaiseEvent(new System.Windows.Input.MouseWheelEventArgs(System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount, -120)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
            });
        }
        await Settle(window);

        var after = window.PageScroller.VerticalOffset;
        File.WriteAllText(Path.Combine(Folder!, "wheel-check.txt"),
            $"{(after > before ? "PASS" : "FAIL")}: page offset {before} -> {after} after a wheel turn over the ListBox");
    }

    /// <summary>Opens the combo box, context menu (with a submenu) and tool tip and renders each popup on the window's ground.</summary>
    private static async Task SavePopups(MainWindow window)
    {
        window.Navigation.SelectedIndex = 0;
        var page = window.ControlsPage;
        page.LanguageBox.BringIntoView();
        await Settle(window);

        page.LanguageBox.IsDropDownOpen = true;
        await Settle(window);
        if (page.LanguageBox.Template.FindName("PART_Popup", page.LanguageBox) is Popup { Child: FrameworkElement combo })
        {
            SaveOnGround(combo, Path.Combine(Folder!, "popup-combobox.png"));
        }
        page.LanguageBox.IsDropDownOpen = false;

        var menu = page.MenuTarget.ContextMenu!;
        menu.PlacementTarget = page.MenuTarget;
        menu.IsOpen = true;
        await Settle(window);
        SaveOnGround(menu, Path.Combine(Folder!, "popup-contextmenu.png"));
        if (menu.Items.OfType<MenuItem>().FirstOrDefault(m => m.HasItems) is { } sortBy)
        {
            sortBy.IsSubmenuOpen = true;
            await Settle(window);
            if (sortBy.Template.FindName("PART_Popup", sortBy) is Popup { Child: FrameworkElement submenu })
            {
                SaveOnGround(submenu, Path.Combine(Folder!, "popup-submenu.png"));
            }
        }
        menu.IsOpen = false;

        var tip = new ToolTip { Content = "Copy to clipboard", PlacementTarget = page.CopyButton, IsOpen = true };
        await Settle(window);
        SaveOnGround(tip, Path.Combine(Folder!, "popup-tooltip.png"));
        tip.IsOpen = false;

        window.Navigation.SelectedIndex = 1;
        var more = window.MoreControlsPage;
        more.MeetingPicker.BringIntoView();
        await Settle(window);
        more.MeetingPicker.IsDropDownOpen = true;
        await Settle(window);
        if (more.MeetingPicker.Template.FindName("PART_Popup", more.MeetingPicker) is Popup { Child: FrameworkElement calendar })
        {
            SaveOnGround(calendar, Path.Combine(Folder!, "popup-datepicker.png"));
        }
        more.MeetingPicker.IsDropDownOpen = false;
    }

    /// <summary>
    /// Custom controls page: the DropDownButton's menu (popup-dropdown.png), an open dialog
    /// (dialog.png) and the settings flyout (flyout.png) over the page.
    /// </summary>
    private static async Task SaveOverlays(MainWindow window)
    {
        window.Navigation.SelectedIndex = 2;
        window.PageScroller.ScrollToTop();
        await Settle(window);

        var page = window.CustomControlsPage;
        var menu = page.ExportButton.Menu!;
        menu.PlacementTarget = page.ExportButton;
        menu.IsOpen = true;
        await Settle(window);
        SaveOnGround(menu, Path.Combine(Folder!, "popup-dropdown.png"));
        menu.IsOpen = false;

        var dialog = new FrostedDialog
        {
            Title = "Discard changes?",
            Content = "You have changed 3 files since they were last synchronised. Discarding puts back the versions on the server.",
            AffirmativeText = "Discard",
            NegativeText = "Keep editing",
        };
        var shown = window.ShowDialogAsync(dialog);
        await Settle(window);
        Save(window, Path.Combine(Folder!, "dialog.png"));
        dialog.Close(MessageDialogResult.Canceled);
        await shown;

        window.SettingsFlyout.IsOpen = true;
        await Settle(window);
        Save(window, Path.Combine(Folder!, "flyout.png"));
        window.SettingsFlyout.IsOpen = false;
        await Settle(window);

        // The pickers' drop-downs
        page.MeetingTime.BringIntoView();
        await Settle(window);
        await SaveDropDown(page.MeetingTime, open => page.MeetingTime.IsDropDownOpen = open, "popup-timepicker.png");
        await SaveDropDown(page.LabelColour, open => page.LabelColour.IsDropDownOpen = open, "popup-colorpicker.png");
        await SaveDropDown(page.OwnerBox, open => page.OwnerBox.IsDropDownOpen = open, "popup-searchcombobox.png");

        // The menu folded to its icons
        page.DemoMenu.BringIntoView();
        page.DemoMenu.IsPaneOpen = false;
        await Settle(window);
        Save(window, Path.Combine(Folder!, "hamburger-compact.png"));
        page.DemoMenu.IsPaneOpen = true;
        await Settle(window);
    }

    /// <summary>
    /// Data grid page: the filters of the Body and Registered columns (popup-filter.png,
    /// popup-filter-dates.png) and the grid grouped by make (grid-grouped.png).
    /// </summary>
    private static async Task SaveGrid(MainWindow window)
    {
        window.Navigation.SelectedIndex = 3;
        window.PageScroller.ScrollToTop();
        await Settle(window);

        var grid = window.GridPage.Cars;
        foreach (var (header, file) in new[] { ("BODY", "popup-filter.png"), ("REGISTERED", "popup-filter-dates.png") })
        {
            // Headers are virtualized with the columns: bring the column into view first.
            if (grid.Columns.FirstOrDefault(c => Equals(c.Header, header)) is { } column)
            {
                grid.ScrollIntoView(grid.Items[0], column);
                await Settle(window);
            }
            if (FindHeader(grid, header) is not { } columnHeader)
            {
                continue;
            }
            columnHeader.BringIntoView();
            await Settle(window);
            if (columnHeader.Template.FindName("PART_FilterButton", columnHeader) is ToggleButton button
                && columnHeader.Template.FindName("PART_FilterPopup", columnHeader) is Popup { Child: FrameworkElement filter })
            {
                button.IsChecked = true;
                await Settle(window);
                SaveOnGround(filter, Path.Combine(Folder!, file));
                button.IsChecked = false;
            }
        }
        // The last columns, with the check boxes GridCheckBox draws (grid-right.png)
        grid.ScrollIntoView(grid.Items[0], grid.Columns[^1]);
        await Settle(window);
        Save(window, Path.Combine(Folder!, "grid-right.png"));
        grid.ScrollIntoView(grid.Items[0], grid.Columns[0]);

        window.GridPage.GroupBy.SelectedIndex = 1;
        await Settle(window);
        Save(window, Path.Combine(Folder!, "grid-grouped.png"));
        window.GridPage.GroupBy.SelectedIndex = 0;
        await Settle(window);
    }

    private static DataGridColumnHeader? FindHeader(DependencyObject parent, string header)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is DataGridColumnHeader { Column.Header: string text } found && text == header)
            {
                return found;
            }
            if (FindHeader(child, header) is { } deeper)
            {
                return deeper;
            }
        }
        return null;
    }

    private static async Task SaveDropDown(Control control, Action<bool> setOpen, string file)
    {
        setOpen(true);
        await Settle((Window)Window.GetWindow(control)!);
        if (control.Template.FindName("PART_Popup", control) is Popup { Child: FrameworkElement child })
        {
            SaveOnGround(child, Path.Combine(Folder!, file));
        }
        setOpen(false);
    }

    /// <summary>
    /// light.png: the window light on the tinted ground, as a plain gradient brush
    /// (left, shows bands) and dithered as FrostedWindow draws it (right).
    /// </summary>
    private static void SaveLightSample()
    {
        const int w = 600, h = 400;
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            var ground = new SolidColorBrush(Color.FromRgb(0x1A, 0x2A, 0x21));
            dc.DrawRectangle(ground, null, new Rect(0, 0, w * 2 + 20, h));
            var gradient = new LinearGradientBrush(Color.FromArgb(0x14, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), new Point(0, 0), new Point(1, 1));
            gradient.GradientStops[1].Offset = LightTexture.Reach;
            dc.DrawRectangle(gradient, null, new Rect(0, 0, w, h));
            var light = LightTexture.Create(w, h);
            dc.DrawImage(light, new Rect(w + 20, 0, light.PixelWidth, light.PixelHeight));
        }

        var bitmap = new RenderTargetBitmap(w * 2 + 20, h, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(Folder!, "light.png"));
        encoder.Save(stream);
    }

    private static void SaveOnGround(FrameworkElement element, string path)
    {
        // The element is drawn at its margin (popups keep room for the shadow there): include it.
        var w = Math.Max(1, element.ActualWidth + element.Margin.Left + element.Margin.Right);
        var h = Math.Max(1, element.ActualHeight + element.Margin.Top + element.Margin.Bottom);
        var ground = new Border { Width = w, Height = h, Background = (Brush)element.FindResource("Frosted.Brushes.Window.Fallback") };
        ground.Measure(new Size(w, h));
        ground.Arrange(new Rect(0, 0, w, h));

        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(w), (int)Math.Ceiling(h), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(ground);
        bitmap.Render(element);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static async Task Settle(Window window)
    {
        window.UpdateLayout();
        await window.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        await Task.Delay(400); // let the 120-150 ms transitions finish
    }

    private static void Save(Window window, string path)
    {
        if (window.Content is not FrameworkElement)
        {
            return;
        }

        // Render the whole template (title bar included), not only the content.
        var root = (FrameworkElement)VisualTreeHelper.GetChild(window, 0);
        var dpi = VisualTreeHelper.GetDpi(root);
        var bitmap = new RenderTargetBitmap(
            (int)Math.Ceiling(root.ActualWidth * dpi.DpiScaleX), (int)Math.Ceiling(root.ActualHeight * dpi.DpiScaleY),
            dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        bitmap.Render(root);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
