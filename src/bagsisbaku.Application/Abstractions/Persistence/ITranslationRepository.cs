using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Abstractions.Persistence;

public interface ITranslationRepository
{
    Task AddProductTranslationsAsync(
        IReadOnlyCollection<ProductTranslation> translations,
        CancellationToken cancellationToken = default);

    Task AddCategoryTranslationsAsync(
        IReadOnlyCollection<CategoryTranslation> translations,
        CancellationToken cancellationToken = default);

    Task AddColorTranslationsAsync(
        IReadOnlyCollection<ColorTranslation> translations,
        CancellationToken cancellationToken = default);

    Task AddSizeTranslationsAsync(
        IReadOnlyCollection<SizeTranslation> translations,
        CancellationToken cancellationToken = default);
}
