using Market.Application.Abstractions.Caching;
using StackExchange.Redis;

namespace Market.Infrastructure.Caching;

public sealed class CatalogCacheVersionService : ICatalogCacheVersionService
{

    public async Task<long> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        return 1;
    }

    public async Task<long> BumpVersionAsync(CancellationToken cancellationToken = default)
    {
        return 1;
    }
}
