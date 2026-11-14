using Keystone.Intranet.Core.Entities;
using Keystone.Intranet.Core.Interfaces;

namespace Keystone.Intranet.Infrastructure.InMemory;

// stand-in for the gridfs bucket until the database is connected
public sealed class InMemoryAttachmentStore : IAttachmentStore
{
    private readonly Dictionary<string, (StoredFile Meta, byte[] Bytes)> _files = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();

    public async Task<StoredFile> SaveAsync(StoredFile meta, Stream content, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        lock (_gate) _files[meta.Id] = (Cloner.Copy(meta), buffer.ToArray());
        return meta;
    }

    public Task<StoredFile?> GetMetaAsync(string id, CancellationToken ct)
    {
        lock (_gate) return Task.FromResult(_files.TryGetValue(id, out var f) ? Cloner.Copy(f.Meta) : null);
    }

    public Task<IReadOnlyList<StoredFile>> GetMetaManyAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        lock (_gate)
        {
            IReadOnlyList<StoredFile> list = ids.Distinct()
                .Where(_files.ContainsKey)
                .Select(id => Cloner.Copy(_files[id].Meta))
                .ToList();
            return Task.FromResult(list);
        }
    }

    public Task<Stream?> OpenReadAsync(string id, CancellationToken ct)
    {
        lock (_gate)
        {
            Stream? stream = _files.TryGetValue(id, out var f) ? new MemoryStream(f.Bytes, writable: false) : null;
            return Task.FromResult(stream);
        }
    }

    public Task AssignToRequestAsync(IEnumerable<string> ids, string requestId, CancellationToken ct)
    {
        lock (_gate)
        {
            foreach (var id in ids.Where(_files.ContainsKey)) _files[id].Meta.RequestId = requestId;
        }
        return Task.CompletedTask;
    }
}
