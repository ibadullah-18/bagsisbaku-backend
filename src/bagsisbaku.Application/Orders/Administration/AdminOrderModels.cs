using bagsisbaku.Application.Orders;

namespace bagsisbaku.Application.Orders.Administration;

public sealed record AdminOrderFilter(
    int? Status,
    int? DeliveryType,
    string? Search,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Page,
    int PageSize);

public sealed record ChangeOrderStatusCommand(
    int Status,
    string? Note);

public sealed record AdminOrderPageModel(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    IReadOnlyList<AdminOrderSummaryModel> Items);

public sealed record AdminOrderSummaryModel(
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

public sealed record AdminOrderDetailsModel(
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
    IReadOnlyList<PlacedOrderItemModel> Items,
    IReadOnlyList<CustomerOrderStatusHistoryModel>
        StatusHistory);