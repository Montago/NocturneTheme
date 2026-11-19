using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Nocturne.Avalonia.Controls;

/// <summary>Puts a small count badge on the top-right corner of its content (a button, an icon).</summary>
public class Badged : ContentControl
{
    public static readonly StyledProperty<object?> BadgeProperty =
        AvaloniaProperty.Register<Badged, object?>(nameof(Badge));

    public static readonly StyledProperty<IBrush?> BadgeBackgroundProperty =
        AvaloniaProperty.Register<Badged, IBrush?>(nameof(BadgeBackground));

    public static readonly StyledProperty<IBrush?> BadgeForegroundProperty =
        AvaloniaProperty.Register<Badged, IBrush?>(nameof(BadgeForeground));

    static Badged()
    {
        FocusableProperty.OverrideDefaultValue<Badged>(false);
    }

    protected override Type StyleKeyOverride => typeof(Badged);

    /// <summary>What the badge shows; the badge is hidden while this is null.</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    /// <summary>The badge's fill (the accent by default).</summary>
    public IBrush? BadgeBackground
    {
        get => GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }

    /// <summary>The badge's text colour (the text on the accent by default).</summary>
    public IBrush? BadgeForeground
    {
        get => GetValue(BadgeForegroundProperty);
        set => SetValue(BadgeForegroundProperty, value);
    }
}

/// <summary>A large glass button for a dashboard: an icon (Content), a count on the right and a title at the bottom.</summary>
public class Tile : Button
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Tile, string?>(nameof(Title));

    public static readonly StyledProperty<object?> CountProperty =
        AvaloniaProperty.Register<Tile, object?>(nameof(Count));

    protected override Type StyleKeyOverride => typeof(Tile);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

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
[TemplatePart(PartContent, typeof(Control))]
public class BusyIndicator : ContentControl
{
    private const string PartContent = "PART_ContentPresenter";

    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<BusyIndicator, bool>(nameof(IsBusy));

    public static readonly StyledProperty<object?> BusyContentProperty =
        AvaloniaProperty.Register<BusyIndicator, object?>(nameof(BusyContent), "Please wait…");

    public static readonly StyledProperty<double> BlurRadiusProperty =
        AvaloniaProperty.Register<BusyIndicator, double>(nameof(BlurRadius), 4);

    private Control? content;

    static BusyIndicator()
    {
        FocusableProperty.OverrideDefaultValue<BusyIndicator>(false);
    }

    protected override Type StyleKeyOverride => typeof(BusyIndicator);

    public bool IsBusy
    {
        get => GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public object? BusyContent
    {
        get => GetValue(BusyContentProperty);
        set => SetValue(BusyContentProperty, value);
    }

    /// <summary>Blur radius of the content while busy (4 by default); 0 turns the blur off.</summary>
    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (content != null)
            content.Effect = null;
        base.OnApplyTemplate(e);
        content = e.NameScope.Find<Control>(PartContent);
        UpdateBlur();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsBusyProperty || change.Property == BlurRadiusProperty)
            UpdateBlur();
    }

    // Set from code: the radius follows BlurRadius, and the content carries the effect only while busy.
    private void UpdateBlur()
    {
        if (content != null)
            content.Effect = IsBusy && BlurRadius > 0 ? new BlurEffect { Radius = BlurRadius } : null;
    }
}

/// <summary>A stack of expanders where opening one closes the others; the open one fills the height left.</summary>
public class Accordion : ItemsControl
{
    static Accordion()
    {
        FocusableProperty.OverrideDefaultValue<Accordion>(false);
    }

    public Accordion()
    {
        AddHandler(Expander.ExpandedEvent, OnItemExpanded);
    }

    protected override Type StyleKeyOverride => typeof(Accordion);

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<Expander>(item, out recycleKey);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new Expander();

    private void OnItemExpanded(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Expander opened || IndexFromContainer(opened) < 0)
            return;
        foreach (var other in GetRealizedContainers().OfType<Expander>())
        {
            if (!ReferenceEquals(other, opened))
                other.IsExpanded = false;
        }
    }
}

/// <summary>
/// Vertical stack where the expanded <see cref="Expander"/> takes all the height the closed ones
/// leave, so their headers sit at the bottom. With unlimited height it behaves like a StackPanel.
/// </summary>
public class AccordionPanel : Panel
{
    private static bool IsFill(Control child) => child is Expander { IsExpanded: true };

    protected override void ChildrenChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);
        // The open item changes the layout of the others
        foreach (var child in Children.OfType<Expander>())
        {
            child.PropertyChanged -= OnChildPropertyChanged;
            child.PropertyChanged += OnChildPropertyChanged;
        }
    }

    private void OnChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Expander.IsExpandedProperty)
            InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = 0.0;
        var fixedHeight = 0.0;
        Control? fill = null;

        foreach (var child in Children)
        {
            if (fill == null && IsFill(child))
            {
                fill = child;
                continue;
            }
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            width = Math.Max(width, child.DesiredSize.Width);
            fixedHeight += child.DesiredSize.Height;
        }

        if (fill == null)
            return new Size(width, fixedHeight);

        var rest = double.IsInfinity(availableSize.Height) ? double.PositiveInfinity : Math.Max(0, availableSize.Height - fixedHeight);
        fill.Measure(new Size(availableSize.Width, rest));
        width = Math.Max(width, fill.DesiredSize.Width);
        return new Size(width, fixedHeight + fill.DesiredSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Control? fill = null;
        var fixedHeight = 0.0;
        foreach (var child in Children)
        {
            if (fill == null && IsFill(child))
                fill = child;
            else
                fixedHeight += child.DesiredSize.Height;
        }

        var y = 0.0;
        foreach (var child in Children)
        {
            var height = ReferenceEquals(child, fill)
                ? Math.Max(child.DesiredSize.Height, finalSize.Height - fixedHeight)
                : child.DesiredSize.Height;
            child.Arrange(new Rect(0, y, finalSize.Width, height));
            y += height;
        }
        return finalSize;
    }
}

/// <summary>
/// A strip of status text along the bottom of a window: a sunken strip with a hairline on top and
/// small secondary text. Items are laid out in a DockPanel: the last one fills the rest, so
/// right-align it to put it at the end. A <see cref="Separator"/> becomes a vertical line.
/// (Avalonia has no StatusBar; this one keeps the WPF names.)
/// </summary>
public class StatusBar : ItemsControl
{
    static StatusBar()
    {
        FocusableProperty.OverrideDefaultValue<StatusBar>(false);
    }

    protected override Type StyleKeyOverride => typeof(StatusBar);

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not (StatusBarItem or Separator);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new StatusBarItem();

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        // A separator between items is a vertical line
        if (container is Separator separator && !separator.IsSet(ThemeProperty)
            && this.TryFindResource("Nocturne.Styles.Separator.Vertical", out var theme) && theme is global::Avalonia.Styling.ControlTheme vertical)
        {
            separator.Theme = vertical;
        }
    }
}

/// <summary>An item of a <see cref="StatusBar"/>: its content, 6px from its neighbours.</summary>
public class StatusBarItem : ContentControl
{
    static StatusBarItem()
    {
        FocusableProperty.OverrideDefaultValue<StatusBarItem>(false);
    }

    protected override Type StyleKeyOverride => typeof(StatusBarItem);
}
