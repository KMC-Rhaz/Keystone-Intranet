namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record ApproveUserInput(
    string UserId,
    int ExpectedVersion,
    IReadOnlyCollection<string>? CompanyIds,
    IReadOnlyCollection<string>? RoleKeys);
