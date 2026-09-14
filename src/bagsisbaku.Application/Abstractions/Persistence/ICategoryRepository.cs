using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetByProductTypeAsync(
        ProductType productType,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        ProductType productType,
        Guid? excludingCategoryId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default);
}
