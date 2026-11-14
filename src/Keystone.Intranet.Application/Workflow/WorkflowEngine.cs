using Keystone.Intranet.Application.Forms;
using Keystone.Intranet.Core;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Workflow;

public sealed class WorkflowEngine(
    IWorkflowDefinitionRepository definitions,
    IRequestRepository requests,
    IAttachmentStore attachments,
    FormValidator validator,
    TransitionGuard guard,
    RequestWriter writer) : IWorkflowEngine
{
    public async Task<RequestInstance> SubmitAsync(SubmitCommand command, ActorContext actor, CancellationToken ct)
    {
        var definition = await definitions.GetActiveByKeyAsync(command.DefinitionKey, ct) ?? throw new NotFoundException();
        if (!actor.Permissions.Has(definition.SubmitPermission)) throw new ForbiddenTransitionException("error.cannot_submit");
        if (!actor.CompanyIds.Contains(command.CompanyId) || !InScope(definition, command.CompanyId))
            throw new ForbiddenTransitionException("error.company");

        var linkedId = await ResolveLinkAsync(definition, command.LinkedRequestId, actor, ct);
        var form = await validator.ValidateAsync(definition, command.Values, actor.UserId, null, ct);
        var now = writer.Now();
        var firstStep = guard.FirstStep(definition);

        var request = new RequestInstance
        {
            ReferenceNo = await writer.NextReferenceAsync(definition.ReferencePrefix, now, ct),
            DefinitionKey = definition.Key,
            DefinitionName = definition.Name,
            DefinitionVersion = definition.Version,
            CompanyId = command.CompanyId,
            Requester = new PersonSnapshot { UserId = actor.UserId, Name = actor.Name },
            Values = form.Values,
            AttachmentIds = form.AttachmentIds,
            LinkedRequestId = linkedId,
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actor.UserId,
            UpdatedBy = actor.UserId
        };
        writer.EnterStep(request, firstStep, now);

        await requests.InsertAsync(request, ct);
        await attachments.AssignToRequestAsync(form.AttachmentIds, request.Id, ct);
        await writer.AppendHistoryAsync(request, HistoryActions.Submitted, null, firstStep.Key, actor, null, now, ct);
        return request;
    }

    public Task<RequestInstance> ApproveAsync(DecisionCommand command, ActorContext actor, CancellationToken ct) =>
        DecideAsync(command, actor, HistoryActions.Approved, ct);

    public Task<RequestInstance> RejectAsync(DecisionCommand command, ActorContext actor, CancellationToken ct) =>
        DecideAsync(command, actor, HistoryActions.Rejected, ct);

    public Task<RequestInstance> SendBackAsync(DecisionCommand command, ActorContext actor, CancellationToken ct) =>
        DecideAsync(command, actor, HistoryActions.Returned, ct);

    public async Task<RequestInstance> ResubmitAsync(ResubmitCommand command, ActorContext actor, CancellationToken ct)
    {
        var (request, definition) = await LoadAsync(command.RequestId, ct);
        guard.EnsureRequester(request, actor);
        guard.EnsureVersion(request, command.ExpectedVersion);
        if (request.Status != RequestStatuses.Returned) throw new ForbiddenTransitionException("error.not_returned");

        var form = await validator.ValidateAsync(definition, command.Values, actor.UserId, request.Id, ct);
        var now = writer.Now();
        var step = definition.Steps.FirstOrDefault(s => s.Key == request.CurrentStepKey) ?? guard.FirstStep(definition);
        request.Values = form.Values;
        request.AttachmentIds = form.AttachmentIds;
        writer.EnterStep(request, step, now);

        await writer.SaveAsync(request, command.ExpectedVersion, actor, now, ct);
        await attachments.AssignToRequestAsync(form.AttachmentIds, request.Id, ct);
        await writer.AppendHistoryAsync(request, HistoryActions.Resubmitted, step.Key, step.Key, actor, null, now, ct);
        return request;
    }

    public async Task<RequestInstance> CancelAsync(CancelCommand command, ActorContext actor, CancellationToken ct)
    {
        var (request, _) = await LoadAsync(command.RequestId, ct);
        guard.EnsureRequester(request, actor);
        guard.EnsureVersion(request, command.ExpectedVersion);
        if (request.Status is not (RequestStatuses.Pending or RequestStatuses.Returned))
            throw new ForbiddenTransitionException("error.cannot_cancel");

        var now = writer.Now();
        var fromStep = request.CurrentStepKey;
        request.Status = RequestStatuses.Cancelled;
        request.CurrentStepKey = null;
        request.DueAt = null;
        request.CompletedAt = now;

        await writer.SaveAsync(request, command.ExpectedVersion, actor, now, ct);
        await writer.AppendHistoryAsync(request, HistoryActions.Cancelled, fromStep, null, actor, null, now, ct);
        return request;
    }

    private async Task<RequestInstance> DecideAsync(DecisionCommand command, ActorContext actor, string action, CancellationToken ct)
    {
        var (request, definition) = await LoadAsync(command.RequestId, ct);
        guard.EnsureVersion(request, command.ExpectedVersion);
        var step = guard.EnsureCanDecide(request, definition, actor);
        var comment = action == HistoryActions.Approved ? Trim(command.Comment) : guard.RequireComment(command.Comment);
        var now = writer.Now();

        if (action == HistoryActions.Approved) writer.ApplyTarget(request, definition, step.OnApprove, now);
        else if (action == HistoryActions.Rejected) writer.ApplyTarget(request, definition, step.OnReject, now);
        // a returned request keeps its step so the resubmission lands back with the same reviewers
        else request.Status = RequestStatuses.Returned;

        await writer.SaveAsync(request, command.ExpectedVersion, actor, now, ct);
        await writer.AppendHistoryAsync(request, action, step.Key, request.CurrentStepKey, actor, comment, now, ct);
        return request;
    }

    private async Task<(RequestInstance, WorkflowDefinition)> LoadAsync(string requestId, CancellationToken ct)
    {
        if (!EntityId.IsValid(requestId)) throw new NotFoundException();
        var request = await requests.GetByIdAsync(requestId, ct) ?? throw new NotFoundException();
        // todo(KS-11): load the definition matching definitionVersion once versioned definitions exist
        var definition = await definitions.GetActiveByKeyAsync(request.DefinitionKey, ct) ?? throw new NotFoundException();
        return (request, definition);
    }

    private async Task<string?> ResolveLinkAsync(WorkflowDefinition definition, string? linkedId, ActorContext actor, CancellationToken ct)
    {
        if (definition.LinkedDefinitionKey is null) return null;
        if (!EntityId.IsValid(linkedId)) throw ValidationFailedException.For("linkedRequestId", "validation.linked_required");

        var linked = await requests.GetByIdAsync(linkedId!, ct);
        var usable = linked is not null
            && linked.Requester.UserId == actor.UserId
            && linked.DefinitionKey == definition.LinkedDefinitionKey
            && linked.Status == RequestStatuses.Approved;
        if (!usable) throw new NotFoundException();
        return linked!.Id;
    }

    private static bool InScope(WorkflowDefinition definition, string companyId) =>
        definition.CompanyScope.Contains("*") || definition.CompanyScope.Contains(companyId);

    private static string? Trim(string? text)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        return trimmed.Length > 1000 ? trimmed[..1000] : trimmed;
    }
}
