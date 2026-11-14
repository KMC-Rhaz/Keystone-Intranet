using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;

namespace Keystone.Intranet.Infrastructure.InMemory;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _byId = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();

    public Task<User?> GetByIdAsync(string id, CancellationToken ct) =>
        Read(() => _byId.TryGetValue(id, out var u) ? Cloner.Copy(u) : null);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        Read(() => _byId.Values.Where(u => u.Email == email).Select(Cloner.Copy).FirstOrDefault());

    public Task<IReadOnlyList<User>> ListByStatusAsync(string status, int limit, CancellationToken ct) =>
        Read<IReadOnlyList<User>>(() => _byId.Values
            .Where(u => u.Status == status)
            .OrderBy(u => u.CreatedAt)
            .Take(limit)
            .Select(Cloner.Copy)
            .ToList());

    public Task<int> CountByStatusAsync(string status, CancellationToken ct) =>
        Read(() => _byId.Values.Count(u => u.Status == status));

    public Task<int> CountWithRoleAsync(string roleKey, string status, CancellationToken ct) =>
        Read(() => _byId.Values.Count(u => u.Status == status && u.RoleKeys.Contains(roleKey)));

    public Task InsertAsync(User user, CancellationToken ct)
    {
        lock (_gate)
        {
            // mirrors the unique email index the database will enforce
            if (_byId.Values.Any(u => u.Email == user.Email)) throw new InvalidOperationException("duplicate email");
            _byId[user.Id] = Cloner.Copy(user);
        }
        return Task.CompletedTask;
    }

    public Task<bool> TryReplaceAsync(User user, int expectedVersion, CancellationToken ct)
    {
        lock (_gate)
        {
            if (!_byId.TryGetValue(user.Id, out var current) || current.Version != expectedVersion)
                return Task.FromResult(false);
            _byId[user.Id] = Cloner.Copy(user);
            return Task.FromResult(true);
        }
    }

    private Task<T> Read<T>(Func<T> query)
    {
        lock (_gate) return Task.FromResult(query());
    }
}
