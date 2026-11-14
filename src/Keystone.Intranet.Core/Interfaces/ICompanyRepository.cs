using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Core.Interfaces;

public interface ICompanyRepository
{
    Task<IReadOnlyList<Company>> ListActiveAsync(CancellationToken ct);
    Task<Company?> GetByIdAsync(string id, CancellationToken ct);
    Task<bool> InsertIfMissingAsync(Company company, CancellationToken ct);
}
