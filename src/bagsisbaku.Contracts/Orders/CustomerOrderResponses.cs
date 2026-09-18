namespace bagsisbaku.Contracts.Orders;

public sealed record CustomerOrderPageResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    IReadOnlyList<CustomerOrderSummaryResponse> Items);

public sealed record CustomerOrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    int Status,
    int DeliveryType,
    int PaymentMethod,
    int UniqueItemCount,
    int TotalQuantity,
    decimal Total,
    string? PreviewImageUrl,
    DateTimeOffset PlacedAtUtc,
    bool CanBeCancelled);

public sealed record CustomerOrderDetailsResponse(
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
    string? PromoCode,
    decimal PromoDiscountAmount,
    decimal TotalDiscountAmount,
    decimal DeliveryFee,
    decimal Total,
    DateTimeOffset PlacedAtUtc,
    DateTimeOffset? ConfirmedAtUtc,
    DateTimeOffset? OnDeliveryAtUtc,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset? CancelledAtUtc,
    bool CanBeCancelled,
    IReadOnlyList<PlacedOrderItemResponse> Items,
    IReadOnlyList<CustomerOrderStatusHistoryResponse>
        StatusHistory);

public sealed record CustomerOrderStatusHistoryResponse(
    Guid Id,
    int? PreviousStatus,
    int NewStatus,
    string? Note,
    DateTimeOffset ChangedAtUtc);