using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>
/// <c>&lt;nc-numeric-up-down asp-for="HoursPerWeek" minimum="0" maximum="40" interval="0.5" /&gt;</c>:
/// a number box with the Nocturne spin buttons. Name, id and validation attributes come from the
/// model like an ordinary input tag helper; without asp-for use value and name. The value is written
/// with a period, as type=number needs.
/// </summary>
[HtmlTargetElement("nc-numeric-up-down", TagStructure = TagStructure.WithoutEndTag)]
public class NumericUpDownTagHelper(IHtmlGenerator generator) : NocturneTagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    public double? Value { get; set; }

    public string? Name { get; set; }

    public double? Minimum { get; set; }

    public double? Maximum { get; set; }

    public double Interval { get; set; } = 1;

    /// <summary>A .NET format for the value, e.g. N1.</summary>
    public string? StringFormat { get; set; }

    /// <summary>ControlHelper.Placeholder: the hint shown while the box is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Hides the step buttons (HideUpDownButtons).</summary>
    public bool HideUpDownButtons { get; set; }

    public bool IsReadOnly { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var attributes = new Dictionary<string, object?>
        {
            ["type"] = "number",
            ["class"] = "nc-text-box",
            ["step"] = Html.Num(Interval),
        };
        if (Minimum is { } min) attributes["min"] = Html.Num(min);
        if (Maximum is { } max) attributes["max"] = Html.Num(max);
        if (Placeholder is not null) attributes["placeholder"] = Placeholder;
        if (IsReadOnly) attributes["readonly"] = "readonly";
        if (!IsEnabled) attributes["disabled"] = "disabled";
        if (ValidationError is not null)
        {
            attributes["aria-invalid"] = "true";
            attributes["data-nc-error"] = ValidationError;
        }

        var number = For is not null ? For.Model : Value;
        var value = number is null ? "" : Convert.ToString(number, CultureInfo.InvariantCulture);
        if (number is IFormattable f && StringFormat is not null)
        {
            // N1 → 37.5; type=number takes no group separators, so the invariant culture
            value = f.ToString(StringFormat, CultureInfo.InvariantCulture).Replace(",", "");
        }
        TagBuilder input;
        if (For is not null)
        {
            // Adds the validation attributes, and input-validation-error after a failed post
            input = generator.GenerateTextBox(ViewContext, For.ModelExplorer, For.Name, value, null, attributes);
        }
        else
        {
            input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            input.MergeAttributes(attributes);
            if (Name is not null) input.MergeAttribute("name", Name);
            input.MergeAttribute("value", value);
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-numeric-up-down");
        if (ToolTip is not null) output.Attributes.SetAttribute("data-nc-tool-tip", ToolTip);
        output.Content.SetHtmlContent(input);
        if (!HideUpDownButtons && !IsReadOnly)
        {
            var disabled = IsEnabled ? "" : " disabled";
            output.Content.AppendHtml(
                "<span class=\"nc-numeric-up-down__spin\">" +
                $"<button type=\"button\" data-nc-step=\"-1\" tabindex=\"-1\" aria-label=\"Decrease\"{disabled}>{Html.StrokeIcon(ViewContext, "ChevronDown")}</button>" +
                $"<button type=\"button\" data-nc-step=\"1\" tabindex=\"-1\" aria-label=\"Increase\"{disabled}>{Html.StrokeIcon(ViewContext, "ChevronUp")}</button>" +
                "</span>");
        }
    }
}

/// <summary>
/// <c>&lt;nc-toggle-switch asp-for="IsActive"&gt;Active&lt;/nc-toggle-switch&gt;</c>: an on/off switch for
/// a bool property, posted like a check box. Without asp-for, give it a name (and is-checked).
/// </summary>
[HtmlTargetElement("nc-toggle-switch")]
public class ToggleSwitchTagHelper(IHtmlGenerator generator) : NocturneTagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    public string? Name { get; set; }

    public string? Content { get; set; }

    public bool IsChecked { get; set; }

    /// <summary>Old name of IsChecked.</summary>
    public bool Checked { get => IsChecked; set => IsChecked = value; }

    public string? Id { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var text = Content is null ? (await output.GetChildContentAsync()).GetContent() : Html.Text(Content);
        var attributes = new Dictionary<string, object?> { ["role"] = "switch" };
        if (!IsEnabled) attributes["disabled"] = "disabled";
        if (Id is not null) attributes["id"] = Id;

        TagBuilder input;
        if (For is not null)
        {
            input = generator.GenerateCheckBox(ViewContext, For.ModelExplorer, For.Name, null, attributes);
        }
        else
        {
            input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            input.MergeAttributes(attributes);
            input.MergeAttribute("type", "checkbox");
            if (Name is not null)
            {
                input.MergeAttribute("name", Name);
                input.MergeAttribute("value", "true");
            }
            if (IsChecked) input.MergeAttribute("checked", "checked");
        }

        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-toggle-switch");
        ApplyCommon(output);
        output.Content.SetHtmlContent(input);
        output.Content.AppendHtml(text);
    }
}

/// <summary>
/// <c>&lt;nc-check-box content="Checked" is-checked="true" /&gt;</c>. IsChecked="null" with IsThreeState shows
/// the indeterminate dash. With asp-for it binds a bool like the input tag helper.
/// </summary>
[HtmlTargetElement("nc-check-box")]
public class CheckBoxTagHelper(IHtmlGenerator generator) : NocturneTagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    public string? Content { get; set; }

    public bool? IsChecked { get; set; } = false;

    public string? Name { get; set; }

    public string? Value { get; set; }

    public string? Id { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    protected virtual string Kind => "checkbox";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var text = Content is null ? (await output.GetChildContentAsync()).GetContent() : Html.Text(Content);
        var attributes = new Dictionary<string, object?>();
        if (!IsEnabled) attributes["disabled"] = "disabled";
        if (Id is not null) attributes["id"] = Id;
        if (IsChecked is null) attributes["data-nc-indeterminate"] = "";

        TagBuilder input;
        if (For is not null && Kind == "checkbox")
        {
            input = generator.GenerateCheckBox(ViewContext, For.ModelExplorer, For.Name, null, attributes);
        }
        else
        {
            input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            input.MergeAttributes(attributes);
            input.MergeAttribute("type", Kind);
            var name = For?.Name ?? Name;
            if (name is not null) input.MergeAttribute("name", name);
            input.MergeAttribute("value", Value ?? (Kind == "checkbox" ? "true" : Content ?? "on"));
            if (IsChecked == true) input.MergeAttribute("checked", "checked");
        }

        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, Kind == "checkbox" ? "nc-check-box" : "nc-radio-button");
        ApplyCommon(output);
        output.Content.SetHtmlContent(input);
        output.Content.AppendHtml(text);
    }
}

/// <summary><c>&lt;nc-radio-button content="Green" group-name="accent" is-checked="true" /&gt;</c></summary>
[HtmlTargetElement("nc-radio-button")]
public class RadioButtonTagHelper(IHtmlGenerator generator) : CheckBoxTagHelper(generator)
{
    /// <summary>GroupName: the radio buttons with the same name exclude each other.</summary>
    public string? GroupName { get => Name; set => Name = value; }

    protected override string Kind => "radio";
}
