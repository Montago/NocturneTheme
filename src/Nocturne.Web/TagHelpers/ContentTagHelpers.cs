using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>Severity of an <see cref="InfoBarTagHelper"/>, as InfoBarSeverity in Nocturne.Wpf.</summary>
public enum InfoBarSeverity
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// <c>&lt;nc-info-bar severity="Warning" title="Almost full." message="The shared drive has 2.1 GB free."&gt;actions&lt;/nc-info-bar&gt;</c>:
/// a rounded strip tinted by its severity, with the matching icon, a bold title before the message,
/// the content as actions on the right and a close button.
/// </summary>
[HtmlTargetElement("nc-info-bar")]
public class InfoBarTagHelper : TagHelper
{
    public InfoBarSeverity Severity { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    /// <summary>Shows the close button (hides the bar in the browser).</summary>
    public bool IsClosable { get; set; } = true;

    /// <summary>False renders the bar hidden, like IsOpen="False".</summary>
    public bool IsOpen { get; set; } = true;

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var actions = (await output.GetChildContentAsync()).GetContent().Trim();
        var severity = Severity.ToString().ToLowerInvariant();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-info-bar", Html.Encoder);
        output.AddClass("nc-info-bar--" + severity, Html.Encoder);
        output.Attributes.SetAttribute("role", Severity == InfoBarSeverity.Error ? "alert" : "status");
        if (!IsOpen)
        {
            output.Attributes.SetAttribute("hidden", "hidden");
        }

        var icon = Severity switch
        {
            InfoBarSeverity.Success => "CheckCircle",
            InfoBarSeverity.Warning => "Warning",
            InfoBarSeverity.Error => "Error",
            _ => "Info",
        };
        output.Content.SetHtmlContent(Html.StrokeIcon(ViewContext, icon, "nc-info-bar__icon"));
        output.Content.AppendHtml("<span class=\"nc-info-bar__text\">");
        if (!string.IsNullOrEmpty(Title))
        {
            output.Content.AppendHtml("<span class=\"nc-info-bar__title\">").Append(Title).AppendHtml("</span> ");
        }
        output.Content.AppendHtml("<span class=\"nc-info-bar__message\">").Append(Message ?? "").AppendHtml("</span>");
        output.Content.AppendHtml("</span>");
        if (actions.Length > 0)
        {
            output.Content.AppendHtml("<span class=\"nc-info-bar__actions\">").AppendHtml(actions).AppendHtml("</span>");
        }
        if (IsClosable)
        {
            output.Content.AppendHtml("<button type=\"button\" class=\"nc-button nc-button--icon nc-button--subtle nc-info-bar__close\" data-nc-dismiss aria-label=\"Close\" title=\"Close\">")
                .AppendHtml(Html.StrokeIcon(ViewContext, "Close"))
                .AppendHtml("</button>");
        }
    }
}

/// <summary>Look of a <see cref="TileTagHelper"/>: the Nocturne.Styles.Tile.* styles.</summary>
public enum TileStyle
{
    /// <summary>The glass of a card.</summary>
    Default,

    /// <summary>Filled with the accent, as the primary button.</summary>
    Accent,
}

/// <summary>
/// <c>&lt;nc-tile title="Active employees" count="8" icon="AccountMultiple" href="/employees" /&gt;</c>:
/// a large glass button with an icon (or the content) at the top left, a count at the top right and
/// a title at the bottom. Without href it renders a &lt;button&gt;.
/// </summary>
[HtmlTargetElement("nc-tile", TagStructure = TagStructure.NormalOrSelfClosing)]
public class TileTagHelper : TagHelper
{
    public string? Title { get; set; }

    public string? Count { get; set; }

    public TileStyle TileStyle { get; set; }

    public string? Href { get; set; }

    /// <summary>A filled Material Design icon at the top left (instead of content).</summary>
    public NocturneIconKind Icon { get; set; }

    /// <summary>One of the theme's line icons at the top left ("Database").</summary>
    public string? StrokeIcon { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var extra = (await output.GetChildContentAsync()).GetContent();
        output.TagMode = TagMode.StartTagAndEndTag;
        if (Href is null)
        {
            output.TagName = "button";
            output.Attributes.SetAttribute("type", "button");
        }
        else
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", Href);
        }
        output.AddClass("nc-tile", Html.Encoder);
        if (TileStyle != TileStyle.Default)
        {
            output.AddClass("nc-tile--" + TileStyle.ToString().ToLowerInvariant(), Html.Encoder);
        }
        output.Content.SetHtmlContent("<span class=\"nc-tile__content\">");
        output.Content.AppendHtml(Html.Icon(ViewContext, Icon) + Html.StrokeIcon(ViewContext, StrokeIcon)).AppendHtml(extra).AppendHtml("</span>");
        if (!string.IsNullOrEmpty(Count))
        {
            output.Content.AppendHtml("<span class=\"nc-tile__count\">").Append(Count).AppendHtml("</span>");
        }
        output.Content.AppendHtml("<span class=\"nc-tile__title\">").Append(Title ?? "").AppendHtml("</span>");
    }
}

/// <summary><c>&lt;nc-group-box header="Synchronisation"&gt;content&lt;/nc-group-box&gt;</c>: a sunken frame with the header in a gap in its top edge.</summary>
[HtmlTargetElement("nc-group-box")]
public class GroupBoxTagHelper : TagHelper
{
    public string? Header { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-group-box", Html.Encoder);
        output.Content.SetHtmlContent("<legend class=\"nc-group-box__header\">");
        output.Content.Append(Header ?? "").AppendHtml("</legend>");
        output.Content.AppendHtml(content);
    }
}

/// <summary>
/// <c>&lt;nc-card header="BUTTON" tag="Button · RepeatButton"&gt;…&lt;/nc-card&gt;</c>: the glass card
/// (Nocturne.Styles.Card) with a section label and a tag chip on the right; without a header a
/// plain card (Nocturne.Styles.Border.Card).
/// </summary>
[HtmlTargetElement("nc-card")]
public class CardTagHelper : TagHelper
{
    public string? Header { get; set; }

    public string? Tag { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "section";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-card", Html.Encoder);
        output.Content.Clear();
        if (Header is not null || Tag is not null)
        {
            output.Content.AppendHtml("<header class=\"nc-card__header\"><h3 class=\"nc-card__title\">").Append(Header ?? "").AppendHtml("</h3>");
            if (Tag is not null)
            {
                output.Content.AppendHtml("<span class=\"nc-tag\">").Append(Tag).AppendHtml("</span>");
            }
            output.Content.AppendHtml("</header>");
        }
        output.Content.AppendHtml(content);
    }
}

/// <summary><c>&lt;nc-tag&gt;TextBox&lt;/nc-tag&gt;</c>: the small monospaced chip (Nocturne.Styles.Tag).</summary>
[HtmlTargetElement("nc-tag")]
public class TagChipTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-tag", Html.Encoder);
    }
}

/// <summary>The Nocturne.Styles.Pill.* styles.</summary>
public enum PillStyle
{
    Success,
    Warning,
    Error,
}

/// <summary><c>&lt;nc-pill pill-style="Warning"&gt;Pending&lt;/nc-pill&gt;</c>: a status pill.</summary>
[HtmlTargetElement("nc-pill")]
public class PillTagHelper : TagHelper
{
    public PillStyle PillStyle { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-pill", Html.Encoder);
        output.AddClass("nc-pill--" + PillStyle.ToString().ToLowerInvariant(), Html.Encoder);
    }
}

/// <summary><c>&lt;nc-badge&gt;2&lt;/nc-badge&gt;</c>: a small count (Nocturne.Styles.Badge), as on a document tab.</summary>
[HtmlTargetElement("nc-badge")]
public class BadgeTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-badge", Html.Encoder);
    }
}

/// <summary>
/// <c>&lt;nc-icon-tile stroke-icon="Shield" /&gt;</c>: a rounded square around an icon
/// (Nocturne.Styles.IconTile; small="true" for .Small). In a selected list item it fills with the accent.
/// </summary>
[HtmlTargetElement("nc-icon-tile", TagStructure = TagStructure.NormalOrSelfClosing)]
public class IconTileTagHelper : TagHelper
{
    public NocturneIconKind Icon { get; set; }

    public string? StrokeIcon { get; set; }

    public bool Small { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-icon-tile", Html.Encoder);
        if (Small) output.AddClass("nc-icon-tile--small", Html.Encoder);
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Content.SetHtmlContent(Html.Icon(ViewContext, Icon) + Html.StrokeIcon(ViewContext, StrokeIcon));
        output.Content.AppendHtml(content);
    }
}

/// <summary>
/// <c>&lt;nc-stroke-icon data="Search" /&gt;</c>: one of the theme's line icons (Nocturne.Icons.*, drawn
/// with round ends on a 24 × 24 grid), 20 × 20 in the text colour; stroke-thickness as in WPF (1.6).
/// </summary>
[HtmlTargetElement("nc-stroke-icon", TagStructure = TagStructure.WithoutEndTag)]
public class StrokeIconTagHelper : TagHelper
{
    /// <summary>The icon: the part of its key after Nocturne.Icons. ("ChevronDown").</summary>
    public string Data { get; set; } = "";

    public double? StrokeThickness { get; set; }

    /// <summary>Text for screen readers when the icon stands alone.</summary>
    public string? Label { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-stroke-icon", Html.Encoder);
        if (Label is null)
        {
            output.Attributes.SetAttribute("aria-hidden", "true");
        }
        else
        {
            output.Attributes.SetAttribute("role", "img");
            output.Attributes.SetAttribute("aria-label", Label);
        }
        output.Attributes.SetAttribute("focusable", "false");
        if (StrokeThickness is { } thickness)
        {
            var style = output.Attributes["style"]?.Value?.ToString();
            output.Attributes.SetAttribute("style", $"stroke-width:{Html.Num(thickness)};{style}");
        }
        var use = new TagBuilder("use");
        use.MergeAttribute("href", Html.StrokeHref(ViewContext, Data));
        output.Content.SetHtmlContent(use);
    }
}
