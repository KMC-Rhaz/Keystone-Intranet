using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Exceptions;
using Keystone.Intranet.Core.Interfaces;
using Keystone.Intranet.Core.Models;

namespace Keystone.Intranet.Application.Files;

public sealed class UploadService(IAttachmentStore store, FileSignatureChecker checker, TimeProvider clock)
{
    public const long MaxBytes = 10 * 1024 * 1024;

    public async Task<StoredFile> UploadAsync(string fileName, long length, Stream content, ActorContext actor, CancellationToken ct)
    {
        if (length <= 0) throw ValidationFailedException.For("file", "validation.file_empty");
        if (length > MaxBytes) throw ValidationFailedException.For("file", "validation.file_too_big");

        // buffer once so the signature check and the store read the exact same bytes
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        if (buffer.Length > MaxBytes) throw ValidationFailedException.For("file", "validation.file_too_big");

        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        var header = new ReadOnlyMemory<byte>(buffer.GetBuffer(), 0, (int)Math.Min(buffer.Length, FileSignatureChecker.HeaderLength));
        var type = checker.Match(extension, header) ?? throw ValidationFailedException.For("file", "validation.file_type");

        buffer.Position = 0;
        var meta = new StoredFile
        {
            OwnerUserId = actor.UserId,
            OriginalName = SafeName(fileName!, extension),
            ContentType = type.ContentType,
            Extension = type.Extension,
            Length = buffer.Length,
            UploadedAt = clock.GetUtcNow().UtcDateTime
        };
        return await store.SaveAsync(meta, buffer, ct);
    }

    private static string SafeName(string fileName, string extension)
    {
        var baseName = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));
        var cleaned = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c is ' ' or '-' or '_').ToArray()).Trim();
        if (cleaned.Length == 0) cleaned = "file";
        if (cleaned.Length > 80) cleaned = cleaned[..80];
        return cleaned + extension;
    }
}
