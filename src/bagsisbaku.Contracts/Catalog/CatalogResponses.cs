namespace bagsisbaku.Contracts.Catalog;

public sealed record CreatedIdResponse(
    Guid Id);

public sealed record CatalogOptionsResponse(
    int ProductType,
    string ProductTypeName,
    Guid? DefaultCategoryId,
    Guid? DefaultSizeId,
    IReadOnlyList<BrandOptionResponse> Brands,
    IReadOnlyList<CategoryOptionResponse> Categories,
    IReadOnlyList<SizeOptionResponse> Sizes,
    IReadOnlyList<ColorOptionResponse> Colors);

public sealed record BrandOptionResponse(
    Guid Id,
    string Name,
    string? ImageUrl);

public sealed record CategoryOptionResponse(
    Guid Id,
    string Name,
    string? IconUrl);

public sealed record SizeOptionResponse(
    Guid Id,
    string Value,
    int SortOrder);

public sealed record ColorOptionResponse(
    Guid Id,
    string Name,
    string? HexCode);
