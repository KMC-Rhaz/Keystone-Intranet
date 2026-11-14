using System.Security.Claims;
using Keystone.Intranet.Application.Security;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Web.Security;

// rebuilds user status, permissions and active company from storage on every request
public sealed class PortalClaimsMiddleware(RequestDelegate next)
{
    public const string CompanyCookie = "ks_company";
    public const string LanguageCookie = "ks_lang";

    public async Task InvokeAsync(
        HttpContext context,
        CurrentActor current,
        IUserRepository users,
        ICompanyRepository companies,
        PermissionEvaluator evaluator)
    {
        current.Language = ResolveLanguage(context, null);
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            var user = await users.GetByIdAsync(userId, context.RequestAborted);
            if (user is not null)
            {
                current.User = user;
                current.Language = ResolveLanguage(context, user.Language);
                if (user.Status == UserStatuses.Approved)
                {
                    var permissions = await evaluator.ResolveAsync(user, context.RequestAborted);
                    current.Actor = new ActorContext(user.Id, user.DisplayName, permissions, user.RoleKeys, user.CompanyIds);
                    var active = await companies.ListActiveAsync(context.RequestAborted);
                    current.AvailableCompanies = active.Where(c => user.CompanyIds.Contains(c.Id)).ToList();
                    var requestedCompany = context.Request.Cookies[CompanyCookie];
                    current.ActiveCompany = current.AvailableCompanies.FirstOrDefault(c => c.Id == requestedCompany)
                        ?? current.AvailableCompanies.FirstOrDefault();
                }
            }
        }

        await next(context);
    }

    private static string ResolveLanguage(HttpContext context, string? userLang)
    {
        var cookie = context.Request.Cookies[LanguageCookie];
        var candidate = !string.IsNullOrEmpty(cookie) ? cookie : userLang;
        return candidate == Languages.Filipino ? Languages.Filipino : Languages.English;
    }
}
