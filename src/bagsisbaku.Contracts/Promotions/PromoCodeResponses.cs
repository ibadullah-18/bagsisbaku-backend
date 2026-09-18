namespace bagsisbaku.Contracts.Promotions;

public sealed record PromoCodePreviewResponse(
    Guid PromoCodeId,
    string Code,
    int DiscountType,
    string DiscountTypeName,
    decimal DiscountValue,
    decimal MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    decimal BasketTotal,
    decimal DiscountAmount,
    decimal TotalAfterDiscount,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);