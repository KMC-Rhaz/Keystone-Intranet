using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using static Keystone.Intranet.Infrastructure.Seeds.SeedBuilders;

namespace Keystone.Intranet.Infrastructure.Seeds;

public static class LeaveRequestDefinition
{
    public const string Key = "leave_request";

    public static WorkflowDefinition Create()
    {
        var dateTo = Field("dateTo", "Last day of leave", FieldTypes.Date, true);
        dateTo.MinFieldRef = "dateFrom";
        var reason = Field("reason", "Reason", FieldTypes.TextArea, true);
        reason.MaxLength = 500;

        return new WorkflowDefinition
        {
            Key = Key,
            Name = "Leave Request",
            Description = "Ask for time off from work.",
            Category = "hr",
            Icon = "calendar",
            ReferencePrefix = "LV",
            SubmitPermission = PermissionCodes.LeaveSubmit,
            PdfTemplateKey = Key,
            SortOrder = 2,
            Form = new FormSchema
            {
                Sections =
                [
                    Section("details", "Leave details", null,
                        Choice("leaveType", "Type of leave",
                            ("vacation", "Vacation"), ("sick", "Sick"), ("emergency", "Emergency"),
                            ("bereavement", "Bereavement"), ("parental", "Maternity or paternity"), ("unpaid", "Unpaid")),
                        Field("dateFrom", "First day of leave", FieldTypes.Date, true),
                        dateTo,
                        Field("halfDay", "This is a half-day leave", FieldTypes.Checkbox),
                        reason,
                        Field("medicalFiles", "Medical certificate (optional)", FieldTypes.File,
                            help: "Needed for sick leave longer than two days."))
                ]
            },
            Steps = [SuperAdminReview()],
            CreatedBy = "seed"
        };
    }
}
