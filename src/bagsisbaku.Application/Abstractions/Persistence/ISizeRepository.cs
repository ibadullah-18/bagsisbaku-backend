using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Abstractions.Persistence;

public interface ISizeRepository
{
    Task<Size?> GetByIdAsync(
        Guid sizeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Size>> GetByProductTypeAsync(
        ProductType productType,
        CancellationToken cancellationToken = default);

    Task<bool> ValueExistsAsync(
        string value,
        ProductType productType,
        Guid? excludingSizeId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Size size,
        CancellationToken cancellationToken = default);
}
