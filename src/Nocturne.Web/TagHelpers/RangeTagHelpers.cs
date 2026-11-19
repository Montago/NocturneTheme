using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary><c>&lt;nc-progress-bar value="75" /&gt;</c>, or is-indeterminate="true" for the running stripes.</summary>
[HtmlTargetElement("nc-progress-bar", TagStructure = TagStructure.WithoutEndTag)]
public class ProgressBarTagHelper : NocturneTagHelper
{
    public double Value { get; set; }

    public double Maximum { get; set; } = 100;

    public bool IsIndeterminate { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "progress";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-progress-bar");
        output.Attributes.SetAttribute("max", Html.Num(Maximum));
        if (!IsIndeterminate)
        {
            output.Attributes.SetAttribute("value", Html.Num(Value));
        }
        ApplyCommon(output);
    }
}

/// <summary>
/// <c>&lt;nc-progress-ring width="20" /&gt;</c>: a quarter arc in the light accent running round a faint track once
/// a second (IsActive="False" hides it); 32 × 32 unless width is given.
/// </summary>
[HtmlTargetElement("nc-progress-ring", TagStructure = TagStructure.WithoutEndTag)]
public class ProgressRingTagHelper : NocturneTagHelper
{
    public bool IsActive { get; set; } = true;

    /// <summary>Width and height in pixels.</summary>
    public double? Width { get; set; }

    /// <summary>The arc of the ring: 32 × 32 drawing units, as the WPF template.</summary>
    internal const string Svg = "<svg viewBox=\"0 0 32 32\" aria-hidden=\"true\"><circle class=\"nc-progress-ring__track\" cx=\"16\" cy=\"16\" r=\"12.5\"/>" +
                                "<path class=\"nc-progress-ring__arc\" d=\"M16,3.5 A12.5,12.5 0 0 1 28.5,16\"/></svg>";

    /// <summary>A ring as markup, for other controls (BusyIndicator, a button that is saving).</summary>
    internal static string Markup(double size) =>
        $"<span class=\"nc-progress-ring\" role=\"progressbar\" aria-label=\"Working\" data-active=\"true\" style=\"--nc-ring-size:{Html.Num(size)}px\">{Svg}</span>";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-progress-ring");
        output.Attributes.SetAttribute("role", "progressbar");
        output.Attributes.SetAttribute("aria-label", "Working");
        output.Attributes.SetAttribute("data-active", IsActive ? "true" : "false");
        if (Width is { } w)
        {
            output.Attributes.SetAttribute("style", $"--nc-ring-size:{Html.Num(w)}px;" + (output.Attributes["style"]?.Value ?? ""));
        }
        ApplyCommon(output);
        output.Content.SetHtmlContent(Svg);
    }
}

/// <summary>Where a Slider draws its ticks.</summary>
public enum TickPlacement
{
    None,
    BottomRight,
}

/// <summary>
/// <c>&lt;nc-slider value="4" maximum="10" tick-placement="BottomRight" tick-frequency="1" is-snap-to-tick-enabled="true" /&gt;</c>.
/// orientation="Vertical" stands it up; name posts the value.
/// </summary>
[HtmlTargetElement("nc-slider", TagStructure = TagStructure.WithoutEndTag)]
public class SliderTagHelper : NocturneTagHelper
{
    public double Value { get; set; }

    public double Minimum { get; set; }

    public double Maximum { get; set; } = 10;

    public double? SmallChange { get; set; }

    public double? LargeChange { get; set; }

    public string Orientation { get; set; } = "Horizontal";

    public TickPlacement TickPlacement { get; set; }

    public double TickFrequency { get; set; } = 1;

    public bool IsSnapToTickEnabled { get; set; }

    public string? Name { get; set; }

    /// <summary>Text for screen readers.</summary>
    public string? Label { get; set; }

    protected virtual bool Range => false;

    protected virtual double RangeLower => Minimum;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var vertical = Orientation.Equals("Vertical", StringComparison.OrdinalIgnoreCase);
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, Range ? "nc-range-slider" : "nc-slider", vertical ? "nc-slider--vertical" : null);
        output.Attributes.SetAttribute("role", "slider");
        output.Attributes.SetAttribute("aria-valuemin", Html.Num(Minimum));
        output.Attributes.SetAttribute("aria-valuemax", Html.Num(Maximum));
        output.Attributes.SetAttribute("aria-valuenow", Html.Num(Value));
        output.Attributes.SetAttribute("aria-orientation", vertical ? "vertical" : "horizontal");
        if (Label is not null) output.Attributes.SetAttribute("aria-label", Label);
        if (Range) output.Attributes.SetAttribute("data-nc-lower-value", Html.Num(RangeLower));
        if (SmallChange is { } small) output.Attributes.SetAttribute("data-nc-small-change", Html.Num(small));
        if (LargeChange is { } large) output.Attributes.SetAttribute("data-nc-large-change", Html.Num(large));
        if (TickPlacement != TickPlacement.None) output.Attributes.SetAttribute("data-nc-ticks", Html.Num(TickFrequency));
        if (IsSnapToTickEnabled)
        {
            output.Attributes.SetAttribute("data-nc-snap", "true");
            if (TickPlacement == TickPlacement.None) output.Attributes.SetAttribute("data-nc-ticks", Html.Num(TickFrequency));
        }
        if (!IsEnabled) output.Attributes.SetAttribute("aria-disabled", "true");
        ApplyCommon(output);

        var ticks = TickPlacement != TickPlacement.None ? "<div class=\"nc-slider__ticks\"></div>" : "";
        var inputs = Name is null ? "" : Range
            ? $"<input type=\"hidden\" name=\"{Html.Attr(Name)}.LowerValue\"><input type=\"hidden\" name=\"{Html.Attr(Name)}.UpperValue\">"
            : $"<input type=\"hidden\" name=\"{Html.Attr(Name)}\">";
        output.Content.SetHtmlContent("<div class=\"nc-slider__track\"></div><div class=\"nc-slider__fill\"></div>" + ticks + "<div class=\"nc-slider__thumb\"></div>" + inputs);
    }
}

/// <summary>
/// <c>&lt;nc-range-slider minimum="0" maximum="10000" lower-value="2500" upper-value="7000" small-change="100" /&gt;</c>:
/// two thumbs; nc-change gives { lowerValue, upperValue }.
/// </summary>
[HtmlTargetElement("nc-range-slider", TagStructure = TagStructure.WithoutEndTag)]
public class RangeSliderTagHelper : SliderTagHelper
{
    public double LowerValue { get; set; }

    public double UpperValue { get => Value; set => Value = value; }

    protected override bool Range => true;

    protected override double RangeLower => LowerValue;
}

/// <summary><c>&lt;nc-rating value="4" /&gt;</c>: stars to click, or is-read-only="true" to only show them.</summary>
[HtmlTargetElement("nc-rating", TagStructure = TagStructure.WithoutEndTag)]
public class RatingTagHelper : NocturneTagHelper
{
    public int Value { get; set; }

    public int Maximum { get; set; } = 5;

    public bool IsReadOnly { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-rating");
        output.Attributes.SetAttribute("style", $"--value: {Value}; --max: {Maximum};" + (output.Attributes["style"]?.Value ?? ""));
        if (IsReadOnly)
        {
            output.Attributes.SetAttribute("role", "img");
            output.Attributes.SetAttribute("aria-label", $"{Value} of {Maximum}");
        }
        else
        {
            output.Attributes.SetAttribute("role", "slider");
            output.Attributes.SetAttribute("aria-valuemin", "0");
            output.Attributes.SetAttribute("aria-valuemax", Maximum.ToString(System.Globalization.CultureInfo.InvariantCulture));
            output.Attributes.SetAttribute("aria-valuenow", Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            output.Attributes.SetAttribute("aria-label", "Rating");
            output.Attributes.SetAttribute("data-nc-input", "");
        }
        ApplyCommon(output);
    }
}
