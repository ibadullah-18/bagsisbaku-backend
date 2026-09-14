using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Abstractions.Persistence;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(
        Guid brandId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Brand>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        Guid? excludingBrandId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Brand brand,
        CancellationToken cancellationToken = default);
}
