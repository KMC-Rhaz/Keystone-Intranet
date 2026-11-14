namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record ApprovalListItemViewModel(
    string Id,
    string ReferenceNo,
    string DefinitionName,
    string RequesterName,
    string CompanyName,
    string SubmittedDate);
