using Keystone.Intranet.Application.Dashboard;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Web.Areas.Admin.ViewModels;
using Keystone.Intranet.Web.Mapping;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Intranet.Web.Areas.Admin.Controllers;

[Area("Admin")]
[RequirePermission(PermissionCodes.DashboardView)]
public sealed class DashboardController(DashboardService dashboard, CurrentActor current) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var metrics = await dashboard.GetMetricsAsync(current.Actor!, ct);
        var activity = await LoadActivityAsync(ct);
        return View(new DashboardViewModel(metrics, activity));
    }

    [HttpGet]
    public async Task<IActionResult> Activity(CancellationToken ct) =>
        PartialView("_ActivityFeed", await LoadActivityAsync(ct));

    private async Task<IReadOnlyList<ActivityItemViewModel>> LoadActivityAsync(CancellationToken ct)
    {
        var entries = await dashboard.GetActivityAsync(current.Actor!, ct);
        return entries.Select(e => new ActivityItemViewModel(
            e.RequestId, e.ReferenceNo, e.DefinitionName, e.Action, e.Actor.Name, e.Comment,
            FormViewModelMapper.FormatLocal(e.At))).ToList();
    }
}
