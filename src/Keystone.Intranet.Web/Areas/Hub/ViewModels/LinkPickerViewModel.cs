namespace Keystone.Intranet.Web.Areas.Hub.ViewModels;

public sealed record LinkPickerViewModel(string DefinitionKey, string Name, IReadOnlyList<LinkableRequestViewModel> Items);
