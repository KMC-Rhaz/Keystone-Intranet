using Keystone.Intranet.Core;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;
using Microsoft.Extensions.Logging;

namespace Keystone.Intranet.Application.Users;

public sealed class UserApprovalService(
    IUserRepository users,
    ICompanyRepository companies,
    TimeProvider clock,
    ILogger<UserApprovalService> logger)
{
    public Task<IReadOnlyList<User>> ListPendingAsync(ActorContext actor, CancellationToken ct)
    {
        EnsureCanManage(actor);
        return users.ListByStatusAsync(UserStatuses.Pending, 200, ct);
    }

    public async Task<User> ApproveAsync(ApproveUserCommand command, ActorContext actor, CancellationToken ct)
    {
        EnsureCanManage(actor);
        var user = await LoadPendingAsync(command.UserId, command.ExpectedVersion, ct);
        var roles = NormalizeRoles(command.RoleKeys, actor);
        var companyIds = await NormalizeCompaniesAsync(command.CompanyIds, ct);

        user.Status = UserStatuses.Approved;
        user.RoleKeys = roles;
        user.CompanyIds = companyIds;
        await SaveAsync(user, command.ExpectedVersion, actor, ct);
        logger.LogInformation("user {UserId} approved by {ActorId} with roles {Roles}", user.Id, actor.UserId, roles);
        return user;
    }

    public async Task<User> RejectAsync(string userId, int expectedVersion, ActorContext actor, CancellationToken ct)
    {
        EnsureCanManage(actor);
        var user = await LoadPendingAsync(userId, expectedVersion, ct);
        user.Status = UserStatuses.Rejected;
        await SaveAsync(user, expectedVersion, actor, ct);
        logger.LogInformation("user {UserId} rejected by {ActorId}", user.Id, actor.UserId);
        return user;
    }

    private static void EnsureCanManage(ActorContext actor)
    {
        if (!actor.Permissions.Has(PermissionCodes.UsersManage)) throw new ForbiddenTransitionException();
    }

    private async Task<User> LoadPendingAsync(string userId, int expectedVersion, CancellationToken ct)
    {
        if (!EntityId.IsValid(userId)) throw new NotFoundException();
        var user = await users.GetByIdAsync(userId, ct) ?? throw new NotFoundException();
        if (user.Version != expectedVersion) throw new ConcurrencyConflictException();
        if (user.Status != UserStatuses.Pending) throw new ForbiddenTransitionException("error.user_not_pending");
        return user;
    }

    private static List<string> NormalizeRoles(IEnumerable<string> requested, ActorContext actor)
    {
        var roles = requested.Where(RoleKeys.All.Contains).ToHashSet();
        // only a super admin can create another super admin
        if (roles.Contains(RoleKeys.SuperAdmin) && !actor.RoleKeys.Contains(RoleKeys.SuperAdmin))
            throw new ForbiddenTransitionException("error.cannot_grant_super_admin");
        roles.Add(RoleKeys.Employee);
        return RoleKeys.All.Where(roles.Contains).ToList();
    }

    private async Task<List<string>> NormalizeCompaniesAsync(IEnumerable<string> requested, CancellationToken ct)
    {
        var known = (await companies.ListActiveAsync(ct)).Select(c => c.Id).ToHashSet();
        var selected = requested.Where(known.Contains).Distinct().ToList();
        if (selected.Count == 0) throw ValidationFailedException.For("companyIds", "validation.company_required");
        return selected;
    }

    private async Task SaveAsync(User user, int expectedVersion, ActorContext actor, CancellationToken ct)
    {
        user.Version = expectedVersion + 1;
        user.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        user.UpdatedBy = actor.UserId;
        if (!await users.TryReplaceAsync(user, expectedVersion, ct)) throw new ConcurrencyConflictException();
    }
}
