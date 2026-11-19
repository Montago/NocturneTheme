using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.TagHelpers;

/// <summary>
/// Base of the &lt;nc-*&gt; tag helpers: the properties every WPF control has. Attribute names are the
/// WPF property names in kebab case (IsEnabled → is-enabled).
/// </summary>
public abstract class NocturneTagHelper : TagHelper
{
    /// <summary>IsEnabled="False": the control is shown disabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>ToolTip: the small popup nocturne.js shows under the pointer.</summary>
    public string? ToolTip { get; set; }

    /// <summary>
    /// Marks the control invalid with this message, as a validation error in WPF: the red frame and
    /// bottom line, and the message in a callout under it while the control has the focus or the mouse.
    /// </summary>
    public string? ValidationError { get; set; }

    /// <summary>Sets data-nc-tool-tip and the invalid look on the element that represents the control.</summary>
    protected void ApplyCommon(TagHelperOutput output)
    {
        if (ToolTip is not null)
        {
            output.Attributes.SetAttribute("data-nc-tool-tip", ToolTip);
        }
        if (ValidationError is not null)
        {
            output.Attributes.SetAttribute("aria-invalid", "true");
            output.Attributes.SetAttribute("data-nc-error", ValidationError);
        }
    }

    /// <summary>The same attributes as markup, for an inner element (a text box inside a wrapper).</summary>
    protected string CommonAttributes(bool toolTip = true, bool error = true)
    {
        var s = "";
        if (toolTip && ToolTip is not null)
        {
            s += $" data-nc-tool-tip=\"{Html.Attr(ToolTip)}\"";
        }
        if (error && ValidationError is not null)
        {
            s += $" aria-invalid=\"true\" data-nc-error=\"{Html.Attr(ValidationError)}\"";
        }
        return s;
    }

    /// <summary>Keeps class, style, id and other plain attributes the page wrote on the tag helper.</summary>
    protected static void AddClasses(TagHelperOutput output, params string?[] classes)
    {
        foreach (var c in classes)
        {
            if (!string.IsNullOrEmpty(c))
            {
                output.AddClass(c, Html.Encoder);
            }
        }
    }
}

/// <summary>Collects the children of a container tag helper (TabControl → TabItems and so on).</summary>
internal sealed class ChildList<T>
{
    public List<T> Items { get; } = [];

    public static ChildList<T> Start(TagHelperContext context)
    {
        var list = new ChildList<T>();
        context.Items[typeof(ChildList<T>)] = list;
        return list;
    }

    public static ChildList<T>? Of(TagHelperContext context) =>
        context.Items.TryGetValue(typeof(ChildList<T>), out var v) ? (ChildList<T>)v : null;
}

internal static class Html
{
    public static readonly HtmlEncoder Encoder = HtmlEncoder.Default;

    public static string Text(string? s) => Encoder.Encode(s ?? "");

    public static string Attr(string? s) => Encoder.Encode(s ?? "");

    public static string Num(double v) => v.ToString(CultureInfo.InvariantCulture);

    public static string Kebab(string pascal) =>
        System.Text.RegularExpressions.Regex.Replace(pascal, "(?<=[a-z0-9])(?=[A-Z])", "-").ToLowerInvariant();

    /// <summary>A unique id for the parts of a control that need one (tab panels, popups).</summary>
    public static string NewId(string prefix) => prefix + "-" + Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// The address of an icon in the sprite, below the site's path base (app.UsePathBase), so the
    /// icons also load when the site runs under a sub-path.
    /// </summary>
    public static string IconHref(ViewContext view, NocturneIconKind kind) =>
        view.HttpContext.Request.PathBase + "/_content/Nocturne.Web/img/icons.svg#" + NocturneIconIds.Get(kind);

    /// <summary>A decorative icon (hidden from screen readers) as markup; empty for None.</summary>
    public static string Icon(ViewContext view, NocturneIconKind kind) => kind == NocturneIconKind.None
        ? ""
        : $"<svg class=\"nc-icon\" aria-hidden=\"true\"><use href=\"{Attr(IconHref(view, kind))}\"></use></svg>";

    /// <summary>
    /// The address of one of the theme's line icons (Styles/Icons.xaml) in the sprite:
    /// "ChevronDown" -> icons.svg#stroke-chevron-down.
    /// </summary>
    public static string StrokeHref(ViewContext view, string name) =>
        view.HttpContext.Request.PathBase + "/_content/Nocturne.Web/img/icons.svg#stroke-" + Kebab(name);

    /// <summary>A decorative line icon (StrokeIcon) as markup; empty for an empty name.</summary>
    public static string StrokeIcon(ViewContext view, string? name, string? extraClass = null) => string.IsNullOrWhiteSpace(name)
        ? ""
        : $"<svg class=\"nc-stroke-icon{(extraClass is null ? "" : " " + extraClass)}\" aria-hidden=\"true\"><use href=\"{Attr(StrokeHref(view, name))}\"></use></svg>";
}

/// <summary>The culture attributes of Calendar and DatePicker (data-nc-culture, data-nc-first-day-of-week).</summary>
internal static class DateCulture
{
    public static void Apply(TagHelperOutput output, string? culture, DayOfWeek? firstDayOfWeek)
    {
        if (!string.IsNullOrWhiteSpace(culture))
        {
            output.Attributes.SetAttribute("data-nc-culture", culture);
        }
        if (firstDayOfWeek is { } day)
        {
            output.Attributes.SetAttribute("data-nc-first-day-of-week", ((int)day).ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>The named culture, or the request's when the name is empty or unknown.</summary>
    public static CultureInfo Of(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            try
            {
                return CultureInfo.GetCultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
            }
        }
        return CultureInfo.CurrentCulture;
    }
}

/// <summary>
/// <c>nc-tool-tip="Saves the changes"</c> on any element: the Nocturne tool tip under the pointer.
/// </summary>
[HtmlTargetElement("*", Attributes = "nc-tool-tip")]
public class ToolTipTagHelper : TagHelper
{
    [HtmlAttributeName("nc-tool-tip")]
    public string? Text { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Text is not null)
        {
            output.Attributes.SetAttribute("data-nc-tool-tip", Text);
        }
    }
}
