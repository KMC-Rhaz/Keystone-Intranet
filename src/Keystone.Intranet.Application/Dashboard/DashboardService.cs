using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Dashboard;

public sealed class DashboardService(
    IRequestRepository requests,
    IUserRepository users,
    IRequestHistoryRepository history,
    TimeProvider clock)
{
    public const int WaitingListSize = 10;
    public const int FeedSize = 20;

    public async Task<DashboardMetrics> GetMetricsAsync(ActorContext actor, CancellationToken ct)
    {
        EnsureCanView(actor);
        var now = clock.GetUtcNow().UtcDateTime;
        var oldest = await requests.ListOldestByStatusAsync(RequestStatuses.Pending, WaitingListSize, ct);

        return new DashboardMetrics(
            await requests.CountByStatusAsync(RequestStatuses.Pending, ct),
            await users.CountByStatusAsync(UserStatuses.Pending, ct),
            await requests.CountByStatusAsync(RequestStatuses.Returned, ct),
            await requests.CountByTypeAsync(RequestStatuses.Pending, ct),
            oldest.Select(r => new WaitingRequest(
                r.Id, r.ReferenceNo, r.DefinitionName, r.RequesterName,
                (int)Math.Max(0, (now - r.SubmittedAt).TotalHours))).ToList());
    }

    public Task<IReadOnlyList<RequestHistoryEntry>> GetActivityAsync(ActorContext actor, CancellationToken ct)
    {
        EnsureCanView(actor);
        return history.ListRecentAsync(FeedSize, ct);
    }

    private static void EnsureCanView(ActorContext actor)
    {
        if (!actor.Permissions.Has(PermissionCodes.DashboardView)) throw new ForbiddenTransitionException();
    }
}
