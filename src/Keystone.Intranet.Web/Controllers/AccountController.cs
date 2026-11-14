using System.Security.Claims;
using Keystone.Intranet.Application.Users;
using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Web.Localization;
using Keystone.Intranet.Web.Models;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Keystone.Intranet.Web.Controllers;

public sealed class AccountController(
    UserSignInService signIn,
    CurrentActor current,
    Translator translator,
    IConfiguration configuration,
    IWebHostEnvironment env) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult SignIn(string? error = null)
    {
        if (current.IsAuthenticated)
            return current.IsApproved ? RedirectToAction("Index", "Home", new { area = "Hub" }) : RedirectToAction(nameof(Waiting));

        var googleEnabled = !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]);
        var devEnabled = env.IsDevelopment() && !googleEnabled;
        var errorMessage = error is not null ? translator.Get(error, current.Language) : null;
        return View(new SignInViewModel(googleEnabled, devEnabled, errorMessage));
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GoogleLogin()
    {
        if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]))
            return RedirectToAction(nameof(SignIn));

        var props = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback)) };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GoogleCallback(CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded || result.Principal is null)
            return RedirectToAction(nameof(SignIn), new { error = "error.unexpected" });

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var verified = result.Principal.FindFirstValue("email_verified");
        if (string.IsNullOrWhiteSpace(email) || !string.Equals(verified, "true", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(SignIn), new { error = "error.forbidden" });

        var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
        var user = await signIn.SignInAsync(email, name, ct);
        await IssueCookieAsync(user.Id);
        return user.Status == UserStatuses.Approved
            ? RedirectToAction("Index", "Home", new { area = "Hub" })
            : RedirectToAction(nameof(Waiting));
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DevSignIn(string email, string displayName, CancellationToken ct)
    {
        // local sign-in is only available in development when google oauth is not configured
        if (!env.IsDevelopment() || !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]))
            return NotFound();

        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(SignIn), new { error = "validation.required" });

        var user = await signIn.SignInAsync(email, string.IsNullOrWhiteSpace(displayName) ? email : displayName, ct);
        await IssueCookieAsync(user.Id);
        return user.Status == UserStatuses.Approved
            ? RedirectToAction("Index", "Home", new { area = "Hub" })
            : RedirectToAction(nameof(Waiting));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Waiting()
    {
        if (current.User is null) return RedirectToAction(nameof(SignIn));
        if (current.IsApproved) return RedirectToAction("Index", "Home", new { area = "Hub" });
        return View(new WaitingViewModel(current.User.DisplayName, current.User.Email, current.User.Status));
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignOutPortal()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(SignIn));
    }

    private Task IssueCookieAsync(string userId)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
