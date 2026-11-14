using Keystone.Intranet.Core.Entities;

namespace Keystone.Intranet.Core.Interfaces;

public interface IAttachmentStore
{
    Task<StoredFile> SaveAsync(StoredFile meta, Stream content, CancellationToken ct);
    Task<StoredFile?> GetMetaAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<StoredFile>> GetMetaManyAsync(IEnumerable<string> ids, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string id, CancellationToken ct);
    Task AssignToRequestAsync(IEnumerable<string> ids, string requestId, CancellationToken ct);
}
