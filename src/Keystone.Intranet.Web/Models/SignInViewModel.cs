namespace Keystone.Intranet.Web.Models;

public sealed record SignInViewModel(bool GoogleEnabled, bool DevSignInEnabled, string? ErrorMessage);
