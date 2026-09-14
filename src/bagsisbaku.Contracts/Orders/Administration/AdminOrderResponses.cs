using bagsisbaku.Contracts.Orders;

namespace bagsisbaku.Contracts.Orders.Administration;

public sealed record AdminOrderPageResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    IReadOnlyList<AdminOrderSummaryResponse> Items);

public sealed record AdminOrderSummaryResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerFullName,
    string CustomerEmail,
    string OrderNumber,
    string RecipientFullName,
    string PhoneNumber,
    int Status,
    int DeliveryType,
    int PaymentMethod,
    int UniqueItemCount,
    int TotalQuantity,
    decimal Total,
    DateTimeOffset PlacedAtUtc,
    bool CanBeCancelled);

public sealed record AdminOrderDetailsResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerFullName,
    string CustomerEmail,
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
    DateTimeOffset? ConfirmedAtUtc,
    DateTimeOffset? OnDeliveryAtUtc,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset? CancelledAtUtc,
    bool CanBeCancelled,
    IReadOnlyList<PlacedOrderItemResponse> Items,
    IReadOnlyList<CustomerOrderStatusHistoryResponse>
        StatusHistory);