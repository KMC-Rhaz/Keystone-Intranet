using System.Collections.Frozen;

namespace Keystone.Intranet.Core.Constants;

public static class ApproverTypes
{
    public const string Role = "role";
    public const string User = "user";

    public static readonly FrozenSet<string> Supported = new[] { Role, User }.ToFrozenSet();
}
