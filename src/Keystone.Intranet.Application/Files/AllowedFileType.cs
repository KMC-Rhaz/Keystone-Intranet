namespace Keystone.Intranet.Application.Files;

public sealed record AllowedFileType(string Extension, string ContentType, Func<ReadOnlyMemory<byte>, bool> Matches);
