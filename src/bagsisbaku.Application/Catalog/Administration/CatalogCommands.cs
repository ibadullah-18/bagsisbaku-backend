namespace bagsisbaku.Application.Catalog.Administration;

public sealed record CreateBrandCommand(
    string Name);

public sealed record CreateCategoryCommand(
    string NameAz,
    string NameRu,
    string NameEn,
    int ProductType);

public sealed record CreateSizeCommand(
    string ValueAz,
    string? ValueRu,
    string? ValueEn,
    int ProductType,
    int SortOrder);

public sealed record CreateColorCommand(
    string NameAz,
    string NameRu,
    string NameEn,
    string? HexCode);

public sealed record SetCatalogDefaultCommand(
    int ProductType,
    Guid CategoryId,
    Guid SizeId);
