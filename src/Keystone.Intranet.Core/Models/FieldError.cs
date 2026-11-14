namespace Keystone.Intranet.Core.Models;

// message is a translation key so the web layer can show it in the user's language
public sealed record FieldError(string Field, string Message);
