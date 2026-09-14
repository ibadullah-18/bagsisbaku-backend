using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Repositories;

internal sealed class BrandRepository(
    ApplicationDbContext dbContext)
    : IBrandRepository
{
    public Task<Brand?> GetByIdAsync(
        Guid brandId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Brands
            .SingleOrDefaultAsync(
                brand => brand.Id == brandId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Brand>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludingBrandId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        return dbContext.Brands.AnyAsync(
            brand =>
                brand.Name == normalizedName &&
                (!excludingBrandId.HasValue ||
                 brand.Id != excludingBrandId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        Brand brand,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(brand);

        await dbContext.Brands.AddAsync(
            brand,
            cancellationToken);
    }
}
