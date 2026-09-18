namespace bagsisbaku.Contracts.Promotions.Administration;

public sealed record AdminPromoCodePageResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    IReadOnlyList<AdminPromoCodeResponse> Items);

public sealed record AdminPromoCodeResponse(
    Guid Id,
    string Name,
    string Code,
    int DiscountType,
    string DiscountTypeName,
    decimal DiscountValue,
    decimal MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    int? UsageLimit,
    int UsageCount,
    int? RemainingUsageCount,
    int? PerCustomerUsageLimit,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    bool IsActive,
    bool IsCurrentlyAvailable,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);