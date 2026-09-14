using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Infrastructure.Persistence.Repositories;

internal sealed class TranslationRepository(
    ApplicationDbContext dbContext)
    : ITranslationRepository
{
    public async Task AddProductTranslationsAsync(
        IReadOnlyCollection<ProductTranslation> translations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(translations);

        if (translations.Count == 0)
        {
            return;
        }

        await dbContext.ProductTranslations.AddRangeAsync(
            translations,
            cancellationToken);
    }

    public async Task AddCategoryTranslationsAsync(
        IReadOnlyCollection<CategoryTranslation> translations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(translations);

        if (translations.Count == 0)
        {
            return;
        }

        await dbContext.CategoryTranslations.AddRangeAsync(
            translations,
            cancellationToken);
    }

    public async Task AddColorTranslationsAsync(
        IReadOnlyCollection<ColorTranslation> translations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(translations);

        if (translations.Count == 0)
        {
            return;
        }

        await dbContext.ColorTranslations.AddRangeAsync(
            translations,
            cancellationToken);
    }

    public async Task AddSizeTranslationsAsync(
        IReadOnlyCollection<SizeTranslation> translations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(translations);

        if (translations.Count == 0)
        {
            return;
        }

        await dbContext.SizeTranslations.AddRangeAsync(
            translations,
            cancellationToken);
    }
}
