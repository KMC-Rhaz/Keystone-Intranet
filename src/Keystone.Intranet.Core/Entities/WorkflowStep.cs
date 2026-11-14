namespace Keystone.Intranet.Core.Entities;

public sealed class WorkflowStep
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public ApproverRule Approver { get; set; } = new();
    public string ApprovePermission { get; set; } = string.Empty;
    public string OnApprove { get; set; } = string.Empty;
    public string OnReject { get; set; } = string.Empty;
    public int? SlaHours { get; set; }
}
