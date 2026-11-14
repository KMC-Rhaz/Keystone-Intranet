namespace Keystone.Intranet.Application.Users;

public sealed record ApproveUserCommand(
    string UserId,
    int ExpectedVersion,
    IReadOnlyCollection<string> CompanyIds,
    IReadOnlyCollection<string> RoleKeys);
