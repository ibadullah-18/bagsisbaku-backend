using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Promotions;

public sealed class PromoCodeUsage : Entity
{
    private PromoCodeUsage()
    {
    }

    private PromoCodeUsage(
        Guid id,
        Guid promoCodeId,
        Guid userId,
        Guid orderId,
        decimal discountAmount,
        DateTimeOffset usedAtUtc)
        : base(id)
    {
        PromoCodeId =
            DomainGuard.NotEmpty(
                promoCodeId,
                nameof(PromoCodeId));

        UserId =
            DomainGuard.NotEmpty(
                userId,
                nameof(UserId));

        OrderId =
            DomainGuard.NotEmpty(
                orderId,
                nameof(OrderId));

        if (discountAmount <= 0)
        {
            throw new DomainException(
                "Promo kod endirimi sıfırdan böyük olmalıdır.");
        }

        DiscountAmount = discountAmount;
        UsedAtUtc = usedAtUtc;
    }

    public Guid PromoCodeId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid OrderId { get; private set; }

    public decimal DiscountAmount
    {
        get;
        private set;
    }

    public DateTimeOffset UsedAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? ReleasedAtUtc
    {
        get;
        private set;
    }

    public bool IsReleased =>
        ReleasedAtUtc.HasValue;

    public void Release(
        DateTimeOffset releasedAtUtc)
    {
        if (IsReleased)
        {
            return;
        }

        if (releasedAtUtc < UsedAtUtc)
        {
            throw new DomainException(
                "Promo kodun geri qaytarılma vaxtı istifadə vaxtından əvvəl ola bilməz.");
        }

        ReleasedAtUtc = releasedAtUtc;
    }
    public static PromoCodeUsage Create(
        Guid promoCodeId,
        Guid userId,
        Guid orderId,
        decimal discountAmount,
        DateTimeOffset usedAtUtc)
    {
        return new PromoCodeUsage(
            Guid.NewGuid(),
            promoCodeId,
            userId,
            orderId,
            discountAmount,
            usedAtUtc);
    }
}