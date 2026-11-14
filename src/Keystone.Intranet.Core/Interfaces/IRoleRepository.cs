using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Core.Interfaces;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<Role>> GetByKeysAsync(IEnumerable<string> keys, CancellationToken ct);
    Task<bool> InsertIfMissingAsync(Role role, CancellationToken ct);
}
