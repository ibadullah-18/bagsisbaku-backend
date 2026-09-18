namespace bagsisbaku.Application.Promotions.Administration;

public sealed record AdminPromoCodeFilter(
    string? Search,
    bool? IsActive,
    int Page,
    int PageSize);

public sealed record CreatePromoCodeCommand(
    string Name,
    string Code,
    int DiscountType,
    decimal DiscountValue,
    decimal MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    int? UsageLimit,
    int? PerCustomerUsageLimit,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);

public sealed record UpdatePromoCodeCommand(
    string Name,
    string Code,
    int DiscountType,
    decimal DiscountValue,
    decimal MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    int? UsageLimit,
    int? PerCustomerUsageLimit,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);

public sealed record SetPromoCodeStatusCommand(
    bool IsActive);

public sealed record AdminPromoCodePageModel(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    IReadOnlyList<AdminPromoCodeModel> Items);

public sealed record AdminPromoCodeModel(
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