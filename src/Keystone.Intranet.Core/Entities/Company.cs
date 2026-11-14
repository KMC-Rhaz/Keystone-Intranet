namespace Keystone.Intranet.Core.Entities;

public sealed class Company : AuditedEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
