namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record ActivityItemViewModel(
    string RequestId,
    string ReferenceNo,
    string DefinitionName,
    string Action,
    string ActorName,
    string? Comment,
    string At);
