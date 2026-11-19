using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Microsoft.Win32;

namespace Nocturne.Avalonia;

/// <summary>
/// Switching the accent at run time: the accent brushes, the primary button, the window's tint and
/// fallback ground and the popup ground all take the new hue. Every theme references the brushes
/// with DynamicResource, so open windows follow at once. The palettes (NocturneAccent.g.cs) are
/// generated from the WPF library.
/// </summary>
public partial class NocturneTheme
{
    private static IResourceDictionary? target;
    private static IPlatformSettings? watchedSettings;
    private static Color? appliedSystemAccent;
    private static Color? lastSeenSystemAccent;

    /// <summary>The accent last set with <see cref="SetAccent"/>.</summary>
    public static NocturneAccent Accent { get; private set; } = NocturneAccent.Blue;

    /// <summary>Raised after <see cref="SetAccent"/> (or a change of the system accent) has replaced the brushes.</summary>
    public static event EventHandler? AccentChanged;

    /// <summary>Raised when the accent colour of the operating system has changed, whichever accent the app uses.</summary>
    public static event EventHandler? SystemAccentChanged;

    /// <summary>
    /// Puts the brushes for <paramref name="accent"/> into <paramref name="resources"/> (by default
    /// the application's resources, where they override the theme's own).
    /// </summary>
    public static void SetAccent(NocturneAccent accent, IResourceDictionary? resources = null)
    {
        resources ??= Application.Current?.Resources ?? throw new InvalidOperationException("No application to set the accent on; pass a ResourceDictionary.");
        target = resources;
        Accent = accent;
        WatchSystemAccent();

        Palette palette;
        if (accent == NocturneAccent.System && ReadSystemPalette() is { } system)
        {
            palette = FromSystem(system);
            appliedSystemAccent = system[3];
        }
        else
        {
            palette = Palettes[accent == NocturneAccent.System ? NocturneAccent.Blue : accent];
            appliedSystemAccent = null;
        }

        Apply(palette, resources);
        AccentChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>The operating system's accent colour, or null where it has none.</summary>
    public static Color? GetSystemAccent() => ReadSystemPalette()?[3];

    /// <summary>Follows the system accent live (all platforms: Avalonia reports the change).</summary>
    private static void WatchSystemAccent()
    {
        var settings = Application.Current?.PlatformSettings;
        if (settings == null || settings == watchedSettings)
            return;
        watchedSettings = settings;
        lastSeenSystemAccent = GetSystemAccent();
        settings.ColorValuesChanged += (_, _) => OnSystemColorsChanged();
    }

    private static void OnSystemColorsChanged()
    {
        var current = GetSystemAccent();
        if (current == lastSeenSystemAccent)
            return;
        lastSeenSystemAccent = current;

        if (Accent == NocturneAccent.System && target != null && current != appliedSystemAccent)
            SetAccent(NocturneAccent.System, target);
        SystemAccentChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void Apply(Palette p, IResourceDictionary resources)
    {
        resources["Nocturne.Colors.Accent"] = p.Accent;
        resources["Nocturne.Colors.Accent.Light"] = p.Light;
        resources["Nocturne.Brushes.Accent"] = Solid(p.Accent);
        resources["Nocturne.Brushes.Accent.Light"] = Solid(p.Light);
        resources["Nocturne.Brushes.Accent.Text"] = Solid(p.Text);
        resources["Nocturne.Brushes.Accent.Text.Hover"] = Solid(p.TextHover);
        resources["Nocturne.Brushes.Text.OnAccent"] = Solid(p.OnAccent);
        resources["Nocturne.Brushes.Button.Primary"] = Vertical(p.PrimaryTop, p.PrimaryBottom);
        resources["Nocturne.Brushes.Button.Primary.Hover"] = Vertical(p.PrimaryHoverTop, p.PrimaryHoverBottom);
        resources["Nocturne.Brushes.Button.Primary.Pressed"] = Solid(p.PrimaryPressed);
        resources["Nocturne.Brushes.Window.Fallback"] = Solid(p.Fallback);
        resources["Nocturne.Brushes.Window.Tint"] = Solid(p.Tint);
        resources["Nocturne.Brushes.Popup"] = Solid(WithAlpha(p.Popup, 0xF7));
        resources["Nocturne.Brushes.TitleBar.Accent"] = Vertical(WithAlpha(p.Accent, 0x4D), WithAlpha(p.Accent, 0x38));
        resources["Nocturne.Brushes.TitleBar.Accent.Border"] = Solid(WithAlpha(p.Light, 0x38));
    }

    /// <summary>
    /// The system accent as seven shades: lightest three, the accent itself, darkest three. Windows
    /// derives them from the user's colour (its AccentPalette); elsewhere they are mixed from the
    /// accent Avalonia reports. Null when there is no system accent.
    /// </summary>
    private static Color[]? ReadSystemPalette()
    {
        if (OperatingSystem.IsWindows() && ReadWindowsPalette() is { } windows)
            return windows;

        if (Application.Current?.PlatformSettings?.GetColorValues() is not { } values || values.AccentColor1.A == 0)
            return null;
        var accent = values.AccentColor1;
        return
        [
            Mix(accent, Colors.White, 0.7), Mix(accent, Colors.White, 0.5), Mix(accent, Colors.White, 0.25),
            accent,
            Mix(accent, Colors.Black, 0.2), Mix(accent, Colors.Black, 0.4), Mix(accent, Colors.Black, 0.6),
        ];
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static Color[]? ReadWindowsPalette()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent");
            if (key?.GetValue("AccentPalette") is not byte[] { Length: >= 28 } bytes)
                return null;

            var colors = new Color[7];
            for (var i = 0; i < colors.Length; i++)
                colors[i] = Color.FromRgb(bytes[i * 4], bytes[i * 4 + 1], bytes[i * 4 + 2]);
            return colors;
        }
        catch (Exception e) when (e is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            return null;
        }
    }

    /// <summary>
    /// A palette from the system shades. Fills and the primary button use the accent and its darker
    /// shades, text the lighter ones; the window ground takes the accent's hue at the darkness the
    /// palettes use, with the saturation toned down for greyish accents.
    /// </summary>
    private static Palette FromSystem(Color[] shades)
    {
        var (light3, light2, accent, dark1, dark2) = (shades[0], shades[1], shades[3], shades[4], shades[5]);
        var (hue, saturation, _) = ToHsl(accent);
        var groundSaturation = Math.Min(1, saturation / 0.6);

        var primaryTop = dark1;
        var onAccent = Luminance(Mix(accent, primaryTop, 0.5)) > 0.25
            ? FromHsl(hue, 0.4 * groundSaturation, 0.08)
            : Colors.White;

        return new Palette(
            Accent: accent,
            Light: light2,
            Text: light3,
            TextHover: Mix(light3, Colors.White, 0.5),
            OnAccent: onAccent,
            PrimaryTop: primaryTop,
            PrimaryBottom: Mix(dark1, dark2, 0.25),
            PrimaryHoverTop: Mix(accent, dark1, 0.5),
            PrimaryHoverBottom: dark1,
            PrimaryPressed: Mix(dark1, dark2, 0.5),
            Fallback: FromHsl(hue, 0.21 * groundSaturation, 0.15),
            Tint: FromHsl(hue, 0.30 * groundSaturation, 0.09),
            Popup: FromHsl(hue, 0.18 * groundSaturation, 0.15));
    }

    private static Color Mix(Color a, Color b, double amount) => Color.FromRgb(
        (byte)Math.Round(a.R + (b.R - a.R) * amount),
        (byte)Math.Round(a.G + (b.G - a.G) * amount),
        (byte)Math.Round(a.B + (b.B - a.B) * amount));

    /// <summary>Relative luminance (WCAG), 0 for black to 1 for white.</summary>
    private static double Luminance(Color c)
    {
        static double Linear(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Linear(c.R) + 0.7152 * Linear(c.G) + 0.0722 * Linear(c.B);
    }

    private static (double Hue, double Saturation, double Lightness) ToHsl(Color c)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
        var l = (max + min) / 2;
        if (max - min < 1e-9)
            return (0, 0, l);

        var d = max - min;
        var s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        var h = max == r ? (g - b) / d + (g < b ? 6 : 0) : max == g ? (b - r) / d + 2 : (r - g) / d + 4;
        return (h * 60, s, l);
    }

    private static Color FromHsl(double hue, double saturation, double lightness)
    {
        var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
        var m = lightness - c / 2;
        var (r, g, b) = (int)(hue / 60) switch
        {
            0 => (c, x, 0.0),
            1 => (x, c, 0.0),
            2 => (0.0, c, x),
            3 => (0.0, x, c),
            4 => (x, 0.0, c),
            _ => (c, 0.0, x),
        };
        return Color.FromRgb((byte)Math.Round((r + m) * 255), (byte)Math.Round((g + m) * 255), (byte)Math.Round((b + m) * 255));
    }

    private static IImmutableSolidColorBrush Solid(Color color) => new ImmutableSolidColorBrush(color);

    private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    private static IBrush Vertical(Color top, Color bottom) => new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops = { new GradientStop(top, 0), new GradientStop(bottom, 1) },
    }.ToImmutable();

    private sealed record Palette(
        Color Accent, Color Light, Color Text, Color TextHover, Color OnAccent,
        Color PrimaryTop, Color PrimaryBottom, Color PrimaryHoverTop, Color PrimaryHoverBottom, Color PrimaryPressed,
        Color Fallback, Color Tint, Color Popup)
    {
        public static Palette FromHex(
            string accent, string light, string text, string textHover, string onAccent,
            (string Top, string Bottom) primary, (string Top, string Bottom) primaryHover, string primaryPressed,
            string fallback, string tint, string popup) => new(
                Color.Parse(accent), Color.Parse(light), Color.Parse(text), Color.Parse(textHover), Color.Parse(onAccent),
                Color.Parse(primary.Top), Color.Parse(primary.Bottom), Color.Parse(primaryHover.Top), Color.Parse(primaryHover.Bottom), Color.Parse(primaryPressed),
                Color.Parse(fallback), Color.Parse(tint), Color.Parse(popup));
    }
}
