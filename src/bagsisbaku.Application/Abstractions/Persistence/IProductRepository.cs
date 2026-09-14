using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<Product?> GetWithDetailsByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<Product?> GetForUpdateWithDetailsByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<bool> ProductCodeExistsAsync(
        string productCode,
        Guid? excludingProductId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);
}
