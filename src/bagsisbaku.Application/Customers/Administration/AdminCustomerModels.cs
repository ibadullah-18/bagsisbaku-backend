namespace bagsisbaku.Application.Customers.Administration;

public sealed record AdminCustomerFilter(
    string? Search,
    bool? IsActive,
    bool? EmailConfirmed,
    DateTimeOffset? CreatedFromUtc,
    DateTimeOffset? CreatedToUtc,
    int Page,
    int PageSize);

public sealed record SetCustomerStatusCommand(
    Guid CustomerId,
    bool IsActive);

public sealed record AdminCustomerPageModel(
    IReadOnlyList<AdminCustomerSummaryModel> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record AdminCustomerSummaryModel(
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

public sealed record AdminCustomerDetailsModel(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    AdminCustomerOrderStatisticsModel OrderStatistics,
    IReadOnlyList<AdminCustomerAddressModel> Addresses,
    IReadOnlyList<AdminCustomerOrderSummaryModel> RecentOrders);

public sealed record AdminCustomerOrderStatisticsModel(
    int TotalOrders,
    int PendingOrders,
    int ConfirmedOrders,
    int OnDeliveryOrders,
    int DeliveredOrders,
    int CancelledOrders,
    decimal TotalSpent);

public sealed record AdminCustomerAddressModel(
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

public sealed record AdminCustomerOrderSummaryModel(
    Guid Id,
    string OrderNumber,
    int Status,
    int DeliveryType,
    int PaymentMethod,
    decimal Total,
    DateTimeOffset PlacedAtUtc);