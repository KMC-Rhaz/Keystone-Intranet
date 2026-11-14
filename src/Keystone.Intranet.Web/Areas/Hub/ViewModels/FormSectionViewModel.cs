namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record FormSectionViewModel(
    string Key,
    string Title,
    string? Intro,
    IReadOnlyList<FormFieldViewModel> Fields);
