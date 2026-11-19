using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>Marks that the tag helpers below sit in a menu (a nc-separator becomes a menu separator).</summary>
internal sealed class MenuScope
{
}

/// <summary>
/// <c>&lt;nc-menu-item header="Cut" input-gesture-text="Ctrl+X" /&gt;</c>; is-checkable / is-checked for a check
/// mark; child nc-menu-items make it a submenu. In a nc-menu bar it is a top-level header.
/// </summary>
[HtmlTargetElement("nc-menu-item")]
public class MenuItemTagHelper : NocturneTagHelper
{
    public string Header { get; set; } = "";

    public string? InputGestureText { get; set; }

    public bool IsCheckable { get; set; }

    public bool IsChecked { get; set; }

    /// <summary>A NocturneIcon in the icon column.</summary>
    public NocturneIconKind Icon { get; set; }

    /// <summary>One of the theme's line icons in the icon column ("Copy").</summary>
    public string? StrokeIcon { get; set; }

    public string? Href { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var topLevel = context.Items.ContainsKey(typeof(MenuTagHelper)) && !context.Items.ContainsKey(typeof(MenuScope));
        context.Items[typeof(MenuScope)] = new MenuScope();
        var children = (await output.GetChildContentAsync()).GetContent().Trim();
        var hasMenu = children.Length > 0;
        var menuId = Html.NewId("nc-submenu");
        var text = Html.Text(Header.Replace("_", ""));

        output.TagName = Href is null ? "button" : "a";
        output.TagMode = TagMode.StartTagAndEndTag;
        if (Href is null) output.Attributes.SetAttribute("type", "button");
        else output.Attributes.SetAttribute("href", Href);
        AddClasses(output, "nc-menu-item");
        if (!IsEnabled) output.Attributes.SetAttribute("disabled", "disabled");
        ApplyCommon(output);

        if (topLevel)
        {
            output.Attributes.SetAttribute("role", "menuitem");
            if (hasMenu)
            {
                output.Attributes.SetAttribute("popovertarget", menuId);
                output.Attributes.SetAttribute("aria-haspopup", "menu");
            }
            output.Content.SetHtmlContent(text);
            if (hasMenu)
            {
                output.PostElement.SetHtmlContent($"<div class=\"nc-context-menu\" popover id=\"{menuId}\" role=\"menu\">{children}</div>");
            }
            return;
        }

        output.Attributes.SetAttribute("role", IsCheckable ? "menuitemcheckbox" : "menuitem");
        if (IsCheckable) output.Attributes.SetAttribute("aria-checked", IsChecked ? "true" : "false");
        var icon = Html.Icon(ViewContext, Icon) + Html.StrokeIcon(ViewContext, StrokeIcon);
        var check = IsCheckable ? Html.StrokeIcon(ViewContext, "Check", "nc-menu-item__check") : "";
        icon = $"<span class=\"nc-menu-item__icon\">{icon}{check}</span>";
        var gesture = InputGestureText is null ? "" : $"<span class=\"nc-menu-item__gesture\">{Html.Text(InputGestureText)}</span>";
        var arrow = hasMenu ? Html.StrokeIcon(ViewContext, "ChevronRight", "nc-menu-item__arrow") : "";
        output.Content.SetHtmlContent(icon + $"<span class=\"nc-menu-item__header\">{text}</span>" + gesture + arrow);
        if (hasMenu)
        {
            output.Attributes.SetAttribute("aria-haspopup", "menu");
            output.Attributes.SetAttribute("data-nc-submenu", menuId);
            // The submenu sits inside its parent menu, so opening it keeps the parent open
            output.PostElement.SetHtmlContent($"<div class=\"nc-context-menu\" popover id=\"{menuId}\" role=\"menu\">{children}</div>");
        }
    }
}

/// <summary><c>&lt;nc-menu&gt;&lt;nc-menu-item header="_File"&gt;…&lt;/nc-menu-item&gt;&lt;/nc-menu&gt;</c>: the menu bar.</summary>
[HtmlTargetElement("nc-menu")]
public class MenuTagHelper : NocturneTagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        context.Items[typeof(MenuTagHelper)] = this;
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-menu");
        output.Attributes.SetAttribute("role", "menubar");
        ApplyCommon(output);
        output.Content.SetHtmlContent(content);
    }
}

/// <summary>
/// <c>&lt;nc-context-menu id="gridMenu"&gt;&lt;nc-menu-item header="Copy" /&gt;&lt;/nc-context-menu&gt;</c>; put
/// data-nc-context-menu="gridMenu" on the element that opens it with a right-click.
/// </summary>
[HtmlTargetElement("nc-context-menu")]
public class ContextMenuTagHelper : TagHelper
{
    public string Id { get; set; } = "";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        context.Items[typeof(MenuScope)] = new MenuScope();
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-context-menu", Html.Encoder);
        output.Attributes.SetAttribute("id", Id);
        output.Attributes.SetAttribute("popover", "");
        output.Attributes.SetAttribute("role", "menu");
        output.Content.SetHtmlContent(content);
    }
}

/// <summary><c>&lt;nc-tool-bar-tray&gt;&lt;nc-tool-bar&gt;…&lt;/nc-tool-bar&gt;&lt;/nc-tool-bar-tray&gt;</c></summary>
[HtmlTargetElement("nc-tool-bar-tray")]
public class ToolBarTrayTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("nc-tool-bar-tray", Html.Encoder);
    }
}

/// <summary>
/// <c>&lt;nc-tool-bar&gt;</c>: the glass bar with the grip; a nc-separator in it is the vertical one, and
/// buttons are subtle until the mouse is over them.
/// </summary>
[HtmlTargetElement("nc-tool-bar")]
public class ToolBarTagHelper : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        context.Items[typeof(ToolBarTagHelper)] = this;
        var content = (await output.GetChildContentAsync()).GetContent();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("nc-tool-bar", Html.Encoder);
        output.Attributes.SetAttribute("role", "toolbar");
        // a nc-separator in here writes the vertical tool bar separator itself
        output.Content.SetHtmlContent("<span class=\"nc-tool-bar__thumb\" aria-hidden=\"true\"></span>" + content);
    }
}
