using System.Collections.Concurrent;
using Keystone.Intranet.Core.Interfaces;

namespace Keystone.Intranet.Infrastructure.InMemory;

public sealed class InMemoryCounterRepository : ICounterRepository
{
    private readonly ConcurrentDictionary<string, long> _counters = new(StringComparer.Ordinal);

    public Task<long> NextAsync(string scope, CancellationToken ct) =>
        Task.FromResult(_counters.AddOrUpdate(scope, 1, (_, current) => current + 1));
}
