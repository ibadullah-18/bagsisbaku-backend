using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Abstractions.Persistence;

public interface IColorRepository
{
    Task<Color?> GetByIdAsync(
        Guid colorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Color>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        Guid? excludingColorId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Color color,
        CancellationToken cancellationToken = default);
}
