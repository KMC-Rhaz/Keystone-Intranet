using System.Collections.Concurrent;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;

namespace Keystone.Intranet.Infrastructure.InMemory;

public sealed class InMemoryRoleRepository : IRoleRepository
{
    private readonly ConcurrentDictionary<string, Role> _byKey = new(StringComparer.Ordinal);

    public Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct)
    {
        IReadOnlyList<Role> list = _byKey.Values.OrderBy(r => r.Key).Select(Cloner.Copy).ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<Role>> GetByKeysAsync(IEnumerable<string> keys, CancellationToken ct)
    {
        IReadOnlyList<Role> list = keys.Distinct()
            .Select(k => _byKey.TryGetValue(k, out var r) ? Cloner.Copy(r) : null)
            .OfType<Role>()
            .ToList();
        return Task.FromResult(list);
    }

    public Task<bool> InsertIfMissingAsync(Role role, CancellationToken ct) =>
        Task.FromResult(_byKey.TryAdd(role.Key, Cloner.Copy(role)));
}
