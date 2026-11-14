namespace Keystone.Intranet.Application.Files;

// the first bytes of a file must match its extension; the client's name and content type are never trusted
public sealed class FileSignatureChecker
{
    public const int HeaderLength = 16;

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Pdf = "%PDF"u8.ToArray();
    private static readonly byte[] Zip = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] OleCompound = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];
    private static readonly string[] HeicBrands = ["heic", "heix", "hevc", "hevx", "mif1", "msf1"];

    private static readonly IReadOnlyList<AllowedFileType> Types =
    [
        new(".jpg", "image/jpeg", h => StartsWith(h, Jpeg)),
        new(".jpeg", "image/jpeg", h => StartsWith(h, Jpeg)),
        new(".png", "image/png", h => StartsWith(h, Png)),
        new(".webp", "image/webp", IsWebp),
        new(".heic", "image/heic", IsHeic),
        new(".pdf", "application/pdf", h => StartsWith(h, Pdf)),
        new(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", h => StartsWith(h, Zip)),
        new(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", h => StartsWith(h, Zip)),
        new(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation", h => StartsWith(h, Zip)),
        new(".doc", "application/msword", h => StartsWith(h, OleCompound)),
        new(".xls", "application/vnd.ms-excel", h => StartsWith(h, OleCompound)),
    ];

    public AllowedFileType? Match(string extension, ReadOnlyMemory<byte> header)
    {
        var type = Types.FirstOrDefault(t => string.Equals(t.Extension, extension, StringComparison.OrdinalIgnoreCase));
        return type is not null && type.Matches(header) ? type : null;
    }

    private static bool StartsWith(ReadOnlyMemory<byte> header, byte[] signature) =>
        header.Length >= signature.Length && header.Span[..signature.Length].SequenceEqual(signature);

    private static bool IsWebp(ReadOnlyMemory<byte> header) =>
        header.Length >= 12 && header.Span[..4].SequenceEqual("RIFF"u8) && header.Span[8..12].SequenceEqual("WEBP"u8);

    private static bool IsHeic(ReadOnlyMemory<byte> header)
    {
        if (header.Length < 12 || !header.Span[4..8].SequenceEqual("ftyp"u8)) return false;
        var brand = System.Text.Encoding.ASCII.GetString(header.Span[8..12]);
        return HeicBrands.Contains(brand);
    }
}
