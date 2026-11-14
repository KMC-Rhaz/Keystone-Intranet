namespace Keystone.Intranet.Core.Interfaces;

public interface ICounterRepository
{
    Task<long> NextAsync(string scope, CancellationToken ct);
}
