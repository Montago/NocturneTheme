using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Nocturne.Wpf.Controls;

/// <summary>The accent colours that <see cref="NocturneTheme.SetAccent"/> can switch to.</summary>
public enum NocturneAccent
{
    /// <summary>Meadow green, the colour of the original design.</summary>
    Green,

    Orange,

    /// <summary>Blue (the default).</summary>
    Blue,

    /// <summary>Gold; text on it is dark, as white on yellow is hard to read.</summary>
    Yellow,

    /// <summary>Teal (it took the place of red, which could not be told from the danger buttons).</summary>
    Teal,

    /// <summary>
    /// The accent colour chosen in Windows (Settings, Personalisation, Colours), followed while the
    /// app runs. Blue where Windows has none.
    /// </summary>
    System,
}

/// <summary>
/// Switches the theme's accent at run time: the accent brushes, the primary button, the window's
/// tint and fallback ground and the popup ground all take the new hue. Every style references
/// the brushes with DynamicResource, so open windows follow at once.
/// </summary>
public static class NocturneTheme
{
    private static readonly Dictionary<NocturneAccent, Palette> Palettes = new()
    {
        [NocturneAccent.Green] = Palette.FromHex(
            accent: "#1F9D57", light: "#3FD27F", text: "#8EF0B8", textHover: "#C2F7D8", onAccent: "#FFFFFF",
            primary: ("#1A8C50", "#157E46"), primaryHover: ("#169457", "#13834A"), primaryPressed: "#126E3D",
            fallback: "#1E2E25", tint: "#101E16", popup: "#1F2D25"),
        [NocturneAccent.Orange] = Palette.FromHex(
            accent: "#D9661A", light: "#FF9A4D", text: "#FFC08F", textHover: "#FFDDC2", onAccent: "#FFFFFF",
            primary: ("#C85A14", "#B44F10"), primaryHover: ("#D2621A", "#BE5614"), primaryPressed: "#9E4510",
            fallback: "#2E241E", tint: "#1E1610", popup: "#2D241F"),
        [NocturneAccent.Blue] = Palette.FromHex(
            accent: "#1F78D1", light: "#4DA3FF", text: "#9CCBFF", textHover: "#C9E2FF", onAccent: "#FFFFFF",
            primary: ("#1A6CC0", "#155FAD"), primaryHover: ("#1D76CF", "#1867B9"), primaryPressed: "#134F8C",
            fallback: "#1E2633", tint: "#10161F", popup: "#1F2633"),
        [NocturneAccent.Yellow] = Palette.FromHex(
            accent: "#D1A112", light: "#F5CB3A", text: "#FFE08A", textHover: "#FFEDBD", onAccent: "#1E1A0C",
            primary: ("#D4A514", "#C29510"), primaryHover: ("#DDB01E", "#CB9E16"), primaryPressed: "#A98410",
            fallback: "#2E2A1E", tint: "#1E1A0F", popup: "#2D291E"),
        [NocturneAccent.Teal] = Palette.FromHex(
            accent: "#17A398", light: "#3FD9CB", text: "#8EEFE5", textHover: "#C2F7F1", onAccent: "#FFFFFF",
            primary: ("#14897F", "#107B72"), primaryHover: ("#16938A", "#13857B"), primaryPressed: "#0F6A62",
            fallback: "#1E2D2C", tint: "#101E1D", popup: "#1F2D2C"),
    };

    private static ResourceDictionary? target;
    private static Color? appliedSystemAccent;
    private static Color? lastSeenSystemAccent = GetSystemAccent();

    /// <summary>The accent last set with <see cref="SetAccent"/>.</summary>
    public static NocturneAccent Accent { get; private set; } = NocturneAccent.Blue;

    /// <summary>Raised after <see cref="SetAccent"/> (or a change of the Windows accent) has replaced the brushes.</summary>
    public static event EventHandler? AccentChanged;

    /// <summary>Raised when the accent colour in Windows has changed, whichever accent the app uses.</summary>
    public static event EventHandler? SystemAccentChanged;

    /// <summary>
    /// Puts the brushes for <paramref name="accent"/> into <paramref name="resources"/> (by default
    /// the application's resources, where they override the ones in Nocturne.xaml).
    /// </summary>
    public static void SetAccent(NocturneAccent accent, ResourceDictionary? resources = null)
    {
        resources ??= Application.Current?.Resources ?? throw new InvalidOperationException("No application to set the accent on; pass a ResourceDictionary.");
        target = resources;
        Accent = accent;

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

    /// <summary>
    /// The accent colour chosen in Windows, or null where Windows has none (before Windows 10).
    /// </summary>
    public static Color? GetSystemAccent() => ReadSystemPalette()?[3];

    /// <summary>
    /// Called by <see cref="NocturneWindow"/> when Windows reports changed settings: picks up a new
    /// accent colour when the app follows the system one.
    /// </summary>
    internal static void OnSystemSettingsChanged()
    {
        var current = GetSystemAccent();
        if (current == lastSeenSystemAccent)
        {
            return;
        }
        lastSeenSystemAccent = current;

        if (Accent == NocturneAccent.System && target != null && current != appliedSystemAccent)
        {
            SetAccent(NocturneAccent.System, target);
        }
        SystemAccentChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void Apply(Palette p, ResourceDictionary resources)
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
        resources["Nocturne.Brushes.Popup"] = Solid(Color.FromArgb(0xF7, p.Popup.R, p.Popup.G, p.Popup.B));
        resources["Nocturne.Brushes.TitleBar.Accent"] = Vertical(WithAlpha(p.Accent, 0x4D), WithAlpha(p.Accent, 0x38));
        resources["Nocturne.Brushes.TitleBar.Accent.Border"] = Solid(WithAlpha(p.Light, 0x38));
    }

    /// <summary>
    /// The accent palette Windows derives from the user's colour: lightest three, the accent
    /// itself, darkest three (the eighth entry is unused). Null when it cannot be read.
    /// </summary>
    private static Color[]? ReadSystemPalette()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent");
            if (key?.GetValue("AccentPalette") is not byte[] { Length: >= 28 } bytes)
            {
                return null;
            }

            var colors = new Color[7];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.FromRgb(bytes[i * 4], bytes[i * 4 + 1], bytes[i * 4 + 2]);
            }
            return colors;
        }
        catch (Exception e) when (e is System.Security.SecurityException or UnauthorizedAccessException or System.IO.IOException)
        {
            return null;
        }
    }

    /// <summary>
    /// A palette from the Windows shades. Fills and the primary button use the accent and its darker
    /// shades, text the lighter ones; the window ground takes the accent's hue at the darkness the
    /// palettes use, with the saturation toned down for greyish accents.
    /// </summary>
    private static Palette FromSystem(Color[] shades)
    {
        var (light3, light2, _, accent, dark1, dark2, _) = (shades[0], shades[1], shades[2], shades[3], shades[4], shades[5], shades[6]);
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
        {
            return (0, 0, l);
        }

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

    private static SolidColorBrush Solid(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

    private static LinearGradientBrush Vertical(Color top, Color bottom)
    {
        var brush = new LinearGradientBrush(top, bottom, new Point(0, 0), new Point(0, 1));
        brush.Freeze();
        return brush;
    }

    private sealed record Palette(
        Color Accent, Color Light, Color Text, Color TextHover, Color OnAccent,
        Color PrimaryTop, Color PrimaryBottom, Color PrimaryHoverTop, Color PrimaryHoverBottom, Color PrimaryPressed,
        Color Fallback, Color Tint, Color Popup)
    {
        public static Palette FromHex(
            string accent, string light, string text, string textHover, string onAccent,
            (string Top, string Bottom) primary, (string Top, string Bottom) primaryHover, string primaryPressed,
            string fallback, string tint, string popup) => new(
                Parse(accent), Parse(light), Parse(text), Parse(textHover), Parse(onAccent),
                Parse(primary.Top), Parse(primary.Bottom), Parse(primaryHover.Top), Parse(primaryHover.Bottom), Parse(primaryPressed),
                Parse(fallback), Parse(tint), Parse(popup));

        private static Color Parse(string hex) => (Color)ColorConverter.ConvertFromString(hex);
    }
}
