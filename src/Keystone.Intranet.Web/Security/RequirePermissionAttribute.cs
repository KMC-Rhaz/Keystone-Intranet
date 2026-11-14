using Microsoft.AspNetCore.Authorization;

namespace Keystone.Intranet.Web.Security;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = PermissionPolicyProvider.Prefix + permission;
    }
}
