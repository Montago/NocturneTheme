using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Nocturne.Wpf.Controls;
using Nocturne.Wpf.Demo.Views;

namespace Nocturne.Wpf.Demo;

/// <summary>
/// Renders every sample of <see cref="GalleryPage"/> to its own PNG on the window's fallback ground,
/// with the drop-downs, menus and tool tips marked <c>Gallery.Open</c> drawn where they would appear.
/// The pictures go to docs/screenshots/controls (tools/update_docs.py copies them).
/// </summary>
internal static class GallerySnapshot
{
    private const double Margin = 16;

    public static async Task SaveAsync(string folder)
    {
        Directory.CreateDirectory(folder);
        var gallery = new GalleryPage();
        var window = new NocturneWindow
        {
            Backdrop = WindowBackdrop.None,
            Content = gallery,
            ShowActivated = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -20000,
            Top = 0,
            Width = 1400,
            Height = 1000,
        };
        window.Show();

        try
        {
            var samples = gallery.Samples.ToList();
            foreach (var sample in samples)
            {
                foreach (var other in samples)
                {
                    other.Visibility = other == sample ? Visibility.Visible : Visibility.Collapsed;
                }
                await IdleAsync(window, 200);

                var menus = new List<FrameworkElement>();
                var closers = new List<Action>();
                foreach (var element in Gallery.Descendants(sample).OfType<FrameworkElement>().Where(Gallery.GetOpen).ToList())
                {
                    Open(element, menus, closers);
                }
                await IdleAsync(window, 450);

                var parts = new List<(Visual Visual, Rect Bounds)>();
                var opened = Gallery.Descendants(sample).OfType<Popup>().Where(p => p.IsOpen && p.Child != null).ToList();
                foreach (var popup in opened)
                {
                    var target = popup.PlacementTarget ?? VisualTreeHelper.GetParent(popup) as UIElement;
                    AddPopup(parts, sample, popup.Child, target, popup.Placement, popup.HorizontalOffset, popup.VerticalOffset);
                }
                foreach (var menu in menus)
                {
                    var (target, placement, h, v) = menu switch
                    {
                        ContextMenu c => (c.PlacementTarget, c.Placement, c.HorizontalOffset, c.VerticalOffset),
                        ToolTip t => (t.PlacementTarget, t.Placement, t.HorizontalOffset, t.VerticalOffset),
                        _ => (null, PlacementMode.Bottom, 0d, 0d),
                    };
                    AddPopup(parts, sample, menu, target, placement, h, v);
                }

                Gallery.FreezeAnimations(sample);
                foreach (var popup in parts.Select(p => p.Visual).OfType<DependencyObject>())
                {
                    Gallery.FreezeAnimations(popup);
                }
                await IdleAsync(window, 0);
                Save(window, sample, parts, Path.Combine(folder, $"{sample.Tag}.png"));

                foreach (var close in closers)
                {
                    close();
                }
                foreach (var menu in menus)
                {
                    menu.SetValue(menu is ToolTip ? ToolTip.IsOpenProperty : ContextMenu.IsOpenProperty, false);
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static void Open(FrameworkElement element, List<FrameworkElement> menus, List<Action> closers)
    {
        switch (element)
        {
            case ComboBox combo:
                combo.IsDropDownOpen = true;
                closers.Add(() => combo.IsDropDownOpen = false);
                break;
            case DatePicker picker:
                picker.IsDropDownOpen = true;
                closers.Add(() => picker.IsDropDownOpen = false);
                break;
            case TimePicker picker:
                picker.IsDropDownOpen = true;
                closers.Add(() => picker.IsDropDownOpen = false);
                break;
            case ColorPicker picker:
                picker.IsDropDownOpen = true;
                closers.Add(() => picker.IsDropDownOpen = false);
                break;
            case MenuItem item:
                // A top-level submenu needs mouse capture, which a window off the screen cannot get,
                // so show the popup directly and highlight the header as an open menu would.
                ForceOpen(item, closers);
                typeof(MenuItem).GetProperty(nameof(MenuItem.IsHighlighted))!.SetValue(item, true);
                break;
            case AutoSuggestBox suggest:
                // Fills the suggestion list; the list itself only opens while the box has focus.
                suggest.Text = "B";
                ForceOpen(suggest, closers);
                break;
            case SearchComboBox search:
                search.IsDropDownOpen = true;
                closers.Add(() => search.IsDropDownOpen = false);
                break;
            case NocturneGrid grid:
                // The filter of the first filtered column, as if its funnel had been clicked.
                var header = Gallery.Descendants(grid).OfType<DataGridColumnHeader>()
                    .FirstOrDefault(h => h.Column != null && NocturneGrid.GetHasFilter(h.Column));
                if (header?.Template?.FindName("PART_FilterButton", header) is ToggleButton button)
                {
                    button.IsChecked = true;
                    closers.Add(() => button.IsChecked = false);
                }
                break;
            case SplitButton { Menu: { } menu }:
                OpenMenu(element, menu, PlacementMode.Bottom, 0, 4, menus);
                break;
            case DropDownButton { Menu: { } menu }:
                OpenMenu(element, menu, PlacementMode.Bottom, 0, 4, menus);
                break;
            case { ContextMenu: { } menu }:
                OpenMenu(element, menu, PlacementMode.Relative, 150, 30, menus);
                break;
            case { ToolTip: { } tip }:
                var toolTip = tip as ToolTip ?? new ToolTip { Content = tip };
                element.ToolTip = toolTip;
                toolTip.PlacementTarget = element;
                toolTip.Placement = PlacementMode.Bottom;
                toolTip.VerticalOffset = 6;
                toolTip.IsOpen = true;
                menus.Add(toolTip);
                break;
        }
    }

    private static void ForceOpen(Control control, List<Action> closers)
    {
        // A local binding overrides the template's IsOpen binding without writing back to the control.
        if (control.Template?.FindName("PART_Popup", control) is Popup popup)
        {
            BindingOperations.SetBinding(popup, Popup.IsOpenProperty, new Binding { Source = true, Mode = BindingMode.OneWay });
            closers.Add(() => BindingOperations.ClearBinding(popup, Popup.IsOpenProperty));
        }
    }

    private static void OpenMenu(FrameworkElement owner, ContextMenu menu, PlacementMode placement, double h, double v, List<FrameworkElement> menus)
    {
        menu.PlacementTarget = owner;
        menu.Placement = placement;
        menu.HorizontalOffset = h;
        menu.VerticalOffset = v;
        menu.IsOpen = true;
        menus.Add(menu);
    }

    // Popups live in their own window, which is clamped to the screen; draw the popup's root where
    // its placement would put it next to the target instead.
    private static void AddPopup(List<(Visual, Rect)> parts, FrameworkElement sample, UIElement child, UIElement? target,
        PlacementMode placement, double h, double v)
    {
        if (target == null || PresentationSource.FromVisual(child)?.RootVisual is not FrameworkElement root)
        {
            return;
        }
        var size = target.RenderSize;
        var anchor = placement switch
        {
            PlacementMode.Right => new Point(size.Width + h, v),
            PlacementMode.Top => new Point(h, -root.ActualHeight + v),
            PlacementMode.Relative or PlacementMode.RelativePoint => new Point(h, v),
            _ => new Point(h, size.Height + v),
        };
        var at = target.TranslatePoint(anchor, sample);
        parts.Add((root, new Rect(at, new Size(root.ActualWidth, root.ActualHeight))));
    }

    private static void Save(Window window, FrameworkElement sample, List<(Visual Visual, Rect Bounds)> parts, string file)
    {
        var bounds = new Rect(0, 0, sample.ActualWidth, sample.ActualHeight);
        foreach (var part in parts)
        {
            var padded = part.Bounds;
            padded.Inflate(Margin, Margin);
            bounds.Union(padded);
        }
        bounds = new Rect(Math.Floor(bounds.X), Math.Floor(bounds.Y), Math.Ceiling(bounds.Width), Math.Ceiling(bounds.Height));

        var drawing = new DrawingVisual();
        using (var dc = drawing.RenderOpen())
        {
            dc.PushTransform(new TranslateTransform(-bounds.X, -bounds.Y));
            dc.DrawRectangle((Brush)window.FindResource("Nocturne.Brushes.Window.Fallback"), null, bounds);
            Draw(dc, sample, new Rect(0, 0, sample.ActualWidth, sample.ActualHeight));
            foreach (var part in parts)
            {
                Draw(dc, part.Visual, part.Bounds);
            }
        }

        var bitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(drawing);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(file);
        encoder.Save(stream);
    }

    private static void Draw(DrawingContext dc, Visual visual, Rect at)
    {
        dc.DrawRectangle(new VisualBrush(visual)
        {
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(at.Size),
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = at,
        }, null, at);
    }

    private static async Task IdleAsync(Window window, int delay)
    {
        await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        await Task.Delay(delay);
        await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }
}
