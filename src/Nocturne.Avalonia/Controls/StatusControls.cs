using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Nocturne.Avalonia.Controls;

/// <summary>A spinning ring for work of unknown length. It is hidden while <see cref="IsActive"/> is false.</summary>
public class ProgressRing : TemplatedControl
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsActive), true);

    static ProgressRing()
    {
        FocusableProperty.OverrideDefaultValue<ProgressRing>(false);
    }

    protected override Type StyleKeyOverride => typeof(ProgressRing);

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}

public enum InfoBarSeverity
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// A message strip inside the page: an icon in the colour of <see cref="Severity"/>, a bold
/// <see cref="Title"/>, the <see cref="Message"/> and, as Content, an optional action (e.g. a button).
/// The close button sets <see cref="IsOpen"/> to false and raises <see cref="Closed"/>.
/// </summary>
[TemplatePart(PartClose, typeof(Button))]
public class InfoBar : ContentControl
{
    private const string PartClose = "PART_Close";

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<InfoBar, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    public static readonly StyledProperty<InfoBarSeverity> SeverityProperty =
        AvaloniaProperty.Register<InfoBar, InfoBarSeverity>(nameof(Severity));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<InfoBar, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<InfoBar, string?>(nameof(Message));

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<InfoBar, bool>(nameof(IsOpen), true, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<InfoBar, bool>(nameof(IsClosable), true);

    private Button? close;

    static InfoBar()
    {
        FocusableProperty.OverrideDefaultValue<InfoBar>(false);
    }

    protected override Type StyleKeyOverride => typeof(InfoBar);

    public event EventHandler<RoutedEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public InfoBarSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>The bar is collapsed while this is false.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Show the close button (true by default).</summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (close != null)
            close.Click -= OnCloseClick;
        base.OnApplyTemplate(e);
        close = e.NameScope.Find<Button>(PartClose);
        if (close != null)
            close.Click += OnCloseClick;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        IsOpen = false;
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
    }
}
