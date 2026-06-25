namespace Market.Application.Abstractions.Caching;

public interface ICatalogCacheVersionService
{
    Task<long> GetVersionAsync(CancellationToken cancellationToken = default);
    Task<long> BumpVersionAsync(CancellationToken cancellationToken = default);
}
