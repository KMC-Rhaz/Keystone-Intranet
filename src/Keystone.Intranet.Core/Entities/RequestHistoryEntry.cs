namespace Keystone.Intranet.Core.Entities;

public sealed class RequestHistoryEntry
{
    public string Id { get; set; } = EntityId.New();
    public string RequestId { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string DefinitionName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? FromStepKey { get; set; }
    public string? ToStepKey { get; set; }
    public PersonSnapshot Actor { get; set; } = new();
    public string? Comment { get; set; }
    public DateTime At { get; set; }
}
