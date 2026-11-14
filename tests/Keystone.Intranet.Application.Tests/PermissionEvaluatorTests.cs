using Keystone.Intranet.Application.Security;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Infrastructure.InMemory;
using Xunit;

namespace Keystone.Intranet.Application.Tests;

public sealed class PermissionEvaluatorTests
{
    [Fact]
    public async Task PendingUserReceivesNoPermissions()
    {
        var roles = new InMemoryRoleRepository();
        await roles.InsertIfMissingAsync(new Role { Key = RoleKeys.SuperAdmin, Permissions = ["*"], IsSystem = true }, CancellationToken.None);
        var evaluator = new PermissionEvaluator(roles);

        var set = await evaluator.ResolveAsync(new User { Status = UserStatuses.Pending, RoleKeys = [RoleKeys.SuperAdmin] }, CancellationToken.None);

        Assert.False(set.Has(PermissionCodes.RequestApprove));
    }

    [Fact]
    public async Task DeniedPermissionsOverrideWildcards()
    {
        var roles = new InMemoryRoleRepository();
        await roles.InsertIfMissingAsync(new Role { Key = RoleKeys.SuperAdmin, Permissions = [PermissionCodes.AllRequests], IsSystem = true }, CancellationToken.None);
        var evaluator = new PermissionEvaluator(roles);

        var user = new User
        {
            Status = UserStatuses.Approved,
            RoleKeys = [RoleKeys.SuperAdmin],
            DeniedPermissions = [PermissionCodes.RequestApprove]
        };
        var set = await evaluator.ResolveAsync(user, CancellationToken.None);

        Assert.True(set.Has(PermissionCodes.CashAdvanceSubmit));
        Assert.False(set.Has(PermissionCodes.RequestApprove));
    }

    [Fact]
    public async Task CustomRoleWildcardIsIgnored()
    {
        var roles = new InMemoryRoleRepository();
        await roles.InsertIfMissingAsync(new Role { Key = "custom", Permissions = [PermissionCodes.AllRequests], IsSystem = false }, CancellationToken.None);
        var evaluator = new PermissionEvaluator(roles);

        var set = await evaluator.ResolveAsync(new User { Status = UserStatuses.Approved, RoleKeys = ["custom"] }, CancellationToken.None);

        Assert.False(set.Has(PermissionCodes.CashAdvanceSubmit));
    }
}
