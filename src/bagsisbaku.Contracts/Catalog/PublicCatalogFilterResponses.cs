namespace bagsisbaku.Contracts.Catalog;

public sealed record PublicCatalogFilterResponse(
    string Language,
    int? ProductType,
    int ProductCount,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    IReadOnlyList<PublicBrandFilterResponse> Brands,
    IReadOnlyList<PublicCategoryFilterResponse> Categories,
    IReadOnlyList<PublicColorFilterResponse> Colors,
    IReadOnlyList<PublicSizeFilterResponse> Sizes);

public sealed record PublicBrandFilterResponse(
    Guid Id,
    string Name,
    string? LogoUrl,
    int ProductCount);

public sealed record PublicCategoryFilterResponse(
    Guid Id,
    string Name,
    string? IconUrl,
    int ProductType,
    int ProductCount);

public sealed record PublicColorFilterResponse(
    Guid Id,
    string Name,
    string? HexCode,
    int ProductCount);

public sealed record PublicSizeFilterResponse(
    Guid Id,
    string Value,
    int ProductType,
    int SortOrder,
    int ProductCount);
