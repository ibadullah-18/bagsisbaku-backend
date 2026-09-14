using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Catalog.Public;

public interface IPublicCatalogFilterQuery
{
    Task<PublicCatalogFilterModel> GetAsync(
        ProductType? productType,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}

public sealed record PublicCatalogFilterModel(
    string Language,
    int? ProductType,
    int ProductCount,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    IReadOnlyList<PublicBrandFilterModel> Brands,
    IReadOnlyList<PublicCategoryFilterModel> Categories,
    IReadOnlyList<PublicColorFilterModel> Colors,
    IReadOnlyList<PublicSizeFilterModel> Sizes);

public sealed record PublicBrandFilterModel(
    Guid Id,
    string Name,
    string? LogoUrl,
    int ProductCount);

public sealed record PublicCategoryFilterModel(
    Guid Id,
    string Name,
    string? IconUrl,
    int ProductType,
    int ProductCount);

public sealed record PublicColorFilterModel(
    Guid Id,
    string Name,
    string? HexCode,
    int ProductCount);

public sealed record PublicSizeFilterModel(
    Guid Id,
    string Value,
    int ProductType,
    int SortOrder,
    int ProductCount);
