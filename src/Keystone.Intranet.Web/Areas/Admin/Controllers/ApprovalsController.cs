using Keystone.Intranet.Application.Forms;
using Keystone.Intranet.Application.Requests;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;
using Keystone.Intranet.Web.Areas.Admin.ViewModels;
using Keystone.Intranet.Web.Localization;
using Keystone.Intranet.Web.Mapping;
using Keystone.Intranet.Web.Models;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Intranet.Web.Areas.Admin.Controllers;

[Area("Admin")]
[RequirePermission(PermissionCodes.RequestApprove)]
public sealed class ApprovalsController(
    RequestQueryService queries,
    IWorkflowEngine engine,
    IWorkflowDefinitionRepository definitions,
    ICompanyRepository companies,
    PlaceholderResolver placeholders,
    FormViewModelMapper mapper,
    CurrentActor current,
    Translator translator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var pending = await queries.ListPendingAsync(current.Actor!, ct);
        var companyNames = (await companies.ListActiveAsync(ct)).ToDictionary(c => c.Id, c => c.ShortName);
        var items = pending.Select(r => new ApprovalListItemViewModel(
            r.Id, r.ReferenceNo, r.DefinitionName, r.RequesterName,
            companyNames.GetValueOrDefault(r.CompanyId, string.Empty),
            FormViewModelMapper.FormatLocal(r.SubmittedAt))).ToList();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Review(string id, CancellationToken ct)
    {
        var request = await queries.GetForViewerAsync(id, current.Actor!, ct);
        var definition = await definitions.GetActiveByKeyAsync(request.DefinitionKey, ct);
        var tokens = new Dictionary<string, string>();
        if (definition?.LinkedDefinitionKey is not null && request.LinkedRequestId is not null)
        {
            var linked = await queries.GetForViewerAsync(request.LinkedRequestId, current.Actor!, ct);
            var linkedDef = await definitions.GetActiveByKeyAsync(linked.DefinitionKey, ct);
            if (linkedDef is not null) tokens = new Dictionary<string, string>(placeholders.BuildTokens(linked, linkedDef));
        }

        var files = await queries.GetAttachmentsAsync(request, ct);
        var history = await queries.GetHistoryAsync(request, ct);
        var company = await companies.GetByIdAsync(request.CompanyId, ct);
        var detail = mapper.BuildDetail(request, definition, tokens, files, history, current.Actor!.UserId, current.Language);
        return View(new ApprovalDetailViewModel(detail, company?.Name ?? string.Empty, request.Requester.UserId == current.Actor!.UserId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve([FromBody] DecisionCommand command, CancellationToken ct)
    {
        await engine.ApproveAsync(command, current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("request.approved", current.Language),
            new { redirectUrl = Url.Action(nameof(Index)) }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject([FromBody] DecisionCommand command, CancellationToken ct)
    {
        await engine.RejectAsync(command, current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("request.rejected", current.Language),
            new { redirectUrl = Url.Action(nameof(Index)) }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendBack([FromBody] DecisionCommand command, CancellationToken ct)
    {
        await engine.SendBackAsync(command, current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("request.returned", current.Language),
            new { redirectUrl = Url.Action(nameof(Index)) }));
    }
}
