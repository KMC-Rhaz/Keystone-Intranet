using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using static Keystone.Intranet.Infrastructure.Seeds.SeedBuilders;

namespace Keystone.Intranet.Infrastructure.Seeds;

// replaces the old receipt google form; tokens fill from the linked cash advance
public static class ReceiptSubmissionDefinition
{
    public const string Key = "receipt_submission";

    public static WorkflowDefinition Create() => new()
    {
        Key = Key,
        Name = "Receipt Submission",
        Description = "Send receipts for a cash advance you received.",
        Category = "finance",
        Icon = "camera",
        ReferencePrefix = "RS",
        SubmitPermission = PermissionCodes.ReceiptSubmit,
        LinkedDefinitionKey = CashAdvanceDefinition.Key,
        SortOrder = 4,
        Form = new FormSchema
        {
            Sections =
            [
                Section("summary", "Receipts for {{REFERENCE}}", "You received {{AMOUNT}} for: {{PURPOSE}}",
                    Money("amountSpent", "Total amount spent (your advance was {{AMOUNT}})"),
                    Money("amountReturned", "Cash returned, if any", required: false),
                    Field("receiptFiles", "Photos or scans of your receipts", FieldTypes.File, true,
                        "You can take a photo with your phone camera."),
                    Field("notes", "Notes", FieldTypes.TextArea))
            ]
        },
        Steps = [SuperAdminReview()],
        CreatedBy = "seed"
    };
}
