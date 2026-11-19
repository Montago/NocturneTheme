using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Nocturne.Avalonia.Demo.Views;

namespace Nocturne.Avalonia.Demo;

/// <summary>
/// Draws every sample of <see cref="GalleryPage"/> to its own PNG on the window ground, with the
/// drop-downs, menus and tool tips marked <see cref="Gallery.OpenProperty"/> drawn open (popups are
/// drawn into the window headless). Each picture is cropped to the sample and its open popups.
/// </summary>
internal static class GallerySnapshot
{
    /// <summary>Draws the samples to <c>&lt;folder&gt;/&lt;Tag&gt;.png</c>; <paramref name="only"/> picks samples by Tag.</summary>
    public static void Save(string folder, ICollection<string>? only = null, double scale = 1)
    {
        Directory.CreateDirectory(folder);
        var gallery = new GalleryPage();
        // Large enough for any sample with its popups; the pictures are cropped
        // Room above the samples too: DatePicker and TimePicker open their columns over the field
        gallery.Margin = new Thickness(0, 260, 0, 0);
        var window = new Window { Width = 1000, Height = 1160, Content = gallery };
        window.SetRenderScaling(scale);
        window.Show();
        try
        {
            var samples = gallery.Samples;
            var chosen = samples.Where(s => only == null || only.Contains(s.Tag as string ?? "", StringComparer.OrdinalIgnoreCase)).ToList();
            if (only != null && chosen.Count == 0)
                throw new ArgumentException($"No gallery sample has the Tag '{string.Join(", ", only)}'. Tags: "
                    + string.Join(", ", samples.Select(s => s.Tag)));

            foreach (var sample in chosen)
            {
                foreach (var other in samples)
                    other.IsVisible = other == sample;
                Capture(window, sample, Path.Combine(folder, $"{sample.Tag}.png"), scale);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static void Capture(Window window, ContentControl sample, string file, double scale)
    {
        Headless.Capture(window);
        var closers = sample.GetVisualDescendants().OfType<Control>().Where(Gallery.GetOpen).Select(Gallery.Open).ToList();

        var frame = Headless.Capture(window);
        var bounds = new Rect(sample.Bounds.Size);
        // Open popups live in the overlay layer, not under the sample
        // Open popups are drawn in the window's popup layer (OverlayPopupHost), not under the sample
        foreach (var host in window.GetVisualDescendants().OfType<OverlayPopupHost>().Where(h => h.IsVisible && h.Bounds.Width > 0))
        {
            if (host.TranslatePoint(default, sample) is { } topLeft)
                bounds = bounds.Union(new Rect(topLeft, host.Bounds.Size));
        }
        var origin = sample.TranslatePoint(default, window) ?? default;
        bounds = bounds.Translate(new Vector(origin.X, origin.Y)).Intersect(new Rect(window.ClientSize));
        SaveCropped(frame, bounds, scale, file);
        Console.WriteLine(file);

        foreach (var close in closers)
            close();
        Headless.Capture(window, 5);
    }

    /// <summary>Copies the part of <paramref name="frame"/> under <paramref name="bounds"/> (in DIPs) to a PNG.</summary>
    private static void SaveCropped(WriteableBitmap frame, Rect bounds, double scale, string file)
    {
        var x = (int)Math.Floor(bounds.X * scale);
        var y = (int)Math.Floor(bounds.Y * scale);
        var width = Math.Min((int)Math.Ceiling(bounds.Right * scale), frame.PixelSize.Width) - x;
        var height = Math.Min((int)Math.Ceiling(bounds.Bottom * scale), frame.PixelSize.Height) - y;

        using var source = frame.Lock();
        using var cropped = new WriteableBitmap(new PixelSize(width, height), frame.Dpi, source.Format, AlphaFormat.Premul);
        using (var target = cropped.Lock())
        {
            var bytesPerPixel = source.Format.BitsPerPixel / 8;
            var row = new byte[width * bytesPerPixel];
            for (var line = 0; line < height; line++)
            {
                System.Runtime.InteropServices.Marshal.Copy(source.Address + (y + line) * source.RowBytes + x * bytesPerPixel, row, 0, row.Length);
                System.Runtime.InteropServices.Marshal.Copy(row, 0, target.Address + line * target.RowBytes, row.Length);
            }
        }
        cropped.Save(file, PngBitmapEncoderOptions.Default);
    }
}
