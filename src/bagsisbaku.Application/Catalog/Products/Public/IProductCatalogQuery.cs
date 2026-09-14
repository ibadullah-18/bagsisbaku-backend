using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Catalog.Products.Public;

public interface IProductCatalogQuery
{
    Task<ProductCatalogPageModel> GetListAsync(
        ProductCatalogFilter filter,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<LocalizedProductDetailsModel?> GetByIdAsync(
        Guid productId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}

public sealed record ProductCatalogFilter(
    string? Search,
    ProductType? ProductType,
    Guid? CategoryId,
    Guid? BrandId,
    IReadOnlyCollection<Guid> SizeIds,
    IReadOnlyCollection<Guid> ColorIds,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? IsDiscounted,
    bool? IsFeatured,
    ProductCatalogSort Sort,
    int Page,
    int PageSize);

public enum ProductCatalogSort
{
    Newest = 1,
    PriceAscending = 2,
    PriceDescending = 3,
    Popular = 4
}

public sealed record ProductCatalogPageModel(
    string Language,
    IReadOnlyList<LocalizedProductListItemModel> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling(
                TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}

public sealed record LocalizedProductListItemModel(
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
    BrandSummaryModel Brand,
    CategorySummaryModel Category,
    string? PrimaryImageUrl,
    long TotalStock);

public sealed record LocalizedProductDetailsModel(
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
    BrandSummaryModel Brand,
    CategorySummaryModel Category,
    IReadOnlyList<PublicProductImageModel> Images,
    IReadOnlyList<PublicProductVariantModel> Variants);

public sealed record BrandSummaryModel(
    Guid Id,
    string Name,
    string? LogoUrl);

public sealed record CategorySummaryModel(
    Guid Id,
    string Name);

public sealed record PublicProductImageModel(
    Guid Id,
    string ImageUrl,
    int SortOrder,
    bool IsPrimary);

public sealed record PublicProductVariantModel(
    Guid Id,
    Guid SizeId,
    string Size,
    Guid ColorId,
    string Color,
    string? HexCode,
    int StockCount);
