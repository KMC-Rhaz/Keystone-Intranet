namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record HubIndexViewModel(string CompanyName, IReadOnlyList<HubCardViewModel> Cards);
