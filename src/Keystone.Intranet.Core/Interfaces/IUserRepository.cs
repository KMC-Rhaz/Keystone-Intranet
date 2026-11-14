using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<User>> ListByStatusAsync(string status, int limit, CancellationToken ct);
    Task<int> CountByStatusAsync(string status, CancellationToken ct);
    Task<int> CountWithRoleAsync(string roleKey, string status, CancellationToken ct);
    Task InsertAsync(User user, CancellationToken ct);
    Task<bool> TryReplaceAsync(User user, int expectedVersion, CancellationToken ct);
}
