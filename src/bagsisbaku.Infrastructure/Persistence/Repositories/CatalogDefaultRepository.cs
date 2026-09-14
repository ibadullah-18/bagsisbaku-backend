using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Repositories;

internal sealed class CatalogDefaultRepository(
    ApplicationDbContext dbContext)
    : ICatalogDefaultRepository
{
    public Task<CatalogDefault?> GetByProductTypeAsync(
        ProductType productType,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CatalogDefaults
            .SingleOrDefaultAsync(
                catalogDefault =>
                    catalogDefault.ProductType == productType,
                cancellationToken);
    }

    public async Task AddAsync(
        CatalogDefault catalogDefault,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(catalogDefault);

        await dbContext.CatalogDefaults.AddAsync(
            catalogDefault,
            cancellationToken);
    }
}
