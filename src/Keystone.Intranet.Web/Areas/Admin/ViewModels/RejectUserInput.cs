namespace Keystone.Intranet.Web.Areas.Admin.ViewModels;

public sealed record RejectUserInput(string UserId, int ExpectedVersion);
