namespace Keystone.Intranet.Application.Users;

public sealed class BootstrapOptions
{
    public const string SectionName = "Bootstrap";

    public List<string> SuperAdminEmails { get; set; } = [];
    public List<string> AllowedDomains { get; set; } = [];
}
