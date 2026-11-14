using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Web.Localization;
using Keystone.Intranet.Web.Models;
using Keystone.Intranet.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Keystone.Intranet.Web.Filters;

// turns domain exceptions into the standard json envelope for ajax calls, or clean http status codes
public sealed class DomainExceptionFilter(CurrentActor current, Translator translator, ILogger<DomainExceptionFilter> logger)
    : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DomainException domain)
        {
            var status = domain switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ForbiddenTransitionException => StatusCodes.Status403Forbidden,
                ConcurrencyConflictException => StatusCodes.Status409Conflict,
                ValidationFailedException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };

            var message = translator.Get(domain.MessageKey, current.Language);
            var errors = domain is ValidationFailedException v
                ? v.Errors.Select(e => new ApiError(e.Field, translator.Get(e.Message, current.Language))).ToList()
                : [];

            if (WantsJson(context.HttpContext.Request))
            {
                context.Result = new JsonResult(ApiResponse.Fail(message, errors)) { StatusCode = status };
                context.ExceptionHandled = true;
                return;
            }

            context.Result = new RedirectToActionResult("Status", "Error", new { area = "", code = status });
            context.ExceptionHandled = true;
            return;
        }

        logger.LogError(context.Exception, "unhandled error trace {TraceId}", context.HttpContext.TraceIdentifier);
        if (WantsJson(context.HttpContext.Request))
        {
            var message = translator.Get("error.unexpected", current.Language);
            context.Result = new JsonResult(ApiResponse.Fail(message)) { StatusCode = StatusCodes.Status500InternalServerError };
            context.ExceptionHandled = true;
        }
    }

    private static bool WantsJson(HttpRequest request) =>
        request.Headers.Accept.Any(a => a is not null && a.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        || request.Headers.ContainsKey("X-Requested-With");
}
