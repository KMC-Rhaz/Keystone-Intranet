namespace Keystone.Intranet.Core.Exceptions;

// one message for missing and forbidden records so ids cannot be probed
public sealed class NotFoundException() : DomainException("error.not_found");
