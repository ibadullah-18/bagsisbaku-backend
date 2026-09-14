namespace bagsisbaku.Application.Catalog.Products;

public sealed record CreateProductVariantCommand(
    Guid? SizeId,
    Guid ColorId,
    int StockCount);

public sealed record CreateProductCommand(
    string NameAz,
    string? DescriptionAz,
    string NameRu,
    string? DescriptionRu,
    string NameEn,
    string? DescriptionEn,
    string ProductCode,
    string? Model,
    decimal Price,
    decimal? DiscountPrice,
    int ProductType,
    Guid? CategoryId,
    Guid BrandId,
    bool IsFeatured,
    IReadOnlyList<CreateProductVariantCommand> Variants);
