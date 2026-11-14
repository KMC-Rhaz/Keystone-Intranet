namespace Keystone.Intranet.Core.Models;

public sealed record CancelCommand(string RequestId, int ExpectedVersion);
