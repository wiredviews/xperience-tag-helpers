using System.Text;
using CMS.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XperienceCommunity.TagHelpers;

[HtmlTargetElement("*", Attributes = HTMLAttributeIf)]
[HtmlTargetElement("*", Attributes = HTMLAttributeIfNot)]
[HtmlTargetElement("*", Attributes = HTMLAttributeIfElse)]
[HtmlTargetElement("*", Attributes = HTMLAttributeIfElseMany)]
public class HTMLConditionalClassesTagHelper : TagHelper
{
    public const string HTMLAttributeIf = "xpc-class-if";
    public const string HTMLAttributeIfNot = "xpc-class-if-not";
    public const string HTMLAttributeIfElse = "xpc-class-if-else";
    public const string HTMLAttributeIfMany = "xpc-class-if-many";
    public const string HTMLAttributeIfElseMany = "xpc-class-if-else-many";

    [HtmlAttributeName(HTMLAttributeIf)]
    public (bool Condition, string Classes) If { get; set; } = (false, "");

    [HtmlAttributeName(HTMLAttributeIfNot)]
    public (bool Condition, string Classes) IfNot { get; set; } = (true, "");

    [HtmlAttributeName(HTMLAttributeIfElse)]
    public (bool Condition, string ClassesIf, string ClassesIfNot) IfEither { get; set; } = (false, "", "");

    [HtmlAttributeName(HTMLAttributeIfMany)]
    public IEnumerable<(bool Condition, string Classes)> IfMany { get; set; } = Enumerable.Empty<(bool Condition, string Classes)>();

    [HtmlAttributeName(HTMLAttributeIfElseMany)]
    public IEnumerable<(bool Condition, string ClassesIf, string ClassesIfNot)> IfElseMany { get; set; } = Enumerable.Empty<(bool Condition, string ClassesIf, string ClassesIfNot)>();

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var existingClassesAttr = context
            .AllAttributes
            .FirstOrDefault(a => string.Equals(a.Name, "class", StringComparison.OrdinalIgnoreCase));

        string existingClasses = ValidationHelper.GetString(existingClassesAttr?.Value, "");

        if (IfMany.Any())
        {
            var classes = new StringBuilder();

            foreach (var (Condition, Classes) in IfMany)
            {

                if (Condition)
                {
                    _ = classes.Append(Classes).Append(' ');
                }
            }

            SetClasses(output, existingClasses, classes.ToString());

            _ = output.Attributes.RemoveAll(HTMLAttributeIfMany);
        }
        if (IfElseMany.Any())
        {
            var classes = new StringBuilder();

            foreach (var (Condition, ClassesIf, ClassesIfNot) in IfElseMany)
            {

                _ = Condition
                    ? classes.Append(ClassesIf)
                    : classes.Append(ClassesIfNot);

                _ = classes.Append(' ');
            }

            SetClasses(output, existingClasses, classes.ToString());

            _ = output.Attributes.RemoveAll(HTMLAttributeIfElseMany);
        }
        else if (!string.IsNullOrWhiteSpace(IfEither.ClassesIf) && !string.IsNullOrWhiteSpace(IfEither.ClassesIfNot))
        {
            if (IfEither.Condition)
            {
                SetClasses(output, existingClasses, IfEither.ClassesIf);
            }
            else
            {
                SetClasses(output, existingClasses, IfEither.ClassesIfNot);
            }

            _ = output.Attributes.RemoveAll(HTMLAttributeIfElse);
        }
        else if (If.Condition)
        {
            SetClasses(output, existingClasses, If.Classes);
            _ = output.Attributes.RemoveAll(HTMLAttributeIf);
        }
        else if (!IfNot.Condition)
        {
            SetClasses(output, existingClasses, IfNot.Classes);
            _ = output.Attributes.RemoveAll(HTMLAttributeIfNot);
        }
    }

    private static void SetClasses(TagHelperOutput output, string existingClasses, string conditionalClasses)
    {
        if (string.IsNullOrWhiteSpace(existingClasses) && string.IsNullOrWhiteSpace(conditionalClasses))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(existingClasses))
        {
            output.Attributes.SetAttribute("class", conditionalClasses);

            return;
        }

        if (string.IsNullOrWhiteSpace(conditionalClasses))
        {
            return;
        }

        output.Attributes.SetAttribute("class", $"{existingClasses} {conditionalClasses}");
    }
}
