using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Path = Avalonia.Controls.Shapes.Path;

namespace Nocturne.Avalonia.Controls;

/// <summary>A row of stars: filled ones in the light accent, empty ones as dark outlines. Click a star to rate; click it again to clear.</summary>
[TemplatePart(PartHost, typeof(Panel))]
public class Rating : TemplatedControl
{
    private const string PartHost = "PART_Host";
    private static readonly Geometry Star = StreamGeometry.Parse("M9,1.5 L11.3,6.3 16.5,7 12.7,10.6 13.6,15.8 9,13.3 4.4,15.8 5.3,10.6 1.5,7 6.7,6.3 z");

    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<Rating, int>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<Rating, int>(nameof(Maximum), 5);

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<Rating, bool>(nameof(IsReadOnly));

    private Panel? host;
    private int hover;

    static Rating()
    {
        FocusableProperty.OverrideDefaultValue<Rating>(true);
    }

    protected override Type StyleKeyOverride => typeof(Rating);

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        host = e.NameScope.Find<Panel>(PartHost);
        Build();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
            Refresh();
        else if (change.Property == MaximumProperty)
            Build();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsReadOnly)
            return;
        if (e.Key is Key.Right or Key.Up)
        {
            Value = Math.Min(Maximum, Value + 1);
            e.Handled = true;
        }
        else if (e.Key is Key.Left or Key.Down)
        {
            Value = Math.Max(0, Value - 1);
            e.Handled = true;
        }
    }

    private void Build()
    {
        if (host == null)
            return;
        host.Children.Clear();
        for (var i = 1; i <= Maximum; i++)
        {
            var index = i;
            var star = new Path
            {
                Data = Star, Width = 18, Height = 18, StrokeThickness = 1.4, StrokeJoin = PenLineJoin.Round,
                Margin = new Thickness(0, 0, 4, 0), Cursor = new Cursor(StandardCursorType.Hand),
            };
            star.PointerEntered += (_, _) =>
            {
                if (!IsReadOnly)
                {
                    hover = index;
                    Refresh();
                }
            };
            star.PointerExited += (_, _) =>
            {
                hover = 0;
                Refresh();
            };
            star.PointerReleased += (_, e) =>
            {
                if (!IsReadOnly && e.InitialPressMouseButton == MouseButton.Left)
                    Value = Value == index ? 0 : index;
            };
            host.Children.Add(star);
        }
        Refresh();
    }

    private void Refresh()
    {
        if (host == null)
            return;
        var shown = hover > 0 ? hover : Value;
        for (var i = 0; i < host.Children.Count; i++)
        {
            if (host.Children[i] is not Path p)
                continue;
            var on = i < shown;
            p.Bind(Shape.FillProperty, p.GetResourceObservable(on ? "Nocturne.Brushes.Accent.Light" : "Nocturne.Brushes.Toggle.Fill"));
            p.Bind(Shape.StrokeProperty, p.GetResourceObservable(on ? "Nocturne.Brushes.Accent.Light" : "Nocturne.Brushes.Toggle.Border"));
        }
    }
}

/// <summary>Slider with two thumbs selecting a range (<see cref="LowerValue"/> to <see cref="UpperValue"/>).</summary>
[TemplatePart(PartTrack, typeof(Control))]
[TemplatePart(PartFill, typeof(Control))]
[TemplatePart(PartLower, typeof(Thumb))]
[TemplatePart(PartUpper, typeof(Thumb))]
public class RangeSlider : TemplatedControl
{
    private const string PartTrack = "PART_Track";
    private const string PartFill = "PART_Fill";
    private const string PartLower = "PART_LowerThumb";
    private const string PartUpper = "PART_UpperThumb";
    // Half the 20px slider thumb
    private const double ThumbHalf = 10;

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(Maximum), 100);

    public static readonly StyledProperty<double> LowerValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(LowerValue), 25, defaultBindingMode: BindingMode.TwoWay, coerce: CoerceLower);

    public static readonly StyledProperty<double> UpperValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(UpperValue), 75, defaultBindingMode: BindingMode.TwoWay, coerce: CoerceUpper);

    public static readonly StyledProperty<double> SmallChangeProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(SmallChange), 1);

    private Control? track;
    private Control? fill;
    private Thumb? lower;
    private Thumb? upper;

    protected override Type StyleKeyOverride => typeof(RangeSlider);

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double LowerValue
    {
        get => GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    public double UpperValue
    {
        get => GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    public double SmallChange
    {
        get => GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    private static double CoerceLower(AvaloniaObject d, double v)
    {
        var r = (RangeSlider)d;
        return Math.Max(r.Minimum, Math.Min(v, r.UpperValue));
    }

    private static double CoerceUpper(AvaloniaObject d, double v)
    {
        var r = (RangeSlider)d;
        return Math.Min(r.Maximum, Math.Max(v, r.LowerValue));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MinimumProperty || change.Property == MaximumProperty
            || change.Property == LowerValueProperty || change.Property == UpperValueProperty)
        {
            // Values set in XAML arrive one at a time; re-coerce so LowerValue="2500" survives a default UpperValue of 75.
            if (change.Property != LowerValueProperty)
                CoerceValue(LowerValueProperty);
            if (change.Property != UpperValueProperty)
                CoerceValue(UpperValueProperty);
            UpdateParts();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        track = e.NameScope.Find<Control>(PartTrack);
        fill = e.NameScope.Find<Control>(PartFill);
        lower = e.NameScope.Find<Thumb>(PartLower);
        upper = e.NameScope.Find<Thumb>(PartUpper);

        if (track != null)
            track.SizeChanged += (_, _) => UpdateParts();
        if (lower != null)
            lower.DragDelta += (_, args) => LowerValue += ToValueDelta(args.Vector.X);
        if (upper != null)
            upper.DragDelta += (_, args) => UpperValue += ToValueDelta(args.Vector.X);
        UpdateParts();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var step = e.Key switch { Key.Right or Key.Up => SmallChange, Key.Left or Key.Down => -SmallChange, _ => 0 };
        if (step == 0)
            return;
        if (upper?.IsFocused == true)
            UpperValue += step;
        else
            LowerValue += step;
        e.Handled = true;
    }

    private double Usable => Math.Max(1, (track?.Bounds.Width ?? 0) - 2 * ThumbHalf);

    private double ToValueDelta(double pixels) => pixels / Usable * (Maximum - Minimum);

    private double ToPixels(double value) => Maximum > Minimum ? (value - Minimum) / (Maximum - Minimum) * Usable : 0;

    private void UpdateParts()
    {
        if (track == null)
            return;
        var a = ToPixels(LowerValue);
        var b = ToPixels(UpperValue);
        if (lower != null)
            Canvas.SetLeft(lower, a);
        if (upper != null)
            Canvas.SetLeft(upper, b);
        if (fill != null)
        {
            Canvas.SetLeft(fill, a + ThumbHalf);
            fill.Width = Math.Max(0, b - a);
        }
    }
}
