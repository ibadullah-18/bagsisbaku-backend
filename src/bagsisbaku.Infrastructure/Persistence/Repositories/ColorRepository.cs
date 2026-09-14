using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Repositories;

internal sealed class ColorRepository(
    ApplicationDbContext dbContext)
    : IColorRepository
{
    public Task<Color?> GetByIdAsync(
        Guid colorId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Colors
            .AsNoTracking()
            .SingleOrDefaultAsync(
                color => color.Id == colorId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Color>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Colors
            .AsNoTracking()
            .OrderBy(color => color.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludingColorId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        return dbContext.Colors.AnyAsync(
            color =>
                color.Name == normalizedName &&
                (!excludingColorId.HasValue ||
                 color.Id != excludingColorId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        Color color,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(color);

        await dbContext.Colors.AddAsync(
            color,
            cancellationToken);
    }
}
