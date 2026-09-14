namespace bagsisbaku.Contracts.Products;

public sealed record PublicProductListResponse(
    string Language,
    IReadOnlyList<PublicProductListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);

public sealed record PublicProductListItemResponse(
    Guid Id,
    string Name,
    string ProductCode,
    string? Model,
    decimal Price,
    decimal? DiscountPrice,
    decimal CurrentPrice,
    bool IsDiscounted,
    bool IsFeatured,
    int ProductType,
    string ProductTypeName,
    PublicBrandResponse Brand,
    PublicCategoryResponse Category,
    string? PrimaryImageUrl,
    long TotalStock);

public sealed record PublicProductDetailsResponse(
    Guid Id,
    string Language,
    string Name,
    string? Description,
    string ProductCode,
    string? Model,
    decimal Price,
    decimal? DiscountPrice,
    bool IsDiscounted,
    bool IsFeatured,
    int ProductType,
    string ProductTypeName,
    PublicBrandResponse Brand,
    PublicCategoryResponse Category,
    IReadOnlyList<PublicProductImageResponse> Images,
    IReadOnlyList<PublicProductVariantResponse> Variants);

public sealed record PublicBrandResponse(
    Guid Id,
    string Name,
    string? LogoUrl);

public sealed record PublicCategoryResponse(
    Guid Id,
    string Name);

public sealed record PublicProductImageResponse(
    Guid Id,
    string ImageUrl,
    int SortOrder,
    bool IsPrimary);

public sealed record PublicProductVariantResponse(
    Guid Id,
    Guid SizeId,
    string Size,
    Guid ColorId,
    string Color,
    string? HexCode,
    int StockCount);
