namespace Keystone.Intranet.Core.Models;

public sealed record ActorContext(
    string UserId,
    string Name,
    PermissionSet Permissions,
    IReadOnlyCollection<string> RoleKeys,
    IReadOnlyCollection<string> CompanyIds);
