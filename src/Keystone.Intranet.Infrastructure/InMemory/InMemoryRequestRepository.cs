using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Infrastructure.InMemory;

public sealed class InMemoryRequestRepository : IRequestRepository
{
    private readonly Dictionary<string, RequestInstance> _byId = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();

    public Task InsertAsync(RequestInstance request, CancellationToken ct)
    {
        lock (_gate)
        {
            // mirrors the unique referenceNo index the database will enforce
            if (_byId.Values.Any(r => r.ReferenceNo == request.ReferenceNo)) throw new InvalidOperationException("duplicate reference");
            _byId[request.Id] = Cloner.Copy(request);
        }
        return Task.CompletedTask;
    }

    public Task<RequestInstance?> GetByIdAsync(string id, CancellationToken ct) =>
        Read(() => _byId.TryGetValue(id, out var r) ? Cloner.Copy(r) : null);

    public Task<bool> TryReplaceAsync(RequestInstance request, int expectedVersion, CancellationToken ct)
    {
        lock (_gate)
        {
            if (!_byId.TryGetValue(request.Id, out var current) || current.Version != expectedVersion)
                return Task.FromResult(false);
            _byId[request.Id] = Cloner.Copy(request);
            return Task.FromResult(true);
        }
    }

    public Task<IReadOnlyList<RequestSummary>> ListByRequesterAsync(string userId, int limit, CancellationToken ct) =>
        Summaries(r => r.Requester.UserId == userId, q => q.OrderByDescending(r => r.SubmittedAt), limit);

    public Task<IReadOnlyList<RequestSummary>> ListByRequesterKeyAndStatusAsync(
        string userId, string definitionKey, string status, CancellationToken ct) =>
        Summaries(r => r.Requester.UserId == userId && r.DefinitionKey == definitionKey && r.Status == status,
            q => q.OrderByDescending(r => r.SubmittedAt), 100);

    public Task<IReadOnlyList<RequestSummary>> ListOldestByStatusAsync(string status, int limit, CancellationToken ct) =>
        Summaries(r => r.Status == status, q => q.OrderBy(r => r.SubmittedAt), limit);

    public Task<int> CountByStatusAsync(string status, CancellationToken ct) =>
        Read(() => _byId.Values.Count(r => r.Status == status));

    public Task<IReadOnlyList<TypeCount>> CountByTypeAsync(string status, CancellationToken ct) =>
        Read<IReadOnlyList<TypeCount>>(() => _byId.Values
            .Where(r => r.Status == status)
            .GroupBy(r => (r.DefinitionKey, r.DefinitionName))
            .Select(g => new TypeCount(g.Key.DefinitionKey, g.Key.DefinitionName, g.Count()))
            .OrderByDescending(t => t.Count)
            .ToList());

    private Task<IReadOnlyList<RequestSummary>> Summaries(
        Func<RequestInstance, bool> filter,
        Func<IEnumerable<RequestInstance>, IEnumerable<RequestInstance>> order,
        int limit) =>
        Read<IReadOnlyList<RequestSummary>>(() => order(_byId.Values.Where(filter))
            .Take(limit)
            .Select(ToSummary)
            .ToList());

    private static RequestSummary ToSummary(RequestInstance r) => new(
        r.Id, r.ReferenceNo, r.DefinitionKey, r.DefinitionName, r.CompanyId,
        r.Requester.UserId, r.Requester.Name, r.Status, r.SubmittedAt, r.CompletedAt);

    private Task<T> Read<T>(Func<T> query)
    {
        lock (_gate) return Task.FromResult(query());
    }
}
