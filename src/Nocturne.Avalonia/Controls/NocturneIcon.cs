using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// One of the Nocturne icons (Material Design Icons, filled), drawn in <see cref="TemplatedControl.Foreground"/>,
/// 20×20 by default: <c>&lt;nc:NocturneIcon Kind="Bell" /&gt;</c>. The set is listed in icons/icons.json.
/// Shortcuts: <see cref="IconExtension"/> (<c>Icon="{nc:Icon Bell}"</c>) and
/// <see cref="IconGeometryExtension"/> (<c>Data="{nc:IconGeometry Bell}"</c>).
/// </summary>
public class NocturneIcon : TemplatedControl
{
    private static readonly Dictionary<NocturneIconKind, Geometry> Cache = [];

    public static readonly StyledProperty<NocturneIconKind> KindProperty =
        AvaloniaProperty.Register<NocturneIcon, NocturneIconKind>(nameof(Kind));

    public static readonly DirectProperty<NocturneIcon, Geometry?> DataProperty =
        AvaloniaProperty.RegisterDirect<NocturneIcon, Geometry?>(nameof(Data), o => o.Data);

    private Geometry? data;

    static NocturneIcon()
    {
        FocusableProperty.OverrideDefaultValue<NocturneIcon>(false);
    }

    public NocturneIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The icon's path in a 24×24 box.</summary>
    public Geometry? Data
    {
        get => data;
        private set => SetAndRaise(DataProperty, ref data, value);
    }

    /// <summary>The geometry of an icon in a 24×24 box, or null for None. Shared; do not change it.</summary>
    public static Geometry? GeometryOf(NocturneIconKind kind)
    {
        if (kind == NocturneIconKind.None)
            return null;
        lock (Cache)
        {
            if (!Cache.TryGetValue(kind, out var geometry))
            {
                // F1: MDI is drawn for the nonzero fill rule; path markup defaults to even-odd
                geometry = StreamGeometry.Parse("F1 " + NocturneIconData.Get(kind));
                Cache[kind] = geometry;
            }
            return geometry;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == KindProperty)
            Data = GeometryOf(Kind);
    }
}

/// <summary>
/// A new <see cref="NocturneIcon"/> where a property takes content: <c>Icon="{nc:Icon ContentSave}"</c>
/// on a MenuItem, <c>Content="{nc:Icon Refresh, Size=18}"</c> on an icon button. A control has one
/// parent, so in a ControlTheme setter (one value shared by every control) use a
/// <see cref="NocturneIcon"/> in the template instead.
/// </summary>
public class IconExtension
{
    public IconExtension()
    {
    }

    public IconExtension(NocturneIconKind kind) => Kind = kind;

    public NocturneIconKind Kind { get; set; }

    /// <summary>Width and height; the theme's 20 when not set.</summary>
    public double Size { get; set; } = double.NaN;

    public NocturneIcon ProvideValue(IServiceProvider serviceProvider)
    {
        var icon = new NocturneIcon { Kind = Kind };
        if (!double.IsNaN(Size))
            icon.Width = icon.Height = Size;
        return icon;
    }
}

/// <summary>
/// The geometry of an icon in a 24×24 box, for a Path of your own:
/// <c>&lt;Path Data="{nc:IconGeometry Bell}" Fill="White" /&gt;</c>. The icons are drawn filled.
/// </summary>
public class IconGeometryExtension
{
    public IconGeometryExtension()
    {
    }

    public IconGeometryExtension(NocturneIconKind kind) => Kind = kind;

    public NocturneIconKind Kind { get; set; }

    public Geometry? ProvideValue(IServiceProvider serviceProvider) => NocturneIcon.GeometryOf(Kind);
}
