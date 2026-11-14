namespace Keystone.Intranet.Core.Models;

public sealed record SubmitCommand(
    string DefinitionKey,
    string CompanyId,
    string? LinkedRequestId,
    IReadOnlyDictionary<string, string?> Values);
