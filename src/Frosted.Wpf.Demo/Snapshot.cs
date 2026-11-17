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

        foreach (var (index, name) in new[] { (0, "controls"), (1, "tokens") })
        {
            window.Navigation.SelectedIndex = index;
            await Settle(window);
            Save(window, Path.Combine(Folder!, name + ".png"));

            // The rest of a page that does not fit: scrolled to the bottom.
            if (window.PageScroller.ScrollableHeight > 0)
            {
                window.PageScroller.ScrollToBottom();
                await Settle(window);
                Save(window, Path.Combine(Folder!, name + "-bottom.png"));
            }
        }

        await SavePopups(window);
        window.Close();
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
    }

    private static void SaveOnGround(FrameworkElement element, string path)
    {
        var w = Math.Max(1, element.ActualWidth);
        var h = Math.Max(1, element.ActualHeight);
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
