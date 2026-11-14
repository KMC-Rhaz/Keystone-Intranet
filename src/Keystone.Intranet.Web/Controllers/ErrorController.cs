using Keystone.Intranet.Web.Localization;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keystone.Intranet.Web.Controllers;

[AllowAnonymous]
public sealed class ErrorController(CurrentActor current, Translator translator) : Controller
{
    [HttpGet]
    public IActionResult Status(int code = 500)
    {
        Response.StatusCode = code;
        var key = code switch
        {
            403 => "error.forbidden",
            404 => "error.not_found",
            _ => "error.unexpected"
        };
        ViewData["ErrorMessage"] = translator.Get(key, current.Language);
        ViewData["StatusCode"] = code;
        return View();
    }
}
