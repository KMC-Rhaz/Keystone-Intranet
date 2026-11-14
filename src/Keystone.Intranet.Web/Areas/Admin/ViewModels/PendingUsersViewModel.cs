namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record PendingUsersViewModel(
    IReadOnlyList<PendingUserViewModel> Users,
    IReadOnlyList<ChoiceOptionViewModel> Companies,
    IReadOnlyList<ChoiceOptionViewModel> Roles);
