using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Intranet.Web.Areas.Hub.Controllers;

[Area("Hub")]
public sealed class PreferencesController(CurrentActor current) : Controller
{
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SwitchCompany(string companyId, string? returnUrl)
    {
        if (current.IsApproved && current.AvailableCompanies.Any(c => c.Id == companyId))
            Response.Cookies.Append(PortalClaimsMiddleware.CompanyCookie, companyId, CookieOptions());
        return LocalRedirect(SafeReturn(returnUrl));
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SwitchLanguage(string language, string? returnUrl)
    {
        var normalized = language == Languages.Filipino ? Languages.Filipino : Languages.English;
        Response.Cookies.Append(PortalClaimsMiddleware.LanguageCookie, normalized, CookieOptions());
        return LocalRedirect(SafeReturn(returnUrl));
    }

    private static CookieOptions CookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(90)
    };

    private string SafeReturn(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home", new { area = "Hub" })!;
}
