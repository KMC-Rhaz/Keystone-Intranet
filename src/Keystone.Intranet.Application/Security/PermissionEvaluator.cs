using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Security;

public sealed class PermissionEvaluator(IRoleRepository roles)
{
    public async Task<PermissionSet> ResolveAsync(User user, CancellationToken ct)
    {
        // pending, rejected and deactivated users hold no permissions at all
        if (user.Status != UserStatuses.Approved) return PermissionSet.Empty;

        var granted = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in await roles.GetByKeysAsync(user.RoleKeys, ct))
        {
            // wildcards are honored only on system roles so a custom role cannot grant everything
            granted.UnionWith(role.Permissions.Where(p => role.IsSystem || !IsWildcard(p)));
        }

        granted.UnionWith(user.ExtraPermissions.Where(p => !IsWildcard(p)));
        return new PermissionSet(granted, user.DeniedPermissions);
    }

    private static bool IsWildcard(string code) => code.Contains(PermissionCodes.Wildcard);
}
