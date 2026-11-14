using System.Collections.Concurrent;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;

namespace Keystone.Intranet.Infrastructure.InMemory;

public sealed class InMemoryWorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _byKey = new(StringComparer.Ordinal);

    public Task<WorkflowDefinition?> GetActiveByKeyAsync(string key, CancellationToken ct)
    {
        var found = _byKey.TryGetValue(key, out var d) && d.IsActive ? Cloner.Copy(d) : null;
        return Task.FromResult(found);
    }

    public Task<IReadOnlyList<WorkflowDefinition>> ListActiveAsync(CancellationToken ct)
    {
        IReadOnlyList<WorkflowDefinition> list = _byKey.Values
            .Where(d => d.IsActive)
            .OrderBy(d => d.SortOrder)
            .Select(Cloner.Copy)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<bool> InsertIfMissingAsync(WorkflowDefinition definition, CancellationToken ct) =>
        Task.FromResult(_byKey.TryAdd(definition.Key, Cloner.Copy(definition)));
}
