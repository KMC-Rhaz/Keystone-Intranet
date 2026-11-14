namespace Keystone.Intranet.Core.Entities;

public sealed class Role : AuditedEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
    public bool IsSystem { get; set; }
}
