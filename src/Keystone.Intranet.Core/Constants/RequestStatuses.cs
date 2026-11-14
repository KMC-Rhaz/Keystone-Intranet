using System.Collections.Frozen;

namespace Keystone.Intranet.Core.Constants;

public static class RequestStatuses
{
    public const string Draft = "draft";
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Cancelled = "cancelled";
    public const string Returned = "returned";

    public static readonly FrozenSet<string> Terminal = new[] { Approved, Rejected, Cancelled }.ToFrozenSet();
}
