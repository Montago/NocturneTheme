using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Frosted.Wpf.Controls;

/// <summary>Puts a small count badge on the top-right corner of its content (a button, an icon).</summary>
public class Badged : ContentControl
{
    static Badged()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Badged), new FrameworkPropertyMetadata(typeof(Badged)));
        FocusableProperty.OverrideMetadata(typeof(Badged), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty BadgeProperty = DependencyProperty.Register(
        nameof(Badge), typeof(object), typeof(Badged), new PropertyMetadata(null));

    /// <summary>What the badge shows; the badge is hidden while this is null.</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public static readonly DependencyProperty BadgeBackgroundProperty = DependencyProperty.Register(
        nameof(BadgeBackground), typeof(Brush), typeof(Badged), new PropertyMetadata(null));

    /// <summary>The badge's fill (the accent by default).</summary>
    public Brush? BadgeBackground
    {
        get => (Brush?)GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }

    public static readonly DependencyProperty BadgeForegroundProperty = DependencyProperty.Register(
        nameof(BadgeForeground), typeof(Brush), typeof(Badged), new PropertyMetadata(null));

    /// <summary>The badge's text colour (the text on the accent by default).</summary>
    public Brush? BadgeForeground
    {
        get => (Brush?)GetValue(BadgeForegroundProperty);
        set => SetValue(BadgeForegroundProperty, value);
    }
}

/// <summary>A large glass button for a dashboard: an icon (Content), a count on the right and a title at the bottom.</summary>
public class Tile : Button
{
    static Tile()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(typeof(Tile)));
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(Tile), new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
        nameof(Count), typeof(object), typeof(Tile), new PropertyMetadata(null));

    /// <summary>A number in the top-right corner; hidden while null.</summary>
    public object? Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
}

/// <summary>
/// Blurs and dims its content and shows a small glass card with a progress ring and
/// <see cref="BusyContent"/> while <see cref="IsBusy"/> is true. The content takes no input meanwhile.
/// </summary>
[TemplatePart(Name = PartContent, Type = typeof(UIElement))]
public class BusyIndicator : ContentControl
{
    private const string PartContent = "PART_Content";

    private UIElement? content;

    static BusyIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyIndicator), new FrameworkPropertyMetadata(typeof(BusyIndicator)));
        FocusableProperty.OverrideMetadata(typeof(BusyIndicator), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
        nameof(IsBusy), typeof(bool), typeof(BusyIndicator), new PropertyMetadata(false, OnBlurChanged));

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly DependencyProperty BusyContentProperty = DependencyProperty.Register(
        nameof(BusyContent), typeof(object), typeof(BusyIndicator), new PropertyMetadata("Please wait…"));

    public object? BusyContent
    {
        get => GetValue(BusyContentProperty);
        set => SetValue(BusyContentProperty, value);
    }

    public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(
        nameof(BlurRadius), typeof(double), typeof(BusyIndicator), new PropertyMetadata(4.0, OnBlurChanged));

    /// <summary>Blur radius of the content while busy (4 by default); 0 turns the blur off.</summary>
    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (content != null)
        {
            content.Effect = null;
        }

        base.OnApplyTemplate();

        content = GetTemplateChild(PartContent) as UIElement;
        UpdateBlur();
    }

    private static void OnBlurChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((BusyIndicator)d).UpdateBlur();

    // Set from code rather than by a trigger: a BlurEffect turns off ClearType, so the content
    // carries it only while busy, and the radius follows BlurRadius.
    private void UpdateBlur()
    {
        if (content != null)
        {
            content.Effect = IsBusy && BlurRadius > 0 ? new BlurEffect { Radius = BlurRadius, KernelType = KernelType.Gaussian } : null;
        }
    }
}

/// <summary>A stack of expanders where opening one closes the others; the open one fills the height left.</summary>
public class Accordion : ItemsControl
{
    static Accordion()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Accordion), new FrameworkPropertyMetadata(typeof(Accordion)));
        FocusableProperty.OverrideMetadata(typeof(Accordion), new FrameworkPropertyMetadata(false));
    }

    public Accordion()
    {
        AddHandler(Expander.ExpandedEvent, new RoutedEventHandler(OnItemExpanded));
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is Expander;

    protected override DependencyObject GetContainerForItemOverride() => new Expander();

    private void OnItemExpanded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not Expander opened || ItemsControlFromItemContainer(opened) != this)
        {
            return;
        }

        for (var i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is Expander other && !ReferenceEquals(other, opened))
            {
                other.IsExpanded = false;
            }
        }
    }
}
