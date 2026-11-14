using Keystone.Intranet.Application.Forms;
using Keystone.Intranet.Application.Workflow;
using Keystone.Intranet.Core;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Models;
using Keystone.Intranet.Infrastructure.InMemory;
using Keystone.Intranet.Infrastructure.Seeds;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Keystone.Intranet.Application.Tests;

public sealed class WorkflowEngineTests
{
    private readonly InMemoryWorkflowDefinitionRepository _defs = new();
    private readonly InMemoryRequestRepository _requests = new();
    private readonly InMemoryRequestHistoryRepository _history = new();
    private readonly WorkflowEngine _engine;
    private readonly string _companyId = EntityId.New();

    public WorkflowEngineTests()
    {
        var companies = new InMemoryCompanyRepository();
        var roles = new InMemoryRoleRepository();
        var runner = new SeedRunner(companies, roles, _defs, TimeProvider.System, NullLogger<SeedRunner>.Instance);
        runner.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

        var attachments = new InMemoryAttachmentStore();
        var validator = new FormValidator(attachments, new FieldValueParser());
        var writer = new RequestWriter(_requests, _history, new InMemoryCounterRepository(), TimeProvider.System);
        _engine = new WorkflowEngine(_defs, _requests, attachments, validator, new TransitionGuard(), writer);
    }

    [Fact]
    public async Task BlocksSelfApproval()
    {
        var admin = MakeActor("u1", RoleKeys.SuperAdmin, PermissionCodes.CashAdvanceSubmit, PermissionCodes.RequestApprove);
        var created = await SubmitCashAdvanceAsync(admin);

        await Assert.ThrowsAsync<ForbiddenTransitionException>(() =>
            _engine.ApproveAsync(new DecisionCommand(created.Id, created.Version, null), admin, CancellationToken.None));
    }

    [Fact]
    public async Task ApproveWritesHistoryAndDetectsStaleVersion()
    {
        var requester = MakeActor("u1", RoleKeys.Employee, PermissionCodes.CashAdvanceSubmit);
        var approver = MakeActor("u2", RoleKeys.SuperAdmin, PermissionCodes.RequestApprove);
        var created = await SubmitCashAdvanceAsync(requester);

        var approved = await _engine.ApproveAsync(new DecisionCommand(created.Id, created.Version, "ok"), approver, CancellationToken.None);
        Assert.Equal(RequestStatuses.Approved, approved.Status);

        var history = await _history.ListByRequestAsync(created.Id, CancellationToken.None);
        Assert.Equal(2, history.Count);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
            _engine.RejectAsync(new DecisionCommand(created.Id, created.Version, "late"), approver, CancellationToken.None));
    }

    [Fact]
    public async Task SendBackAndResubmitReturnsToPending()
    {
        var requester = MakeActor("u1", RoleKeys.Employee, PermissionCodes.CashAdvanceSubmit);
        var approver = MakeActor("u2", RoleKeys.SuperAdmin, PermissionCodes.RequestApprove);
        var created = await SubmitCashAdvanceAsync(requester);

        var returned = await _engine.SendBackAsync(new DecisionCommand(created.Id, created.Version, "add details"), approver, CancellationToken.None);
        Assert.Equal(RequestStatuses.Returned, returned.Status);

        var resubmitted = await _engine.ResubmitAsync(
            new ResubmitCommand(returned.Id, returned.Version, new Dictionary<string, string?>
            {
                ["purpose"] = "Updated purpose",
                ["amount"] = "500",
                ["dateNeeded"] = "2026-11-25"
            }), requester, CancellationToken.None);
        Assert.Equal(RequestStatuses.Pending, resubmitted.Status);
    }

    private Task<Core.Entities.RequestInstance> SubmitCashAdvanceAsync(ActorContext actor) =>
        _engine.SubmitAsync(new SubmitCommand(CashAdvanceDefinition.Key, _companyId, null, new Dictionary<string, string?>
        {
            ["purpose"] = "Office supplies",
            ["amount"] = "500",
            ["dateNeeded"] = "2026-11-25"
        }), actor, CancellationToken.None);

    private ActorContext MakeActor(string id, string role, params string[] permissions) =>
        new(id, $"User {id}", new PermissionSet(permissions, []), [role], [_companyId]);
}
