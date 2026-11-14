namespace Keystone.Intranet.Core.Models;

public sealed record ResubmitCommand(
    string RequestId,
    int ExpectedVersion,
    IReadOnlyDictionary<string, string?> Values);
