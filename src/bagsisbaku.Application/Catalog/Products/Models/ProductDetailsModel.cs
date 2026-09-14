using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Catalog.Products.Models;

public sealed record ProductDetailsModel(
    Guid Id,
    string Name,
    string? Description,
    string ProductCode,
    string? Model,
    decimal Price,
    decimal? DiscountPrice,
    bool IsDiscounted,
    bool IsFeatured,
    bool IsActive,
    int ViewCount,
    ProductType ProductType,
    Guid CategoryId,
    Guid BrandId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyList<ProductImageModel> Images,
    IReadOnlyList<ProductVariantModel> Variants);

public sealed record ProductImageModel(
    Guid Id,
    string ImageUrl,
    int SortOrder,
    bool IsPrimary);

public sealed record ProductVariantModel(
    Guid Id,
    Guid SizeId,
    Guid ColorId,
    int StockCount,
    bool IsActive);
