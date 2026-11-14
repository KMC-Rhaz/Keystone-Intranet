using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;

namespace Keystone.Intranet.Infrastructure.InMemory;

public sealed class InMemoryRequestHistoryRepository : IRequestHistoryRepository
{
    private readonly List<RequestHistoryEntry> _entries = [];
    private readonly Lock _gate = new();

    public Task AppendAsync(RequestHistoryEntry entry, CancellationToken ct)
    {
        lock (_gate) _entries.Add(Cloner.Copy(entry));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RequestHistoryEntry>> ListByRequestAsync(string requestId, CancellationToken ct)
    {
        lock (_gate)
        {
            IReadOnlyList<RequestHistoryEntry> list = _entries.Where(e => e.RequestId == requestId)
                .OrderBy(e => e.At).Select(Cloner.Copy).ToList();
            return Task.FromResult(list);
        }
    }

    public Task<IReadOnlyList<RequestHistoryEntry>> ListRecentAsync(int limit, CancellationToken ct)
    {
        lock (_gate)
        {
            IReadOnlyList<RequestHistoryEntry> list = _entries.OrderByDescending(e => e.At)
                .Take(limit).Select(Cloner.Copy).ToList();
            return Task.FromResult(list);
        }
    }
}
