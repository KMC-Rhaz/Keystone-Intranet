namespace Keystone.Intranet.Core.Exceptions;

// messageKey is a translation key, never raw technical text
public abstract class DomainException(string messageKey) : Exception(messageKey)
{
    public string MessageKey { get; } = messageKey;
}
