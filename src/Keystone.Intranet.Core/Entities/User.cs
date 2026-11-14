using Keystone.Intranet.Core.Constants;

namespace Keystone.Intranet.Core.Entities;

public sealed class User : AuditedEntity
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = UserStatuses.Pending;
    public List<string> CompanyIds { get; set; } = [];
    public List<string> RoleKeys { get; set; } = [];
    public List<string> ExtraPermissions { get; set; } = [];
    public List<string> DeniedPermissions { get; set; } = [];
    public string Language { get; set; } = Languages.English;
    public DateTime? LastLoginAt { get; set; }
    public int Version { get; set; } = 1;
}
