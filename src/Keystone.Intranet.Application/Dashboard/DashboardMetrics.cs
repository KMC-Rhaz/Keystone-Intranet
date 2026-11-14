using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Dashboard;

public sealed record DashboardMetrics(
    int PendingRequests,
    int PendingUsers,
    int ReturnedRequests,
    IReadOnlyList<TypeCount> PendingByType,
    IReadOnlyList<WaitingRequest> LongestWaiting);
