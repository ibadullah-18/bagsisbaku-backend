namespace bagsisbaku.Contracts.Products;

public sealed record CreateProductRequest
{
    public required string NameAz { get; init; }

    public string? DescriptionAz { get; init; }

    public required string NameRu { get; init; }

    public string? DescriptionRu { get; init; }

    public required string NameEn { get; init; }

    public string? DescriptionEn { get; init; }

    public required string ProductCode { get; init; }

    public string? Model { get; init; }

    public decimal Price { get; init; }

    public decimal? DiscountPrice { get; init; }

    public int ProductType { get; init; }

    public Guid? CategoryId { get; init; }

    public Guid BrandId { get; init; }

    public bool IsFeatured { get; init; }

    public IReadOnlyList<CreateProductVariantRequest> Variants
    {
        get;
        init;
    } = [];
}

public sealed record CreateProductVariantRequest
{
    public Guid? SizeId { get; init; }

    public Guid ColorId { get; init; }

    public int StockCount { get; init; }
}
