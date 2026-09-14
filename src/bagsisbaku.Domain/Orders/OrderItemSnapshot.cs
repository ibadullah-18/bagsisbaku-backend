using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Domain.Orders;

public sealed record OrderItemSnapshot(
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string ProductCode,
    ProductType ProductType,
    string BrandName,
    string? ImageUrl,
    Guid SizeId,
    string Size,
    Guid ColorId,
    string Color,
    string? HexCode,
    decimal OriginalUnitPrice,
    decimal UnitPrice,
    int Quantity);