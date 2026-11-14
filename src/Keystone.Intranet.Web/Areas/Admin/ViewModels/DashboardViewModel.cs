using Keystone.Intranet.Application.Dashboard;

namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record DashboardViewModel(
    DashboardMetrics Metrics,
    IReadOnlyList<ActivityItemViewModel> Activity);
