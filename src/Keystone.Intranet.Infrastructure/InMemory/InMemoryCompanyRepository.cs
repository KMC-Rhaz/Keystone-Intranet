using System.Collections.Concurrent;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;

namespace Keystone.Intranet.Infrastructure.InMemory;

public sealed class InMemoryCompanyRepository : ICompanyRepository
{
    private readonly ConcurrentDictionary<string, Company> _byKey = new(StringComparer.Ordinal);

    public Task<IReadOnlyList<Company>> ListActiveAsync(CancellationToken ct)
    {
        IReadOnlyList<Company> list = _byKey.Values.Where(c => c.IsActive).OrderBy(c => c.SortOrder).Select(Cloner.Copy).ToList();
        return Task.FromResult(list);
    }

    public Task<Company?> GetByIdAsync(string id, CancellationToken ct)
    {
        var match = _byKey.Values.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(match is null ? null : Cloner.Copy(match));
    }

    public Task<bool> InsertIfMissingAsync(Company company, CancellationToken ct) =>
        Task.FromResult(_byKey.TryAdd(company.Key, Cloner.Copy(company)));
}
