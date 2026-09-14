using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(
    ApplicationDbContext dbContext)
    : IProductRepository
{
    public Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products.SingleOrDefaultAsync(
            product => product.Id == productId,
            cancellationToken);
    }

    public Task<Product?> GetWithDetailsByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .AsNoTracking()
            .Include(product => product.Images)
            .Include(product => product.Variants)
            .SingleOrDefaultAsync(
                product => product.Id == productId,
                cancellationToken);
    }

    public Task<Product?> GetForUpdateWithDetailsByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .Include(product => product.Images)
            .Include(product => product.Variants)
            .SingleOrDefaultAsync(
                product => product.Id == productId,
                cancellationToken);
    }

    public Task<bool> ProductCodeExistsAsync(
        string productCode,
        Guid? excludingProductId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = productCode.Trim();

        return dbContext.Products.AnyAsync(
            product =>
                product.ProductCode == normalizedCode &&
                (!excludingProductId.HasValue ||
                 product.Id != excludingProductId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        await dbContext.Products.AddAsync(
            product,
            cancellationToken);
    }
}
