using Keystone.Intranet.Application.Forms;
using Keystone.Intranet.Application.Requests;
using Keystone.Intranet.Core;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;
using Keystone.Intranet.Web.Areas.Hub.ViewModels;
using Keystone.Intranet.Web.Localization;
using Keystone.Intranet.Web.Mapping;
using Keystone.Intranet.Web.Models;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Intranet.Web.Areas.Hub.Controllers;

[Area("Hub")]
[Authorize]
public sealed class RequestsController(
    IWorkflowDefinitionRepository definitions,
    IWorkflowEngine engine,
    RequestQueryService queries,
    PlaceholderResolver placeholders,
    FormViewModelMapper mapper,
    CurrentActor current,
    Translator translator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!current.IsApproved) return RedirectToAction("Waiting", "Account", new { area = "" });
        var list = await queries.ListMineAsync(current.Actor!, ct);
        var items = list.Select(r => new RequestListItemViewModel(
            r.Id, r.ReferenceNo, r.DefinitionName, r.Status,
            translator.StatusLabel(r.Status, current.Language),
            FormViewModelMapper.FormatLocal(r.SubmittedAt))).ToList();
        return View(new MyRequestsViewModel(items));
    }

    [HttpGet]
    public async Task<IActionResult> New(string key, string? linked, CancellationToken ct)
    {
        if (!current.IsApproved) return RedirectToAction("Waiting", "Account", new { area = "" });
        var definition = await definitions.GetActiveByKeyAsync(key, ct) ?? throw new NotFoundException();
        if (!current.HasPermission(definition.SubmitPermission)) throw new ForbiddenTransitionException();

        if (definition.LinkedDefinitionKey is not null && string.IsNullOrEmpty(linked))
            return await ShowLinkPickerAsync(definition, ct);

        var tokens = await LoadTokensAsync(definition, linked, ct);
        var submitUrl = Url.Action(nameof(Submit), "Requests", new { area = "Hub" })!;
        var vm = mapper.BuildForm(definition, tokens, submitUrl, translator.Get("request.submit", current.Language), linked);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string id, CancellationToken ct)
    {
        if (!current.IsApproved) return RedirectToAction("Waiting", "Account", new { area = "" });
        var request = await queries.GetForViewerAsync(id, current.Actor!, ct);
        var definition = await definitions.GetActiveByKeyAsync(request.DefinitionKey, ct);
        var tokens = definition is null ? new Dictionary<string, string>() : await LoadTokensAsync(definition, request.LinkedRequestId, ct);
        var files = await queries.GetAttachmentsAsync(request, ct);
        var history = await queries.GetHistoryAsync(request, ct);
        return View(mapper.BuildDetail(request, definition, tokens, files, history, current.Actor!.UserId, current.Language));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken ct)
    {
        if (!current.IsApproved) return RedirectToAction("Waiting", "Account", new { area = "" });
        var request = await queries.GetForViewerAsync(id, current.Actor!, ct);
        if (request.Requester.UserId != current.Actor!.UserId) throw new NotFoundException();
        if (request.Status != RequestStatuses.Returned) throw new ForbiddenTransitionException("error.not_returned");

        var definition = await definitions.GetActiveByKeyAsync(request.DefinitionKey, ct) ?? throw new NotFoundException();
        var tokens = await LoadTokensAsync(definition, request.LinkedRequestId, ct);
        var files = await queries.GetAttachmentsAsync(request, ct);
        var submitUrl = Url.Action(nameof(Resubmit), "Requests", new { area = "Hub" })!;
        var vm = mapper.BuildForm(definition, tokens, submitUrl, translator.Get("request.resubmit", current.Language),
            request.LinkedRequestId, request, files);
        return View("New", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit([FromBody] SubmitRequestInput input, CancellationToken ct)
    {
        if (!current.IsApproved) return Forbid();
        var companyId = current.ActiveCompany?.Id ?? throw new ForbiddenTransitionException("error.company");
        var created = await engine.SubmitAsync(
            new SubmitCommand(input.DefinitionKey, companyId, input.LinkedRequestId, input.Values ?? new Dictionary<string, string?>()),
            current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("request.submitted", current.Language),
            new { requestId = created.Id, redirectUrl = Url.Action(nameof(Detail), new { id = created.Id }) }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resubmit([FromBody] ResubmitRequestInput input, CancellationToken ct)
    {
        if (!current.IsApproved) return Forbid();
        var updated = await engine.ResubmitAsync(
            new ResubmitCommand(input.RequestId, input.ExpectedVersion, input.Values ?? new Dictionary<string, string?>()),
            current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("request.resubmitted", current.Language),
            new { requestId = updated.Id, redirectUrl = Url.Action(nameof(Detail), new { id = updated.Id }) }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel([FromBody] CancelCommand input, CancellationToken ct)
    {
        if (!current.IsApproved) return Forbid();
        await engine.CancelAsync(input, current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("request.cancelled", current.Language),
            new { redirectUrl = Url.Action(nameof(Detail), new { id = input.RequestId }) }));
    }

    private async Task<IActionResult> ShowLinkPickerAsync(WorkflowDefinition definition, CancellationToken ct)
    {
        var items = (await queries.ListLinkableAsync(current.Actor!, definition.LinkedDefinitionKey!, ct))
            .Select(r => new LinkableRequestViewModel(r.Id, r.ReferenceNo, FormViewModelMapper.FormatLocal(r.SubmittedAt)))
            .ToList();
        return View("PickLink", new LinkPickerViewModel(definition.Key, definition.Name, items));
    }

    private async Task<IReadOnlyDictionary<string, string>> LoadTokensAsync(
        WorkflowDefinition definition, string? linkedId, CancellationToken ct)
    {
        if (definition.LinkedDefinitionKey is null || string.IsNullOrEmpty(linkedId))
            return new Dictionary<string, string>();
        if (!EntityId.IsValid(linkedId)) throw new NotFoundException();

        var linked = await queries.GetForViewerAsync(linkedId, current.Actor!, ct);
        if (linked.Requester.UserId != current.Actor!.UserId || linked.DefinitionKey != definition.LinkedDefinitionKey)
            throw new NotFoundException();

        var linkedDef = await definitions.GetActiveByKeyAsync(linked.DefinitionKey, ct) ?? throw new NotFoundException();
        return placeholders.BuildTokens(linked, linkedDef);
    }
}
