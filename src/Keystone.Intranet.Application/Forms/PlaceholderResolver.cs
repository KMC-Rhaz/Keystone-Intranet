using System.Text.RegularExpressions;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Application.Forms;

// fills {{TOKEN}} text from a linked request; unknown tokens stay visible so typos are easy to spot
public sealed partial class PlaceholderResolver(ValueFormatter formatter)
{
    public const string ReferenceToken = "REFERENCE";
    public const string RequesterToken = "REQUESTER";

    public string Resolve(string? text, IReadOnlyDictionary<string, string> tokens)
    {
        if (string.IsNullOrEmpty(text) || tokens.Count == 0) return text ?? string.Empty;
        return TokenPattern().Replace(text, m => tokens.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);
    }

    public IReadOnlyDictionary<string, string> BuildTokens(RequestInstance linked, WorkflowDefinition linkedDefinition)
    {
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ReferenceToken] = linked.ReferenceNo,
            [RequesterToken] = linked.Requester.Name
        };

        foreach (var field in linkedDefinition.Form.AllFields().Where(f => f.Type != FieldTypes.File))
        {
            linked.Values.TryGetValue(field.Key, out var raw);
            tokens[field.Key] = formatter.Format(field, raw);
        }

        return tokens;
    }

    [GeneratedRegex(@"\{\{\s*([A-Za-z0-9_]+)\s*\}\}")]
    private static partial Regex TokenPattern();
}
