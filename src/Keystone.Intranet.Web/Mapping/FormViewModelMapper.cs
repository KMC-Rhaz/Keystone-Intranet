using System.Globalization;
using Keystone.Intranet.Application.Forms;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Web.Areas.Hub.ViewModels;
using Keystone.Intranet.Web.Localization;

namespace Keystone.Intranet.Web.Mapping;

// maps definitions and answers to browser-safe view models without workflow rules or permissions
public sealed class FormViewModelMapper(PlaceholderResolver placeholders, ValueFormatter formatter, Translator translator)
{
    public FormViewModel BuildForm(
        WorkflowDefinition definition,
        IReadOnlyDictionary<string, string> tokens,
        string submitUrl,
        string submitLabel,
        string? linkedRequestId = null,
        RequestInstance? existing = null,
        IReadOnlyList<StoredFile>? files = null)
    {
        var sections = definition.Form.Sections.Select(s => new FormSectionViewModel(
            s.Key,
            placeholders.Resolve(s.Title, tokens),
            s.Intro is null ? null : placeholders.Resolve(s.Intro, tokens),
            s.Fields.Select(f => new FormFieldViewModel(
                f.Key,
                placeholders.Resolve(f.Label, tokens),
                f.Type,
                f.Required,
                f.HelpText is null ? null : placeholders.Resolve(f.HelpText, tokens),
                f.MaxLength,
                f.Min,
                f.Max,
                f.MinFieldRef,
                f.Options.Select(o => new FormOptionViewModel(o.Value, o.Label)).ToList())).ToList())).ToList();

        return new FormViewModel(
            definition.Key,
            definition.Name,
            definition.Description,
            linkedRequestId,
            existing?.Id,
            existing?.Version,
            submitUrl,
            submitLabel,
            sections,
            existing?.Values ?? new Dictionary<string, string>(),
            (files ?? []).Select(f => new AttachmentViewModel(f.Id, f.OriginalName, f.Length)).ToList());
    }

    public IReadOnlyList<AnswerViewModel> FormatAnswers(
        RequestInstance request,
        WorkflowDefinition? definition,
        IReadOnlyDictionary<string, string> tokens)
    {
        if (definition is null)
            return request.Values.Select(kv => new AnswerViewModel(kv.Key, kv.Value)).ToList();

        var answers = new List<AnswerViewModel>();
        foreach (var field in definition.Form.AllFields().Where(f => f.Type != FieldTypes.File))
        {
            request.Values.TryGetValue(field.Key, out var raw);
            var label = placeholders.Resolve(field.Label, tokens);
            answers.Add(new AnswerViewModel(label, formatter.Format(field, raw)));
        }
        return answers;
    }

    public RequestDetailViewModel BuildDetail(
        RequestInstance request,
        WorkflowDefinition? definition,
        IReadOnlyDictionary<string, string> tokens,
        IReadOnlyList<StoredFile> files,
        IReadOnlyList<RequestHistoryEntry> history,
        string currentUserId,
        string language)
    {
        var isOwner = request.Requester.UserId == currentUserId;
        var canEdit = isOwner && request.Status == RequestStatuses.Returned;
        var canCancel = isOwner && request.Status is RequestStatuses.Pending or RequestStatuses.Returned;

        return new RequestDetailViewModel(
            request.Id,
            request.ReferenceNo,
            request.DefinitionName,
            request.Requester.Name,
            request.Status,
            translator.StatusLabel(request.Status, language),
            FormatLocal(request.SubmittedAt),
            request.Version,
            canEdit,
            canCancel,
            FormatAnswers(request, definition, tokens),
            files.Select(f => new AttachmentViewModel(f.Id, f.OriginalName, f.Length)).ToList(),
            history.Select(h => new HistoryItemViewModel(
                h.Action, h.Actor.Name, h.Comment, FormatLocal(h.At))).ToList());
    }

    public static string FormatLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), ManilaZone())
            .ToString("d MMM yyyy, h:mm tt", CultureInfo.InvariantCulture);

    private static TimeZoneInfo ManilaZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"); }
    }
}
