using Keystone.Intranet.Core;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Requests;

// read side for requests; every lookup by id checks that the caller may see the record
public sealed class RequestQueryService(
    IRequestRepository requests,
    IRequestHistoryRepository history,
    IAttachmentStore attachments)
{
    public async Task<RequestInstance> GetForViewerAsync(string requestId, ActorContext actor, CancellationToken ct)
    {
        if (!EntityId.IsValid(requestId)) throw new NotFoundException();
        var request = await requests.GetByIdAsync(requestId, ct) ?? throw new NotFoundException();
        if (!CanView(request.Requester.UserId, actor)) throw new NotFoundException();
        return request;
    }

    public Task<IReadOnlyList<RequestHistoryEntry>> GetHistoryAsync(RequestInstance viewed, CancellationToken ct) =>
        history.ListByRequestAsync(viewed.Id, ct);

    public Task<IReadOnlyList<StoredFile>> GetAttachmentsAsync(RequestInstance viewed, CancellationToken ct) =>
        attachments.GetMetaManyAsync(viewed.AttachmentIds, ct);

    public Task<IReadOnlyList<RequestSummary>> ListMineAsync(ActorContext actor, CancellationToken ct) =>
        requests.ListByRequesterAsync(actor.UserId, 200, ct);

    public Task<IReadOnlyList<RequestSummary>> ListLinkableAsync(ActorContext actor, string definitionKey, CancellationToken ct) =>
        requests.ListByRequesterKeyAndStatusAsync(actor.UserId, definitionKey, RequestStatuses.Approved, ct);

    public Task<IReadOnlyList<RequestSummary>> ListPendingAsync(ActorContext actor, CancellationToken ct)
    {
        if (!actor.Permissions.Has(PermissionCodes.RequestApprove)) throw new ForbiddenTransitionException();
        return requests.ListOldestByStatusAsync(RequestStatuses.Pending, 200, ct);
    }

    public async Task<(StoredFile Meta, Stream Content)> OpenAttachmentAsync(string fileId, ActorContext actor, CancellationToken ct)
    {
        if (!EntityId.IsValid(fileId)) throw new NotFoundException();
        var meta = await attachments.GetMetaAsync(fileId, ct) ?? throw new NotFoundException();
        if (!CanView(meta.OwnerUserId, actor)) throw new NotFoundException();
        var content = await attachments.OpenReadAsync(fileId, ct) ?? throw new NotFoundException();
        return (meta, content);
    }

    public static bool CanView(string ownerId, ActorContext actor) =>
        ownerId == actor.UserId
        || actor.Permissions.Has(PermissionCodes.RequestViewAll)
        || actor.Permissions.Has(PermissionCodes.RequestApprove);
}
