namespace Keystone.Intranet.Application.Dashboard;

public sealed record WaitingRequest(
    string RequestId,
    string ReferenceNo,
    string DefinitionName,
    string RequesterName,
    int HoursWaiting);
