namespace Keystone.Intranet.Core.Exceptions;

public sealed class ConcurrencyConflictException() : DomainException("error.stale");
