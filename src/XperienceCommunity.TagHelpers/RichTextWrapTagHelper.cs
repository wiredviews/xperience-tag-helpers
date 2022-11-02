using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XperienceCommunity.TagHelpers;

/// <summary>
/// A Tag Helper to conditionally wrap the output of CKEditor Rich Text.
/// </summary>
/// <remarks>
/// When the 'xpc-rich-text-wrap' attribute is placed on an element, the first child element
/// will be inspected to determine if it's the same element type as its parent. If it is, the parent
/// element (that the attribute is placed on) is removed from the output, otherwise it is rendered.
/// This can be helpful when you want to guarantee that Rich Text is wrapped in a specific type of element,
/// like a paragraph, but only if the Rich Text doesn't already contain one.
/// </remarks>
[HtmlTargetElement("*", Attributes = TagHelperAttribute)]
public class RichTextWrapTagHelper : TagHelper
{
    public const string TagHelperAttribute = "xpc-rich-text-wrap";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        string contentText = content.GetContent().TrimStart();

        _ = output.Attributes.RemoveAll(TagHelperAttribute);

        if (!contentText.StartsWith("<"))
        {
            return;
        }

        if (contentText.StartsWith($"<{context.TagName} ", StringComparison.OrdinalIgnoreCase))
        {
            output.TagName = null;

            return;
        }
    }
}
