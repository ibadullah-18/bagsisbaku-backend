namespace bagsisbaku.Contracts.Customers.Administration;

public sealed record AdminCustomerPageResponse(
    IReadOnlyList<AdminCustomerSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record AdminCustomerSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsActive,
    int TotalOrders,
    int DeliveredOrders,
    decimal TotalSpent,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastLoginAtUtc);

public sealed record AdminCustomerDetailsResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    AdminCustomerOrderStatisticsResponse OrderStatistics,
    IReadOnlyList<AdminCustomerAddressResponse> Addresses,
    IReadOnlyList<AdminCustomerOrderSummaryResponse> RecentOrders);

public sealed record AdminCustomerOrderStatisticsResponse(
    int TotalOrders,
    int PendingOrders,
    int ConfirmedOrders,
    int OnDeliveryOrders,
    int DeliveredOrders,
    int CancelledOrders,
    decimal TotalSpent);

public sealed record AdminCustomerAddressResponse(
    Guid Id,
    string Title,
    string RecipientFullName,
    string PhoneNumber,
    string City,
    string? District,
    string AddressLine,
    string? PostalCode,
    string? DeliveryNote,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record AdminCustomerOrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    int Status,
    int DeliveryType,
    int PaymentMethod,
    decimal Total,
    DateTimeOffset PlacedAtUtc);