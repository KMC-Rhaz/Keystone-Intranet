namespace Keystone.Intranet.Core.Constants;

public static class PermissionCodes
{
    public const string CashAdvanceSubmit = "request.cash_advance.submit";
    public const string LeaveSubmit = "request.leave.submit";
    public const string PaymentCollectionSubmit = "request.payment_collection.submit";
    public const string ReceiptSubmit = "request.receipt.submit";
    public const string RequestApprove = "request.approve";
    public const string RequestViewAll = "request.all.view";
    public const string DashboardView = "admin.dashboard.view";
    public const string UsersManage = "admin.users.manage";
    public const string WorkflowManage = "admin.workflow.manage";

    public const string AllRequests = "request.*";
    public const string AllAdmin = "admin.*";
    public const char Wildcard = '*';
}
