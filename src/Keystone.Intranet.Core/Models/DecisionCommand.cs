namespace Keystone.Intranet.Core.Models;

public sealed record DecisionCommand(string RequestId, int ExpectedVersion, string? Comment);
