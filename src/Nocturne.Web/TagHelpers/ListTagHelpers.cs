using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>
/// An item of a ListBox: <c>&lt;nc-list-box-item content="Security" description="Passwords, 2FA and certificates" stroke-icon="Shield" /&gt;</c>.
/// With an icon the item gets an icon tile (filled with the accent while selected); a description goes under the content.
/// </summary>
[HtmlTargetElement("nc-list-box-item", ParentTag = "nc-list-box")]
public class ListBoxItemTagHelper : TagHelper
{
    public string? Content { get; set; }

    /// <summary>A second, smaller line under the content.</summary>
    public string? Description { get; set; }

    public string? Value { get; set; }

    public bool IsEnabled { get; set; } = true;

    /// <summary>A filled Material Design icon in an icon tile before the content.</summary>
    public NocturneIconKind Icon { get; set; }

    /// <summary>One of the theme's line icons in an icon tile before the content ("Shield").</summary>
    public string? StrokeIcon { get; set; }

    /// <summary>With href the item is a link (a navigation list).</summary>
    public string? Href { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    internal string Html { get; private set; } = "";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = Content is null ? (await output.GetChildContentAsync()).GetContent() : TagHelpers.Html.Text(Content);
        var icon = TagHelpers.Html.Icon(ViewContext, Icon) + TagHelpers.Html.StrokeIcon(ViewContext, StrokeIcon);
        var navigation = context.Items.TryGetValue(typeof(ListBoxTagHelper), out var parent) && ((ListBoxTagHelper)parent).ListBoxStyle == ListBoxStyle.Navigation;
        if (icon.Length > 0 || Description is not null)
        {
            var tile = icon.Length == 0 ? "" : $"<span class=\"nc-icon-tile{(navigation ? " nc-icon-tile--small" : "")}\" aria-hidden=\"true\">{icon}</span>";
            var description = Description is null ? "" : $"<span class=\"nc-list-box-item__description\">{TagHelpers.Html.Text(Description)}</span>";
            content = $"{tile}<span class=\"nc-list-box-item__text\"><span class=\"nc-list-box-item__title\">{content}</span>{description}</span>";
        }
        Html = content;
        ChildList<ListBoxItemTagHelper>.Of(context)?.Items.Add(this);
        output.SuppressOutput();
    }
}

/// <summary>The ListBox styles.</summary>
public enum ListBoxStyle
{
    Default,

    /// <summary>Nocturne.Styles.ListBox.Navigation: a side menu, items of an icon tile and a label.</summary>
    Navigation,
}

/// <summary>
/// <c>&lt;nc-list-box selected-index="1"&gt;&lt;nc-list-box-item content="…" /&gt;…&lt;/nc-list-box&gt;</c>: single
/// selection with the mouse and the arrow keys; name posts the selected value.
/// </summary>
[HtmlTargetElement("nc-list-box")]
public class ListBoxTagHelper : NocturneTagHelper
{
    public int SelectedIndex { get; set; } = -1;

    public string? Name { get; set; }

    public ListBoxStyle ListBoxStyle { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var children = ChildList<ListBoxItemTagHelper>.Start(context);
        context.Items[typeof(ListBoxTagHelper)] = this;
        await output.GetChildContentAsync();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-list-box", ListBoxStyle == ListBoxStyle.Navigation ? "nc-list-box--navigation" : null);
        output.Attributes.SetAttribute("role", "listbox");
        output.Attributes.SetAttribute("tabindex", "0");
        if (Name is not null) output.Attributes.SetAttribute("data-nc-name", Name);
        if (!IsEnabled) output.Attributes.SetAttribute("aria-disabled", "true");
        ApplyCommon(output);
        var html = string.Concat(children.Items.Select((item, i) =>
        {
            var value = item.Value is null ? "" : $" data-value=\"{Html.Attr(item.Value)}\"";
            var off = item.IsEnabled ? "" : " aria-disabled=\"true\"";
            var tag = item.Href is null ? "div" : "a";
            var href = item.Href is null ? "" : $" href=\"{Html.Attr(item.Href)}\"";
            return $"<{tag} class=\"nc-list-box-item\" role=\"option\" aria-selected=\"{(i == SelectedIndex ? "true" : "false")}\"{href}{value}{off}>{item.Html}</{tag}>";
        }));
        if (Name is not null && SelectedIndex >= 0 && SelectedIndex < children.Items.Count)
        {
            html += $"<input type=\"hidden\" name=\"{Html.Attr(Name)}\" value=\"{Html.Attr(children.Items[SelectedIndex].Value ?? children.Items[SelectedIndex].Content)}\">";
        }
        output.Content.SetHtmlContent(html);
    }
}

/// <summary>ScrollBarVisibility of a ScrollViewer.</summary>
public enum ScrollBarVisibility
{
    Auto,
    Visible,
    Hidden,
    Disabled,
}

/// <summary>
/// <c>&lt;nc-scroll-viewer&gt;…&lt;/nc-scroll-viewer&gt;</c>: a scrolling box with the Nocturne bars, thin rounded
/// bars that show while the mouse is over the box.
/// </summary>
[HtmlTargetElement("nc-scroll-viewer")]
public class ScrollViewerTagHelper : NocturneTagHelper
{
    public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; } = ScrollBarVisibility.Disabled;

    public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

    private static string Css(ScrollBarVisibility v) => v switch
    {
        ScrollBarVisibility.Visible => "scroll",
        ScrollBarVisibility.Hidden or ScrollBarVisibility.Disabled => "hidden",
        _ => "auto",
    };

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        AddClasses(output, "nc-scroll-viewer");
        output.Attributes.SetAttribute("style", $"overflow-x:{Css(HorizontalScrollBarVisibility)};overflow-y:{Css(VerticalScrollBarVisibility)};" + (output.Attributes["style"]?.Value ?? ""));
        output.Attributes.SetAttribute("tabindex", "-1");
        ApplyCommon(output);
    }
}

/// <summary>
/// <c>&lt;nc-tree-view-item header="Contoso" is-expanded="true"&gt; child items &lt;/nc-tree-view-item&gt;</c>.
/// An item with children gets the [+]/[−] box.
/// </summary>
[HtmlTargetElement("nc-tree-view-item")]
public class TreeViewItemTagHelper : TagHelper
{
    public string Header { get; set; } = "";

    public bool IsExpanded { get; set; }

    public bool IsSelected { get; set; }

    public string? Value { get; set; }

    /// <summary>A filled Material Design icon before the header.</summary>
    public NocturneIconKind Icon { get; set; }

    /// <summary>One of the theme's line icons before the header ("Folder").</summary>
    public string? StrokeIcon { get; set; }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var children = (await output.GetChildContentAsync()).GetContent().Trim();
        var value = Value is null ? "" : $" data-value=\"{Html.Attr(Value)}\"";
        var icon = Html.Icon(ViewContext, Icon) + Html.StrokeIcon(ViewContext, StrokeIcon);
        var item = $"<span class=\"nc-tree-view__item\" role=\"treeitem\" aria-selected=\"{(IsSelected ? "true" : "false")}\"{value}>{icon}{Html.Text(Header)}</span>";
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(children.Length == 0
            ? item
            : $"<details{(IsExpanded ? " open" : "")}><summary>{item}</summary><ul role=\"group\">{children}</ul></details>");
    }
}

/// <summary><c>&lt;nc-tree-view&gt;&lt;nc-tree-view-item …&gt;…&lt;/nc-tree-view&gt;</c>: the dark tree with bold items.</summary>
[HtmlTargetElement("nc-tree-view")]
public class TreeViewTagHelper : NocturneTagHelper
{
    public string? Name { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-tree-view");
        output.Attributes.SetAttribute("role", "tree");
        if (Name is not null) output.Attributes.SetAttribute("data-nc-name", Name);
        ApplyCommon(output);
    }
}

/// <summary>A column of a DataGrid or ListView, collected by the parent.</summary>
public abstract class GridColumnTagHelper : TagHelper
{
    public string Header { get; set; } = "";

    /// <summary>Binding: the property of each item to show.</summary>
    public string? Binding { get; set; }

    /// <summary>A pixel width, or * for the rest.</summary>
    public string? Width { get; set; }

    /// <summary>StringFormat: a .NET format such as N1 or d.</summary>
    public string? StringFormat { get; set; }

    /// <summary>Right for numbers.</summary>
    public string? HorizontalAlignment { get; set; }

    /// <summary>Ascending or Descending: the sort arrow in the header (the order the rows come in).</summary>
    public string? SortDirection { get; set; }

    /// <summary>The property to sort by when it is not the binding.</summary>
    public string? SortMemberPath { get; set; }

    /// <summary>False keeps a click on the header from sorting the rows.</summary>
    public bool CanUserSort { get; set; } = true;

    /// <summary>False keeps the column at its width.</summary>
    public bool CanUserResize { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ChildList<GridColumnTagHelper>.Of(context)?.Items.Add(this);
        output.SuppressOutput();
    }

    private static object? Value(object? item, string? path) =>
        item is null || path is null ? null : item.GetType().GetProperty(path)?.GetValue(item);

    internal string Cell(object? item)
    {
        var value = Value(item, Binding);
        return value is IFormattable f && StringFormat is not null
            ? f.ToString(StringFormat, CultureInfo.CurrentCulture)
            : Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";
    }

    /// <summary>
    /// data-sort-value for values whose text does not sort right in every culture: numbers as
    /// invariant numbers, dates as ISO 8601, enums by their value, as WPF compares them.
    /// </summary>
    internal string SortAttribute(object? item)
    {
        var inv = CultureInfo.InvariantCulture;
        var key = Value(item, SortMemberPath ?? Binding) switch
        {
            DateTime d => d.ToString("s", inv),
            DateTimeOffset d => d.UtcDateTime.ToString("s", inv),
            DateOnly d => d.ToString("yyyy-MM-dd", inv),
            TimeOnly t => t.ToString("HH:mm:ss.fffffff", inv),
            TimeSpan t => t.Ticks.ToString(inv),
            Enum e => Convert.ToDecimal(e, inv).ToString(inv),
            IConvertible c and (byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal) => c.ToString(inv),
            string s when SortMemberPath is not null => s,
            _ => null,
        };
        return key is null ? "" : $" data-sort-value=\"{Html.Attr(key)}\"";
    }

    internal string WidthStyle => Width is null || Width == "*" ? "" : $" style=\"width:{Html.Attr(Width)}px\"";

    internal string Align => string.Equals(HorizontalAlignment, "Right", StringComparison.OrdinalIgnoreCase) ? " class=\"nc-data-grid__number\"" : "";
}

/// <summary><c>&lt;nc-data-grid-text-column header="Hours" binding="Hours" string-format="N1" width="60" horizontal-alignment="Right" /&gt;</c></summary>
[HtmlTargetElement("nc-data-grid-text-column", ParentTag = "nc-data-grid", TagStructure = TagStructure.WithoutEndTag)]
public class DataGridTextColumnTagHelper : GridColumnTagHelper
{
}

/// <summary><c>&lt;nc-grid-view-column header="Employee" display-member-binding="Employee" width="150" /&gt;</c></summary>
[HtmlTargetElement("nc-grid-view-column", ParentTag = "nc-list-view", TagStructure = TagStructure.WithoutEndTag)]
public class GridViewColumnTagHelper : GridColumnTagHelper
{
    public string? DisplayMemberBinding { get => Binding; set => Binding = value; }
}

/// <summary>
/// <c>&lt;nc-data-grid items="@rows" selected-index="1" height="190"&gt;&lt;nc-data-grid-text-column … /&gt;&lt;/nc-data-grid&gt;</c>:
/// a read-only grid with a sticky header, filled from the items like ItemsSource.
/// </summary>
[HtmlTargetElement("nc-data-grid")]
public class DataGridTagHelper : NocturneTagHelper
{
    public IEnumerable Items { get; set; } = Array.Empty<object>();

    public int SelectedIndex { get; set; } = -1;

    /// <summary>Height in pixels; the rows scroll under the header.</summary>
    public double? Height { get; set; }

    /// <summary>Single (default) or None.</summary>
    public string SelectionMode { get; set; } = "Single";

    /// <summary>False: the headers do not sort the rows (sort on the server, show it with sort-direction).</summary>
    public bool CanUserSortColumns { get; set; } = true;

    /// <summary>False: the columns cannot be resized.</summary>
    public bool CanUserResizeColumns { get; set; } = true;

    /// <summary>HeadersVisibility All: a row header before each row, as DataGridRowHeader.</summary>
    public bool RowHeaders { get; set; }

    protected virtual string HostClass => "nc-data-grid-host";

    protected virtual string TableClass => "nc-data-grid";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columns = ChildList<GridColumnTagHelper>.Start(context);
        await output.GetChildContentAsync();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, HostClass);
        if (Height is { } h)
        {
            output.Attributes.SetAttribute("style", $"height:{Html.Num(h)}px;" + (output.Attributes["style"]?.Value ?? ""));
        }
        ApplyCommon(output);
        var options = (SelectionMode == "None" ? "" : " data-nc-selection=\"single\"") +
            (CanUserSortColumns ? "" : " data-nc-sort=\"false\"") +
            (CanUserResizeColumns ? "" : " data-nc-resize=\"false\"");
        var head = string.Concat(columns.Items.Select(c =>
        {
            // Headers stay left-aligned over right-aligned numbers, as in a WPF DataGrid
            var attributes = (c.SortDirection is null ? "" : $" aria-sort=\"{c.SortDirection.ToLowerInvariant()}\"") +
                (c.CanUserSort ? "" : " data-nc-sort=\"false\"") +
                (c.CanUserResize ? "" : " data-nc-resize=\"false\"");
            return $"<th{c.WidthStyle}{attributes}>{Html.Text(c.Header)}</th>";
        }));
        var rowHeaders = RowHeaders && TableClass != "";
        if (rowHeaders) head = "<th class=\"nc-data-grid__row-header\" data-nc-sort=\"false\" data-nc-resize=\"false\"></th>" + head;
        var rows = string.Concat(Items.Cast<object?>().Select((item, i) =>
            $"<tr aria-selected=\"{(i == SelectedIndex ? "true" : "false")}\">" + (rowHeaders ? "<th class=\"nc-data-grid__row-header\" scope=\"row\"></th>" : "") +
            string.Concat(columns.Items.Select(c => $"<td{c.Align}{c.SortAttribute(item)}>{Html.Text(c.Cell(item))}</td>")) + "</tr>"));
        var tableClass = TableClass + (rowHeaders ? " nc-data-grid--row-headers" : "");
        output.Content.SetHtmlContent($"<table class=\"{tableClass}\"{options}><thead><tr>{head}</tr></thead><tbody>{rows}</tbody></table>");
    }
}

/// <summary><c>&lt;nc-list-view items="@rows" selected-index="2"&gt;&lt;nc-grid-view-column … /&gt;&lt;/nc-list-view&gt;</c>: ListView with a GridView.</summary>
[HtmlTargetElement("nc-list-view")]
public class ListViewTagHelper : DataGridTagHelper
{
    protected override string HostClass => "nc-list-view";

    protected override string TableClass => "";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);
        output.Attributes.SetAttribute("tabindex", "0");
    }
}

/// <summary>
/// <c>&lt;nc-calendar selected-date="@DateTime.Today" /&gt;</c>: the month view with the year and decade views behind the
/// header, with month and day names in the browser's culture unless culture (or data-nc-culture on a container) names one.
/// </summary>
[HtmlTargetElement("nc-calendar", TagStructure = TagStructure.WithoutEndTag)]
public class CalendarTagHelper : NocturneTagHelper
{
    public DateTime? SelectedDate { get; set; }

    public DateTime? DisplayDate { get; set; }

    /// <summary>The culture of the month and day names ("da-DK"); the browser's when empty.</summary>
    public string? Culture { get; set; }

    /// <summary>The first day of the week; the culture's when empty.</summary>
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>IsTodayHighlighted: the ring around today.</summary>
    public bool IsTodayHighlighted { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        AddClasses(output, "nc-calendar", IsTodayHighlighted ? null : "nc-calendar--no-today");
        if (SelectedDate is { } s) output.Attributes.SetAttribute("data-selected-date", s.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        if (DisplayDate is { } d) output.Attributes.SetAttribute("data-display-date", d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        DateCulture.Apply(output, Culture, FirstDayOfWeek);
        ApplyCommon(output);
    }
}
