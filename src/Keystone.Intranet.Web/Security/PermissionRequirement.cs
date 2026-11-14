using Microsoft.AspNetCore.Authorization;

namespace Keystone.Intranet.Web.Security;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
