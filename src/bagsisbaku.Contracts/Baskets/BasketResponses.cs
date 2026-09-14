namespace bagsisbaku.Contracts.Baskets;

public sealed record BasketResponse(
    Guid Id,
    string Language,
    int UniqueItemCount,
    int TotalQuantity,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    bool CanCheckout,
    IReadOnlyList<BasketItemResponse> Items);

public sealed record BasketItemResponse(
    Guid Id,
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string ProductCode,
    int ProductType,
    string BrandName,
    string? ImageUrl,
    Guid SizeId,
    string Size,
    Guid ColorId,
    string Color,
    string? HexCode,
    decimal OriginalUnitPrice,
    decimal UnitPrice,
    decimal UnitDiscountAmount,
    int Quantity,
    decimal LineSubtotal,
    decimal LineDiscountAmount,
    decimal LineTotal,
    int StockCount,
    bool IsAvailable,
    string AvailabilityCode);