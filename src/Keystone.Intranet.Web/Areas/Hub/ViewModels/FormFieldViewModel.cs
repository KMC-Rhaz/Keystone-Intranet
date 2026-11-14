namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record FormFieldViewModel(
    string Key,
    string Label,
    string Type,
    bool Required,
    string? HelpText,
    int? MaxLength,
    decimal? Min,
    decimal? Max,
    string? MinFieldRef,
    IReadOnlyList<FormOptionViewModel> Options);
