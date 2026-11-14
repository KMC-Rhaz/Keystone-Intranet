using System.Text.RegularExpressions;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Application.Forms;

// rejects broken definitions when they are published or seeded, never at submit time
public sealed partial class DefinitionValidator
{
    public IReadOnlyList<string> Validate(WorkflowDefinition definition, IReadOnlySet<string> knownKeys)
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(definition.Key)) problems.Add("key is missing");
        if (string.IsNullOrWhiteSpace(definition.ReferencePrefix)) problems.Add("referencePrefix is missing");
        if (string.IsNullOrWhiteSpace(definition.SubmitPermission)) problems.Add("submitPermission is missing");
        if (definition.LinkedDefinitionKey is { } linked && !knownKeys.Contains(linked))
            problems.Add($"linkedDefinitionKey '{linked}' does not exist");

        problems.AddRange(ValidateFields(definition));
        problems.AddRange(ValidateSteps(definition));
        return problems.Select(p => $"{definition.Key}: {p}").ToList();
    }

    private static IEnumerable<string> ValidateFields(WorkflowDefinition definition)
    {
        var fields = definition.Form.AllFields().ToList();
        if (fields.Count == 0) yield return "form has no fields";

        foreach (var duplicate in fields.GroupBy(f => f.Key).Where(g => g.Count() > 1))
            yield return $"field key '{duplicate.Key}' is used twice";

        foreach (var field in fields)
        {
            if (!FieldKeyPattern().IsMatch(field.Key)) yield return $"field key '{field.Key}' must be letters and digits";
            if (!FieldTypes.All.Contains(field.Type)) yield return $"field '{field.Key}' has unknown type '{field.Type}'";
            if (field.Type == FieldTypes.Select && field.Options.Count == 0) yield return $"field '{field.Key}' has no options";
            if (field.MinFieldRef is { } r && fields.All(f => f.Key != r)) yield return $"field '{field.Key}' refers to missing '{r}'";
        }
    }

    private static IEnumerable<string> ValidateSteps(WorkflowDefinition definition)
    {
        if (definition.Steps.Count == 0) yield return "workflow has no steps";
        var stepKeys = definition.Steps.Select(s => s.Key).ToHashSet();
        if (stepKeys.Count != definition.Steps.Count) yield return "step keys must be unique";

        foreach (var step in definition.Steps)
        {
            if (!ApproverTypes.Supported.Contains(step.Approver.Type))
                yield return $"step '{step.Key}' has unsupported approver type '{step.Approver.Type}'";
            if (string.IsNullOrWhiteSpace(step.Approver.Value)) yield return $"step '{step.Key}' approver has no value";
            if (!IsTarget(step.OnApprove, stepKeys)) yield return $"step '{step.Key}' onApprove target is unknown";
            if (!IsTarget(step.OnReject, stepKeys)) yield return $"step '{step.Key}' onReject target is unknown";
        }
    }

    private static bool IsTarget(string target, HashSet<string> stepKeys) =>
        RequestStatuses.Terminal.Contains(target) || stepKeys.Contains(target);

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9]*$")]
    private static partial Regex FieldKeyPattern();
}
