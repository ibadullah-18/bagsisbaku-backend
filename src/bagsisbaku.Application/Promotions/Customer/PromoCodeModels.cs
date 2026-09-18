namespace bagsisbaku.Application.Promotions.Customer;

public sealed record ValidatePromoCodeCommand(
    string Code);

public sealed record PromoCodePreviewModel(
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