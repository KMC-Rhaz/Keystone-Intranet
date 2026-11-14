using Keystone.Intranet.Web.Areas.Hub.ViewModels;

namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record ApprovalDetailViewModel(
    RequestDetailViewModel Detail,
    string CompanyName,
    bool IsOwnRequest);
