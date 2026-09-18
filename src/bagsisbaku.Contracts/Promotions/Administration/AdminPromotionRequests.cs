namespace bagsisbaku.Contracts.Promotions.Administration;

public sealed record CreatePromoCodeRequest(
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

public sealed record UpdatePromoCodeRequest(
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

public sealed record SetPromoCodeStatusRequest(
    bool IsActive);