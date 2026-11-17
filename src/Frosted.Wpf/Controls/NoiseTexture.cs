using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Frosted.Wpf.Controls;

/// <summary>
/// The surface of the glass: a 512 x 512 tile of fine, even grain that makes it look sandblasted.
/// <para>
/// Every pixel gets an independent value from a hash of its position (no generator whose
/// neighbouring values correlate, which showed as diagonal streaks). A Gaussian blur that wraps
/// around the edges softens the single-pixel noise into round specks of the requested size and
/// keeps the tile seamless. Light specks are a little stronger than dark ones, as on sandblasted
/// glass, which scatters light. The tile is drawn from a fixed seed, so it looks the same on
/// every run.
/// </para>
/// </summary>
public static class NoiseTexture
{
    /// <summary>Width and height of the tile in pixels.</summary>
    public const int Size = 512;

    /// <summary>Speck size of <see cref="Image"/>, in pixels at 96 DPI.</summary>
    public const double DefaultGrainSize = 1;

    private const int Mask = Size - 1;
    private const ulong Seed = 20261117;
    private const double LightStrength = 17;
    private const double DarkStrength = 11;
    private static readonly Dictionary<int, BitmapSource> Cache = [];

    /// <summary>The default grain at 96 DPI, used by <c>Frosted.Brushes.Noise</c>.</summary>
    public static BitmapSource Image { get; } = Get(DefaultGrainSize);

    /// <summary>
    /// The tile with specks about <paramref name="grainPixels"/> device pixels across (1 to 16;
    /// 1 is plain single-pixel noise). Tiles are cached per size and frozen.
    /// </summary>
    public static BitmapSource Get(double grainPixels)
    {
        var key = (int)Math.Round(Math.Clamp(grainPixels, 1, 16) * 10);
        lock (Cache)
        {
            if (!Cache.TryGetValue(key, out var bitmap))
            {
                bitmap = Create(key / 10.0);
                Cache[key] = bitmap;
            }
            return bitmap;
        }
    }

    private static BitmapSource Create(double grainPixels)
    {
        // A speck g pixels across is a Gaussian with sigma of about 0.45 (g - 1); 1 stays sharp.
        var values = Normalise(Blur(WhiteNoise(), (grainPixels - 1) * 0.45));

        var pixels = new byte[Size * Size * 4];
        for (var i = 0; i < values.Length; i++)
        {
            var v = values[i];
            var alpha = (byte)Math.Min(255, Math.Abs(v) * (v > 0 ? LightStrength : DarkStrength));
            var value = v > 0 ? alpha : (byte)0; // premultiplied: white at alpha a is (a, a, a, a)
            pixels[i * 4] = value;
            pixels[i * 4 + 1] = value;
            pixels[i * 4 + 2] = value;
            pixels[i * 4 + 3] = alpha;
        }

        var bitmap = BitmapSource.Create(Size, Size, 96, 96, PixelFormats.Pbgra32, null, pixels, Size * 4);
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>One independent value in -1..1 per pixel.</summary>
    private static double[] WhiteNoise()
    {
        var values = new double[Size * Size];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = Hash((ulong)i) / (double)ulong.MaxValue * 2 - 1;
        }
        return values;
    }

    /// <summary>Scales the values to a spread of 1, so every speck size gets the same contrast.</summary>
    private static double[] Normalise(double[] values)
    {
        var variance = 0.0;
        foreach (var v in values)
        {
            variance += v * v;
        }
        var scale = 1 / Math.Sqrt(variance / values.Length);
        for (var i = 0; i < values.Length; i++)
        {
            values[i] *= scale;
        }
        return values;
    }

    /// <summary>Separable Gaussian blur that wraps around the edges (keeps the tile seamless).</summary>
    private static double[] Blur(double[] source, double sigma)
    {
        if (sigma < 0.05)
        {
            return source;
        }

        var radius = (int)Math.Ceiling(sigma * 3);
        var kernel = new double[radius * 2 + 1];
        var sum = 0.0;
        for (var k = -radius; k <= radius; k++)
        {
            sum += kernel[k + radius] = Math.Exp(-k * k / (2 * sigma * sigma));
        }
        for (var k = 0; k < kernel.Length; k++)
        {
            kernel[k] /= sum;
        }

        var horizontal = new double[source.Length];
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var acc = 0.0;
                for (var k = -radius; k <= radius; k++)
                {
                    acc += source[y * Size + ((x + k) & Mask)] * kernel[k + radius];
                }
                horizontal[y * Size + x] = acc;
            }
        }

        var result = new double[source.Length];
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var acc = 0.0;
                for (var k = -radius; k <= radius; k++)
                {
                    acc += horizontal[((y + k) & Mask) * Size + x] * kernel[k + radius];
                }
                result[y * Size + x] = acc;
            }
        }
        return result;
    }

    /// <summary>SplitMix64: a well-mixed 64-bit value for every pixel index.</summary>
    private static ulong Hash(ulong index)
    {
        var z = (index + 1) * 0x9E3779B97F4A7C15UL + Seed;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
