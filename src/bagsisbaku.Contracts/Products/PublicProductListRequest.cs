using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Products;

public sealed record PublicProductListRequest
{
    [StringLength(200)]
    public string? Search { get; init; }

    [Range(1, 2)]
    public int? ProductType { get; init; }

    public Guid? CategoryId { get; init; }

    public Guid? BrandId { get; init; }

    public Guid[] SizeIds { get; init; } = [];

    public Guid[] ColorIds { get; init; } = [];

    [Range(
        typeof(decimal),
        "0",
        "999999999")]
    public decimal? MinPrice { get; init; }

    [Range(
        typeof(decimal),
        "0",
        "999999999")]
    public decimal? MaxPrice { get; init; }

    public bool? IsDiscounted { get; init; }

    public bool? IsFeatured { get; init; }

    public string Language { get; init; } = "az";

    public string Sort { get; init; } = "newest";

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 48)]
    public int PageSize { get; init; } = 12;
}
