using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Abstractions.Persistence;

public interface ICatalogDefaultRepository
{
    Task<CatalogDefault?> GetByProductTypeAsync(
        ProductType productType,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CatalogDefault catalogDefault,
        CancellationToken cancellationToken = default);
}
