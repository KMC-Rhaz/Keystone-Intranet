using System.Text.Json;

namespace Keystone.Intranet.Infrastructure.InMemory;

// deep copies keep callers from mutating stored state, mirroring how a real database behaves
public static class Cloner
{
    public static T Copy<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.SerializeToUtf8Bytes(value))!;
}
