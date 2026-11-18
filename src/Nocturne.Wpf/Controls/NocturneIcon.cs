using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Nocturne.Wpf.Controls;

/// <summary>
/// One of the Nocturne icons (Material Design Icons, filled), drawn in <see cref="Control.Foreground"/>,
/// 20×20 by default: <c>&lt;nc:NocturneIcon Kind="Bell" /&gt;</c>. The set is listed in icons/icons.json.
/// Shortcuts: <see cref="IconExtension"/> (<c>Icon="{nc:Icon Bell}"</c>) and
/// <see cref="IconGeometryExtension"/> (<c>Data="{nc:IconGeometry Bell}"</c>).
/// </summary>
public class NocturneIcon : Control
{
    private static readonly Dictionary<NocturneIconKind, Geometry> Cache = [];

    static NocturneIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NocturneIcon), new FrameworkPropertyMetadata(typeof(NocturneIcon)));
        FocusableProperty.OverrideMetadata(typeof(NocturneIcon), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(NocturneIcon), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(NocturneIconKind), typeof(NocturneIcon),
        new PropertyMetadata(NocturneIconKind.None, (d, e) => d.SetValue(DataPropertyKey, GeometryOf((NocturneIconKind)e.NewValue))));

    public NocturneIconKind Kind
    {
        get => (NocturneIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private static readonly DependencyPropertyKey DataPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Data), typeof(Geometry), typeof(NocturneIcon), new PropertyMetadata(null));

    public static readonly DependencyProperty DataProperty = DataPropertyKey.DependencyProperty;

    /// <summary>The icon's path in a 24×24 box.</summary>
    public Geometry? Data => (Geometry?)GetValue(DataProperty);

    /// <summary>The frozen geometry of an icon in a 24×24 box, or null for None.</summary>
    public static Geometry? GeometryOf(NocturneIconKind kind)
    {
        if (kind == NocturneIconKind.None)
        {
            return null;
        }
        lock (Cache)
        {
            if (!Cache.TryGetValue(kind, out var geometry))
            {
                // F1: MDI is drawn for the nonzero fill rule; WPF's path markup defaults to even-odd
                geometry = Geometry.Parse("F1 " + NocturneIconData.Get(kind));
                geometry.Freeze();
                Cache[kind] = geometry;
            }
            return geometry;
        }
    }
}

/// <summary>
/// A new <see cref="NocturneIcon"/> where a property takes content: <c>Icon="{nc:Icon ContentSave}"</c>
/// on a MenuItem or HamburgerMenuItem, <c>Content="{nc:Icon Refresh, Size=18}"</c> on an icon button.
/// An element has one parent, so in a Style setter (one value shared by every control) use a
/// <see cref="NocturneIcon"/> in a template instead.
/// </summary>
[MarkupExtensionReturnType(typeof(NocturneIcon))]
public class IconExtension : MarkupExtension
{
    public IconExtension()
    {
    }

    public IconExtension(NocturneIconKind kind) => Kind = kind;

    [ConstructorArgument("kind")]
    public NocturneIconKind Kind { get; set; }

    /// <summary>Width and height; the style's 20 when not set.</summary>
    public double Size { get; set; } = double.NaN;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var icon = new NocturneIcon { Kind = Kind };
        if (!double.IsNaN(Size))
        {
            icon.Width = icon.Height = Size;
        }
        return icon;
    }
}

/// <summary>
/// The frozen geometry of an icon in a 24×24 box, for a Path or a DrawingImage of your own:
/// <c>&lt;Path Data="{nc:IconGeometry Bell}" Fill="White" /&gt;</c>. The icons are drawn filled.
/// </summary>
[MarkupExtensionReturnType(typeof(Geometry))]
public class IconGeometryExtension : MarkupExtension
{
    public IconGeometryExtension()
    {
    }

    public IconGeometryExtension(NocturneIconKind kind) => Kind = kind;

    [ConstructorArgument("kind")]
    public NocturneIconKind Kind { get; set; }

    public override object? ProvideValue(IServiceProvider serviceProvider) => NocturneIcon.GeometryOf(Kind);
}
