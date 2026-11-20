using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>The TabControl styles: the segmented control (default) and Nocturne.Styles.TabControl.Document.</summary>
public enum TabControlStyle
{
    /// <summary>Tabs share a sunken strip; the selected one has a light pill and a short accent stripe that glide to a new tab.</summary>
    Default,

    /// <summary>Browser-like document tabs over a panel, with close buttons, badges and a "+" button.</summary>
    Document,
}

/// <summary>
/// <c>&lt;nc-tab-item header="General"&gt;page&lt;/nc-tab-item&gt;</c> inside a nc-tab-control; name gives the tab a
/// stable id (and the #name in the address for use-hash). Document tabs take show-close-button and badge (TabHelper).
/// </summary>
[HtmlTargetElement("nc-tab-item", ParentTag = "nc-tab-control")]
public class TabItemTagHelper : TagHelper
{
    public string Header { get; set; } = "";

    public string? Name { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsSelected { get; set; }

    /// <summary>TabHelper.ShowCloseButton: an × that closes the tab (document tabs).</summary>
    public bool ShowCloseButton { get; set; }

    /// <summary>TabHelper.Badge: a small count after the header (document tabs).</summary>
    public string? Badge { get; set; }

    internal string Html { get; private set; } = "";

    internal string? Class { get; private set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        Html = (await output.GetChildContentAsync()).GetContent();
        Class = output.Attributes["class"]?.Value?.ToString();
        ChildList<TabItemTagHelper>.Of(context)?.Items.Add(this);
        output.SuppressOutput();
    }
}

/// <summary><c>&lt;nc-tab-control-tag&gt;</c>: the TabControl's Tag, shown at the right of the tab strip.</summary>
[HtmlTargetElement("nc-tab-control-tag", ParentTag = "nc-tab-control")]
public class TabControlTagTagHelper : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context.Items.TryGetValue(typeof(TabControlTagHelper), out var parent))
        {
            ((TabControlTagHelper)parent).Tag = (await output.GetChildContentAsync()).GetContent();
        }
        output.SuppressOutput();
    }
}

/// <summary>
/// <c>&lt;nc-tab-control selected-index="0"&gt;&lt;nc-tab-item header="General"&gt;…&lt;/nc-tab-item&gt;&lt;/nc-tab-control&gt;</c>.
/// nocturne.js switches the pages and glides the selection to the new tab. A document tab's × raises
/// "nc-tab-closing" (cancelable) and removes the tab; the "+" raises "nc-tab-add".
/// </summary>
[HtmlTargetElement("nc-tab-control")]
public class TabControlTagHelper : NocturneTagHelper
{
    public TabControlStyle TabControlStyle { get; set; }

    public int SelectedIndex { get; set; }

    public string? Id { get; set; }

    /// <summary>Keeps the selected tab in the address (#name), so a link opens that tab.</summary>
    public bool UseHash { get; set; }

    /// <summary>TabHelper.ShowAddButton: a "+" after the document tabs.</summary>
    public bool ShowAddButton { get; set; }

    /// <summary>The content of nc-tab-control-tag (the children get a copy of context.Items, so they write here).</summary>
    [HtmlAttributeNotBound]
    public string Tag { get; set; } = "";

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var tabs = ChildList<TabItemTagHelper>.Start(context);
        context.Items[typeof(TabControlTagHelper)] = this;
        await output.GetChildContentAsync();
        var id = Id ?? Html.NewId("nc-tabs");
        var document = TabControlStyle == TabControlStyle.Document;
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", id);
        AddClasses(output, "nc-tab-control", document ? "nc-tab-control--document" : null);
        if (UseHash) output.Attributes.SetAttribute("data-nc-hash", "true");
        ApplyCommon(output);

        var selected = tabs.Items.FindIndex(t => t.IsSelected);
        if (selected < 0) selected = SelectedIndex;
        var close = Html.StrokeIcon(ViewContext, "Close");
        var strip = string.Concat(tabs.Items.Select((t, i) =>
        {
            var key = t.Name ?? i.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var on = i == selected;
            var name = t.Name is null ? "" : $" data-name=\"{Html.Attr(t.Name)}\"";
            var badge = document && t.Badge is not null ? $"<span class=\"nc-badge\">{Html.Text(t.Badge)}</span>" : "";
            var closeButton = document && t.ShowCloseButton
                ? $"<span class=\"nc-tab-item__close\" role=\"button\" data-nc-close-tab aria-label=\"Close\" title=\"Close\">{close}</span>"
                : "";
            return $"<button type=\"button\" class=\"nc-tab-item{(closeButton.Length > 0 ? " nc-tab-item--closable" : "")}\" role=\"tab\" id=\"{id}-tab-{Html.Attr(key)}\" aria-controls=\"{id}-panel-{Html.Attr(key)}\" " +
                   $"aria-selected=\"{(on ? "true" : "false")}\" tabindex=\"{(on ? "0" : "-1")}\"{name}{(t.IsEnabled ? "" : " disabled")}>" +
                   $"<span class=\"nc-tab-item__header\">{Html.Text(t.Header)}</span>{badge}{closeButton}</button>";
        }));
        var panels = string.Concat(tabs.Items.Select((t, i) =>
        {
            var key = t.Name ?? i.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var cls = t.Class is null ? "" : " " + Html.Attr(t.Class);
            return $"<div class=\"nc-tab-control__content{cls}\" role=\"tabpanel\" id=\"{id}-panel-{Html.Attr(key)}\" aria-labelledby=\"{id}-tab-{Html.Attr(key)}\"{(i == selected ? "" : " hidden")}>{t.Html}</div>";
        }));
        var tag = Tag;
        var add = document && ShowAddButton
            ? $"<button type=\"button\" class=\"nc-tab-control__add\" data-nc-add-tab aria-label=\"New tab\" title=\"New tab\">{Html.StrokeIcon(ViewContext, "Plus")}</button>"
            : "";
        var indicator = document ? "" : "<span class=\"nc-tab-control__indicator\" aria-hidden=\"true\"></span>";
        output.Content.SetHtmlContent(
            "<div class=\"nc-tab-control__header\">" +
            $"<div class=\"nc-tab-control__strip\" role=\"tablist\">{indicator}{strip}</div>{add}" +
            (tag.Length > 0 ? $"<div class=\"nc-tab-control__tag\">{tag}</div>" : "") +
            "</div>" +
            (document ? $"<div class=\"nc-tab-control__panel\">{panels}</div>" : panels));
    }
}

/// <summary><c>&lt;nc-expander header="Billing address" is-expanded="true"&gt;…&lt;/nc-expander&gt;</c>: a 48px glass header whose chevron turns.</summary>
[HtmlTargetElement("nc-expander")]
public class ExpanderTagHelper : NocturneTagHelper
{
    public string Header { get; set; } = "";

    public bool IsExpanded { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "details";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-expander");
        if (IsExpanded) output.Attributes.SetAttribute("open", "open");
        if (context.Items.TryGetValue(typeof(AccordionTagHelper), out var group)) output.Attributes.SetAttribute("name", (string)group);
        if (!IsEnabled) output.Attributes.SetAttribute("aria-disabled", "true");
        ApplyCommon(output);
        output.Content.SetHtmlContent($"<summary{(IsEnabled ? "" : " tabindex=\"-1\"")}>{Html.Text(Header)}</summary><div class=\"nc-expander__content\">{content}</div>");
    }
}

/// <summary><c>&lt;nc-accordion&gt;&lt;nc-expander …&gt;…&lt;/nc-accordion&gt;</c>: Expanders 6px apart, one open at a time.</summary>
[HtmlTargetElement("nc-accordion")]
public class AccordionTagHelper : NocturneTagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        context.Items[typeof(AccordionTagHelper)] = Html.NewId("nc-accordion");
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-accordion");
        ApplyCommon(output);
        output.Content.SetHtmlContent(content);
    }
}

/// <summary>
/// <c>&lt;nc-busy-indicator is-busy="true" busy-content="Loading orders…"&gt;content&lt;/nc-busy-indicator&gt;</c>:
/// blurs and dims the content under a small card with a progress ring (blur-radius, 4 by default, as in WPF).
/// Set data-busy from script.
/// </summary>
[HtmlTargetElement("nc-busy-indicator")]
public class BusyIndicatorTagHelper : NocturneTagHelper
{
    public bool IsBusy { get; set; }

    public string BusyContent { get; set; } = "Please wait…";

    /// <summary>Blur radius of the content while busy, as WPF's BlurRadius (4 by default); 0 turns the blur off.</summary>
    public double? BlurRadius { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-busy-indicator");
        if (BlurRadius is { } radius)
        {
            // WPF's Gaussian radius is about three times CSS blur()'s standard deviation.
            output.Attributes.SetAttribute("style", $"--nc-busy-blur:{Html.Num(Math.Round(Math.Max(0, radius) / 3, 2))}px;" + (output.Attributes["style"]?.Value ?? ""));
        }
        output.Attributes.SetAttribute("data-busy", IsBusy ? "true" : "false");
        output.Attributes.SetAttribute("aria-busy", IsBusy ? "true" : "false");
        ApplyCommon(output);
        output.Content.SetHtmlContent(
            $"<div class=\"nc-busy-indicator__content\">{content}</div>" +
            $"<div class=\"nc-busy-indicator__card\" role=\"status\">{ProgressRingTagHelper.Markup(22)}<span>{Html.Text(BusyContent)}</span></div>");
    }
}

/// <summary><c>&lt;nc-badged badge="12"&gt;&lt;nc-button&gt;Inbox&lt;/nc-button&gt;&lt;/nc-badged&gt;</c>: an accent count on the top-right corner.</summary>
[HtmlTargetElement("nc-badged")]
public class BadgedTagHelper : NocturneTagHelper
{
    public string? Badge { get; set; }

    /// <summary>BadgeBackground as a CSS colour ("var(--nc-error)"); the accent by default.</summary>
    public string? BadgeBackground { get; set; }

    /// <summary>BadgeForeground as a CSS colour; the text on the accent by default.</summary>
    public string? BadgeForeground { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-badged");
        ApplyCommon(output);
        var style = (BadgeBackground is null ? "" : $"background:{Html.Attr(BadgeBackground)};") + (BadgeForeground is null ? "" : $"color:{Html.Attr(BadgeForeground)};");
        var styleAttribute = style.Length == 0 ? "" : $" style=\"{style}\"";
        output.Content.SetHtmlContent(content + (Badge is null ? "" : $"<span class=\"nc-badged__badge\"{styleAttribute}>{Html.Text(Badge)}</span>"));
    }
}

/// <summary>
/// <c>&lt;nc-grid-splitter /&gt;</c> between two panes of a flex row (or of a flex column with
/// <c>resize-direction="Rows"</c>): a hairline with a small grip; drag it to resize the pane before it.
/// </summary>
[HtmlTargetElement("nc-grid-splitter", TagStructure = TagStructure.WithoutEndTag)]
public class GridSplitterTagHelper : NocturneTagHelper
{
    /// <summary><c>Columns</c> (default) or <c>Rows</c>.</summary>
    public string ResizeDirection { get; set; } = "Columns";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var rows = ResizeDirection.Equals("Rows", StringComparison.OrdinalIgnoreCase);
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-grid-splitter", rows ? "nc-grid-splitter--rows" : null);
        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute("aria-orientation", rows ? "horizontal" : "vertical");
        ApplyCommon(output);
    }
}
