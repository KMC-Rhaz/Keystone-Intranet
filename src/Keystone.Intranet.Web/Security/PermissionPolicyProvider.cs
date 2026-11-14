using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Keystone.Intranet.Web.Security;

// turns any [RequirePermission("area.resource.action")] policy name into a permission requirement
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string Prefix = "perm:";

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(Prefix, StringComparison.Ordinal))
            return base.GetPolicyAsync(policyName);

        var code = policyName[Prefix.Length..];
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(code))
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
