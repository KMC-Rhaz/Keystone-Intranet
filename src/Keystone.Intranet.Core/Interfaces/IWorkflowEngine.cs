using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Core.Interfaces;

// the only component allowed to change request status, step or approvers
public interface IWorkflowEngine
{
    Task<RequestInstance> SubmitAsync(SubmitCommand command, ActorContext actor, CancellationToken ct);
    Task<RequestInstance> ApproveAsync(DecisionCommand command, ActorContext actor, CancellationToken ct);
    Task<RequestInstance> RejectAsync(DecisionCommand command, ActorContext actor, CancellationToken ct);
    Task<RequestInstance> SendBackAsync(DecisionCommand command, ActorContext actor, CancellationToken ct);
    Task<RequestInstance> ResubmitAsync(ResubmitCommand command, ActorContext actor, CancellationToken ct);
    Task<RequestInstance> CancelAsync(CancelCommand command, ActorContext actor, CancellationToken ct);
}
