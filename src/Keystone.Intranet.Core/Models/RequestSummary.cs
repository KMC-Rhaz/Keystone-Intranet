namespace Keystone.Intranet.Core.Models;

public sealed record RequestSummary(
    string Id,
    string ReferenceNo,
    string DefinitionKey,
    string DefinitionName,
    string CompanyId,
    string RequesterId,
    string RequesterName,
    string Status,
    DateTime SubmittedAt,
    DateTime? CompletedAt);
