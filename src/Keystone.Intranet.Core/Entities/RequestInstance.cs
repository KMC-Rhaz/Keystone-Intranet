using Keystone.Intranet.Core.Constants;

namespace Keystone.Intranet.Core.Entities;

public sealed class RequestInstance : AuditedEntity
{
    public string ReferenceNo { get; set; } = string.Empty;
    public string DefinitionKey { get; set; } = string.Empty;
    public string DefinitionName { get; set; } = string.Empty;
    public int DefinitionVersion { get; set; }
    public string CompanyId { get; set; } = string.Empty;
    public PersonSnapshot Requester { get; set; } = new();
    public Dictionary<string, string> Values { get; set; } = [];
    public List<string> AttachmentIds { get; set; } = [];
    public string Status { get; set; } = RequestStatuses.Draft;
    public string? CurrentStepKey { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? LinkedRequestId { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? DrivePdfUrl { get; set; }
    public string? DriveFolderUrl { get; set; }
    public int Version { get; set; } = 1;
}
