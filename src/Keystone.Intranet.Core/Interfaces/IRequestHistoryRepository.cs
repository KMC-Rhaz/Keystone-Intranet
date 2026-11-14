using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Core.Interfaces;

// append-only: there is deliberately no update or delete method
public interface IRequestHistoryRepository
{
    Task AppendAsync(RequestHistoryEntry entry, CancellationToken ct);
    Task<IReadOnlyList<RequestHistoryEntry>> ListByRequestAsync(string requestId, CancellationToken ct);
    Task<IReadOnlyList<RequestHistoryEntry>> ListRecentAsync(int limit, CancellationToken ct);
}
