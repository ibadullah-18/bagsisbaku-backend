namespace bagsisbaku.Application.Baskets;

public sealed record BasketModel(
    Guid Id,
    string Language,
    int UniqueItemCount,
    int TotalQuantity,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    bool CanCheckout,
    IReadOnlyList<BasketItemModel> Items);

public sealed record BasketItemModel(
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

public sealed record AddBasketItemCommand(
    Guid ProductVariantId,
    int Quantity);

public sealed record UpdateBasketItemQuantityCommand(
    int Quantity);