using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Core.Interfaces;

public interface IWorkflowDefinitionRepository
{
    Task<WorkflowDefinition?> GetActiveByKeyAsync(string key, CancellationToken ct);
    Task<IReadOnlyList<WorkflowDefinition>> ListActiveAsync(CancellationToken ct);
    Task<bool> InsertIfMissingAsync(WorkflowDefinition definition, CancellationToken ct);
}
