using System.Text;
using CMS.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XperienceCommunity.TagHelpers;

[HtmlTargetElement("*", Attributes = HTMLAttributeIf)]
[HtmlTargetElement("*", Attributes = HTMLAttributeIfNot)]
[HtmlTargetElement("*", Attributes = HTMLAttributeIfElse)]
[HtmlTargetElement("*", Attributes = HTMLAttributeIfElseMany)]
public class HTMLConditionalAttributesTagHelper : TagHelper
{
    public const string HTMLAttributeIf = "xpc-attr-if";
    public const string HTMLAttributeIfNot = "xpc-attr-if-not";
    public const string HTMLAttributeIfElse = "xpc-attr-if-else";
    public const string HTMLAttributeIfMany = "xpc-attr-if-many";
    public const string HTMLAttributeIfElseMany = "xpc-attr-if-else-many";

    [HtmlAttributeName(HTMLAttributeIf)]
    public (bool Condition, string AttributeName, string Value) If { get; set; } = (false, "", "");

    [HtmlAttributeName(HTMLAttributeIfNot)]
    public (bool Condition, string AttributeName, string Value) IfNot { get; set; } = (true, "", "");

    [HtmlAttributeName(HTMLAttributeIfElse)]
    public (bool Condition, string AttributeName, string ValueIf, string ValueIfNot) IfEither { get; set; } = (false, "", "", "");

    [HtmlAttributeName(HTMLAttributeIfMany)]
    public IEnumerable<(bool Condition, string AttributeName, string Value)> IfMany { get; set; } = Enumerable.Empty<(bool Condition, string AttributeName, string Value)>();

    [HtmlAttributeName(HTMLAttributeIfElseMany)]
    public IEnumerable<(bool Condition, string AttributeName, string ValueIf, string ValueIfNot)> IfElseMany { get; set; } = Enumerable.Empty<(bool Condition, string AttributeName, string ValueIf, string ValueIfNot)>();

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (IfMany.Any())
        {
            var lookup = new Dictionary<string, string>();

            foreach (var (Condition, AttributeName, Value) in IfMany)
            {
                if (Condition)
                {
                    if (lookup.ContainsKey(AttributeName))
                    {
                        lookup[AttributeName] += $" {Value}";
                    }
                    else
                    {
                        lookup[AttributeName] = Value;
                    }
                }
            }

            foreach (var (key, value) in lookup)
            {
                SetAttributeValue(output, key, GetAttributeValue(context, key), value);
            }

            _ = output.Attributes.RemoveAll(HTMLAttributeIfMany);
        }
        else if (IfElseMany.Any())
        {
            var lookup = new Dictionary<string, string>();

            foreach (var (Condition, AttributeName, ValueIf, ValueIfNot) in IfElseMany)
            {
                string value = Condition
                    ? ValueIf
                    : ValueIfNot;

                if (lookup.ContainsKey(AttributeName))
                {
                    lookup[AttributeName] += $" {value}";
                }
                else
                {
                    lookup[AttributeName] = value;
                }
            }

            foreach (var (key, value) in lookup)
            {
                SetAttributeValue(output, key, GetAttributeValue(context, key), value);
            }

            _ = output.Attributes.RemoveAll(HTMLAttributeIfElseMany);
        }
        else if (!string.IsNullOrWhiteSpace(IfEither.ValueIf) && !string.IsNullOrWhiteSpace(IfEither.ValueIfNot))
        {
            string existingValue = GetAttributeValue(context, IfEither.AttributeName);

            if (IfEither.Condition)
            {
                SetAttributeValue(output, IfEither.AttributeName, existingValue, IfEither.ValueIf);
            }
            else
            {
                SetAttributeValue(output, IfEither.AttributeName, existingValue, IfEither.ValueIfNot);
            }

            _ = output.Attributes.RemoveAll(HTMLAttributeIfElse);
        }
        else if (If.Condition)
        {
            SetAttributeValue(output, If.AttributeName, GetAttributeValue(context, If.AttributeName), If.Value);
            _ = output.Attributes.RemoveAll(HTMLAttributeIf);
        }
        else if (!IfNot.Condition)
        {
            SetAttributeValue(output, IfNot.AttributeName, GetAttributeValue(context, IfNot.AttributeName), IfNot.Value);
            _ = output.Attributes.RemoveAll(HTMLAttributeIfNot);
        }
    }

    private string GetAttributeValue(TagHelperContext context, string attributeName)
    {
        var existingAttr = context
            .AllAttributes
            .FirstOrDefault(a => string.Equals(a.Name, attributeName, StringComparison.OrdinalIgnoreCase));

        return ValidationHelper.GetString(existingAttr?.Value, "");
    }

    private static void SetAttributeValue(TagHelperOutput output, string attributeName, string existingValues, string conditionalValues)
    {
        if (string.IsNullOrWhiteSpace(existingValues) && string.IsNullOrWhiteSpace(conditionalValues))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(existingValues))
        {
            output.Attributes.SetAttribute(attributeName, conditionalValues);

            return;
        }

        if (string.IsNullOrWhiteSpace(conditionalValues))
        {
            return;
        }

        output.Attributes.SetAttribute(attributeName, $"{existingValues} {conditionalValues}");
    }
}
