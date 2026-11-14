using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Web.Areas.Hub.ViewModels;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Intranet.Web.Areas.Hub.Controllers;

[Area("Hub")]
[Authorize]
public sealed class HomeController(IWorkflowDefinitionRepository definitions, CurrentActor current) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!current.IsApproved) return RedirectToAction("Waiting", "Account", new { area = "" });
        var all = await definitions.ListActiveAsync(ct);
        var cards = all
            .Where(d => current.HasPermission(d.SubmitPermission))
            .Select(d => new HubCardViewModel(d.Key, d.Name, d.Description, d.Category, d.Icon, d.LinkedDefinitionKey is not null))
            .ToList();
        return View(new HubIndexViewModel(current.ActiveCompany?.Name ?? "Keystone", cards));
    }
}
