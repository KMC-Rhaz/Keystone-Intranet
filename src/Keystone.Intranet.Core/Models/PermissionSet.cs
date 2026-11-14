using System.Collections.Frozen;

namespace Keystone.Intranet.Core.Models;

// effective permissions of one user; denied codes always win over grants and wildcards
public sealed class PermissionSet
{
    private readonly FrozenSet<string> _exact;
    private readonly string[] _prefixes;
    private readonly FrozenSet<string> _denied;

    public PermissionSet(IEnumerable<string> granted, IEnumerable<string> denied)
    {
        var list = granted.ToList();
        _exact = list.Where(p => !p.EndsWith('*')).ToFrozenSet(StringComparer.Ordinal);
        _prefixes = list.Where(p => p.EndsWith('*')).Select(p => p[..^1]).Distinct().ToArray();
        _denied = denied.ToFrozenSet(StringComparer.Ordinal);
    }

    public static PermissionSet Empty { get; } = new([], []);

    public bool Has(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || _denied.Contains(code)) return false;
        return _exact.Contains(code) || _prefixes.Any(p => code.StartsWith(p, StringComparison.Ordinal));
    }
}
