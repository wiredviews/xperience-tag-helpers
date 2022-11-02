using System.Text;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XperienceCommunity.TagHelpers;

public interface IImageViewModel
{
    string RelativePath { get; }
    string? AltText { get; }
    string? Title { get; }
    int Width { get; }
    int Height { get; }
}

[HtmlTargetElement("img", Attributes = TagHelperAttribute)]
public class ImageTagHelper : TagHelper
{
    public const string TagHelperAttribute = "xpc-image";
    public const string TagHelperConstraintAttribute = "xpc-image-size-constraint";
    public const string TagHelperSizesAttribute = "xpc-image-sizes";
    public const string TagHelperSrcSetAttribute = "xpc-image-srcset";

    /// <summary>
    /// The image which is the source of content for the element
    /// </summary>
    [HtmlAttributeName(TagHelperAttribute)]
    public IImageViewModel? Image { get; set; } = null;

    /// <summary>
    /// An optional <see cref="SizeConstraint"/> to limit the served image size
    /// </summary>
    [HtmlAttributeName(TagHelperConstraintAttribute)]
    public SizeConstraint Constraint { get; set; } = SizeConstraint.Empty;

    /// <summary>
    /// An optional set of values to define the size values. Cannot be used with <see cref="SrcSet" />
    /// </summary>
    /// <remarks>
    /// See: https://developer.mozilla.org/en-US/docs/Learn/HTML/Multimedia_and_embedding/Responsive_images#how_do_you_create_responsive_images
    /// </remarks>
    [HtmlAttributeName(TagHelperSrcSetAttribute)]
    public IEnumerable<(int maxWidthOrHeight, string xDescriptor)> Sizes { get; set; } = Enumerable.Empty<(int, string)>();

    /// <summary>
    /// An optional set of values to define the srcset values. Cannot be used with <see cref="Sizes" />
    /// </summary>
    /// <remarks>
    /// See: https://developer.mozilla.org/en-US/docs/Learn/HTML/Multimedia_and_embedding/Responsive_images#how_do_you_create_responsive_images
    /// </remarks>
    [HtmlAttributeName(TagHelperSizesAttribute)]
    public IEnumerable<(double factor, int maxWidthOrHeight)> SrcSet { get; set; } = Enumerable.Empty<(double, int)>();

    public string Alt { get; set; } = "";
    public string Title { get; set; } = "";
    public string Loading { get; set; } = "";

    public override int Order => 10;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!string.IsNullOrWhiteSpace(Alt))
        {
            output.CopyHtmlAttribute(nameof(Alt), context);
        }

        if (!string.IsNullOrWhiteSpace(Title))
        {
            output.CopyHtmlAttribute(nameof(Title), context);
        }

        if (!string.IsNullOrWhiteSpace(Loading))
        {
            output.CopyHtmlAttribute(nameof(Loading), context);
        }

        if (Image is IImageViewModel vm)
        {
            RenderImage(vm, output);
        }
        else
        {
            output.SuppressOutput();
        }
    }

    public void RenderImage(IImageViewModel image, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(image.RelativePath))
        {
            output.Attributes.SetAttribute($"data-{TagHelperAttribute}-error", "Image path is missing");

            ClearTagHelperAttributes(output);

            return;
        }

        // Xperience can't process these formats with size constraints parameters and the Empty constraint will be skipped
        bool skipConstraining = image.RelativePath.Contains(".svg", StringComparison.OrdinalIgnoreCase) || image.RelativePath.Contains(".webp", StringComparison.OrdinalIgnoreCase);

        if (skipConstraining)
        {
            output.Attributes.SetAttribute("src", image.RelativePath);
        }
        else
        {
            var fileUrl = new FileUrl(image.RelativePath, true).WithSizeConstraint(Constraint);

            output.Attributes.SetAttribute("src", fileUrl.RelativePath);

            SetSrcSet(image, output);
        }

        if (image.Width > 0)
        {
            output.Attributes.SetAttribute("width", image.Width);
        }

        if (image.Height > 0)
        {
            output.Attributes.SetAttribute("height", image.Height);
        }

        if (string.IsNullOrWhiteSpace(Alt))
        {
            output.Attributes.SetAttribute("alt", image.AltText);
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            output.Attributes.SetAttribute("title", image.Title);
        }

        if (string.IsNullOrWhiteSpace(Loading))
        {
            output.Attributes.SetAttribute("loading", "lazy");
        }

        ClearTagHelperAttributes(output);
    }

    private void SetSrcSet(IImageViewModel image, TagHelperOutput output)
    {
        var srcSetSb = new StringBuilder();
        var sizesSb = new StringBuilder();

        if (SrcSet.Any())
        {
            foreach (var (factor, maxWidthOrHeight) in SrcSet)
            {
                var url = new FileUrl(image.RelativePath, true).WithSizeConstraint(SizeConstraint.MaxWidthOrHeight(maxWidthOrHeight));

                srcSetSb
                    .Append($"{url.RelativePath} {factor}x")
                    .Append(",");
            }
        }
        else if (Sizes.Any())
        {
            foreach (var (maxWidthOrHeight, xDescriptor) in Sizes)
            {
                var url = new FileUrl(image.RelativePath, true).WithSizeConstraint(SizeConstraint.MaxWidthOrHeight(maxWidthOrHeight));

                srcSetSb
                    .Append($"{url.RelativePath} {maxWidthOrHeight}w")
                    .Append(",");

                sizesSb
                    .Append($"(max-width: {xDescriptor}) {maxWidthOrHeight}px")
                    .Append(",");
            }
        }

        if (srcSetSb.Length > 0)
        {
            // Trim trailing comma
            // https://stackoverflow.com/a/17215160/939634
            srcSetSb.Length--;

            output.Attributes.SetAttribute("srcset", srcSetSb.ToString());
        }

        if (sizesSb.Length > 0)
        {
            // Trim trailing comma
            // https://stackoverflow.com/a/17215160/939634
            sizesSb.Length--;

            output.Attributes.SetAttribute("sizes", sizesSb.ToString());
        }
    }

    private void ClearTagHelperAttributes(TagHelperOutput output)
    {
        _ = output.Attributes.RemoveAll(TagHelperAttribute);
        _ = output.Attributes.RemoveAll(TagHelperConstraintAttribute);
        _ = output.Attributes.RemoveAll(TagHelperSizesAttribute);
        _ = output.Attributes.RemoveAll(TagHelperSrcSetAttribute);
    }
}
