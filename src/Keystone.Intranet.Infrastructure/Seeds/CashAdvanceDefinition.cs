using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using static Keystone.Intranet.Infrastructure.Seeds.SeedBuilders;

namespace Keystone.Intranet.Infrastructure.Seeds;

public static class CashAdvanceDefinition
{
    public const string Key = "cash_advance";

    public static WorkflowDefinition Create()
    {
        var amount = Money("amount", "How much do you need?", help: "Amount in pesos.");
        amount.Min = 1;
        var purpose = Field("purpose", "What is it for?", FieldTypes.TextArea, true);
        purpose.MaxLength = 500;

        return new WorkflowDefinition
        {
            Key = Key,
            Name = "Cash Advance",
            Description = "Ask for money ahead of a work expense.",
            Category = "finance",
            Icon = "wallet",
            ReferencePrefix = "CA",
            SubmitPermission = PermissionCodes.CashAdvanceSubmit,
            PdfTemplateKey = Key,
            SortOrder = 1,
            Form = new FormSchema
            {
                Sections =
                [
                    Section("details", "Cash advance details", null,
                        purpose,
                        amount,
                        Field("dateNeeded", "When do you need it?", FieldTypes.Date, true),
                        Field("supportingFiles", "Supporting files (optional)", FieldTypes.File,
                            help: "Quotes, approvals or anything that explains the expense."))
                ]
            },
            Steps = [SuperAdminReview()],
            CreatedBy = "seed"
        };
    }
}
