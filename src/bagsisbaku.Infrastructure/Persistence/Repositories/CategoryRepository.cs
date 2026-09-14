using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(
    ApplicationDbContext dbContext)
    : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Categories
            .SingleOrDefaultAsync(
                category => category.Id == categoryId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Category>>
        GetByProductTypeAsync(
            ProductType productType,
            CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(
                category =>
                    category.ProductType == productType)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        ProductType productType,
        Guid? excludingCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        return dbContext.Categories.AnyAsync(
            category =>
                category.Name == normalizedName &&
                category.ProductType == productType &&
                (!excludingCategoryId.HasValue ||
                 category.Id != excludingCategoryId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        await dbContext.Categories.AddAsync(
            category,
            cancellationToken);
    }
}
