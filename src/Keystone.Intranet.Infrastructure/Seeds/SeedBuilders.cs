using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Infrastructure.Seeds;

// small builders so seeded definitions read like the json documents they will become
public static class SeedBuilders
{
    public static WorkflowStep SuperAdminReview() => new()
    {
        Key = "super_admin_review",
        Name = "Super admin review",
        Order = 1,
        Approver = new ApproverRule { Type = ApproverTypes.Role, Value = RoleKeys.SuperAdmin },
        ApprovePermission = PermissionCodes.RequestApprove,
        OnApprove = RequestStatuses.Approved,
        OnReject = RequestStatuses.Rejected,
        SlaHours = 48
    };

    public static FormField Field(string key, string label, string type, bool required = false, string? help = null) =>
        new() { Key = key, Label = label, Type = type, Required = required, HelpText = help };

    public static FormField Money(string key, string label, bool required = true, string? help = null) =>
        new() { Key = key, Label = label, Type = FieldTypes.Currency, Required = required, HelpText = help, Min = 0, Max = 10_000_000 };

    public static FormField Choice(string key, string label, params (string Value, string Label)[] options) => new()
    {
        Key = key,
        Label = label,
        Type = FieldTypes.Select,
        Required = true,
        Options = options.Select(o => new FieldOption { Value = o.Value, Label = o.Label }).ToList()
    };

    public static FormSection Section(string key, string title, string? intro, params FormField[] fields) =>
        new() { Key = key, Title = title, Intro = intro, Fields = [.. fields] };
}
