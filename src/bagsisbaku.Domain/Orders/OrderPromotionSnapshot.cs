using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Orders;

public sealed class OrderPromotionSnapshot
{
    public const int MaximumCodeLength = 50;

    public OrderPromotionSnapshot(
        Guid promoCodeId,
        string code,
        decimal discountAmount)
    {
        PromoCodeId =
            DomainGuard.NotEmpty(
                promoCodeId,
                nameof(PromoCodeId));

        Code =
            DomainGuard.Required(
                code,
                nameof(Code),
                MaximumCodeLength)
            .ToUpperInvariant();

        if (discountAmount <= 0)
        {
            throw new DomainException(
                "Promo kod endirimi sıfırdan böyük olmalıdır.");
        }

        DiscountAmount =
            decimal.Round(
                discountAmount,
                2,
                MidpointRounding.AwayFromZero);
    }

    public Guid PromoCodeId { get; }

    public string Code { get; }

    public decimal DiscountAmount { get; }
}