namespace Keystone.Intranet.Core.Entities;

public sealed class StoredFile
{
    public string Id { get; set; } = EntityId.New();
    public string OwnerUserId { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Length { get; set; }
    public DateTime UploadedAt { get; set; }
}
