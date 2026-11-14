namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record SubmitRequestInput(
    string DefinitionKey,
    string? LinkedRequestId,
    Dictionary<string, string?>? Values);
