using Keystone.Intranet.Application.Users;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Web.Areas.Admin.ViewModels;
using Keystone.Intranet.Web.Localization;
using Keystone.Intranet.Web.Mapping;
using Keystone.Intranet.Web.Models;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Intranet.Web.Areas.Admin.Controllers;

[Area("Admin")]
[RequirePermission(PermissionCodes.UsersManage)]
public sealed class UsersController(
    UserApprovalService userApprovals,
    ICompanyRepository companies,
    IRoleRepository roles,
    CurrentActor current,
    Translator translator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var pending = await userApprovals.ListPendingAsync(current.Actor!, ct);
        var allCompanies = (await companies.ListActiveAsync(ct))
            .Select(c => new ChoiceOptionViewModel(c.Id, c.ShortName)).ToList();
        var canGrantSuper = current.Actor!.RoleKeys.Contains(RoleKeys.SuperAdmin);
        var allRoles = (await roles.ListAsync(ct))
            .Where(r => r.Key != RoleKeys.Employee && (canGrantSuper || r.Key != RoleKeys.SuperAdmin))
            .Select(r => new ChoiceOptionViewModel(r.Key, r.Name)).ToList();

        var items = pending.Select(u => new PendingUserViewModel(
            u.Id, u.DisplayName, u.Email, FormViewModelMapper.FormatLocal(u.CreatedAt), u.Version)).ToList();
        return View(new PendingUsersViewModel(items, allCompanies, allRoles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve([FromBody] ApproveUserInput input, CancellationToken ct)
    {
        await userApprovals.ApproveAsync(
            new ApproveUserCommand(input.UserId, input.ExpectedVersion, input.CompanyIds ?? [], input.RoleKeys ?? []),
            current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("user.approved", current.Language),
            new { redirectUrl = Url.Action(nameof(Index)) }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject([FromBody] RejectUserInput input, CancellationToken ct)
    {
        await userApprovals.RejectAsync(input.UserId, input.ExpectedVersion, current.Actor!, ct);
        return Json(ApiResponse.Ok(translator.Get("user.rejected", current.Language),
            new { redirectUrl = Url.Action(nameof(Index)) }));
    }
}
