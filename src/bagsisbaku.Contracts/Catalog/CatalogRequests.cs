namespace bagsisbaku.Contracts.Catalog;

public sealed record CreateBrandRequest
{
    public required string Name { get; init; }
}

public sealed record CreateCategoryRequest
{
    public required string NameAz { get; init; }

    public required string NameRu { get; init; }

    public required string NameEn { get; init; }

    public int ProductType { get; init; }
}

public sealed record CreateSizeRequest
{
    public required string ValueAz { get; init; }

    public string? ValueRu { get; init; }

    public string? ValueEn { get; init; }

    public int ProductType { get; init; }

    public int SortOrder { get; init; }
}

public sealed record CreateColorRequest
{
    public required string NameAz { get; init; }

    public required string NameRu { get; init; }

    public required string NameEn { get; init; }

    public string? HexCode { get; init; }
}

public sealed record SetCatalogDefaultRequest
{
    public Guid CategoryId { get; init; }

    public Guid SizeId { get; init; }
}
