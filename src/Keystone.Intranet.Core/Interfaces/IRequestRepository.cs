using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Core.Interfaces;

public interface IRequestRepository
{
    Task InsertAsync(RequestInstance request, CancellationToken ct);
    Task<RequestInstance?> GetByIdAsync(string id, CancellationToken ct);
    Task<bool> TryReplaceAsync(RequestInstance request, int expectedVersion, CancellationToken ct);
    Task<IReadOnlyList<RequestSummary>> ListByRequesterAsync(string userId, int limit, CancellationToken ct);
    Task<IReadOnlyList<RequestSummary>> ListByRequesterKeyAndStatusAsync(
        string userId, string definitionKey, string status, CancellationToken ct);
    Task<IReadOnlyList<RequestSummary>> ListOldestByStatusAsync(string status, int limit, CancellationToken ct);
    Task<int> CountByStatusAsync(string status, CancellationToken ct);
    Task<IReadOnlyList<TypeCount>> CountByTypeAsync(string status, CancellationToken ct);
}
