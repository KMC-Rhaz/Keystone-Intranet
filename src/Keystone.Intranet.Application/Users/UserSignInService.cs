using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace Keystone.Intranet.Application.Users;

// turns a verified google identity into a portal account; google proves who, the portal decides access
public sealed class UserSignInService(
    IUserRepository users,
    ICompanyRepository companies,
    IOptions<BootstrapOptions> options,
    TimeProvider clock)
{
    public async Task<User> SignInAsync(string email, string displayName, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (!IsDomainAllowed(normalized)) throw new ForbiddenTransitionException("error.domain_not_allowed");

        var now = clock.GetUtcNow().UtcDateTime;
        var user = await users.GetByEmailAsync(normalized, ct);
        if (user is null)
        {
            user = new User { Email = normalized, DisplayName = Clean(displayName, normalized), CreatedAt = now };
            await ApplyBootstrapAsync(user, ct);
            user.LastLoginAt = now;
            user.UpdatedAt = now;
            await users.InsertAsync(user, ct);
            return user;
        }

        var expected = user.Version;
        if (user.Status == UserStatuses.Pending) await ApplyBootstrapAsync(user, ct);
        user.DisplayName = Clean(displayName, user.DisplayName);
        user.LastLoginAt = now;
        user.UpdatedAt = now;
        user.Version = expected + 1;
        if (!await users.TryReplaceAsync(user, expected, ct)) throw new ConcurrencyConflictException();
        return user;
    }

    private async Task ApplyBootstrapAsync(User user, CancellationToken ct)
    {
        var isBootstrap = options.Value.SuperAdminEmails
            .Any(e => string.Equals(e.Trim(), user.Email, StringComparison.OrdinalIgnoreCase));
        if (!isBootstrap) return;

        var all = await companies.ListActiveAsync(ct);
        user.Status = UserStatuses.Approved;
        user.RoleKeys = [RoleKeys.Employee, RoleKeys.SuperAdmin];
        user.CompanyIds = all.Select(c => c.Id).ToList();
        user.UpdatedBy = "bootstrap";
    }

    private bool IsDomainAllowed(string email)
    {
        var domains = options.Value.AllowedDomains.Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
        if (domains.Count == 0) return true;
        var domain = email[(email.LastIndexOf('@') + 1)..];
        return domains.Any(d => string.Equals(d.Trim(), domain, StringComparison.OrdinalIgnoreCase));
    }

    private static string Clean(string? name, string fallback)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return fallback;
        return trimmed.Length > 120 ? trimmed[..120] : trimmed;
    }
}
