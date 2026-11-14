namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record RequestListItemViewModel(
    string Id,
    string ReferenceNo,
    string DefinitionName,
    string Status,
    string StatusLabel,
    string SubmittedDate);
