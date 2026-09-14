using bagsisbaku.Domain.Orders;

namespace bagsisbaku.Application.Orders;

public sealed record PlaceOrderCommand(
    DeliveryType DeliveryType,
    Guid? CustomerAddressId,
    string? PickupRecipientFullName,
    string? PickupPhoneNumber,
    string? CustomerNote);

public sealed record PlacedOrderModel(
    Guid Id,
    string OrderNumber,
    string Language,
    int Status,
    int DeliveryType,
    int PaymentMethod,
    Guid? CustomerAddressId,
    string RecipientFullName,
    string PhoneNumber,
    string? City,
    string? District,
    string? AddressLine,
    string? PostalCode,
    string? DeliveryNote,
    string? CustomerNote,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal DeliveryFee,
    decimal Total,
    DateTimeOffset PlacedAtUtc,
    bool CanBeCancelled,
    IReadOnlyList<PlacedOrderItemModel> Items);

public sealed record PlacedOrderItemModel(
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
    decimal LineTotal);