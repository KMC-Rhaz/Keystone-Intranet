using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Core.Exceptions;

public sealed class ValidationFailedException(IReadOnlyList<FieldError> errors, string messageKey = "error.validation")
    : DomainException(messageKey)
{
    public IReadOnlyList<FieldError> Errors { get; } = errors;

    public static ValidationFailedException For(string field, string messageKey) =>
        new([new FieldError(field, messageKey)]);
}
