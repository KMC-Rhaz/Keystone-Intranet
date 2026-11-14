namespace Keystone.Intranet.Web.Models;

public sealed record ApiResponse(
    bool Success,
    string Message,
    object? Data,
    IReadOnlyList<ApiError> Errors,
    string Toast)
{
    public static ApiResponse Ok(string message, object? data = null, string toast = "success") =>
        new(true, message, data, [], toast);

    public static ApiResponse Fail(string message, IReadOnlyList<ApiError>? errors = null, string toast = "error") =>
        new(false, message, null, errors ?? [], toast);
}
