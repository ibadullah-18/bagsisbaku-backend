using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Catalog.Administration.Models;

public sealed record CatalogOptionsModel(
    ProductType ProductType,
    Guid? DefaultCategoryId,
    Guid? DefaultSizeId,
    IReadOnlyList<BrandOptionModel> Brands,
    IReadOnlyList<CategoryOptionModel> Categories,
    IReadOnlyList<SizeOptionModel> Sizes,
    IReadOnlyList<ColorOptionModel> Colors);

public sealed record BrandOptionModel(
    Guid Id,
    string Name,
    string? ImageUrl);

public sealed record CategoryOptionModel(
    Guid Id,
    string Name,
    string? IconUrl);

public sealed record SizeOptionModel(
    Guid Id,
    string Value,
    int SortOrder);

public sealed record ColorOptionModel(
    Guid Id,
    string Name,
    string? HexCode);
