namespace bagsisbaku.Contracts.Products;

public sealed record CreateProductResponse(
    Guid Id);

public sealed record ProductDetailsResponse(
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
    int ProductType,
    string ProductTypeName,
    Guid CategoryId,
    Guid BrandId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyList<ProductImageResponse> Images,
    IReadOnlyList<ProductVariantResponse> Variants);

public sealed record ProductImageResponse(
    Guid Id,
    string ImageUrl,
    int SortOrder,
    bool IsPrimary);

public sealed record ProductVariantResponse(
    Guid Id,
    Guid SizeId,
    Guid ColorId,
    int StockCount,
    bool IsActive);
