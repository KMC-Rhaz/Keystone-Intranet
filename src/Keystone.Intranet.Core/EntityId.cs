using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Keystone.Intranet.Core;

// ids use the 24-hex ObjectId shape so the later MongoDB switch keeps every stored id valid
public static partial class EntityId
{
    public static string New()
    {
        var seconds = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Span<byte> bytes = stackalloc byte[12];
        bytes[0] = (byte)(seconds >> 24);
        bytes[1] = (byte)(seconds >> 16);
        bytes[2] = (byte)(seconds >> 8);
        bytes[3] = (byte)seconds;
        RandomNumberGenerator.Fill(bytes[4..]);
        return Convert.ToHexStringLower(bytes);
    }

    public static bool IsValid(string? value) => value is not null && Pattern().IsMatch(value);

    [GeneratedRegex("^[a-f0-9]{24}$")]
    private static partial Regex Pattern();
}
