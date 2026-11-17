using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nocturne.Wpf.Controls;

/// <summary>
/// The soft light from the window's top-left corner, drawn as a dithered bitmap.
/// <para>
/// As a gradient brush the light (8 % white fading out over the first 38 % of the diagonal) has
/// only about 20 alpha steps, which show as diagonal bands across the window. Here every pixel rounds its exact
/// value up or down at random (a hash of its position), so the steps dissolve into noise finer
/// than the eye can see.
/// </para>
/// </summary>
public static class LightTexture
{
    /// <summary>Alpha of the white light in the corner (0x14 = 8 %).</summary>
    public const double MaxAlpha = 20;

    /// <summary>How far along the diagonal (0 to 1) the light reaches before it is gone.</summary>
    public const double Reach = 0.38;

    private const ulong Seed = 0x5EED_11647;

    /// <summary>
    /// The top-left part of a <paramref name="width"/> x <paramref name="height"/> window that the
    /// light covers (any unit). The light falls off along the window's diagonal, as a
    /// LinearGradientBrush from (0,0) to (1,1) does: t = (x·w + y·h) / (w² + h²).
    /// </summary>
    public static Size Region(double width, double height)
    {
        var diagonal = width * width + height * height;
        return new Size(
            Math.Min(width, Reach * diagonal / Math.Max(1, width)),
            Math.Min(height, Reach * diagonal / Math.Max(1, height)));
    }

    /// <summary>
    /// The light for a window of <paramref name="windowWidth"/> x <paramref name="windowHeight"/>
    /// device pixels. The bitmap covers only <see cref="Region"/>; place it at the top left,
    /// pixel for pixel.
    /// </summary>
    public static BitmapSource Create(int windowWidth, int windowHeight)
    {
        var region = Region(windowWidth, windowHeight);
        var width = Math.Max(1, (int)Math.Ceiling(region.Width));
        var height = Math.Max(1, (int)Math.Ceiling(region.Height));
        var diagonal = (double)windowWidth * windowWidth + (double)windowHeight * windowHeight;
        var pixels = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var t = ((x + 0.5) * windowWidth + (y + 0.5) * windowHeight) / diagonal;
                if (t >= Reach)
                {
                    continue;
                }

                var exact = MaxAlpha * (1 - t / Reach);
                var random = Hash((ulong)y << 32 | (uint)x) / (double)ulong.MaxValue;
                var alpha = (byte)Math.Min(255, Math.Floor(exact + random));
                var i = (y * width + x) * 4;
                pixels[i] = alpha;     // premultiplied white: (a, a, a, a)
                pixels[i + 1] = alpha;
                pixels[i + 2] = alpha;
                pixels[i + 3] = alpha;
            }
        }

        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, pixels, width * 4);
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>SplitMix64: a well-mixed 64-bit value per pixel.</summary>
    private static ulong Hash(ulong key)
    {
        var z = (key + 1) * 0x9E3779B97F4A7C15UL + Seed;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
