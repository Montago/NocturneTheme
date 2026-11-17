using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Frosted.Wpf.Controls;

/// <summary>
/// The grain that makes the glass look matte: 256 x 256 pixels of faint light and dark
/// specks, drawn once from a fixed seed so it looks the same on every run. It tiles without
/// seams because every pixel is independent of its neighbours.
/// </summary>
public static class NoiseTexture
{
    private const int Size = 256;

    /// <summary>The texture as a frozen image, used by <c>Frosted.Brushes.Noise</c>.</summary>
    public static BitmapSource Image { get; } = Create();

    private static BitmapSource Create()
    {
        var random = new Random(20261117);
        var pixels = new byte[Size * Size * 4];
        for (var i = 0; i < pixels.Length; i += 4)
        {
            // Squaring keeps most specks faint and a few brighter; light and dark in equal numbers.
            var strength = random.NextDouble();
            var alpha = (byte)(strength * strength * 38);
            var light = random.Next(2) == 0;
            var value = light ? alpha : (byte)0; // premultiplied: white at alpha a is (a, a, a, a)
            pixels[i] = value;
            pixels[i + 1] = value;
            pixels[i + 2] = value;
            pixels[i + 3] = alpha;
        }

        var bitmap = BitmapSource.Create(Size, Size, 96, 96, PixelFormats.Pbgra32, null, pixels, Size * 4);
        bitmap.Freeze();
        return bitmap;
    }
}
