namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record FormViewModel(
    string DefinitionKey,
    string Name,
    string Description,
    string? LinkedRequestId,
    string? ResubmitRequestId,
    int? ExpectedVersion,
    string SubmitUrl,
    string SubmitLabel,
    IReadOnlyList<FormSectionViewModel> Sections,
    IReadOnlyDictionary<string, string> InitialValues,
    IReadOnlyList<AttachmentViewModel> InitialFiles);
