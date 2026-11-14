namespace Keystone.Intranet.Core.Constants;

public static class RoleKeys
{
    public const string Employee = "employee";
    public const string Sales = "sales";
    public const string Finance = "finance";
    public const string ItAdmin = "it_admin";
    public const string SuperAdmin = "super_admin";

    public static readonly IReadOnlyList<string> All = [Employee, Sales, Finance, ItAdmin, SuperAdmin];
}
