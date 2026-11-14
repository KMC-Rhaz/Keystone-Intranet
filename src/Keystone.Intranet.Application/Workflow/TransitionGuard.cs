using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Workflow;

// pure checks that decide whether a transition is allowed; no storage access here
public sealed class TransitionGuard
{
    public WorkflowStep FirstStep(WorkflowDefinition definition) =>
        definition.Steps.OrderBy(s => s.Order).FirstOrDefault() ?? throw new ForbiddenTransitionException();

    public void EnsureVersion(RequestInstance request, int expectedVersion)
    {
        if (request.Version != expectedVersion) throw new ConcurrencyConflictException();
    }

    public WorkflowStep EnsureCanDecide(RequestInstance request, WorkflowDefinition definition, ActorContext actor)
    {
        if (request.Status != RequestStatuses.Pending) throw new ForbiddenTransitionException("error.not_pending");

        // block self-approval so requesters can never clear their own request
        if (request.Requester.UserId == actor.UserId) throw new ForbiddenTransitionException("error.self_approval");

        var step = definition.Steps.FirstOrDefault(s => s.Key == request.CurrentStepKey)
            ?? throw new ForbiddenTransitionException();
        var stepPermission = string.IsNullOrEmpty(step.ApprovePermission) ? PermissionCodes.RequestApprove : step.ApprovePermission;
        if (!actor.Permissions.Has(PermissionCodes.RequestApprove) || !actor.Permissions.Has(stepPermission))
            throw new ForbiddenTransitionException();
        if (!MatchesApprover(step.Approver, actor)) throw new ForbiddenTransitionException();
        return step;
    }

    public void EnsureRequester(RequestInstance request, ActorContext actor)
    {
        // same error as a missing record so other people's ids reveal nothing
        if (request.Requester.UserId != actor.UserId) throw new NotFoundException();
    }

    public bool MatchesApprover(ApproverRule rule, ActorContext actor) => rule.Type switch
    {
        ApproverTypes.Role => rule.Value is not null && actor.RoleKeys.Contains(rule.Value),
        ApproverTypes.User => rule.Value == actor.UserId,
        _ => false
    };

    public string? RequireComment(string? comment)
    {
        var trimmed = comment?.Trim();
        if (string.IsNullOrEmpty(trimmed)) throw ValidationFailedException.For("comment", "validation.comment_required");
        if (trimmed.Length > 1000) throw ValidationFailedException.For("comment", "validation.too_long");
        return trimmed;
    }
}
