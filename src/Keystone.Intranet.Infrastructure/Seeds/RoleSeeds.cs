using Keystone.Intranet.Core.Constants;
using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Infrastructure.Seeds;

public static class RoleSeeds
{
    public static IReadOnlyList<Role> All() =>
    [
        Make(RoleKeys.Employee, "Employee",
            PermissionCodes.CashAdvanceSubmit, PermissionCodes.LeaveSubmit, PermissionCodes.ReceiptSubmit),
        Make(RoleKeys.Sales, "Sales", PermissionCodes.PaymentCollectionSubmit),
        Make(RoleKeys.Finance, "Finance (view only)", PermissionCodes.RequestViewAll),
        Make(RoleKeys.ItAdmin, "IT Admin",
            PermissionCodes.DashboardView, PermissionCodes.UsersManage, PermissionCodes.WorkflowManage),
        Make(RoleKeys.SuperAdmin, "Super Admin", PermissionCodes.AllRequests, PermissionCodes.AllAdmin),
    ];

    private static Role Make(string key, string name, params string[] permissions) =>
        new() { Key = key, Name = name, Permissions = [.. permissions], IsSystem = true, CreatedBy = "seed" };
}
