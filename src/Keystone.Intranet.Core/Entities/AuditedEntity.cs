namespace Keystone.Intranet.Core.Entities;

public abstract class AuditedEntity
{
    public string Id { get; set; } = EntityId.New();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public int SchemaVersion { get; set; } = 1;
}
