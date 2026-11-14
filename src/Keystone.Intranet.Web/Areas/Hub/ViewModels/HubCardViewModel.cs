namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record HubCardViewModel(
    string Key,
    string Name,
    string Description,
    string Category,
    string Icon,
    bool RequiresLink);
