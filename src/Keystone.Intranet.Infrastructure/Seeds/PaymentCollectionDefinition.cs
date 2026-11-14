using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;
using static Keystone.Intranet.Infrastructure.Seeds.SeedBuilders;

namespace Keystone.Intranet.Infrastructure.Seeds;

public static class PaymentCollectionDefinition
{
    public const string Key = "payment_collection";

    public static WorkflowDefinition Create()
    {
        var amount = Money("amount", "Amount received");
        amount.Min = 1;

        return new WorkflowDefinition
        {
            Key = Key,
            Name = "Payment Collection",
            Description = "Report a payment received from a client.",
            Category = "sales",
            Icon = "receipt",
            ReferencePrefix = "PC",
            SubmitPermission = PermissionCodes.PaymentCollectionSubmit,
            SortOrder = 3,
            Form = new FormSchema
            {
                Sections =
                [
                    Section("client", "Client and payment", null,
                        Field("clientName", "Client name", FieldTypes.Text, true),
                        Field("invoiceNumber", "Invoice or billing number", FieldTypes.Text),
                        amount,
                        Choice("paymentMethod", "How did they pay?",
                            ("cash", "Cash"), ("bank_transfer", "Bank transfer"), ("check", "Check"), ("ewallet", "E-wallet")),
                        Field("paymentDate", "Date received", FieldTypes.Date, true),
                        Field("paymentReference", "Bank, check or e-wallet reference", FieldTypes.Text)),
                    Section("proof", "Proof of payment", "A super admin approves this once they confirm the money arrived.",
                        Field("proofFiles", "Photo or scan of the proof of payment", FieldTypes.File, true),
                        Field("notes", "Notes", FieldTypes.TextArea))
                ]
            },
            Steps = [SuperAdminReview()],
            CreatedBy = "seed"
        };
    }
}
