using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Repositories;

internal sealed class SizeRepository(
    ApplicationDbContext dbContext)
    : ISizeRepository
{
    public Task<Size?> GetByIdAsync(
        Guid sizeId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Sizes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                size => size.Id == sizeId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Size>>
        GetByProductTypeAsync(
            ProductType productType,
            CancellationToken cancellationToken = default)
    {
        return await dbContext.Sizes
            .AsNoTracking()
            .Where(size => size.ProductType == productType)
            .OrderBy(size => size.SortOrder)
            .ThenBy(size => size.Value)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ValueExistsAsync(
        string value,
        ProductType productType,
        Guid? excludingSizeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedValue = value.Trim();

        return dbContext.Sizes.AnyAsync(
            size =>
                size.Value == normalizedValue &&
                size.ProductType == productType &&
                (!excludingSizeId.HasValue ||
                 size.Id != excludingSizeId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        Size size,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(size);

        await dbContext.Sizes.AddAsync(
            size,
            cancellationToken);
    }
}
