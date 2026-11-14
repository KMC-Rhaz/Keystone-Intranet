namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record ResubmitRequestInput(
    string RequestId,
    int ExpectedVersion,
    Dictionary<string, string?>? Values);
