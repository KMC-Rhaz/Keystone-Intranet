namespace Keystone.Intranet.Core.Exceptions;

public sealed class ForbiddenTransitionException(string messageKey = "error.forbidden")
    : DomainException(messageKey);
