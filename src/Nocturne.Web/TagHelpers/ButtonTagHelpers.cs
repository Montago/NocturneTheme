using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>The Nocturne.Styles.Button.* styles.</summary>
public enum ButtonStyle
{
    /// <summary>The light glass fill (the secondary button).</summary>
    Default,

    /// <summary>The accent fill, for the main action.</summary>
    Primary,

    /// <summary>Deep red whatever the accent, for what cannot be undone.</summary>
    Danger,

    /// <summary>No fill until hovered.</summary>
    Subtle,

    /// <summary>36 × 36, for an icon alone.</summary>
    Icon,

    /// <summary>36 × 36 without a fill until hovered.</summary>
    IconSubtle,
}

internal static class ButtonStyles
{
    public static string? Class(ButtonStyle style) => style switch
    {
        ButtonStyle.Default => null,
        ButtonStyle.IconSubtle => "nc-button--icon nc-button--subtle",
        _ => "nc-button--" + style.ToString().ToLowerInvariant(),
    };
}

/// <summary>
/// <c>&lt;nc-button button-style="Primary" icon="Plus"&gt;New&lt;/nc-button&gt;</c>: a button; with href a link that
/// looks like one. Content or the element's own content is the text; icon (a NocturneIcon kind) or
/// stroke-icon (a line icon of the theme, "Refresh") goes before it.
/// </summary>
[HtmlTargetElement("nc-button")]
public class ButtonTagHelper : NocturneTagHelper
{
    public ButtonStyle ButtonStyle { get; set; }

    public string? Content { get; set; }

    public string? Href { get; set; }

    /// <summary>A filled Material Design icon before the content.</summary>
    public NocturneIconKind Icon { get; set; }

    /// <summary>One of the theme's line icons before the content (Nocturne.Icons.Refresh → "Refresh").</summary>
    public string? StrokeIcon { get; set; }

    /// <summary>button, submit or reset.</summary>
    public string Type { get; set; } = "button";

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    protected virtual void Extra(TagHelperOutput output)
    {
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var inner = (await output.GetChildContentAsync()).GetContent();
        output.TagMode = TagMode.StartTagAndEndTag;
        if (Href is not null && IsEnabled)
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", Href);
        }
        else
        {
            output.TagName = "button";
            output.Attributes.SetAttribute("type", Type);
            if (!IsEnabled)
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }
        }
        AddClasses(output, "nc-button", ButtonStyles.Class(ButtonStyle));
        ApplyCommon(output);
        Extra(output);
        output.Content.SetHtmlContent(Html.Icon(ViewContext, Icon) + Html.StrokeIcon(ViewContext, StrokeIcon));
        if (Content is null) output.Content.AppendHtml(inner); else output.Content.Append(Content);
    }
}

/// <summary><c>&lt;nc-toggle-button is-checked="true"&gt;Bold&lt;/nc-toggle-button&gt;</c>: stays pressed (aria-pressed), primary while checked.</summary>
[HtmlTargetElement("nc-toggle-button")]
public class ToggleButtonTagHelper : ButtonTagHelper
{
    public bool IsChecked { get; set; }

    protected override void Extra(TagHelperOutput output)
    {
        output.Attributes.SetAttribute("aria-pressed", IsChecked ? "true" : "false");
        output.Attributes.SetAttribute("data-nc-toggle", "");
    }
}

/// <summary><c>&lt;nc-repeat-button&gt;</c>: clicks when pressed and again every 33 ms while held, after 0.4 s.</summary>
[HtmlTargetElement("nc-repeat-button")]
public class RepeatButtonTagHelper : ButtonTagHelper
{
    protected override void Extra(TagHelperOutput output) => output.Attributes.SetAttribute("data-nc-repeat", "");
}

/// <summary><c>&lt;nc-hyperlink href="…"&gt;Open timesheet&lt;/nc-hyperlink&gt;</c>: the link in the light accent.</summary>
[HtmlTargetElement("nc-hyperlink")]
public class HyperlinkTagHelper : NocturneTagHelper
{
    public string? Href { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-hyperlink");
        if (IsEnabled)
        {
            output.Attributes.SetAttribute("href", Href ?? "#");
        }
        else
        {
            output.Attributes.SetAttribute("aria-disabled", "true");
        }
        ApplyCommon(output);
    }
}

/// <summary>A MenuItem of a SplitButton or DropDownButton menu, collected by the parent.</summary>
internal sealed record MenuPart(string Html);

/// <summary>
/// <c>&lt;nc-split-button content="Synchronise" button-style="Primary"&gt;&lt;nc-menu-item header="Upload only" /&gt;…&lt;/nc-split-button&gt;</c>:
/// the main action plus a menu behind the chevron. The children are its menu items.
/// </summary>
[HtmlTargetElement("nc-split-button")]
public class SplitButtonTagHelper : NocturneTagHelper
{
    public string? Content { get; set; }

    /// <summary>Default or Primary (Nocturne.Styles.SplitButton.Primary, .DropDownButton.Primary).</summary>
    public ButtonStyle ButtonStyle { get; set; }

    /// <summary>A filled Material Design icon before the content.</summary>
    public NocturneIconKind Icon { get; set; }

    /// <summary>One of the theme's line icons before the content ("Plus").</summary>
    public string? StrokeIcon { get; set; }

    public string? Id { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    protected virtual bool Split => true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        context.Items[typeof(MenuScope)] = new MenuScope();
        var menu = (await output.GetChildContentAsync()).GetContent();
        var menuId = (Id ?? Html.NewId("nc-menu")) + "-menu";
        var style = ButtonStyle == ButtonStyle.Primary ? " nc-button--primary" : "";
        var disabled = IsEnabled ? "" : " disabled";
        var icon = Html.Icon(ViewContext, Icon) + Html.StrokeIcon(ViewContext, StrokeIcon);
        var chevron = "<i class=\"nc-caret\" aria-hidden=\"true\"></i>";
        var popup = $"<div class=\"nc-context-menu\" popover id=\"{menuId}\" role=\"menu\">{menu}</div>";
        output.TagMode = TagMode.StartTagAndEndTag;
        if (Split)
        {
            output.TagName = "div";
            AddClasses(output, "nc-split-button", ButtonStyle == ButtonStyle.Primary ? "nc-split-button--primary" : null);
            output.Content.SetHtmlContent(
                $"<button type=\"button\" class=\"nc-split-button__main\"{disabled}>{icon}{Html.Text(Content)}</button>" +
                $"<button type=\"button\" class=\"nc-split-button__arrow\" popovertarget=\"{menuId}\" aria-haspopup=\"menu\" aria-label=\"More options\" tabindex=\"-1\"{disabled}>{chevron}</button>" + popup);
            if (!IsEnabled) output.Attributes.SetAttribute("aria-disabled", "true");
        }
        else
        {
            output.TagName = "span";
            AddClasses(output, "nc-drop-down-button");
            output.Content.SetHtmlContent(
                $"<button type=\"button\" class=\"nc-button{style}\" popovertarget=\"{menuId}\" aria-haspopup=\"menu\"{disabled}>{icon}{Html.Text(Content)}{chevron}</button>" + popup);
        }
        if (Id is not null)
        {
            output.Attributes.SetAttribute("id", Id);
        }
        ApplyCommon(output);
    }
}

/// <summary><c>&lt;nc-drop-down-button content="Export"&gt;&lt;nc-menu-item header="PDF" /&gt;&lt;/nc-drop-down-button&gt;</c>: a button that opens a menu.</summary>
[HtmlTargetElement("nc-drop-down-button")]
public class DropDownButtonTagHelper : SplitButtonTagHelper
{
    protected override bool Split => false;
}
