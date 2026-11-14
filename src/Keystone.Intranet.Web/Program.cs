using System.Threading.RateLimiting;
using Keystone.Intranet.Application;
using Keystone.Intranet.Infrastructure;
using Keystone.Intranet.Infrastructure.Seeds;
using Keystone.Intranet.Web.Filters;
using Keystone.Intranet.Web.Localization;
using Keystone.Intranet.Web.Mapping;
using Keystone.Intranet.Web.Middleware;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeystoneApplication(builder.Configuration);
builder.Services.AddKeystoneInMemoryStorage();

builder.Services.AddSingleton<Translator>();
builder.Services.AddScoped<CurrentActor>();
builder.Services.AddScoped<FormViewModelMapper>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var secureCookie = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = builder.Environment.IsDevelopment() ? "ks_auth" : "__Host-ks_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = secureCookie;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/SignIn";
        options.AccessDeniedPath = "/Error/Status?code=403";
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleSecret;
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");
    });
}

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.Name = builder.Environment.IsDevelopment() ? "ks_csrf" : "__Host-ks_csrf";
    options.Cookie.SecurePolicy = secureCookie;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
    options.AddFixedWindowLimiter("auth", o => { o.PermitLimit = 20; o.Window = TimeSpan.FromMinutes(1); });
    options.AddFixedWindowLimiter("upload", o => { o.PermitLimit = 30; o.Window = TimeSpan.FromMinutes(1); });
});

builder.Services.AddControllersWithViews(options => options.Filters.Add<DomainExceptionFilter>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<SeedRunner>().RunAsync(CancellationToken.None);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Status?code=500");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<PortalClaimsMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { area = "Hub", controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();
