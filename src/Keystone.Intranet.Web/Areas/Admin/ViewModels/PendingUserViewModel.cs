namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record PendingUserViewModel(
    string Id,
    string DisplayName,
    string Email,
    string SignedUpAt,
    int Version);
