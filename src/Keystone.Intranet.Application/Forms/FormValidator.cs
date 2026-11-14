using Keystone.Intranet.Core;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Forms;

// the only authority on submitted answers; rebuilds values from known field keys so nothing else reaches storage
public sealed class FormValidator(IAttachmentStore attachments, FieldValueParser parser)
{
    public const int MaxFilesPerField = 10;

    public async Task<ValidatedForm> ValidateAsync(
        WorkflowDefinition definition,
        IReadOnlyDictionary<string, string?> input,
        string uploaderId,
        string? existingRequestId,
        CancellationToken ct)
    {
        // operator-shaped keys must never come near the database
        if (input.Keys.Any(k => k.StartsWith('$') || k.Contains('.')))
            throw new ValidationFailedException([]);

        var errors = new List<FieldError>();
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var attachmentIds = new List<string>();

        foreach (var field in definition.Form.AllFields())
        {
            var raw = input.TryGetValue(field.Key, out var v) ? v?.Trim() ?? string.Empty : string.Empty;
            var error = field.Type == FieldTypes.File
                ? await ValidateFilesAsync(field, raw, uploaderId, existingRequestId, values, attachmentIds, ct)
                : ValidateScalar(field, raw, values);
            if (error is not null) errors.Add(new FieldError(field.Key, error));
        }

        errors.AddRange(CheckDateOrder(definition, values));
        if (errors.Count > 0) throw new ValidationFailedException(errors);
        return new ValidatedForm(values, attachmentIds);
    }

    private string? ValidateScalar(FormField field, string raw, Dictionary<string, string> values)
    {
        if (raw.Length == 0 && field.Type != FieldTypes.Checkbox)
            return field.Required ? "validation.required" : null;

        var (value, error) = parser.Parse(field, raw);
        if (error is null && value is not null) values[field.Key] = value;
        return error;
    }

    private async Task<string?> ValidateFilesAsync(
        FormField field, string raw, string uploaderId, string? existingRequestId,
        Dictionary<string, string> values, List<string> attachmentIds, CancellationToken ct)
    {
        var ids = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct().ToList();
        if (ids.Count == 0) return field.Required ? "validation.file_required" : null;
        if (ids.Count > MaxFilesPerField || ids.Any(id => !EntityId.IsValid(id))) return "validation.file";

        // a submission may only use files the same person uploaded and that no other request has claimed
        var metas = await attachments.GetMetaManyAsync(ids, ct);
        var allOwned = metas.Count == ids.Count && metas.All(m =>
            m.OwnerUserId == uploaderId && (m.RequestId is null || m.RequestId == existingRequestId));
        if (!allOwned) return "validation.file";

        values[field.Key] = string.Join(',', ids);
        attachmentIds.AddRange(ids);
        return null;
    }

    private static IEnumerable<FieldError> CheckDateOrder(WorkflowDefinition definition, Dictionary<string, string> values)
    {
        foreach (var field in definition.Form.AllFields().Where(f => f.Type == FieldTypes.Date && f.MinFieldRef is not null))
        {
            if (!values.TryGetValue(field.Key, out var raw) || !values.TryGetValue(field.MinFieldRef!, out var minRaw)) continue;
            if (FieldValueParser.TryParseDate(raw, out var date) && FieldValueParser.TryParseDate(minRaw, out var min) && date < min)
                yield return new FieldError(field.Key, "validation.date_before");
        }
    }
}
