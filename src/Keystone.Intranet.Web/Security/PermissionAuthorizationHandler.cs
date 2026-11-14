using Microsoft.AspNetCore.Authorization;

namespace Keystone.Intranet.Web.Security;

public sealed class PermissionAuthorizationHandler(CurrentActor current)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (current.IsApproved && current.HasPermission(requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
