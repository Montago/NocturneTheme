using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Nocturne.Web.Demo.TagHelpers;

/// <summary>
/// <c>&lt;demo-row key="Theme" value="Nocturne (dark)" /&gt;</c>: the Demo.Key / Demo.Value pair of the WPF
/// demo, a label on the left and its value on the right.
/// </summary>
[HtmlTargetElement("demo-row", TagStructure = TagStructure.WithoutEndTag)]
public class DemoRowTagHelper : TagHelper
{
    public string Key { get; set; } = "";

    public string Value { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var e = HtmlEncoder.Default;
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "demo-row");
        output.Content.SetHtmlContent($"<span class=\"demo-row__key\">{e.Encode(Key)}</span><span class=\"demo-row__value\">{e.Encode(Value)}</span>");
    }
}
