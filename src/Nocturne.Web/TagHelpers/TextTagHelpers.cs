using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>The Nocturne.Styles.TextBlock.* styles.</summary>
public enum TextBlockStyle
{
    Default,
    Title,
    Subtitle,
    Eyebrow,
    ListTitle,
    Body,
    Caption,
    Label,
    Hint,
    Error,
    Mono,
}

/// <summary><c>&lt;nc-text-block text-block-style="Title" text="Companies" /&gt;</c>: Title becomes h1, Subtitle h2, the rest p.</summary>
[HtmlTargetElement("nc-text-block")]
public class TextBlockTagHelper : NocturneTagHelper
{
    public TextBlockStyle TextBlockStyle { get; set; }

    public string? Text { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var inner = (await output.GetChildContentAsync()).GetContent();
        output.TagName = TextBlockStyle switch { TextBlockStyle.Title => "h1", TextBlockStyle.Subtitle => "h2", _ => "p" };
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, TextBlockStyle == TextBlockStyle.Default ? null : "nc-text-block--" + Html.Kebab(TextBlockStyle.ToString()));
        if (output.TagName == "p")
        {
            output.Attributes.SetAttribute("style", "margin:0;" + (output.Attributes["style"]?.Value ?? ""));
        }
        ApplyCommon(output);
        output.Content.SetHtmlContent(Text is null ? inner : Html.Text(Text));
    }
}

/// <summary><c>&lt;nc-label content="Name" target="NameBox" /&gt;</c>: Label with Target (the for attribute).</summary>
[HtmlTargetElement("nc-label")]
public class LabelTagHelper : NocturneTagHelper
{
    public string? Content { get; set; }

    public string? Target { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var inner = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-label");
        if (Target is not null)
        {
            output.Attributes.SetAttribute("for", Target);
        }
        output.Content.SetHtmlContent(Content is null ? inner : Html.Text(Content));
    }
}

/// <summary>
/// <c>&lt;nc-separator /&gt;</c>: the etched line; inside a menu the menu separator, inside a tool bar
/// the vertical one.
/// </summary>
[HtmlTargetElement("nc-separator", TagStructure = TagStructure.WithoutEndTag)]
public class SeparatorTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!context.Items.ContainsKey(typeof(MenuScope)) && context.Items.ContainsKey(typeof(ToolBarTagHelper)))
        {
            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.AddClass("nc-tool-bar__separator", Html.Encoder);
            output.Attributes.SetAttribute("role", "separator");
            return;
        }
        output.TagName = "hr";
        output.TagMode = TagMode.SelfClosing;
        output.AddClass(context.Items.ContainsKey(typeof(MenuScope)) ? "nc-menu-separator" : "nc-separator", Html.Encoder);
    }
}

/// <summary>The Nocturne.Styles.TextBox.* styles.</summary>
public enum TextBoxStyle
{
    Default,
    /// <summary>The magnifier on the left (ControlHelper.Icon = Nocturne.Icons.Search).</summary>
    Search,
    MultiLine,
}

/// <summary>
/// <c>&lt;nc-text-box asp-for="Name" placeholder="Search…" clear-button="true" text-box-style="Search" /&gt;</c>:
/// TextBox with ControlHelper.Placeholder and ControlHelper.ClearButton. Without asp-for use text and name.
/// </summary>
[HtmlTargetElement("nc-text-box", TagStructure = TagStructure.WithoutEndTag)]
public class TextBoxTagHelper(IHtmlGenerator generator) : NocturneTagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    public string? Text { get; set; }

    public string? Name { get; set; }

    public string? Id { get; set; }

    /// <summary>ControlHelper.Placeholder: the hint shown while the box is empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>ControlHelper.ClearButton: an × that empties the box, shown while it has text.</summary>
    public bool ClearButton { get; set; }

    public bool IsReadOnly { get; set; }

    public TextBoxStyle TextBoxStyle { get; set; }

    /// <summary>The input type: text, email, search, tel …</summary>
    public string Type { get; set; } = "text";

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    protected virtual string InputType => Type;

    protected virtual string? Value => Text;

    /// <summary>The buttons after the box (the eye of a PasswordBox).</summary>
    protected virtual string ExtraButtons => "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var multiLine = TextBoxStyle == TextBoxStyle.MultiLine;
        var search = TextBoxStyle == TextBoxStyle.Search;
        var attributes = new Dictionary<string, object?>
        {
            ["class"] = "nc-text-box",
        };
        // a blank placeholder still lets CSS see that the box is empty (:placeholder-shown hides the clear button)
        var placeholder = Placeholder ?? (ClearButton ? " " : null);
        if (placeholder is not null) attributes["placeholder"] = placeholder;
        if (IsReadOnly) attributes["readonly"] = "readonly";
        if (!IsEnabled) attributes["disabled"] = "disabled";
        if (Id is not null) attributes["id"] = Id;
        if (ToolTip is not null) attributes["data-nc-tool-tip"] = ToolTip;
        if (ValidationError is not null)
        {
            attributes["aria-invalid"] = "true";
            attributes["data-nc-error"] = ValidationError;
        }
        // The page's own attributes go on the box; its style goes on the host when there is one (the
        // icon and the buttons inside the field are placed in the host, so it must be the field's size)
        var extra = ExtraButtons;
        var hosted = ClearButton || search || extra.Length > 0;
        string? hostStyle = null;
        foreach (var a in output.Attributes.ToList())
        {
            if (a.Name == "class") attributes["class"] += " " + a.Value;
            else if (a.Name == "style" && hosted && !multiLine) hostStyle = a.Value?.ToString();
            else attributes[a.Name] = a.Value?.ToString();
        }
        output.Attributes.Clear();

        TagBuilder box;
        if (For is not null)
        {
            box = multiLine
                ? generator.GenerateTextArea(ViewContext, For.ModelExplorer, For.Name, 0, 0, attributes)
                : generator.GenerateTextBox(ViewContext, For.ModelExplorer, For.Name, For.Model, null, Merge(attributes, "type", InputType));
        }
        else if (multiLine)
        {
            box = new TagBuilder("textarea");
            box.MergeAttributes(attributes);
            if (Name is not null) box.MergeAttribute("name", Name);
            box.InnerHtml.Append(Value ?? "");
        }
        else
        {
            box = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            box.MergeAttributes(attributes);
            box.MergeAttribute("type", InputType);
            if (Name is not null) box.MergeAttribute("name", Name);
            if (Value is not null) box.MergeAttribute("value", Value);
        }

        if (hosted)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "nc-text-box-host" + (search ? " nc-text-box-host--search" : ""));
            if (hostStyle is not null) output.Attributes.SetAttribute("style", hostStyle);
            output.Content.SetHtmlContent(box);
            if (ClearButton && !IsReadOnly)
            {
                output.Content.AppendHtml("<button type=\"button\" class=\"nc-text-box-host__button nc-text-box-host__clear\" data-nc-clear aria-label=\"Clear\" title=\"Clear\" tabindex=\"-1\"></button>");
            }
            output.Content.AppendHtml(extra);
        }
        else
        {
            output.TagName = null;
            output.Content.SetHtmlContent(box);
        }
    }

    private static Dictionary<string, object?> Merge(Dictionary<string, object?> a, string key, string value) => new(a) { [key] = value };
}

/// <summary><c>&lt;nc-password-box password="…" placeholder="Password" can-reveal="true" /&gt;</c></summary>
[HtmlTargetElement("nc-password-box", TagStructure = TagStructure.WithoutEndTag)]
public class PasswordBoxTagHelper(IHtmlGenerator generator) : TextBoxTagHelper(generator)
{
    public string? Password { get; set; }

    /// <summary>PasswordBoxHelper.CanReveal: an eye button that shows the password while it is pressed in.</summary>
    public bool CanReveal { get; set; }

    protected override string InputType => "password";

    protected override string? Value => Password;

    protected override string ExtraButtons => CanReveal
        ? "<button type=\"button\" class=\"nc-text-box-host__button nc-text-box-host__reveal\" data-nc-reveal aria-pressed=\"false\" aria-label=\"Show password\" title=\"Show password\" tabindex=\"-1\"></button>"
        : "";
}

/// <summary>An item of a ComboBox: <c>&lt;nc-combo-box-item content="Seattle" value="sea" /&gt;</c>.</summary>
[HtmlTargetElement("nc-combo-box-item", ParentTag = "nc-combo-box", TagStructure = TagStructure.WithoutEndTag)]
public class ComboBoxItemTagHelper : TagHelper
{
    public string Content { get; set; } = "";

    public string? Value { get; set; }

    public bool IsEnabled { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ChildList<ComboBoxItemTagHelper>.Of(context)?.Items.Add(this);
        output.SuppressOutput();
    }
}

/// <summary>
/// <c>&lt;nc-combo-box selected-index="1"&gt;&lt;nc-combo-box-item content="Seattle" /&gt;…&lt;/nc-combo-box&gt;</c>, or with
/// asp-for and asp-items. IsEditable makes it a text box with the list as suggestions; Watermark shows
/// while nothing is chosen.
/// </summary>
[HtmlTargetElement("nc-combo-box")]
public class ComboBoxTagHelper(IHtmlGenerator generator) : NocturneTagHelper
{
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("asp-items")]
    public IEnumerable<SelectListItem>? Items { get; set; }

    public int SelectedIndex { get; set; } = -1;

    public bool IsEditable { get; set; }

    /// <summary>The text of an editable combo box.</summary>
    public string? Text { get; set; }

    /// <summary>ControlHelper.Placeholder: shown while nothing is chosen.</summary>
    public string? Placeholder { get; set; }

    public string? Name { get; set; }

    public string? Id { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var children = ChildList<ComboBoxItemTagHelper>.Start(context);
        await output.GetChildContentAsync();
        var items = children.Items;
        var disabled = IsEnabled ? "" : " disabled";

        if (IsEditable)
        {
            var listId = Html.NewId("nc-combo");
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            AddClasses(output, "nc-combo-box", "nc-combo-box--editable");
            var name = Name is null ? "" : $" name=\"{Html.Attr(Name)}\"";
            var id = Id is null ? "" : $" id=\"{Html.Attr(Id)}\"";
            var watermark = Placeholder is null ? "" : $" placeholder=\"{Html.Attr(Placeholder)}\"";
            output.Content.SetHtmlContent(
                $"<input class=\"nc-text-box\" list=\"{listId}\" value=\"{Html.Attr(Text)}\"{name}{id}{watermark}{disabled}{CommonAttributes()}>" +
                $"<button type=\"button\" class=\"nc-text-box-button nc-text-box-button--drop-down\" tabindex=\"-1\" aria-label=\"Open\"{disabled}></button>" +
                $"<datalist id=\"{listId}\">" + string.Concat(items.Select(i => $"<option value=\"{Html.Attr(i.Content)}\"></option>")) + "</datalist>");
            return;
        }

        var attributes = new Dictionary<string, object?> { ["class"] = "nc-combo-box" };
        if (!IsEnabled) attributes["disabled"] = "disabled";
        if (Id is not null) attributes["id"] = Id;
        if (Placeholder is not null) attributes["data-nc-placeholder"] = Placeholder;
        if (ToolTip is not null) attributes["data-nc-tool-tip"] = ToolTip;
        if (ValidationError is not null)
        {
            attributes["aria-invalid"] = "true";
            attributes["data-nc-error"] = ValidationError;
        }
        foreach (var a in output.Attributes.ToList())
        {
            if (a.Name == "class") attributes["class"] += " " + a.Value;
            else attributes[a.Name] = a.Value?.ToString();
        }
        output.Attributes.Clear();

        TagBuilder select;
        if (For is not null)
        {
            select = generator.GenerateSelect(ViewContext, For.ModelExplorer, Placeholder, For.Name, Items ?? [], false, attributes);
        }
        else
        {
            select = new TagBuilder("select");
            select.MergeAttributes(attributes);
            if (Name is not null) select.MergeAttribute("name", Name);
            if (Placeholder is not null)
            {
                select.InnerHtml.AppendHtml($"<option value=\"\" disabled hidden{(SelectedIndex < 0 ? " selected" : "")}>{Html.Text(Placeholder)}</option>");
            }
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var value = item.Value is null ? "" : $" value=\"{Html.Attr(item.Value)}\"";
                var selected = i == SelectedIndex ? " selected" : "";
                var off = item.IsEnabled ? "" : " disabled";
                select.InnerHtml.AppendHtml($"<option{value}{selected}{off}>{Html.Text(item.Content)}</option>");
            }
        }
        output.TagName = null;
        output.Content.SetHtmlContent(select);
    }
}

/// <summary>
/// <c>&lt;nc-date-picker selected-date="@DateTime.Today" placeholder="Select a date" /&gt;</c>: the text box, the
/// calendar button and a Calendar drop-down. With name, a hidden input posts the date as yyyy-MM-dd.
/// The date is shown in the browser's culture unless culture (or data-nc-culture on a container) names one, as its
/// short date unless date-format (or data-nc-date-format on a container) gives a custom format.
/// </summary>
[HtmlTargetElement("nc-date-picker", TagStructure = TagStructure.WithoutEndTag)]
public class DatePickerTagHelper : NocturneTagHelper
{
    public DateTime? SelectedDate { get; set; }

    public string Placeholder { get; set; } = "Select a date";

    /// <summary>IsTodayHighlighted: the ring around today in the drop-down's calendar.</summary>
    public bool IsTodayHighlighted { get; set; } = true;

    public string? Name { get; set; }

    /// <summary>The culture of the text and the calendar ("da-DK"); the browser's when empty.</summary>
    public string? Culture { get; set; }

    /// <summary>The first day of the week in the calendar; the culture's when empty.</summary>
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// A .NET custom date format for the text ("dd-MM-yyyy", "d. MMMM yyyy"); the culture's short date when empty.
    /// Typed dates are read in its order of day, month and year, with month names where it has them.
    /// </summary>
    public string? DateFormat { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-date-picker", IsTodayHighlighted ? null : "nc-calendar--no-today");
        var date = SelectedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
        output.Attributes.SetAttribute("data-selected-date", date);
        DateCulture.Apply(output, Culture, FirstDayOfWeek);
        var format = string.IsNullOrWhiteSpace(DateFormat) ? null : DateFormat;
        if (format is not null)
        {
            output.Attributes.SetAttribute("data-nc-date-format", format);
        }
        // nocturne.js writes the text again in the browser's culture; this is what shows without script
        var text = SelectedDate is { } selected ? Html.Attr(FormatDate(selected, format, DateCulture.Of(Culture))) : "";
        var disabled = IsEnabled ? "" : " disabled";
        output.Content.SetHtmlContent(
            $"<input class=\"nc-text-box\" value=\"{text}\" placeholder=\"{Html.Attr(Placeholder)}\"{disabled}{CommonAttributes()}>" +
            $"<button type=\"button\" class=\"nc-text-box-button nc-text-box-button--calendar\" tabindex=\"-1\" aria-label=\"Show calendar\"{disabled}></button>" +
            (Name is null ? "" : $"<input type=\"hidden\" name=\"{Html.Attr(Name)}\" value=\"{date}\">"));
    }

    // A one-letter format is a custom format here, as in nocturne.js ("d" is the day, not the short date)
    private static string FormatDate(DateTime date, string? format, CultureInfo culture) =>
        format is null ? date.ToString("d", culture)
        : date.ToString(format.Length == 1 ? "%" + format : format, culture);
}

/// <summary><c>&lt;nc-time-picker selected-time="08:30" minute-step="30" placeholder="Meeting time" /&gt;</c></summary>
[HtmlTargetElement("nc-time-picker", TagStructure = TagStructure.WithoutEndTag)]
public class TimePickerTagHelper : NocturneTagHelper
{
    public TimeSpan? SelectedTime { get; set; }

    public int MinuteStep { get; set; } = 15;

    public string? Placeholder { get; set; }

    public string? Name { get; set; }


    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-time-picker");
        output.Attributes.SetAttribute("data-nc-minute-step", MinuteStep.ToString(CultureInfo.InvariantCulture));
        var text = SelectedTime?.ToString(@"hh\:mm", CultureInfo.InvariantCulture) ?? "";
        var watermark = Placeholder is null ? "" : $" placeholder=\"{Html.Attr(Placeholder)}\"";
        var name = Name is null ? "" : $" name=\"{Html.Attr(Name)}\"";
        var disabled = IsEnabled ? "" : " disabled";
        output.Content.SetHtmlContent(
            $"<input class=\"nc-text-box\" value=\"{text}\"{watermark}{name}{disabled}{CommonAttributes()}>" +
            $"<button type=\"button\" class=\"nc-text-box-button nc-text-box-button--clock\" tabindex=\"-1\" aria-label=\"Choose a time\"{disabled}></button>");
    }
}

/// <summary>
/// <c>&lt;nc-auto-suggest-box items="@names" placeholder="Search companies…" /&gt;</c>: a text box that suggests
/// the items containing what you type.
/// </summary>
[HtmlTargetElement("nc-auto-suggest-box", TagStructure = TagStructure.WithoutEndTag)]
public class AutoSuggestBoxTagHelper : NocturneTagHelper
{
    public IEnumerable<string> Items { get; set; } = [];

    public string? Text { get; set; }

    public string? Placeholder { get; set; }

    public string? Name { get; set; }


    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-auto-suggest-box");
        var watermark = Placeholder is null ? "" : $" placeholder=\"{Html.Attr(Placeholder)}\"";
        var name = Name is null ? "" : $" name=\"{Html.Attr(Name)}\"";
        var disabled = IsEnabled ? "" : " disabled";
        output.Content.SetHtmlContent(
            $"<input class=\"nc-text-box\" type=\"search\" autocomplete=\"off\" value=\"{Html.Attr(Text)}\"{watermark}{name}{disabled}{CommonAttributes()}>" +
            $"<script type=\"application/json\">{Json(Items)}</script>");
    }

    internal static string Json(object items) =>
        JsonSerializer.Serialize(items).Replace("</", "<\\/", StringComparison.Ordinal);
}

/// <summary>How a SearchComboBox matches what you type.</summary>
public enum SearchFilterMode
{
    Contains,
    StartsWith,
}

/// <summary>
/// <c>&lt;nc-search-combo-box items="@companies" display-member-path="Name" selected-value-path="Id" selected-value="1042" /&gt;</c>:
/// a combo box you type into to filter a long list. The items go to the page as JSON.
/// </summary>
[HtmlTargetElement("nc-search-combo-box", TagStructure = TagStructure.WithoutEndTag)]
public class SearchComboBoxTagHelper : NocturneTagHelper
{
    public System.Collections.IEnumerable Items { get; set; } = Array.Empty<object>();

    public string? DisplayMemberPath { get; set; }

    public string? SelectedValuePath { get; set; }

    public object? SelectedValue { get; set; }

    /// <summary>The selected item itself (for lists of strings).</summary>
    public object? SelectedItem { get; set; }

    public SearchFilterMode FilterMode { get; set; }

    public string? Placeholder { get; set; }

    public string? Name { get; set; }

    public string? Id { get; set; }


    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-search-combo-box");
        if (Id is not null) output.Attributes.SetAttribute("id", Id);
        if (DisplayMemberPath is not null) output.Attributes.SetAttribute("data-nc-display-member", DisplayMemberPath);
        if (SelectedValuePath is not null) output.Attributes.SetAttribute("data-nc-value-member", SelectedValuePath);
        if (FilterMode == SearchFilterMode.StartsWith) output.Attributes.SetAttribute("data-nc-filter-mode", "StartsWith");
        var selected = SelectedValue ?? SelectedItem;
        if (selected is not null) output.Attributes.SetAttribute("data-nc-selected-value", Convert.ToString(selected, CultureInfo.InvariantCulture));
        var watermark = Placeholder is null ? "" : $" placeholder=\"{Html.Attr(Placeholder)}\"";
        var disabled = IsEnabled ? "" : " disabled";
        output.Content.SetHtmlContent(
            $"<input class=\"nc-text-box\" autocomplete=\"off\" role=\"combobox\"{watermark}{disabled}{CommonAttributes()}>" +
            $"<button type=\"button\" class=\"nc-text-box-button nc-text-box-button--drop-down\" tabindex=\"-1\" aria-label=\"Open\"{disabled}></button>" +
            (Name is null ? "" : $"<input type=\"hidden\" name=\"{Html.Attr(Name)}\">") +
            $"<script type=\"application/json\">{AutoSuggestBoxTagHelper.Json(Items)}</script>");
    }
}

/// <summary><c>&lt;nc-color-picker selected-color="#1F78D1" /&gt;</c>: a swatch button that opens the theme palette.</summary>
[HtmlTargetElement("nc-color-picker", TagStructure = TagStructure.WithoutEndTag)]
public class ColorPickerTagHelper : NocturneTagHelper
{
    public string SelectedColor { get; set; } = "#1F78D1";

    public string? Id { get; set; }

    /// <summary>The colours to choose from (ColorPicker.Palette); the theme's 24 when empty.</summary>
    public IEnumerable<string>? Palette { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("type", "button");
        AddClasses(output, "nc-color-picker");
        if (Id is not null) output.Attributes.SetAttribute("id", Id);
        if (Palette is not null) output.Attributes.SetAttribute("data-nc-palette", string.Join(" ", Palette));
        output.Attributes.SetAttribute("data-selected-color", SelectedColor);
        output.Attributes.SetAttribute("aria-haspopup", "dialog");
        if (!IsEnabled) output.Attributes.SetAttribute("disabled", "disabled");
        ApplyCommon(output);
        output.Content.SetHtmlContent("<span class=\"nc-color-picker__swatch\"></span><span class=\"nc-color-picker__text\"></span><span class=\"nc-color-picker__chevron\" aria-hidden=\"true\"></span>");
    }
}
