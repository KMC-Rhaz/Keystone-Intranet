namespace Keystone.Intranet.Core.Entities;

public sealed class ApproverRule
{
    public string Type { get; set; } = string.Empty;
    public string? Value { get; set; }
}
