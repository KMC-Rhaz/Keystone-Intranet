namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record RequestDetailViewModel(
    string Id,
    string ReferenceNo,
    string DefinitionName,
    string RequesterName,
    string Status,
    string StatusLabel,
    string SubmittedAt,
    int Version,
    bool CanEdit,
    bool CanCancel,
    IReadOnlyList<AnswerViewModel> Answers,
    IReadOnlyList<AttachmentViewModel> Attachments,
    IReadOnlyList<HistoryItemViewModel> History);
