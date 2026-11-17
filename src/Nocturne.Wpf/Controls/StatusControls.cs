using System.Windows;
using System.Windows.Controls;

namespace Nocturne.Wpf.Controls;

/// <summary>A spinning ring for work of unknown length. It is hidden while <see cref="IsActive"/> is false.</summary>
public class ProgressRing : Control
{
    static ProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
        FocusableProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive), typeof(bool), typeof(ProgressRing), new PropertyMetadata(true));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
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
[TemplatePart(Name = PartClose, Type = typeof(Button))]
public class InfoBar : ContentControl
{
    private const string PartClose = "PART_Close";
    private Button? close;

    static InfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
        FocusableProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(false));
    }

    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        nameof(Closed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InfoBar));

    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(
        nameof(Severity), typeof(InfoBarSeverity), typeof(InfoBar), new PropertyMetadata(InfoBarSeverity.Info));

    public InfoBarSeverity Severity
    {
        get => (InfoBarSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(InfoBar), new PropertyMetadata(null));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(InfoBar), new PropertyMetadata(null));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(InfoBar), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>The bar is collapsed while this is false.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable), typeof(bool), typeof(InfoBar), new PropertyMetadata(true));

    /// <summary>Show the close button (true by default).</summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (close != null)
        {
            close.Click -= OnCloseClick;
        }
        base.OnApplyTemplate();
        close = GetTemplateChild(PartClose) as Button;
        if (close != null)
        {
            close.Click += OnCloseClick;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        IsOpen = false;
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
    }
}
