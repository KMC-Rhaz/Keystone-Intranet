using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Workflow;

// shared persistence steps for the engine: version-guarded saves and history rows
public sealed class RequestWriter(
    IRequestRepository requests,
    IRequestHistoryRepository history,
    ICounterRepository counters,
    TimeProvider clock)
{
    public DateTime Now() => clock.GetUtcNow().UtcDateTime;

    public async Task<string> NextReferenceAsync(string prefix, DateTime now, CancellationToken ct)
    {
        var scope = $"{prefix}-{now.Year}";
        var next = await counters.NextAsync(scope, ct);
        return $"{scope}-{next:D6}";
    }

    public async Task SaveAsync(RequestInstance request, int expectedVersion, ActorContext actor, DateTime now, CancellationToken ct)
    {
        request.Version = expectedVersion + 1;
        request.UpdatedAt = now;
        request.UpdatedBy = actor.UserId;
        // the version filter runs before history so a lost race never leaves a phantom history row
        if (!await requests.TryReplaceAsync(request, expectedVersion, ct)) throw new ConcurrencyConflictException();
    }

    public Task AppendHistoryAsync(
        RequestInstance request, string action, string? fromStep, string? toStep,
        ActorContext actor, string? comment, DateTime now, CancellationToken ct) =>
        history.AppendAsync(new RequestHistoryEntry
        {
            RequestId = request.Id,
            ReferenceNo = request.ReferenceNo,
            DefinitionName = request.DefinitionName,
            Action = action,
            FromStepKey = fromStep,
            ToStepKey = toStep,
            Actor = new PersonSnapshot { UserId = actor.UserId, Name = actor.Name },
            Comment = comment,
            At = now
        }, ct);

    public void ApplyTarget(RequestInstance request, WorkflowDefinition definition, string target, DateTime now)
    {
        if (RequestStatuses.Terminal.Contains(target))
        {
            request.Status = target;
            request.CurrentStepKey = null;
            request.DueAt = null;
            request.CompletedAt = now;
            return;
        }

        var next = definition.Steps.First(s => s.Key == target);
        EnterStep(request, next, now);
    }

    public void EnterStep(RequestInstance request, WorkflowStep step, DateTime now)
    {
        request.Status = RequestStatuses.Pending;
        request.CurrentStepKey = step.Key;
        request.DueAt = step.SlaHours is { } hours ? now.AddHours(hours) : null;
    }
}
